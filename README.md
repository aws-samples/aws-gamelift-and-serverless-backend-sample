# GameLift Examples for Unity and C++ with Serverless Backend

  * [Key Features](#key-features)
  * [Contents](#contents)
  * [Architecture Diagram](#architecture-diagram)
  * [Preliminary Setup for the Backend](#preliminary-setup-for-the-backend)
  * [Deployment with Bash Scripts](#deployment-with-bash-scripts)
  * [Deployment with PowerShell Scripts](#deployment-with-powershell-scripts)
  * [Implementation Overview](#implementation-overview)
    + [Serverless Backend Service](#serverless-backend-service)
  * [License](#license)

This repository contains a GameLift example solution with a backend service designed for getting started with MacOS, Windows and mobile session-based multiplayer game development and leveraging deployment automation.

This Readme includes the architecture overview, as well as deployment instructions and documentation for the serverless backend services of the solution. You can then branch out to the Unity and C++ specific Readme files as needed for the game client and game server setup.

**Note**: _“The sample code; software libraries; command line tools; proofs of concept; templates; or other related technology (including any of the foregoing that are provided by our personnel) is provided to you as AWS Content under the AWS Customer Agreement, or the relevant written agreement between you and AWS (whichever applies). You should not use this AWS Content in your production accounts, or on production or other critical data. You are responsible for testing, securing, and optimizing the AWS Content, such as sample code, as appropriate for production grade use based on your specific quality control practices and standards. Deploying AWS Content may incur AWS charges for creating or using AWS chargeable resources, such as running Amazon EC2 instances or using Amazon S3 storage.”_

# Key Features
* Uses CloudFormation to automate the deployment of all resources
* Uses a Serverless API to initiate matchmaking built with Serverless Application Model
* Leverages FlexMatch latency-based matchmaking
* Runs on Amazon Linux 2 on the GameLift service in two Regional locations
* Uses Cognito Identity Pools to store user identities and authenticate them against the backend
* Deployed with shell (MacOS) or PowerShell (Windows) scripts
* Includes configuration to push custom logs and metrics to CloudWatch with CloudWatch Agent
* Client works on multiple platforms including mobile
* Uses Unity engine or C++ for server and client

The project is a simple "game" where 2-10 players join the same session and move around with their 3D characters. The movement inputs are sent to the server which runs the game simulation on a headless Unity process and syncs state back to all players.

# Contents

The project contains:
* **A Backend Project** created with Serverless Application Model (SAM) to create an API backend for matchmaking requests (`GameServiceAPI`)
* **Fleet deployment automation** leveraging AWS CloudFormation to deploy all GameLift resources (`FleetDeployment`)
* **A build folder for the server build** which includes a set of pre-required files for configuration and where you will build your Linux server build from Unity (`LinuxServerBuild`)
* **A Unity version of the game server and client** (`GameLiftExampleUnityProject`)
* **An C++ version of the game server and client** (`CppServerAndClient`)

# Architecture Diagrams

The architecture is explained through two diagrams. The first one focuses on the GameLift resources and the second one on the serverless backend. Both diagrams contain all components of the solution, just the level of detail is different based on the focus.

### GameLift Resources

![Architecture Diagram GameLift Resources](Architecture_gamelift.png "Architecture Diagram GameLift Resources")

### Serverless Backend

![Architecture Diagram Backend](Architecture_small.png "Architecture Diagram Backend")

# Preliminary Setup for the Backend

1. **Make sure you have the following tools installed**
    1. **Install and configure the AWS CLI**
        * Follow these instructions to install: [AWS CLI Installation](https://docs.aws.amazon.com/cli/latest/userguide/cli-chap-install.html)
        * Configure the CLI: [AWS CLI Configuration](https://docs.aws.amazon.com/cli/latest/userguide/cli-chap-configure.html#cli-quick-configuration)
    2. **Install SAM CLI**
        * Follow these instructions to install the Serverless Application Model (SAM) CLI: [SAM CLI Installation](https://docs.aws.amazon.com/serverless-application-model/latest/developerguide/serverless-sam-cli-install.html)
    3. **Install Node.js**
        * Required for the SAM build: [Node.js Downloads](https://nodejs.org/en/download/)
2. **Select deployment Region**
    * The solution can be deployed in any AWS Region that supports Amazon GameLift FlexMatch. For details see the [Amazon GameLift FAQ](https://aws.amazon.com/gamelift/faq/) and look for "In which AWS Regions can I place a FlexMatch matchmaker?"

# Deployment with Bash Scripts

Note: If you want to do the end to end deployment in a Cloud9 browser IDE for the C++ solution, please refer to the [C++ deployment README](CppServerAndClient/README.md) for details on how to set that up.

1. **Set up your configuration** (`configuration.sh`)
    * Modify the script to set the `region` variable to your selected region for the backend services and GameLift resources
    * Modify the script to set the `deploymentbucketname` to a **globally unique** name for the code deployment bucket
    * Set the `secondaryregion` variable in the script to your selected secondary location as we're running the Fleet in two different Regions
2. **Deploy the Backend API and PreRequirements stacks** (`deployBackendAndPreRequirements.sh`)
    * Make sure you have the SAM CLI installed
    * Run the script to deploy the backend API and the PreRequirements Stack (`deployBackendAndPreRequirements.sh`)
    * This will run two scripts to deploy both the serverless backend with SAM (GameServiceAPI/deploy.sh) as well as the Cognito and IAM resources we need for configuration with CloudFormation (FleetDeployment/deployPreRequirements.sh).
3. **Set the role to CloudWatch Agent configuration** (`LinuxServerBuild/amazon-cloudwatch-agent.json`)
    * Open file LinuxServerBuild/amazon-cloudwatch-agent.json in your favourite text editor
    * Replace the `role_arn` value with role provided as output by the previous script
    * You can also find the ARN in the CloudFormation stack, in IAM console or as output of Step 2
4. **Move to Unity or C++ instructions** for the game server and client builds deployment
    * [Unity deployment README](CppServerAndClient/README_UnityClientServer.md)
    * [C++ deployment README](CppServerAndClient/README.md)

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
    * You can also find the ARN in the CloudFormation stack, in IAM console or as output of Step 2s
4. **Move to Unity instructions** for the game server and client builds deployment. C++ deployment doesn't support Windows currently.
    * [Unity deployment README](CppServerAndClient/README_UnityClientServer.md)

# Implementation Overview

## Serverless Backend Service

The backend service is Serverless and built with Serverless Application Model. The AWS resources are defined in the SAM template `template.yaml` within the GameServiceAPI folder. SAM CLI uses this template to generate the `gameservice.yaml` template that is then deployed with CloudFormation. SAM greatly simplifies defining Serverless backends.

The backend contains three key Lambda functions: **RequestMatchmakingFunction** and **RequestMatchStatusFunction** are defined as Node.js scripts within the gameservice folder. These functions are called by the API Gateway defined in the template that uses AWS_IAM authentication. Only signed requests are allowed and the clients use their Cognito credentials to sign the requests. This way we also have their **Cognito identity** available within Lambda which is used to securely identify users and access their data in DynamoDB.

**ProcessMatchmakingEventsFunction** is triggered by Amazon SNS events published by GameLift FlexMatch. It will catch the MatchmakingSucceeded events and write the results in DynamoDB Table "GameLiftExampleMatchmakingTickets". RequestMatchStatusFunction will use the DynamoDB table to check if a ticket has succeeded matchmaking. This way we don't need to use the DescribeMatchmaking API of GameLift which can easily throttle with a large player count. The DynamoDB Table also has a TTL field and and configuration which means the tickets will be automatically removed after one hour of creation.

The SAM template defines IAM Policies to allow the Lambda functions to access both GameLift to request matchmaking as well as DynamoDB to access the player data. It is best practice to never allow game clients to access these resources directly as this can open different attack vectors to your resources.

### GameLiftExamplePreRequirements Stack

  * an **IAM Role** for the GameLift Fleet EC2 instances that allows access to CloudWatch to push logs and custom metrics
  * a **Cognito Identity Pool** that will be used to store player identities and the associated **IAM Roles** for unauthenticated and authenticated users that clients use to access the backend API through API Gateway. We don't authenticate users in the example but you could connect their Facebook identitities for example or any custom identities to Cognito

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
