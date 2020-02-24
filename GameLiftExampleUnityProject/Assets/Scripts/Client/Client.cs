// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using Amazon;
using Amazon.CognitoIdentity;

// *** MAIN CLIENT CLASS FOR MANAGING CLIENT CONNECTIONS AND MESSAGES ***

public class Client : MonoBehaviour
{
    // Prefabs for the player and enemy objects referenced from the scene object
    public GameObject characterPrefab;
	public GameObject enemyPrefab;

#if CLIENT

    // Local player
    private NetworkClient networkClient;
    private NetworkPlayer localPlayer;

    // List of enemy players
    private List<NetworkPlayer> enemyPlayers = new List<NetworkPlayer>();

    //We get events back from the NetworkServer through this static list
    public static List<SimpleMessage> messagesToProcess = new List<SimpleMessage>();

    private float updateCounter = 0.0f;

    //Cognito credentials for sending signed requests to the API
    public static Amazon.Runtime.ImmutableCredentials cognitoCredentials = null;

    // Helper function check if an enemy exists in the enemy list already
    private bool EnemyPlayerExists(int clientId)
    {
        foreach(NetworkPlayer player in enemyPlayers)
        {
            if(player.GetPlayerId() == clientId)
            {
                return true;
            }
        }
        return false;
    }

    // Helper function to find and enemy from the enemy list
    private NetworkPlayer GetEnemyPlayer(int clientId)
    {
        foreach (NetworkPlayer player in enemyPlayers)
        {
            if (player.GetPlayerId() == clientId)
            {
                return player;
            }
        }
        return null;
    }

    // Called by Unity when the Gameobject is created
    void Start()
    {
        FindObjectOfType<UIManager>().SetTextBox("Setting up Client..");

        // Set up Mobile SDK
        UnityInitializer.AttachToGameObject(this.gameObject);
        AWSConfigs.AWSRegion = MatchmakingClient.regionString;
        AWSConfigs.HttpClient = AWSConfigs.HttpClientOption.UnityWebRequest;

        // Get Cognito Identity and start Connecting to server once we have the identity
        CognitoAWSCredentials credentials = new CognitoAWSCredentials(
            MatchmakingClient.identityPoolID,
            MatchmakingClient.region
        );
        credentials.GetCredentialsAsync(
            (response) => {
                Debug.Log("Received CognitoCredentials: " + response.Response);
                cognitoCredentials = response.Response;

                // Start a coroutine for the connection process to keep UI updated while it's happening
                StartCoroutine(ConnectToServer());
            }
        );
    }

    // Update is called once per frame
    void Update()
    {
        if (this.localPlayer != null)
        {
            // Process any messages we have received over the network
            this.ProcessMessages();

            // Only send updates 10 times per second to avoid flooding server with messages
            this.updateCounter += Time.deltaTime;
            if (updateCounter < 0.1f)
            {
                return;
            }
            this.updateCounter = 0.0f;

            // Send current position data to other players through the server
            this.SendPosition();

            // Receive new messages
            this.networkClient.Update();
        }
    }

    // Do matchmaking and connect to the server endpoint received
    // This is a coroutine to simplify the logic and keep our UI updated throughout the process
    IEnumerator ConnectToServer()
    {
        FindObjectOfType<UIManager>().SetTextBox("Connecting to backend..");

        yield return null;

        // Start network client and connect to server
        this.networkClient = new NetworkClient();
        // We will wait for the matchmaking and connection coroutine to end before creating the player
        yield return StartCoroutine(this.networkClient.DoMatchMakingAndConnect());

        if (this.networkClient.ConnectionSucceeded())
        {
            // Create character
            this.localPlayer = new NetworkPlayer(0);
            this.localPlayer.Initialize(characterPrefab, new Vector3(UnityEngine.Random.Range(-5,5), 1, UnityEngine.Random.Range(-5, 5)));
            this.networkClient.SendMessage(this.localPlayer.GetSpawnMessage());
        }

        yield return null;
    }

    // Process messages received from server
    void ProcessMessages()
    {
        List<int> justLeftClients = new List<int>();
        List<int> clientsMoved = new List<int>();

        // Go through any messages to process
        foreach (SimpleMessage msg in messagesToProcess)
        {
            // players spawn and position messages
            if (msg.messageType == MessageType.Spawn || msg.messageType == MessageType.Position || msg.messageType == MessageType.PlayerLeft)
            {
                if (msg.messageType == MessageType.Spawn && this.EnemyPlayerExists(msg.clientId) == false)
                {
                    Debug.Log("Enemy spawned: " + msg.float1 + "," + msg.float2 + "," + msg.float3);
                    NetworkPlayer enemyPlayer = new NetworkPlayer(msg.clientId);
                    this.enemyPlayers.Add(enemyPlayer);
                    enemyPlayer.Spawn(msg, this.enemyPrefab);
                }
                else if (msg.messageType == MessageType.Position && justLeftClients.Contains(msg.clientId) == false)
                {
                    Debug.Log("Enemy pos received: " + msg.float1 + "," + msg.float2 + "," + msg.float3);
                    //Setup enemycharacter if not done yet
                    if (this.EnemyPlayerExists(msg.clientId) == false)
                    {
                        Debug.Log("Creating new");
                        NetworkPlayer newPlayer = new NetworkPlayer(msg.clientId);
                        this.enemyPlayers.Add(newPlayer);
                        newPlayer.Spawn(msg, this.enemyPrefab);
                    }
                    // We pass the prefab with the position message as it might be the enemy is not spawned yet
                    NetworkPlayer enemyPlayer = this.GetEnemyPlayer(msg.clientId);
                    enemyPlayer.ReceivePosition(msg, this.enemyPrefab);

                    clientsMoved.Add(msg.clientId);
                }
                else if(msg.messageType == MessageType.PlayerLeft)
                {
                    // A player left, remove from list and delete gameobject
                    NetworkPlayer enemyPlayer = this.GetEnemyPlayer(msg.clientId);
                    if(enemyPlayer != null)
                    {
                        enemyPlayer.DeleteGameObject();
                        this.enemyPlayers.Remove(enemyPlayer);
                        justLeftClients.Add(msg.clientId);
                    }
                }
            }
        }
        messagesToProcess.Clear();

        // Interpolate all enemy players towards their current target
        foreach (var enemyPlayer in this.enemyPlayers)
        {
            enemyPlayer.InterpolateToTarget();
        }
    }

    // Sends the position of the local palyer to the server
    void SendPosition()
    {
        // Send position if changed
        var newPosMessage = this.localPlayer.GetPositionMessage();
        if(newPosMessage != null)
            this.networkClient.SendMessage(newPosMessage);
    }

#endif

}