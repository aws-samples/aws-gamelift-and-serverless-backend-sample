// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

// Helper class to serialize and deserialize messages to a network stream

using System;
using System.IO;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class NetworkProtocol
{
    public static SimpleMessage[] Receive(TcpClient client)
    {
        try
        {
            NetworkStream stream = client.GetStream();
            var messages = new List<SimpleMessage>();
            while (stream.DataAvailable)
            {
                try
                {
                    using (var reader = new BinaryReader(stream, Encoding.UTF8, true))
                    {
                        // We always expect a SimpleMessage in this sample
                        SimpleMessage message = new SimpleMessage();
                        message.messageType = (MessageType) reader.ReadInt32();
                        message.message = reader.ReadString();
                        message.clientId = reader.ReadInt32();
                        message.float1 = reader.ReadSingle();
                        message.float2 = reader.ReadSingle();
                        message.float3 = reader.ReadSingle();
                        message.float4 = reader.ReadSingle();
                        message.float5 = reader.ReadSingle();
                        message.float6 = reader.ReadSingle();
                        message.float7 = reader.ReadSingle();
                        messages.Add(message);
                    }
                }
                catch (Exception e)
                {
                    Debug.Log("Error receiving a message: " + e.Message);
                    Debug.Log("Aborting the rest of the messages");
                    return null;
                }
            }
            return messages.ToArray();
        }
        catch (Exception e)
        {
            System.Console.WriteLine("Error accessing message stream: " + e.Message);
        }

        return null;
    }

    public static bool Send(TcpClient client, SimpleMessage message)
    {
        try
        {
            if (client == null) { return false; }
            NetworkStream stream = client.GetStream();
            using (var writer = new BinaryWriter(stream, Encoding.UTF8, true))
            {
                // Write the SimpleMessage contents. You should implement a system for different message types for more complex scenarios
                writer.Write((Int32)message.messageType);
                writer.Write(message.message);
                writer.Write((Int32)message.clientId);
                writer.Write(message.float1);
                writer.Write(message.float2);
                writer.Write(message.float3);
                writer.Write(message.float4);
                writer.Write(message.float5);
                writer.Write(message.float6);
                writer.Write(message.float7);

                return true;
            }
        }
        catch (Exception e)
        {
            System.Console.WriteLine("Error sending data: " + e.Message);
            return false;
        }

        return false;
    }
}
