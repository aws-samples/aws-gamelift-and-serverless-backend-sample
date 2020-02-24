// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

// Very simple StatsD client supporting counter and gauge
// Built for the GameLift Unity Example

// NOTE: You can use any StatsD client implementation instead which are more full-featured

using System;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

public class SimpleStatsdClient
{
    private UdpClient udpClient;
    private string commonTags = "";

    public SimpleStatsdClient(string host, int port)
    {
        try
        {
            this.udpClient = new UdpClient("localhost", 8125);
        }
        catch (Exception e)
        {
            Debug.Log("Failed to connect to statsD: " + e.Message);
        }
    }

    public void SetCommonTagString(string tagString)
    {
        this.commonTags = tagString;
    }

    public void SendCounter(string name, int value, string tags = null)
    {
        string data = this.CreateData(name, value.ToString(), "c", tags);
        this.SendData(data);
    }

    public void SendGauge(string name, int value, string tags = null)
    {
        string data = this.CreateData(name, value.ToString(), "g", tags);
        this.SendData(data);
    }

    private string CreateData(string metricName, string metricValue, string metricType, string overrideTags)
    {
        // Override the tags (CW dimensions) if provided, otherwise use the common tags we have set
        if(overrideTags != null)
            return metricName + ":" + metricValue + "|" + metricType + "|" + overrideTags;
        else
            return metricName +":" + metricValue + "|" + metricType + "|" + this.commonTags;
    }

    // Sends data to the local StatsD agent which will forward it eventually to CloudWatch
    private void SendData(string data)
    {
        if (this.udpClient != null)
        {
            try
            {
                Byte[] sendBytes = Encoding.ASCII.GetBytes(data);
                udpClient.Send(sendBytes, sendBytes.Length);
            }
            catch (Exception e)
            {
                Debug.Log("Failed to send data with statsD: " + e.Message);
            }
        }
    }
}
