// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

#include "Client.h"

#include <stdio.h>
#include <unistd.h>
#include <chrono>
#include <aws/core/Aws.h>
#include <aws/core/utils/logging/LogLevel.h>
#include <aws/core/client/ClientConfiguration.h>
#include <aws/cognito-identity/CognitoIdentityClient.h>
#include <aws/cognito-identity/model/GetIdRequest.h>
#include <aws/cognito-identity/model/GetIdResult.h>
#include <aws/cognito-identity/model/GetCredentialsForIdentityRequest.h>
#include <aws/core/Region.h>
#include <aws/core/auth/AWSAuthSigner.h>
#include <aws/core/auth/AWSCredentials.h>
#include <aws/core/auth/AWSCredentialsProvider.h>
#include <aws/core/auth/AWSCredentialsProviderChain.h>

#include <aws/core/http/HttpRequest.h>
#include <aws/core/http/URI.h>
#include <aws/core/http/HttpResponse.h>
#include <aws/core/http/HttpTypes.h>
#include <aws/core/http/HttpClientFactory.h>
#include <aws/core/http/HttpClient.h>

#include <aws/core/utils/json/JsonSerializer.h>

#include <stdio.h>
#include <sys/socket.h>
#include <arpa/inet.h>

using namespace Aws;
using namespace Aws::Auth;
using namespace Aws::CognitoIdentity;
using namespace Aws::CognitoIdentity::Model;
using namespace Aws::Client;
using namespace Aws::Http;
using namespace Aws::Utils::Json;

// Connecting to game server with TCP
int ConnectToServer(String ip, int port, String playerSessionId)
{
    std::cout << "Connecting to: " << ip << ":" << port << "\n";
    
    int sock = 0, valread;
    struct sockaddr_in serv_addr;
    const char *playerSessionIdCharBuffer = playerSessionId.c_str();
    char buffer[1024] = {0};
    // Create Socket (AF_INET = IPv4, SOCK_STREAM = TCP, 0 = only supported protocol (TCP))
    if ((sock = socket(AF_INET, SOCK_STREAM, 0)) < 0)
    {
        std::cout << "\n Socket creation error \n";
        return -1;
    }
   
    serv_addr.sin_family = AF_INET;
    serv_addr.sin_port = htons(port);
       
    // Convert IPv4 and IPv6 addresses from text to binary form
    if(inet_pton(AF_INET, ip.c_str(), &serv_addr.sin_addr)<=0) 
    {
        std::cout << "\nInvalid address or Address not supported \n";
        return -1;
    }
   
    if (connect(sock, (struct sockaddr *)&serv_addr, sizeof(serv_addr)) < 0)
    {
        std::cout << "\nConnection Failed \n";
        return -1;
    }
    send(sock , playerSessionIdCharBuffer , strlen(playerSessionIdCharBuffer) , 0 );
    std::cout << "Player session ID sent" << std::endl;
    valread = read( sock , buffer, 1024);
    std::cout << "Response from server: " << buffer << std::endl << std::endl;
     std::cout << "We'll just end the session right away, game server will close it down in 10 seconds..." << std::endl << std::endl;
}

String GetLatencyString(String region)
{
    Aws::Client::ClientConfiguration clientConfiguration;
    auto pingClient = CreateHttpClient(clientConfiguration);

    // We will make HTTP request to DynamoDB which is available in all the GameLift supported regions and replies to a root HTTP request with "healthy"
    auto pingRequest = CreateHttpRequest(URI("https://dynamodb."+region+".amazonaws.com"),
                HttpMethod::HTTP_GET, Aws::Utils::Stream::DefaultResponseStreamFactoryMethod);
    
    // First request opens up the TCP connection so we measure the two after that and get the average over an opened connection
    pingClient->MakeRequest(pingRequest);
    auto startRegion1Ping = std::chrono::high_resolution_clock::now();
    pingClient->MakeRequest(pingRequest);
    pingClient->MakeRequest(pingRequest);
    auto endRegion1Ping = std::chrono::high_resolution_clock::now();
    auto durationRegion1Ping = std::chrono::duration_cast<std::chrono::milliseconds>(endRegion1Ping - startRegion1Ping) / 2.0f;
    
    int latency = (int)durationRegion1Ping.count();
    
    // Cap the latency to 1, only relevant when calling from the same Region where it rounds down to 0 easily and FlexMatch doesn't approve that value
    if(latency < 1)
        latency = 1;
    
    return region + "_" + std::to_string(latency);
}

