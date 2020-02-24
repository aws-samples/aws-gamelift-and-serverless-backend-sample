// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System.Net;
using System.Net.Sockets;
using UnityEngine;
using Aws.GameLift.Server;
using System;
using System.Collections.Generic;

#if SERVER

// *** MONOBEHAVIOUR TO MANAGE SERVER LOGIC *** //

public class Server : MonoBehaviour
{
    //We get events back from the NetworkServer through this static list
    public static List<SimpleMessage> messagesToProcess = new List<SimpleMessage>();

    NetworkServer server;

    // Start is called before the first frame update
    void Start()
    {
        var gameliftServer = GameObject.FindObjectOfType<GameLift>();
        server = new NetworkServer(gameliftServer);    
    }

    // Update is called once per frame
    void Update()
    {
        server.Update();

        // Go through any messages to process (on the game world)
        foreach(SimpleMessage msg in messagesToProcess)
        {
            // NOTE: We should spawn players and set positions also on server side here and validate actions. For now we just pass this data to clients
        }
        messagesToProcess.Clear();
    }

    public void DisconnectAll()
    {
        this.server.DisconnectAll();
    }

}

// *** SERVER NETWORK LOGIC *** //

public class NetworkServer
{
	private TcpListener listener;
    private List<TcpClient> clients = new List<TcpClient>();
    private List<TcpClient> readyClients = new List<TcpClient>();
    private List<TcpClient> clientsToRemove = new List<TcpClient>();

    private GameLift gamelift = null;

    public NetworkServer(GameLift gamelift)
	{
        this.gamelift = gamelift;

        //Start the TCP server
        int port = this.gamelift.listeningPort;
        Debug.Log("Starting server on port " + port);
        listener = new TcpListener(IPAddress.Any, this.gamelift.listeningPort);
        Debug.Log("Listening at: " + listener.LocalEndpoint.ToString());
		listener.Start();
	}

    // Checks if socket is still connected
    private bool IsSocketConnected(TcpClient client)
    {
        var bClosed = false;

        // Detect if client disconnected
        if (client.Client.Poll(0, SelectMode.SelectRead))
        {
            byte[] buff = new byte[1];
            if (client.Client.Receive(buff, SocketFlags.Peek) == 0)
            {
                // Client disconnected
                bClosed = true;
            }
        }

        return !bClosed;
    }

    public void Update()
	{
		// Are there any new connections pending?
		if (listener.Pending())
		{
            System.Console.WriteLine("Client pending..");
			TcpClient client = listener.AcceptTcpClient();
            client.NoDelay = true; // Use No Delay to send small messages immediately. UDP should be used for even faster messaging
            System.Console.WriteLine("Client accepted.");

            // We have a maximum of 10 clients per game
            if(this.clients.Count < 10)
            {
                this.clients.Add(client);
                return;
            }
            else
            {
                // game already full, reject the connection
                try
                {
                    SimpleMessage message = new SimpleMessage(MessageType.Reject, "game already full");
                    NetworkProtocol.Send(client, message);
                }
                catch (SocketException) { }
            }

		}

        // Iterate through clients and check if they have new messages or are disconnected
        int playerIdx = 0;
        foreach (var client in this.clients)
		{
            try
            {
                if (client == null) continue;
                if (this.IsSocketConnected(client) == false)
                {
                    System.Console.WriteLine("Client not connected anymore");
                    this.clientsToRemove.Add(client);
                }
                var messages = NetworkProtocol.Receive(client);
                foreach(SimpleMessage message in messages)
                {
                    System.Console.WriteLine("Received message: " + message.message + " type: " + message.messageType);
                    bool disconnect = HandleMessage(playerIdx, client, message);
                    if (disconnect)
                        this.clientsToRemove.Add(client);
                }
            }
            catch (Exception e)
            {
                System.Console.WriteLine("Error receiving from a client: " + e.Message);
                this.clientsToRemove.Add(client);
            }
            playerIdx++;
		}

        //Remove dead clients
        foreach (var clientToRemove in this.clientsToRemove)
        {
            try
            {
                this.RemoveClient(clientToRemove);
            }
            catch (Exception e)
            {
                System.Console.WriteLine("Couldn't remove client: " + e.Message);
            }
        }
        this.clientsToRemove.Clear();

        //End game if no clients
        if(this.gamelift.GameStarted())
        {
            if(this.clients.Count <= 0)
            {
                System.Console.WriteLine("Clients gone, stop session");
                this.gamelift.TerminateGameSession();
            }
        }

        // Simple test for the the StatsD client: Send current amount of player online
        if (this.gamelift.GameStarted())
        {
            this.gamelift.GetStatsdClient().SendGauge("game.ClientSocketsConnected", this.clients.Count);
        }
    }

