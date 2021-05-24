# GameLift Example for Unity with Serverless Backend

  * [Key Features](#key-features)
  * [Contents](#contents)
  * [Architecture Diagram](#architecture-diagram)
  * [Preliminary Setup](#preliminary-setup)
  * [Deployment with Bash Scripts](#deployment-with-bash-scripts)
  * [Deployment with PowerShell Scripts](#deployment-with-powershell-scripts)
  * [Implementation Overview](#implementation-overview)
    + [GameLift Resources](#gamelift-resources)
    + [Serverless Backend Service](#serverless-backend-service)
    + [Game Server](#game-server)
    + [Game Client](#game-client)
  * [License](#license)

This repository contains a simple GameLift example with a backend service designed for getting started with MacOS, Windows and mobile session-based multiplayer game development and leveraging deployment automation.

**Note**: This repository exists for **example purposes only** and you always need to build and validate your own solution for production use.

# Key Features
* Uses CloudFormation to automate the deployment of all resources
* Uses a Serverless API to initiate matchmaking built with Serverless Application Model
* Leverages FlexMatch latency-based matchmaking
* Runs on Amazon Linux 2 on the GameLift service in two Regional locations
* Uses Cognito Identity Pools to store user identities and authenticate them against the backend
* Deployed with shell (MacOS) or PowerShell (Windows) scripts
* Includes configuration to push custom logs and metrics to CloudWatch with CloudWatch Agent
* Client works on multiple platforms including mobile
* Uses Unity engine for server and client

The project is a simple "game" where 2-10 players join the same session and move around with their 3D characters. The movement inputs are sent to the server which runs the game simulation on a headless Unity process and syncs state back to all players.

# Contents

The project contains:
* **A Unity Project** that will be used for both Client and Server builds (`GameLiftExampleUnityProject`)
* **A Backend Project** created with Serverless Application Model (SAM) to create an API backend for matchmaking requests (`GameServiceAPI`)
* **Fleet deployment automation** leveraging AWS CloudFormation to deploy all GameLift resources (`FleetDeployment`)
* **A build folder for the server build** which includes a set of pre-required files for configuration and where you will build your Linux server build from Unity (`LinuxServerBuild`)

# Architecture Diagrams

The architecture is explained through two diagrams. The first one focuses on the GameLift resources and the second one on the serverless backend. Both diagrams contain all components of the solution, just the level of detail is different based on the focus.

### GameLift Resources

![Architecture Diagram GameLift Resources](Architecture_gamelift.png "Architecture Diagram GameLift Resources")

### Serverless Backend

![Architecture Diagram Backend](Architecture_small.png "Architecture Diagram Backend")

# Preliminary Setup

1. **Install and configure the AWS CLI**
    * Follow these instructions to install: [AWS CLI Installation](https://docs.aws.amazon.com/cli/latest/userguide/cli-chap-install.html)
    * Configure the CLI: [AWS CLI Configuration](https://docs.aws.amazon.com/cli/latest/userguide/cli-chap-configure.html#cli-quick-configuration)
2. **Install Unity3D 2019**
    * Use the instructions on Unity website for installing: [Unity Hub Installation](https://docs.unity3d.com/Manual/GettingStartedInstallingHub.html)
3. **Install SAM CLI**
    * Follow these instructions to install the Serverless Application Model (SAM) CLI: [SAM CLI Installation](https://docs.aws.amazon.com/serverless-application-model/latest/developerguide/serverless-sam-cli-install.html)
4. **Install Node.js**
    * Required for the SAM build: [Node.js Downloads](https://nodejs.org/en/download/)
4. **Install external dependencies**
    1. [GameLift Server SDK](https://docs.aws.amazon.com/gamelift/latest/developerguide/integration-engines-unity-using.html): **Download** and **build** the GameLift Server X# SDK (4.5) and **copy** all of the generated dll files to `GameLiftExampleUnityProject/Assets/Dependencies/GameLiftServerSDK/` folder. Visual Studio is the best tool to build the project with.
    2. [Download the AWS .NET SDK](https://sdk-for-net.amazonwebservices.com/latest/v3/aws-sdk-netstandard2.0.zip) and copy the following files to `UnityProject/Assets/Dependencies/`: `AWSSDK.CognitoIdentity.dll`, `AWSSDK.CognitoIdentityProvider.dll`, `AWSSDK.Core.dll`, `AWSSDK.SecurityToken.dll`, `Microsoft.Bcl.AsyncInterfaces.dll`, `System.Threading.Tasks.Extensions.dll`.
    3. [Signature Calculation Example](https://docs.aws.amazon.com/AmazonS3/latest/API/samples/AmazonS3SigV4_Samples_CSharp.zip): **Download** the S3 example for signing API Requests and **copy the folders** `Signers` and `Util` to `GameLiftExampleUnityProject/Assets/Dependencies/` folder. We will use these to sign the requests against API Gateway with Cognito credentials. After this you should not see any errors in your Unity console.
5. **Select deployment Region**
    * The solution can be deployed in any AWS Region that supports Amazon GameLift FlexMatch. For details see the [Amazon GameLift FAQ](https://aws.amazon.com/gamelift/faq/) and look for "In which AWS Regions can I place a FlexMatch matchmaker?"

# Deployment with Bash Scripts

1. **Deploy the Backend API with SAM** (`GameServiceAPI/deploy.sh`)
    * Make sure you have the SAM CLI installed
    * Open file GameServiceAPI/deploy.sh in your favourite text editor
    * Modify the script to set the `region` variable to your selected region
    * Modify the script to set the `deploymentbucketname` to a **globally unique** name for the code deployment bucket
    * Run the script to deploy the backend API (`cd GameServiceAPI && sh deploy.sh && cd ..`)
2. **Deploy the Pre-Requirements for the GameLift Resources (Cognito Resources and Instance Role)** (`FleetDeployment/deployPreRequirements.sh`)
    * Open file FleetDeployment/deployPreRequirements.sh in your favourite text editor
    * Set the region variable in the script to your selected region
    * Run the script (`cd FleetDeployment && sh deployPreRequirements.sh && cd ..`)
3. **Set the role to CloudWatch Agent configuration** (`LinuxServerBuild/amazon-cloudwatch-agent.json`)
    * Open file LinuxServerBuild/amazon-cloudwatch-agent.json in your favourite text editor
    * Replace the `role_arn` value with role provided as output by the previous script
    * You can also find the ARN in the CloudFormation stack, in IAM console or as output of Step 2
4. **Set the API endpoint and the Cognito Identity Pool to the Unity Project**
    * Open Unity Hub, add the GameLiftExampleUnityProject and open it (Unity 2019.2.16 or higher recommended)
    * Set the value of `static string apiEndpoint` to the endpoint created by the backend deployment in `GameLiftExampleUnityProject/Assets/Scripts/Client/MatchmakingClient.cs`. You can find this endpoint from the `gameservice-backend` Stack Outputs in CloudFormation, from the SAM CLI stack deployment outputs or from the API Gateway console (make sure to have the `/Prod/` in the url)
    * Set the value of `static string identityPoolID` to the identity pool created by the Pre-Requirements deployment. You can also find the ARN in the CloudFormation stack, in the Amazon Cognito console or as the output of Step 2
    * Set the value of `public static string regionString` and `public static Amazon.RegionEndpoint region` to the values of your selected region
    * NOTE: At this point, this part of the code is not compiled because we are using Server build configuration. The code might show up greyed out in your editor.
5. **Build the server build**
    * In Unity go to "File -> Build Settings"
    * Go to "Player Settings" and find the Scripting Define Symbols ("Player settings" -> "Player" -> "Other Settings" -> "Scripting Define Symbol")
    * Replace the the Scripting Define Symbol with `SERVER`. Remember to press Enter after changing the value. C# scripts will use this directive to include server code and exclude client code
    * Close Player Settings and return to Build Settings
    * Switch the target platform to `Linux`. If you don't have it available, you need to install Linux platform support in Unity Hub.
    * Check the box `Server Build`
    * Build the project to the `LinuxServerBuild` folder (Click "Build" and in new window choose "LinuxServerBuild" folder, enter the **exact name** "GameLiftExampleServer" in "Save as" field and click "Save")
6. **Deploy the build and the GameLift resources** (`FleetDeployment/deployBuildAndUpdateGameLiftResources.sh`)
    * Open file FleetDeployment/deployBuildAndUpdateGameLiftResources.sh in your favourite text editor
    * Set the region variable in the script to your selected region
    * Set the secondaryregion variable in the script to your selected secondary location as we're running the Fleet in two different Regions (this will be used by the latency-based matchmaking)
    * Run the script (`cd FleetDeployment && sh deployBuildAndUpdateGameLiftResources.sh && cd ..`)
    * This will take some time as the fleet instance AMI will be built and all the GameLift resources deployed
    * You should see all the resources created in the GameLift console (Fleet, Alias, Build, Queue, Matchmaking Rule Set and Matchmaking Configuration) as well as in CloudFormation
7. **Build and run two clients**
    * Set the the Scripting Define Symbol `CLIENT` in the *Player Settings* in the Unity Project (File -> "Build Settings" -> "Player settings" → "Player" → "Other Settings" → "Scripting Define Symbol" → Replace completely to "CLIENT")
    * Open the scene "GameWorld" in Scenes/GameWorld
    * Open Build Settings (File -> Build Settings) in Unity and set target platform to `Mac OSX` (or whatever the platform you are using) and *uncheck* the box `Server Build`
    * Build the client to any folder (Click "Build", select your folder and click "Save")
    * You can run two clients by running one in the Unity Editor and one with the created build. This way the clients will get different Cognito identities. If you run multiple copies of the build, they will have the same identity (and hence same player ID) and will NOT be matched.
    * You will see a 5-10 second delay in case you connect only 2 clients. This is because the matchmaking expects 4 clients minimum and will relax the rules after 5 seconds. It also expects a smaller than 50ms latency for the clients to one of the supported Regions and relaxes this rule to 200ms after 10 seconds. 
    * **The clients need to connect within 20 seconds** as this is the timeout value for the matchmaking

# Deployment with PowerShell Scripts

1. **Deploy the Backend API with SAM** (`GameServiceAPI/deploy.ps1`)
    * Make sure you have the SAM CLI installed
    * Open file GameServiceAPI/deploy.ps1 in your favourite text editor
    * Modify the script to set the `region` variable to your selected region
    * Modify the script to set the `deploymentbucketname` to a **globally unique** name for the code deployment bucket
    * Run the `deploy.ps1` script
2. **Deploy the Pre-Requirements** (`FleetDeployment/deployPreRequirements.ps1`)
    * Open file FleetDeployment/deployPreRequirements.ps1 in your favourite text editor
    * Set the region variable in the script to your selected region
    * Run the `deployPreRequirements.ps1` script
3. **Set the role to CloudWatch Agent configuration** (`LinuxServerBuild/amazon-cloudwatch-agent.json`)
    * Open file LinuxServerBuild/amazon-cloudwatch-agent.json in your favourite text editor
    * Replace the `role_arn` value with role provided as output by the previous script
    * You can also find the ARN in the CloudFormation stack, in IAM console or as output of Step 2
4. **Set the API endpoint and the Cognito Identity Pool to the Unity Project**
    * Open Unity Hub, add the GameLiftExampleUnityProject and open it (Unity 2019.2.16 or higher recommended)
    * Set the value of `static string apiEndpoint` to the endpoint created by the backend deployment in `GameLiftExampleUnityProject/Assets/Scripts/Client/MatchmakingClient.cs`. You can find this endpoint from the `gameservice-backend` Stack Outputs in CloudFormation, from the SAM CLI stack deployment outputs or from the API Gateway console (make sure to have the `/Prod/` in the url)
    * Set the value of `static string identityPoolID` to the identity pool created by the Pre-Requirements deployment. You can also find the ARN in the CloudFormation stack, in the Amazon Cognito console or as the output of Step 2
    * Set the value of `public static string regionString` and `public static Amazon.RegionEndpoint region` to the values of your selected region
    * NOTE: At this point, this part of the code is not compiled because we are using Server build configuration. The code might show up greyed out in your editor.
5. **Build the server build**
    * In Unity go to "File -> Build Settings"
    * Go to "Player Settings" and find the Scripting Define Symbols ("Player settings" -> "Player" -> "Other Settings" -> "Scripting Define Symbol")
    * Replace the the Scripting Define Symbol with `SERVER`. Remember to press Enter after changing the value. C# scripts will use this directive to include server code and exclude client code
    * Close Player Settings and return to Build Settings
    * Switch the target platform to `Linux`. If you don't have it available, you need to install Linux platform support in Unity Hub.
    * Check the box `Server Build`
    * Build the project to the `LinuxServerBuild` folder (Click "Build" and in new window choose "LinuxServerBuild" folder, enter "GameLiftExampleServer" in "Save as" field and click "Save")
6. **Deploy the build and the GameLift resources** (`FleetDeployment/deployBuildAndUpdateGameLiftResources.ps1`)
    * Open file FleetDeployment/deployBuildAndUpdateGameLiftResources.ps1 in your favourite text editor
    * Set the region variable in the script to your selected region
    * Set the secondaryregion variable in the script to your selected secondary location as we're running the Fleet in two different Regions (this will be used by the latency-based matchmaking)
    * Run the script `deployBuildAndUpdateGameLiftResources.ps1`
    * This will take some time as the fleet instance AMI will be built and all the GameLift resources deployed
    * You should see all the resources created in the GameLift console (Fleet, Alias, Build, Queue, Matchmaking Rule Set and Matchmaking Configuration) as well as in CloudFormation
7. **Build and run two clients**
    * Set the the Scripting Define Symbol `CLIENT` in the *Player Settings* in the Unity Project (File -> "Build Settings" -> "Player settings" → "Player" → "Other Settings" → "Scripting Define Symbol" → Replace completely to "CLIENT")
    * Open the scene "GameWorld" in Scenes/GameWorld
    * Open Build Settings (File -> Build Settings) in Unity and set target platform to `Windows` (or whatever the platform you are using) and *uncheck* the box `Server Build`
    * Build the client to any folder (Click "Build", select your folder and click "Save")
    * You can run two clients by running one in the Unity Editor and one with the created build. This way the clients will get different Cognito identities. If you run multiple copies of the build, they will have the same identity (and hence same player ID) and will NOT be matched.
    * You will see a 5-10 second delay in case you connect only 2 clients. This is because the matchmaking expects 4 clients minimum and will relax the rules after 5 seconds. It also expects a smaller than 50ms latency for the clients to one of the supported Regions and relaxes this rule to 200ms after 10 seconds.
    * **The clients need to connect within 20 seconds** as this is the timeout value for the matchmaking

# Implementation Overview

## GameLift Resources

GameLift resources are deployed with CloudFormation templates. Two CloudFormation Stacks are created by the shell scripts: **GameLiftExamplePreRequirements** with `prerequirements.yaml` template and **GameliftExampleResources** with `gamelift.yaml` template.

### GameLiftExamplePreRequirements Stack

  * an **IAM Role** for the GameLift Fleet EC2 instances that allows access to CloudWatch to push logs and custom metrics
  * a **Cognito Identity Pool** that will be used to store player identities and the associated **IAM Roles** for unauthenticated and authenticated users that clients use to access the backend API through API Gateway. We don't authenticate users in the example but you could connect their Facebook identitities for example or any custom identities to Cognito

### GameLiftExampleResources Stack

  * a **FlexMatch Matchmaking Rule Set** that defines a single team with 4 to 10 players and a requirement for the player skill levels to be within a distance of 10. All players will have the same skill level in the example that is stored in DynamoDB by the backend service. There is also an expansion to relax the rules to minimum of 2 players after 5 seconds. When you connect with 2 clients, you will see this 5 second delay before the expansion is activated. The FlexMatch Rule Set also defines a latency requirement of < 50ms for the clients. This is relaxed to 200ms after 10 seconds. The clients make HTTPS requests to Amazon endpoints to measure their latency and send this data to the backend which forwards it to the matchmaker.
  * a **FlexMatch Matchmaking Configuration** that uses the Rule Set and routes game session placement requests to the Queue.
  * a **GameLift Queue** that is used to place game sessions on the GameLift Fleet. In the example we have a single fleet behind the Queue and it has two Regional locations (home Region and one secondary Region Location). You could have multiple Fleets within the Home Region (for example a Spot Fleet and a failover On-Demand Fleet for cost optimization). The queue has latency configuration for selecting the best Region for each group of players generated by FlexMatch based on their latency.
  * a **GameLift Fleet** that sits behind the Queue and uses the latest game server build uploaded by the `deployBuildAndUpdateGameLiftResources.sh` script. The Fleet has two Regional locations and runs on Amazon Linux 2 and there are two game server processes running on each instance. The ports for the processes are defined as parameters to the game server process and matching ports are enabled for inbound traffic to the fleet. You can pack more game servers on each instance based on the instance size and the resource requirements of your server. Our example uses C5.Large instance type which is a good starting point for compute intensive workloads.

## Serverless Backend Service

The backend service is Serverless and built with Serverless Application Model. The AWS resources are defined in the SAM template `template.yaml` within the GameServiceAPI folder. SAM CLI uses this template to generate the `gameservice.yaml` template that is then deployed with CloudFormation. SAM greatly simplifies defining Serverless backends.

The backend contains three key Lambda functions: **RequestMatchmakingFunction** and **RequestMatchStatusFunction** are defined as Node.js scripts within the gameservice folder. These functions are called by the API Gateway defined in the template that uses AWS_IAM authentication. Only signed requests are allowed and the clients use their Cognito credentials to sign the requests. This way we also have their **Cognito identity** available within Lambda which is used to securely identify users and access their data in DynamoDB.

**ProcessMatchmakingEventsFunction** is triggered by Amazon SNS events published by GameLift FlexMatch. It will catch the MatchmakingSucceeded events and write the results in DynamoDB Table "GameLiftExampleMatchmakingTickets". RequestMatchStatusFunction will use the DynamoDB table to check if a ticket has succeeded matchmaking. This way we don't need to use the DescribeMatchmaking API of GameLift which can easily throttle with a large player count. The DynamoDB Table also has a TTL field and and configuration which means the tickets will be automatically removed after one hour of creation.

The SAM template defines IAM Policies to allow the Lambda functions to access both GameLift to request matchmaking as well as DynamoDB to access the player data. It is best practice to never allow game clients to access these resources directly as this can open different attack vectors to your resources.

## Game Server

Both the client and server are using Unity. The server is built with `SERVER` scripting define symbol which is used in the C# scripts to enable and disable different parts of the code.

**Key code files:**
  * `Scripts/Server/GameLift.cs`: Here we will initialize GameLift with the GameLift Server SDK. The port to be used is extracted from the command line arguments and the port is also used as part of the log file to have different log files for different server processes. Game session activations, health checks and other configuration follow closely the examples provided in the [GameLift Developer Guide](https://docs.aws.amazon.com/gamelift/latest/developerguide/gamelift-sdk-server-api.html). Game sessions are defined as "started" once 2 players at least have joined (done in the `Server.cs` script) and terminated when players have left or in case players don't join within 5 seconds after new game session info is received.
  * `Scripts/Server/Server.cs`: Here we will start a TCP Server listening to the port defined in the command line arguments to receive TCP connections from clients. We will handle any messages from clients, run the simulation at 30 frames / second and send the state back to the clients on each frame. Messages use a binary format with **BinaryFormatter** that serializes and deserializes the **SimpleMessage** class directly to the network stream. It is recommended to use a binary format instead of a text format to minimize the size or your packets. BinaryFormatter is not the most optimal in size and you might want to consider options such as Protocol Buffers to reduce the packet size. BinaryFormatter is used to keep the example simple and clean. Sending and receiving messages is done with the `Scripts/NetworkingShared/NetworkProtocol.cs` class that is used by both the client and the server.
  * `Scripts/Server/SimpleStatsdClient.cs` is used to send custom game session specific metrics to CloudWatch through the CloudWatch Agent running on the instances. These metrics are tagged with the game session which will be presented as a Dimension in CloudWatch. As StatsD is used with UDP traffic within localhost, collecting metrics is fast and has low CPU footprint in the game server process.

**CloudWatch Agent**

CloudWatch agent is initialized in the `install.sh` script when a Fleet is created. This will start the agent on each individual instance with the configuration provided in `LinuxServerBuild/amazon-cloudwatch-agent.json`. We will send game session log files from both the server processes with the fixed file names based on the ports. We will also start a StatsD client to send custom metrics to CloudWatch Metrics. It's worth noting that the different Locations of the Fleets will send these metrics and logs to CloudWatch in their own Region.

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
