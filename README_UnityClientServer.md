# GameLift Example with Serverless Backend: Unity Version Server and Client

  * [Preliminary Setup](#preliminary-setup)
  * [Architecture Diagram](#architecture-diagram)
  * [Deployment with Bash Scripts](#deployment-with-bash-scripts)
  * [Deployment with PowerShell Scripts](#deployment-with-powershell-scripts)
  * [Implementation Overview](#implementation-overview)
    + [GameLift Resources](#gamelift-resources)
    + [Game Server](#game-server)
    + [Game Client](#game-client)
  * [License](#license)

This Readme contains the details for the Unity-based game server and client. See the [main README](../README.md) for details on the backend architecture.

**Note**: _“The sample code; software libraries; command line tools; proofs of concept; templates; or other related technology (including any of the foregoing that are provided by our personnel) is provided to you as AWS Content under the AWS Customer Agreement, or the relevant written agreement between you and AWS (whichever applies). You should not use this AWS Content in your production accounts, or on production or other critical data. You are responsible for testing, securing, and optimizing the AWS Content, such as sample code, as appropriate for production grade use based on your specific quality control practices and standards. Deploying AWS Content may incur AWS charges for creating or using AWS chargeable resources, such as running Amazon EC2 instances or using Amazon S3 storage.”_

# Architecture Diagram

The architecture diagram introduced here focuses on the GameLift resources.

### GameLift Resources

![Architecture Diagram GameLift Resources](Architecture_gamelift.png "Architecture Diagram GameLift Resources")

# Preliminary Setup

1. **Install Unity3D 2019 or Unity 2020**
        * Use the instructions on Unity website for installing: [Unity Hub Installation](https://docs.unity3d.com/Manual/GettingStartedInstallingHub.html)
2. **Install external dependencies**
        1. [GameLift Server SDK](https://docs.aws.amazon.com/gamelift/latest/developerguide/integration-engines-unity-using.html): **Download** and **build** the GameLift Server C# SDK (4.5) and **copy** all of the generated dll files to `GameLiftExampleUnityProject/Assets/Dependencies/GameLiftServerSDK/` folder. Visual Studio is the best tool to build the project with. **NOTE:** If you're using Unity 2020 LTS, you will need to **rename** the Newtonsoft.Json.dll to something else like Newtonsoft.Json.GameLift.dll as it conflicts with a package that is natively included to Unity in 2020. Make sure to do this **before** upgrading the project to Unity 2020 to avoid upgrade issues.
        2. [Download the AWS .NET SDK](https://sdk-for-net.amazonwebservices.com/latest/v3/aws-sdk-netstandard2.0.zip) and copy the following files to `UnityProject/Assets/Dependencies/`: `AWSSDK.CognitoIdentity.dll`, `AWSSDK.CognitoIdentityProvider.dll`, `AWSSDK.Core.dll`, `AWSSDK.SecurityToken.dll`, `Microsoft.Bcl.AsyncInterfaces.dll`, `System.Threading.Tasks.Extensions.dll`.
        3. [Signature Calculation Example](https://docs.aws.amazon.com/AmazonS3/latest/API/samples/AmazonS3SigV4_Samples_CSharp.zip): **Download** the S3 example for signing API Requests and **copy the folders** `Signers` and `Util` to `GameLiftExampleUnityProject/Assets/Dependencies/` folder. We will use these to sign the requests against API Gateway with Cognito credentials. After this you should not see any errors in your Unity console.

# Deployment with Bash Scripts

1. **Set the API endpoint and the Cognito Identity Pool to the Unity Project**
    * Open Unity Hub, add the GameLiftExampleUnityProject and open it (Unity 2019.2.16 or higher recommended)
    * Set the value of `static string apiEndpoint` to the endpoint created by the backend deployment in `GameLiftExampleUnityProject/Assets/Scripts/Client/MatchmakingClient.cs`. You can find this endpoint from the `gameservice-backend` Stack Outputs in CloudFormation, from the SAM CLI stack deployment outputs or from the API Gateway console (make sure to have the `/Prod/` in the url)
    * Set the value of `static string identityPoolID` to the identity pool created by the Pre-Requirements deployment. You can also find the ARN in the CloudFormation stack, in the Amazon Cognito console or as the output of Step 2
    * Set the value of `public static string regionString` and `public static Amazon.RegionEndpoint region` to the values of your selected region. Set the value of `secondaryLocationRegionString` to your selected secondary region for the Fleet. The sessions are then placed based on client latency.
    * NOTE: At this point, this part of the code is not compiled because we are using Server build configuration. The code might show up greyed out in your editor.
2. **Build the server build and deploy the build and the GameLift resources**
    * In Unity select "GameLift -> BuildLinuxServer" from the menu. This will set the scripting define symbols to SERVER for the server build and build the server.
    * Select the `LinuxServerBuild` folder when requested and select "Choose". Wait for the build to finish.
    * Run the script (`cd FleetDeployment && sh deployBuildAndUpdateGameLiftResources.sh && cd ..`)
    * This will take some time as the fleet instance AMI will be built and all the GameLift resources deployed
    * You should see all the resources created in the GameLift console (Fleet, Alias, Build, Queue, Matchmaking Rule Set and Matchmaking Configuration) as well as in CloudFormation
3. **Build and run two clients**
    * In Unity select "GameLift -> BuildMacOSClient" or ""GameLift -> BuildWindowsClient" based on your platform. This will set the scripting define symbols to CLIENT and do the build.
    * Create a folder in your preferred location and select "Choose" to build.
    * Open the scene "GameWorld" in the folder Scenes/
    * You can run two clients by running one in the Unity Editor and one with the created build. This way the clients will get different Cognito identities for matchmaking to work.
    * You will see a 5-10 second delay in case you connect only 2 clients. This is because the matchmaking expects 4 clients minimum and will relax the rules after 5 seconds. It also expects a smaller than 50ms latency for the clients to one of the supported Regions and relaxes this rule to 200ms after 10 seconds. 
    * **The clients need to connect within 20 seconds** as this is the timeout value for the matchmaking

# Deployment with PowerShell Scripts

1. **Set the API endpoint and the Cognito Identity Pool to the Unity Project**
    * Open Unity Hub, add the GameLiftExampleUnityProject and open it (Unity 2019.2.16 or higher recommended)
    * Set the value of `static string apiEndpoint` to the endpoint created by the backend deployment in `GameLiftExampleUnityProject/Assets/Scripts/Client/MatchmakingClient.cs`. You can find this endpoint from the `gameservice-backend` Stack Outputs in CloudFormation, from the SAM CLI stack deployment outputs or from the API Gateway console (make sure to have the `/Prod/` in the url)
    * Set the value of `static string identityPoolID` to the identity pool created by the Pre-Requirements deployment. You can also find the ARN in the CloudFormation stack, in the Amazon Cognito console or as the output of Step 2
    * Set the value of `public static string regionString` and `public static Amazon.RegionEndpoint region` to the values of your selected region. Set the value of `secondaryLocationRegionString` to your selected secondary region for the Fleet. The sessions are then placed based on client latency.
    * NOTE: At this point, this part of the code is not compiled because we are using Server build configuration. The code might show up greyed out in your editor.
2. **Build the server build and deploy the build and the GameLift resources**
    * In Unity select "GameLift -> BuildLinuxServer" from the menu. This will set the scripting define symbols to SERVER for the server build and build the server.
    * Select the `LinuxServerBuild` folder when requested and select "Choose". Wait for the build to finish.
    * Open file FleetDeployment/deployBuildAndUpdateGameLiftResources.ps1 in your favourite text editor
    * Set the region variable in the script to your selected region
    * Set the secondaryregion variable in the script to your selected secondary location as we're running the Fleet in two different Regions (this will be used by the latency-based matchmaking)
    * Run the script `deployBuildAndUpdateGameLiftResources.ps1`
    * This will take some time as the fleet instance AMI will be built and all the GameLift resources deployed
    * You should see all the resources created in the GameLift console (Fleet, Alias, Build, Queue, Matchmaking Rule Set and Matchmaking Configuration) as well as in CloudFormation
3. **Build and run two clients**
    * In Unity select "GameLift -> BuildMacOSClient" or ""GameLift -> BuildWindowsClient" based on your platform. This will set the scripting define symbols to CLIENT and do the build.
    * Create a folder in your preferred location and select "Choose" to build.
    * Open the scene "GameWorld" in Scenes/GameWorld
    * You can run two clients by running one in the Unity Editor and one with the created build. This way the clients will get different Cognito identities for matchmaking to work.
    * You will see a 5-10 second delay in case you connect only 2 clients. This is because the matchmaking expects 4 clients minimum and will relax the rules after 5 seconds. It also expects a smaller than 50ms latency for the clients to one of the supported Regions and relaxes this rule to 200ms after 10 seconds.
    * **The clients need to connect within 20 seconds** as this is the timeout value for the matchmaking

# Implementation Overview

## GameLift Resources

The GameLift resources are deployed with `gamelift.yaml` template. The stack is named **GameLiftExampleResources**

### GameLiftExampleResources Stack

  * a **FlexMatch Matchmaking Rule Set** that defines a single team with 4 to 10 players and a requirement for the player skill levels to be within a distance of 10. All players will have the same skill level in the example that is stored in DynamoDB by the backend service. There is also an expansion to relax the rules to minimum of 2 players after 5 seconds. When you connect with 2 clients, you will see this 5 second delay before the expansion is activated. The FlexMatch Rule Set also defines a latency requirement of < 50ms for the clients. This is relaxed to 200ms after 10 seconds. The clients make HTTPS requests to Amazon endpoints to measure their latency and send this data to the backend which forwards it to the matchmaker.
  * a **FlexMatch Matchmaking Configuration** that uses the Rule Set and routes game session placement requests to the Queue.
  * a **GameLift Queue** that is used to place game sessions on the GameLift Fleet. In the example we have a single fleet behind the Queue and it has two Regional locations (home Region and one secondary Region Location). You could have multiple Fleets within the Home Region (for example a Spot Fleet and a failover On-Demand Fleet for cost optimization). The queue has latency configuration for selecting the best Region for each group of players generated by FlexMatch based on their latency.
  * a **GameLift Fleet** that sits behind the Queue and uses the latest game server build uploaded by the `deployBuildAndUpdateGameLiftResources.sh` script. The Fleet has two Regional locations and runs on Amazon Linux 2 and there are two game server processes running on each instance. The ports for the processes are defined as parameters to the game server process and matching ports are enabled for inbound traffic to the fleet. You can pack more game servers on each instance based on the instance size and the resource requirements of your server. Our example uses C5.Large instance type which is a good starting point for compute intensive workloads.

## Game Server

Both the client and server are using Unity. The server is built with `SERVER` scripting define symbol which is used in the C# scripts to enable and disable different parts of the code.

**Key code files:**
  * `Scripts/Server/GameLift.cs`: Here we will initialize GameLift with the GameLift Server SDK. The port to be used is extracted from the command line arguments and the port is also used as part of the log file to have different log files for different server processes. Game session activations, health checks and other configuration follow closely the examples provided in the [GameLift Developer Guide](https://docs.aws.amazon.com/gamelift/latest/developerguide/gamelift-sdk-server-api.html). Game sessions are defined as "started" once 2 players at least have joined (done in the `Server.cs` script) and terminated when players have left or in case players don't join within 5 seconds after new game session info is received.
  * `Scripts/Server/Server.cs`: Here we will start a TCP Server listening to the port defined in the command line arguments to receive TCP connections from clients. We will handle any messages from clients, run the simulation at 30 frames / second and send the state back to the clients on each frame. Messages use a binary format with **BinaryFormatter** that serializes and deserializes the **SimpleMessage** class directly to the network stream. It is recommended to use a binary format instead of a text format to minimize the size or your packets. BinaryFormatter is not the most optimal in size and you might want to consider options such as Protocol Buffers to reduce the packet size. BinaryFormatter is used to keep the example simple and clean. Sending and receiving messages is done with the `Scripts/NetworkingShared/NetworkProtocol.cs` class that is used by both the client and the server.
  * `Scripts/Server/SimpleStatsdClient.cs` is used to send custom game session specific metrics to CloudWatch through the CloudWatch Agent running on the instances. These metrics are tagged with the game session which will be presented as a Dimension in CloudWatch. As StatsD is used with UDP traffic within localhost, collecting metrics is fast and has low CPU footprint in the game server process.

**CloudWatch Agent**

CloudWatch agent is initialized in the `install.sh` script when a Fleet is created. This will start the agent on each individual instance with the configuration provided in `LinuxServerBuild/amazon-cloudwatch-agent.json`. We will send game session log files from both the server processes with the fixed file names based on the ports. Process level metrics such as memory and cpu utilization, are sent with the `procstat`-configuration. We identify the processes based on the `-port` parameter in the command line arguments. We will also start a StatsD client to send custom metrics to CloudWatch Metrics. It's worth noting that the different Locations of the Fleets will send these metrics and logs to CloudWatch in their own Region.

A key thing to notice is that we need to define the **Instance Role** to be used by the agent. The IAM Role provided by the instance metatadata will not send metrics and logs correctly as it is a role in the GameLift service accounts.

## Game Client

The game client is using Unity and is tested on MacOS, Windows and iOS platforms but should work on any platform. The input for the player character is arrow keys or WASD so there is no input option on mobile currently. The client is built with `CLIENT` scripting define symbol which is used in the C# scripts to enable and disable different parts of the code. The client will only send input to the server and the characters will move based on the state information received from the server.

**Latency measurements**

The client measures latency (in Client.cs) by sending HTTPS requests to AWS regional endpoints (DynamoDB in this example) of the Regions we have defined. It sends three requests over the same connection and measures the average of the latter two. This way we can measure the TCP latency without the handshakes and get more stable results from an average of two. This data is passed to the backend which will include it in matchmaking tickets and sessions will be placed to appropriate Regions based on the latency.

**Connection Process**

Client uses AWS .NET SDK to request a Cognito Identity and connects to the Serverless backend with HTTPS and signs the requests to API Gateway with the credentials provided by Cognito. After the matchmaking is done, the client will use the connection info provided by the serverless backend (which it receives from GameLift FlexMatch) to connect directly to the server with a TCP connection. The client sends the PlayerSessionID it receives from the Serverless backend to the server and the server validates this ID with the GameLift service.

**Key code files:**
  * `Scripts/Client/Client.cs`: This is the main class of the client that initiates the matchmaking and connects to the server. It also processes all messages received from the server and updates the associated player entities based on them. Enemy players will be spawned and removed as they join and leave and their movement will be interpolated based on the position messages received. We will also send move commands from our local player to the server here.
  * `Scripts/Client/MatchMakingClient.cs`: This is the HTTPS client to the backend service that makes the signed requests to request matchmaking and request the status of a matchmaking ticket.
  * `Scripts/Client/NetworkClient.cs`: This is the TCP Client class that manages the TCP connection to the server and sending/receiving of messages. It uses NetworkProtocol in `NetworkProtocol.cs` to serialize and deserialize messages in a binary format in the same way as the server. 

# License

This example is licensed under the Apache 2.0 License. See LICENSE file.