    public void DisconnectAll()
    {
        // warn clients
        SimpleMessage message = new SimpleMessage(MessageType.Disconnect);
        TransmitMessage(message);
        // disconnect connections
        foreach (var client in this.clients)
        {
            this.clientsToRemove.Add(client);
        }

        //Reset the client lists
        this.clients = new List<TcpClient>();
        this.readyClients = new List<TcpClient>();
	}

    //Transmit message to multiple clients
	private void TransmitMessage(SimpleMessage msg, TcpClient excludeClient = null)
	{
        // send the same message to all players
        foreach (var client in this.clients)
		{
            //Skip if this is the excluded client
            if(excludeClient != null && excludeClient == client)
            {
                continue;
            }

			try
			{
				NetworkProtocol.Send(client, msg);
			}
			catch (Exception e)
			{
                this.clientsToRemove.Add(client);
			}
		}
    }

    //Send message to single client
    private void SendMessage(TcpClient client, SimpleMessage msg)
    {
        try
        {
            NetworkProtocol.Send(client, msg);
        }
        catch (Exception e)
        {
            this.clientsToRemove.Add(client);
        }
    }

    private bool HandleMessage(int playerIdx, TcpClient client, SimpleMessage msg)
	{
        if (msg.messageType == MessageType.Connect)
        {
            HandleConnect(playerIdx, msg.message, client);
        }
        else if (msg.messageType == MessageType.Disconnect)
        {
            this.clientsToRemove.Add(client);
            return true;
        }
        else if (msg.messageType == MessageType.Ready)
            HandleReady(client);
        else if (msg.messageType == MessageType.Spawn)
            HandleSpawn(client, msg);
        else if (msg.messageType == MessageType.Position)
            HandlePos(client, msg);

        return false;
    }

	private void HandleConnect(int playerIdx, string json, TcpClient client)
	{
        // respond with the player id and the current state.
        //Connect player
        var outcome = GameLiftServerAPI.AcceptPlayerSession(json);
        if (outcome.Success)
        {
            System.Console.WriteLine(":) PLAYER SESSION VALIDATED");
        }
        else
        {
            System.Console.WriteLine(":( PLAYER SESSION REJECTED. AcceptPlayerSession() returned " + outcome.Error.ToString());
            this.clientsToRemove.Add(client);
        }
	}

	private void HandleReady(TcpClient client)
	{
        // start the game once all connected clients have requested to start (RETURN key)
        this.readyClients.Add(client);

        if (readyClients.Count >= 2)
        {
            System.Console.WriteLine("Enough clients, let's start the game!");
            this.gamelift.StartGame();
        }
	}

    private void HandleSpawn(TcpClient client, SimpleMessage message)
    {
        // Get client id (index in list for now)
        int clientId = this.clients.IndexOf(client);

        System.Console.WriteLine("Player " + clientId + " spawned with coordinates: " + message.float1 + "," + message.float2 + "," + message.float3);

        // Add client ID
        message.clientId = clientId;

        // Add to list to create the gameobject instance on the server
        Server.messagesToProcess.Add(message);

        //Inform the other clients about the player pos
        this.TransmitMessage(message, excludeClient: client);

        // Just testing the StatsD client
        this.gamelift.GetStatsdClient().SendCounter("players.PlayerSpawn", 1);
    }

    private void HandlePos(TcpClient client, SimpleMessage message)
    {
        // Get client id (index in list for now)
        int clientId = this.clients.IndexOf(client);

        System.Console.WriteLine("Got pos from client: " + clientId + " with coordinates: " + message.float1 + "," + message.float2 + "," + message.float3);

        // Add client ID
        message.clientId = clientId;

        // Add to list to create the gameobject instance on the service
        Server.messagesToProcess.Add(message);

        // Inform the other clients about the player pos
        // (NOTE: We should validate it's legal and actually share the server view of the position)
        this.TransmitMessage(message, excludeClient: client);

        // Just testing the StatsD client
        this.gamelift.GetStatsdClient().SendCounter("players.PlayerPositionUpdate", 1);
    }

    private void RemoveClient(TcpClient client)
    {
        //Let the other clients know the player was removed
        int clientId = this.clients.IndexOf(client);

        SimpleMessage message = new SimpleMessage(MessageType.PlayerLeft);
        message.clientId = clientId;
        TransmitMessage(message, client);

        // Disconnect and remove
        this.DisconnectPlayer(client);
        this.clients.Remove(client);
        this.readyClients.Remove(client);
    }

	private void DisconnectPlayer(TcpClient client)
	{
        try
        {
            // remove the client and close the connection
            if (client != null)
            {
                NetworkStream stream = client.GetStream();
                stream.Close();
                client.Close();
            }
        }
        catch (Exception e)
        {
            System.Console.WriteLine("Failed to disconnect player: " + e.Message);
        }
	}
}
#endif
