// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Net.Sockets;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

#if CLIENT

// *** NETWORK CLIENT FOR TCP CONNECTIONS WITH THE SERVER ***

public class NetworkClient
{
    private MatchmakingClient matchmakingClient;

	private TcpClient client = null;

	private MatchStatusInfo matchStatusInfo = null;

	private bool connectionSucceeded = false;
	public bool ConnectionSucceeded() { return connectionSucceeded; }

    public NetworkClient()
    {
        this.matchmakingClient = new MatchmakingClient();
    }

    // Calls the matchmaking client to do matchmaking against the backend and then connects to the game server with TCP
    public IEnumerator DoMatchMakingAndConnect(Dictionary<string,double> regionLatencies)
	{
		Debug.Log("Request matchmaking...");
        GameObject.FindObjectOfType<UIManager>().SetTextBox("Requesting matchmaking...");
        yield return null;
        var matchMakingRequestInfo = this.matchmakingClient.RequestMatchMaking(regionLatencies);
		Debug.Log("TicketId: " + matchMakingRequestInfo.TicketId);

		if (matchMakingRequestInfo != null)
		{
			bool matchmakingDone = false;
			int tries = 0;
			while (!matchmakingDone)
			{
				Debug.Log("Checking match status...");
				GameObject.FindObjectOfType<UIManager>().SetTextBox("Checking match status...");
				yield return null;
				this.matchStatusInfo = this.matchmakingClient.RequestMatchStatus(matchMakingRequestInfo.TicketId);
				if (matchStatusInfo.PlayerSessionId.Equals("NotPlacedYet"))
				{
					Debug.Log("Still waiting for placement");
					GameObject.FindObjectOfType<UIManager>().SetTextBox("Still waiting for placement...");
					yield return new WaitForSeconds(2.0f);
				}
				else
				{
					Debug.Log("Matchmaking done!");
					GameObject.FindObjectOfType<UIManager>().SetTextBox("Matchmaking done! Connecting to server...");
					yield return null;
					matchmakingDone = true;

					// Set the info to UI
					GameObject.FindObjectOfType<UIManager>().SetGameServerInfo("Game Server IP: " + this.matchStatusInfo.IpAddress + "\n" + "Port: " + this.matchStatusInfo.Port + "\n" + "PlayerSessionId: " + this.matchStatusInfo.PlayerSessionId + "\n");

					// Matchmaking done, connect to the servers
					Connect();
				}
				tries++;

                // Return null if we failed after 20 tries
				if (tries >= 10)
				{
					GameObject.FindObjectOfType<UIManager>().SetTextBox("Aborting matchmaking, no match done on 20 seconds");
					Debug.Log("Aborting matchmaking, no match done on 20 seconds");
					// Wait a while and restart
					yield return new WaitForSeconds(1.5f);
					var clientObject = GameObject.FindObjectOfType<Client>();
					clientObject.Restart();
					break;
				}
				yield return null;
			}
		}
		else
		{
			GameObject.FindObjectOfType<UIManager>().SetTextBox("Matchmaking failed! Not connected.");
			Debug.Log("Matchmaking request failed!");
		}

		yield return null;
	}

    // Called by the client to receive new messages
	public void Update()
	{
		if (client == null) return;
		var messages = NetworkProtocol.Receive(client);
        
		foreach (SimpleMessage msg in messages)
		{
			HandleMessage(msg);
		}
	}

	private bool TryConnect()
	{
		try
		{
			//Connect with matchmaking info
			Debug.Log("Connect..");
			this.client = new TcpClient();
			var result = client.BeginConnect(this.matchStatusInfo.IpAddress, this.matchStatusInfo.Port, null, null);

			var success = result.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(2));

			if (!success)
			{
				throw new Exception("Failed to connect.");
			}
			client.NoDelay = true; // Use No Delay to send small messages immediately. UDP should be used for even faster messaging
			Debug.Log("Done");

			// Send the player session ID to server so it can validate the player
            SimpleMessage connectMessage = new SimpleMessage(MessageType.Connect, this.matchStatusInfo.PlayerSessionId);
            this.SendMessage(connectMessage);

			return true;
		}
		catch (Exception e)
		{
			Debug.Log(e.Message);
			client = null;
			GameObject.FindObjectOfType<UIManager>().SetTextBox("Failed to connect: " + e.Message);
			return false;
		}
	}

	private void Connect()
	{
		// try to connect to a local server
		if (TryConnect() == false)
		{
			Debug.Log("Failed to connect to server");
			GameObject.FindObjectOfType<UIManager>().SetTextBox("Connection to server failed.");

			// Restart the client
			var clientObject = GameObject.FindObjectOfType<Client>();
			clientObject.Restart();
		}
		else
		{
			//We're ready to play, let the server know
			this.Ready();
			GameObject.FindObjectOfType<UIManager>().SetTextBox("Connected to server");
		}
	}

	// Send ready to play message to server
	public void Ready()
	{
		if (client == null) return;
		this.connectionSucceeded = true;

        // Send READY message to let server know we are ready
        SimpleMessage message = new SimpleMessage(MessageType.Ready);
		try
		{
			NetworkProtocol.Send(client, message);
		}
		catch (SocketException e)
		{
			HandleDisconnect();
		}
	}

    // Send serialized binary message to server
    public void SendMessage(SimpleMessage message)
    {
        if (client == null) return;
        try
        {
            NetworkProtocol.Send(client, message);
        }
        catch (SocketException e)
        {
            HandleDisconnect();
        }
    }

	// Send disconnect message to server
	public void Disconnect()
	{
		if (client == null) return;
        SimpleMessage message = new SimpleMessage(MessageType.Disconnect);
		try
		{
			NetworkProtocol.Send(client, message);
		}

		finally
		{
			HandleDisconnect();
		}
	}

	// Handle a message received from the server
	private void HandleMessage(SimpleMessage msg)
	{
		// parse message and pass json string to relevant handler for deserialization
		//Debug.Log("Message received:" + msg.messageType + ":" + msg.message);
		if (msg.messageType == MessageType.Reject)
			HandleReject();
		else if (msg.messageType == MessageType.Disconnect)
			HandleDisconnect();
		else if (msg.messageType == MessageType.Spawn)
			HandleOtherPlayerSpawned(msg);
		else if (msg.messageType == MessageType.Position)
			HandleOtherPlayerPos(msg);
		else if (msg.messageType == MessageType.PositionOwn)
			HandlePlayerPos(msg);
		else if (msg.messageType == MessageType.PlayerLeft)
			HandleOtherPlayerLeft(msg);
	}

	private void HandleReject()
	{
		NetworkStream stream = client.GetStream();
		stream.Close();
		client.Close();
		client = null;
	}

	private void HandleDisconnect()
	{
		try
		{
			Debug.Log("Got disconnected by server");
			GameObject.FindObjectOfType<UIManager>().SetTextBox("Got disconnected by server");
			NetworkStream stream = client.GetStream();
			stream.Close();
			client.Close();
			client = null;
		}
		catch (Exception e)
		{
			Debug.Log("Error when disconnecting, setting client to null.");
			client = null;
		}
	}

	private void HandleOtherPlayerSpawned(SimpleMessage message)
	{
		Client.messagesToProcess.Add(message);
	}

	private void HandlePlayerPos(SimpleMessage message)
	{
		Client.messagesToProcess.Add(message);
	}

	private void HandleOtherPlayerPos(SimpleMessage message)
    {
		Client.messagesToProcess.Add(message);
	}

	private void HandleOtherPlayerLeft(SimpleMessage message)
	{
		Client.messagesToProcess.Add(message);
	}
}

#endif

