// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

// Here you would define your message types for the messages between server and client
// NOTE: We are using BinaryFormatter for simplicity to serialize/deserialize binary messages.
//       There are more optimal solutions such as Protocol buffer available.

using System;

[Serializable]
public enum MessageType
{
    Connect,
    Disconnect,
    Ready,
    Reject,
    Spawn,
    Position,
    PlayerLeft
};

// We will use the same message for all requests so it will include a type and optional float values (used for position and orientation)
[Serializable]
public class SimpleMessage
{
    public SimpleMessage(MessageType type, string message = "")
    {
        this.messageType = type;
        this.message = message;
    }

    public void SetFloats(float float1, float float2, float float3, float float4, float float5, float float6, float float7)
    {
        this.float1 = float1; this.float2 = float2; this.float3 = float3; this.float4 = float4; this.float5 = float5; this.float6 = float6; this.float7 = float7;
    }

    public MessageType messageType { get; set; }
    public string message { get; set; }
    public int clientId { get; set; }

    // As we are using one generic message for simplicity, we always have all possible data here
    // You would likely want to use different classes for different message types
    public float float1 { get; set; }
    public float float2 { get; set; }
    public float float3 { get; set; }
    public float float4 { get; set; }
    public float float5 { get; set; }
    public float float6 { get; set; }
    public float float7 { get; set; }
}