int main () {
    
    // Init the AWS SDK
    Aws::SDKOptions options;
    Aws::InitAPI(options);
    Aws::Client::ClientConfiguration clientConfiguration;
    clientConfiguration.region = REGION;    // region must be set for Amazon Cognito operations
    auto cognitoIdentityClient = Aws::MakeShared<CognitoIdentityClient>("CognitoIdentityClient", clientConfiguration);

    // Request the Cognito Identity, NOTE: We will always get a new identity as this is not cached! You should cache the identity in your local player data
    GetIdRequest getIdRequest;
    getIdRequest.WithIdentityPoolId(identityPoolId);
    GetIdOutcome idResult = cognitoIdentityClient->GetId(getIdRequest);
    GetCredentialsForIdentityRequest getCredentialsRequest;
    getCredentialsRequest.WithIdentityId(idResult.GetResult().GetIdentityId());
    GetCredentialsForIdentityOutcome getCredentialsResult = cognitoIdentityClient->GetCredentialsForIdentity(getCredentialsRequest);
    auto credentials = getCredentialsResult.GetResult().GetCredentials();
    std::cout << "Got Cognito Identity: " << getCredentialsResult.GetResult().GetIdentityId() << "\n";
    std::cout << "Got Credentials: " << credentials.GetAccessKeyId() << ":" << credentials.GetSecretKey() << "\n";
    
    // REQUEST MATCHMAKING
    std::cout << "Signing a matchmaking request...\n";
    
    //Generate the Region latencies string for the API call by pinging the DynamoDB endpoints with HTTP, this information will be used by FlexMatch to match player and by GameLift Queue to place sessions
    String latencies = "";
    latencies += GetLatencyString(regionString) + "_" + GetLatencyString(secondaryRegionString);
    std::cout << "Latencies: " << latencies << std::endl;
    
    // Create a credentials provider with the Cognito credentials to sign the requests
    auto credentialsProvider = Aws::MakeShared<Aws::Auth::SimpleAWSCredentialsProvider>("api-gateway-client", credentials.GetAccessKeyId(), credentials.GetSecretKey(), credentials.GetSessionToken());
    auto requestSigner = MakeShared<AWSAuthV4Signer>("", credentialsProvider, "execute-api", regionString);
    
    // Create and sign the HTTP request
    auto getGameSessionRequest = CreateHttpRequest(URI(backendApiUrl +"requestmatchmaking?latencies=" + latencies),
                HttpMethod::HTTP_GET, Aws::Utils::Stream::DefaultResponseStreamFactoryMethod);
    requestSigner->SignRequest(*getGameSessionRequest);
    
    std::cout << "requesting matchmaking...\n\n";
    // make the HTTP request
    auto httpClient = CreateHttpClient(clientConfiguration);
    auto requestMatchmakingResponse = httpClient->MakeRequest(getGameSessionRequest);
    std::stringstream ss;
    ss << requestMatchmakingResponse->GetResponseBody().rdbuf();
    std::cout << ss.str() + "\n\n";
    JsonValue matchMakingRequestJson = JsonValue(ss.str());
    String ticketId = matchMakingRequestJson.View().GetString("TicketId");
    std::cout << "Got Ticket ID: " << ticketId << "\n";
    
    // REQUEST MATCH STATUS UNTIL IT'S DONE
    // We will try 10 times with a 2s interval. The Matchmaking will time out after 20s so no point trying after that
    int tries = 0;
    while(tries < 10)
    {
        std::cout << "Signing a match status request...\n";
        // Create and sign the HTTP request
        auto getMatchStatusRequest = CreateHttpRequest(URI(backendApiUrl + "requestmatchstatus?ticketId=" + ticketId),
                    HttpMethod::HTTP_GET, Aws::Utils::Stream::DefaultResponseStreamFactoryMethod);
        requestSigner->SignRequest(*getMatchStatusRequest);
        
        std::cout << "requesting match status...\n\n";
        // make the HTTP request
        auto requestMatchStatusResponse = httpClient->MakeRequest(getMatchStatusRequest);
        ss.str(""); //clear the string stream
        ss << requestMatchStatusResponse->GetResponseBody().rdbuf();
        std::cout << ss.str() + "\n\n";
        
        JsonValue matchStatusResponseJson = JsonValue(ss.str());
        String playerSessionId = matchStatusResponseJson.View().GetString("PlayerSessionId");
        int port = matchStatusResponseJson.View().GetInteger("Port");
        String ip = matchStatusResponseJson.View().GetString("IpAddress");
        std::cout << "Player Session ID: " + playerSessionId + "\n";
        if(playerSessionId.compare("NotPlacedYet") == 0)
        {
            std::cout << "Not placed yet, try again in 2s\n\n";
        }
        else
        {
            std::cout << "Placement to session done! Connect to server and send player session ID\n\n";
            int success = ConnectToServer(ip, port, playerSessionId);
            // We're done, break out
            break;
        }
    
        tries++;
        if(tries < 10)
            sleep(2);
    }
    
    //Shutdown the AWS SDK
    Aws::ShutdownAPI(options);
    
    return 0;
}