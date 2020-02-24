// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

// *** SERIALIZATION OBJECTS FOR MATCHMAKING API REQUESTS ***
[System.Serializable]
public class GameSessionInfo
{
    public string PlayerSessionId;
    public string PlayerId;
    public string GameSessionId;
    public string FleetId;
    public string CreationTime;
    public string Status;
    public string IpAddress;
    public int Port;
}
[System.Serializable]
public class MatchMakingRequestInfo
{
    [System.Serializable]
    public class PlayerInfo
    {
        public string PlayerId;
    }

    public string TicketId;
    public string Status;
    public PlayerInfo[] Players;
}
[System.Serializable]
public class MatchStatusInfo
{
    public string IpAddress;
    public int Port;
    public string PlayerSessionId;
    public string DnsName;
}