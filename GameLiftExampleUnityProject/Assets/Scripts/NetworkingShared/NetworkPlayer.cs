// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

//Encapsulates a GameObject and manages spawning and interpolation of an enemy player

using UnityEngine;

public class NetworkPlayer
{
    private GameObject character;
    private bool localPlayer;
    int playerId;

    private Vector3 previousPos = Vector3.zero;
    private Vector3 targetPos = Vector3.zero;
    private Quaternion targetOrientation = Quaternion.identity;

    public NetworkPlayer(int playerId)
    {
        this.playerId = playerId;
    }

    public void DeleteGameObject()
    {
        GameObject.Destroy(this.character);
    }

    public int GetPlayerId()
    {
        return playerId;
    }

    // This is called for the local player only
    public void Initialize(GameObject characterPrefab, Vector3 pos)
    {
        // Create character
        Quaternion rotation = Quaternion.identity;
        this.character = GameObject.Instantiate(characterPrefab, pos, rotation);
        this.localPlayer = true;
    }

    // *** FOR SENDING MESSAGES (LOCAL PLAYER) *** //

    public SimpleMessage GetSpawnMessage()
    {
        SimpleMessage message = new SimpleMessage(MessageType.Spawn);
        Vector3 pos = this.character.transform.position;
        Quaternion rotation = this.character.transform.rotation;
        message.SetFloats(pos.x, pos.y, pos.z, rotation.x, rotation.y, rotation.z, rotation.w);
        return message;
    }

    public SimpleMessage GetPositionMessage()
    {
        Vector3 pos = this.character.transform.position;
        if(Vector3.Distance(pos, this.previousPos) > 0.01f)
        {
            SimpleMessage message = new SimpleMessage(MessageType.Position);
            this.previousPos = pos;
            Quaternion rotation = this.character.transform.rotation;
            message.SetFloats(pos.x, pos.y, pos.z, rotation.x, rotation.y, rotation.z, rotation.w);
            return message;
        }
        return null;
    }

    // *** FOR RECEIVING MESSAGES (REMOTE PLAYERS) *** //

    // This is called for remote players only
    public void Spawn(SimpleMessage msg, GameObject characterPrefab)
    {
        this.localPlayer = false;

        //Position
        float x = msg.float1;
        float y = msg.float2;
        float z = msg.float3;
        //Orientation
        float qx = msg.float4;
        float qy = msg.float5;
        float qz = msg.float6;
        float qw = msg.float7;

        if (this.character == null)
        {
            Debug.Log("Enemy not spawned yet, spawn");
            this.character = GameObject.Instantiate(characterPrefab, new Vector3(x, y, z), new Quaternion(qx, qy, qz, qw));
        }
    }

    public void ReceivePosition(SimpleMessage msg, GameObject characterPrefab)
    {
        //Position
        float x = msg.float1;
        float y = msg.float2;
        float z = msg.float3;
        //Orientation
        float qx = msg.float4;
        float qy = msg.float5;
        float qz = msg.float6;
        float qw = msg.float7;

        // We spawn here too as it might be the enemy spawned before us
        if (this.character == null)
        {
            Debug.Log("Enemy not spawned yet, spawn");
            this.character = GameObject.Instantiate(characterPrefab, new Vector3(x, y, z), new Quaternion(qx, qy, qz, qw));
        }

        // Set the target position for interpolation which is done in InterpolateToTarget on every frame
        this.targetPos = new Vector3(x, y, z);
        this.targetOrientation = new Quaternion(qx, qy, qz, qw);
    }

    // Interpolate to target
    public void InterpolateToTarget()
    {
        Vector3 move = targetPos - this.character.transform.position;
        float distance = move.magnitude;
        
        Vector3 positionDifference = this.targetPos - this.character.transform.position;
        Vector3 interpolateMove = 0.5f * positionDifference;
        this.character.transform.SetPositionAndRotation(this.character.transform.position + interpolateMove, this.targetOrientation);
    }
}
