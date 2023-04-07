// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System.Net;
using System.Net.Sockets;
using UnityEngine;
using Aws.GameLift.Server;
using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using System.IO;

// *** MONOBEHAVIOUR TO MANAGE SERVER LOGIC *** //

public class Server : MonoBehaviour
{
    public GameObject playerPrefab;

    #if SERVER

    // List of players
    public List<NetworkPlayer> players = new List<NetworkPlayer>();
    public int rollingPlayerId = 0; //Rolling player id that is used to give new players an ID when connecting

    //We get events back from the NetworkServer through this static list
    public static List<SimpleMessage> messagesToProcess = new List<SimpleMessage>();

    NetworkServer server;

    // Helper function to check if a player exists in the enemy list already
    private bool PlayerExists(int clientId)
    {
        foreach (NetworkPlayer player in players)
        {
            if (player.GetPlayerId() == clientId)
            {
                return true;
            }
        }
        return false;
    }

    // Helper function to find a player from the enemy list
    private NetworkPlayer GetPlayer(int clientId)
    {
        foreach (NetworkPlayer player in players)
        {
            if (player.GetPlayerId() == clientId)
            {
                return player;
            }
        }
        return null;
    }

    public void RemovePlayer(int clientId)
    {
        foreach (NetworkPlayer player in players)
        {
            if (player.GetPlayerId() == clientId)
            {
                player.DeleteGameObject();
                players.Remove(player);
                return;
            }
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        // Set the target framerate to 30 FPS to avoid running at 100% CPU. Clients send input at 20 FPS
        Application.targetFrameRate = 30;

        var gameliftServer = GameObject.FindObjectOfType<GameLift>();
        server = new NetworkServer(gameliftServer, this);    
    }

    // FixedUpdate is called 30 times per second (configured in Project Settings -> Time -> Fixed TimeStep).
    // This is the interval we're running the simulation and processing messages on the server
    void FixedUpdate()
    {
        // Update the Network server to check client status and get messages
        server.Update();

        // Process any messages we received
        this.ProcessMessages();

        // Move players based on latest input and update player states to clients
        for (int i = 0; i < this.players.Count; i++)
        {
            var player = this.players[i];
            // Move
            player.Move();

            // Send state if changed
            var positionMessage = player.GetPositionMessage();
            if (positionMessage != null)
            {
                positionMessage.clientId = player.GetPlayerId();
                this.server.TransmitMessage(positionMessage, player.GetPlayerId());
                //Send to the player him/herself
                positionMessage.messageType = MessageType.PositionOwn;
                this.server.SendMessage(player.GetPlayerId(), positionMessage);
            }
        }
    }

    private void ProcessMessages()
    {
        // Go through any messages we received to process
        foreach (SimpleMessage msg in messagesToProcess)
        {
            // Spawn player
            if (msg.messageType == MessageType.Spawn)
            {
                Debug.Log("Player spawned: " + msg.float1 + "," + msg.float2 + "," + msg.float3);
                NetworkPlayer player = new NetworkPlayer(msg.clientId);
                this.players.Add(player);
                player.Spawn(msg, this.playerPrefab);
                player.SetPlayerId(msg.clientId);

                // Send all existing player positions to the newly joined
                for (int i = 0; i < this.players.Count-1; i++)
                {
                    var otherPlayer = this.players[i];
                    // Send state
                    var positionMessage = otherPlayer.GetPositionMessage(overrideChangedCheck: true);
                    if (positionMessage != null)
                    {
                        positionMessage.clientId = otherPlayer.GetPlayerId();
                        this.server.SendMessage(player.GetPlayerId(), positionMessage);
                    }
                }
            }

            // Set player input
            if (msg.messageType == MessageType.PlayerInput)
            {
                // Only handle input if the player exists
                if (this.PlayerExists(msg.clientId))
                {
                    //Debug.Log("Player moved: " + msg.float1 + "," + msg.float2 + " ID: " + msg.clientId);

                    if (this.PlayerExists(msg.clientId))
                    {
                        var player = this.GetPlayer(msg.clientId);
                        player.SetInput(msg);
                    }
                    else
                    {
                        Debug.Log("PLAYER MOVED BUT IS NOT SPAWNED! SPAWN TO RANDOM POS");
                        Vector3 spawnPos = new Vector3(UnityEngine.Random.Range(-5, 5), 1, UnityEngine.Random.Range(-5, 5));
                        var quat = Quaternion.identity;
                        SimpleMessage tmpMsg = new SimpleMessage(MessageType.Spawn);
                        tmpMsg.SetFloats(spawnPos.x, spawnPos.y, spawnPos.z, quat.x, quat.y, quat.z, quat.w);
                        tmpMsg.clientId = msg.clientId;

                        NetworkPlayer player = new NetworkPlayer(msg.clientId);
                        this.players.Add(player);
                        player.Spawn(tmpMsg, this.playerPrefab);
                        player.SetPlayerId(msg.clientId);
                    }
                }
                else
                {
                    Debug.Log("Player doesn't exists anymore, don't take in input: " + msg.clientId);
                }
            }
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
    // Clients are stored as a dictionary of the TCPCLient and the ClientID
    private Dictionary<TcpClient, int> clients = new Dictionary<TcpClient,int>();
    private List<TcpClient> readyClients = new List<TcpClient>();
    private List<TcpClient> clientsToRemove = new List<TcpClient>();

    private GameLift gamelift = null;
    private Server server = null;

    public int GetPlayerCount() { return clients.Count; }


    public NetworkServer(GameLift gamelift, Server server)
	{
        this.server = server;
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

            // We have a maximum of 5 clients per game
            if(this.clients.Count < 5)
            {
                // Add client and give it the Id of the value of rollingPlayerId
                this.clients.Add(client, this.server.rollingPlayerId);
                this.server.rollingPlayerId++;
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
            var tcpClient = client.Key;
            try
            {
                if (tcpClient == null) continue;
                if (this.IsSocketConnected(tcpClient) == false)
                {
                    System.Console.WriteLine("Client not connected anymore");
                    this.clientsToRemove.Add(tcpClient);
                }
                var messages = NetworkProtocol.Receive(tcpClient);
                if(messages != null)
                {
                    foreach(SimpleMessage message in messages)
                    {
                        //System.Console.WriteLine("Received message: " + message.message + " type: " + message.messageType);
                        bool disconnect = HandleMessage(playerIdx, tcpClient, message);
                        if (disconnect)
                            this.clientsToRemove.Add(tcpClient);
                    }
                }
            }
            catch (Exception e)
            {
                System.Console.WriteLine("Error receiving from a client: " + e.Message);
                this.clientsToRemove.Add(tcpClient);
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
            this.clientsToRemove.Add(client.Key);
        }

        //Reset the client lists
        this.clients = new Dictionary<TcpClient, int>();
        this.readyClients = new List<TcpClient>();
        this.server.players = new List<NetworkPlayer>();
	}

    public void TransmitMessage(SimpleMessage msg, int excludeClient)
    {
        // send the same message to all players
        foreach (var client in this.clients)
        {
            //Skip if this is the excluded client
            if (client.Value == excludeClient)
            {
                continue;
            }

            try
            {
                NetworkProtocol.Send(client.Key, msg);
            }
            catch (Exception e)
            {
                this.clientsToRemove.Add(client.Key);
            }
        }
    }

    //Transmit message to multiple clients
	public void TransmitMessage(SimpleMessage msg, TcpClient excludeClient = null)
	{
        // send the same message to all players
        foreach (var client in this.clients)
		{
            //Skip if this is the excluded client
            if(excludeClient != null && excludeClient == client.Key)
            {
                continue;
            }

			try
			{
				NetworkProtocol.Send(client.Key, msg);
			}
			catch (Exception e)
			{
                this.clientsToRemove.Add(client.Key);
			}
		}
    }

    private TcpClient SearchClient(int clientId)
    {
        foreach(var client in this.clients)
        {
            if(client.Value == clientId)
            {
                return client.Key;
            }
        }
        return null;
    }

    public void SendMessage(int clientId, SimpleMessage msg)
    {
        try
        {
            TcpClient client = this.SearchClient(clientId);
            SendMessage(client, msg);
        }
        catch (Exception e)
        {
            Console.WriteLine("Failed to send message to client: " + clientId);
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
        else if (msg.messageType == MessageType.PlayerInput)
            HandleMove(client, msg);

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
        // start the game once we have at least one client online
        this.readyClients.Add(client);

        if (readyClients.Count >= 1)
        {
            System.Console.WriteLine("We have our first player in, let's start the game!");
            this.gamelift.StartGame();
        }
	}

    private void HandleSpawn(TcpClient client, SimpleMessage message)
    {
        // Get client id (this is the value in the dictionary where the TCPClient is the key)
        int clientId = this.clients[client];

        System.Console.WriteLine("Player " + clientId + " spawned with coordinates: " + message.float1 + "," + message.float2 + "," + message.float3);

        // Add client ID
        message.clientId = clientId;

        // Add to list to create the gameobject instance on the server
        Server.messagesToProcess.Add(message);

        // Just testing the StatsD client
        this.gamelift.GetStatsdClient().SendCounter("players.PlayerSpawn", 1);
    }

    private void HandleMove(TcpClient client, SimpleMessage message)
    {
        // Get client id (this is the value in the dictionary where the TCPClient is the key)
        int clientId = this.clients[client];

        //System.Console.WriteLine("Got move from client: " + clientId + " with input: " + message.float1 + "," + message.float2);

        // Add client ID
        message.clientId = clientId;

        // Add to list to create the gameobject instance on the server
        Server.messagesToProcess.Add(message);
    }

    private void RemoveClient(TcpClient client)
    {
        //Let the other clients know the player was removed
        int clientId = this.clients[client];

        SimpleMessage message = new SimpleMessage(MessageType.PlayerLeft);
        message.clientId = clientId;
        TransmitMessage(message, client);

        // Disconnect and remove
        this.DisconnectPlayer(client);
        this.clients.Remove(client);
        this.readyClients.Remove(client);
        this.server.RemovePlayer(clientId);
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

    #endif
}
