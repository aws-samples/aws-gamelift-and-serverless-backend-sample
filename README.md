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

This Readme includes the architecture overview, as well as deployment instructions and documentation for the serverless backend services of the solution. You can then branch out to the Unity and C++ specific Readmes ([Unity deployment README](README_UnityClientServer.md) or [C++ deployment README](CppServerAndClient/README.md)) as needed for the game client, game server setup and GameLift resources setup.

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

# Architecture Diagram

The architecture diagram introduced here focuses on the serverless backend but it also includes the GameLift components on a high level. See the Unity and C++ Readmes for detailed GameLift resource diagrams.

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
    * Set the `region` variable to your selected region for the backend services and GameLift resources
    * Set the `deploymentbucketname` to a **globally unique** name for the code deployment bucket
    * Set the `secondaryregion` variable in the script to your selected secondary location as we're running the Fleet in two different Regions
2. **Deploy the Backend API and PreRequirements stacks** (`deployBackendAndPreRequirements.sh`)
    * Run the script to deploy the backend API and the PreRequirements Stack (`deployBackendAndPreRequirements.sh`)
    * This will run two scripts to deploy both the serverless backend with SAM (GameServiceAPI/deploy.sh) as well as the Cognito and IAM resources we need for configuration with CloudFormation (FleetDeployment/deployPreRequirements.sh).
    * The script will automatically replace the `role_arn` value in `LinuxServerBuild/amazon-cloudwatch-agent.json` with the one created by the stack
3. **Move to Unity or C++ instructions** for the game server and client builds deployment
    * [Unity deployment README](README_UnityClientServer.md)
    * [C++ deployment README](CppServerAndClient/README.md)

# Deployment with PowerShell Scripts

1. **Set up your configuration** (`configuration.xml`)
    * Set the `Region` value to your selected region for the backend services and GameLift resources
    * Set the `DeploymentBucketName` to a **globally unique** name for the code deployment bucket
    * Set the `SecondaryRegion` value in the script to your selected secondary location as we're running the Fleet in two different Regions
2. **Deploy the Backend API and PreRequirements stacks** (`deployBackendAndPreRequirements.ps1`)
    * Run the script to deploy the backend API and the PreRequirements Stack (`./deployBackendAndPreRequirements.ps1`)
    * This will run two scripts to deploy both the serverless backend with SAM (GameServiceAPI/deploy.ps1) as well as the Cognito and IAM resources we need for configuration with CloudFormation (FleetDeployment/deployPreRequirements.ps1).
    * Select 'R' when prompted to run the individual scripts
    * Replace the value of `role_arn` in `LinuxServerBuild/amazon-cloudwatch-agent.json` with the one created by the stack. You can find this as an output of the deployment script or as an Output of the GameLiftExamplePreRequirements stack in CloudFormation.
3. **Set the role to CloudWatch Agent configuration** (`LinuxServerBuild/amazon-cloudwatch-agent.json`)
    * Open file LinuxServerBuild/amazon-cloudwatch-agent.json in your favourite text editor
    * Replace the `role_arn` value with role provided as output by the previous script
    * You can also find the ARN in the CloudFormation stack, in IAM console or as output of Step 2
4. **Move to Unity instructions** for the game server and client builds deployment. C++ deployment doesn't support Windows currently.
    * [Unity deployment README](README_UnityClientServer.md)

# Implementation Overview

## Serverless Backend Service

The backend service is Serverless and built with Serverless Application Model. The AWS resources are defined in the SAM template `template.yaml` within the GameServiceAPI folder. SAM CLI uses this template to generate the `gameservice.yaml` template that is then deployed with CloudFormation. SAM greatly simplifies defining Serverless backends.

The backend contains three key Lambda functions: **RequestMatchmakingFunction** and **RequestMatchStatusFunction** are defined as Node.js scripts within the gameservice folder. These functions are called by the API Gateway defined in the template that uses AWS_IAM authentication. Only signed requests are allowed and the clients use their Cognito credentials to sign the requests. This way we also have their **Cognito identity** available within Lambda which is used to securely identify users and access their data in DynamoDB.

**ProcessMatchmakingEventsFunction** is triggered by Amazon SNS events published by GameLift FlexMatch. It will catch the MatchmakingSucceeded events and write the results in DynamoDB Table "GameLiftExampleMatchmakingTickets". RequestMatchStatusFunction will use the DynamoDB table to check if a ticket has succeeded matchmaking. This way we don't need to use the DescribeMatchmaking API of GameLift which can easily throttle with a large player count. The DynamoDB Table also has a TTL field and and configuration which means the tickets will be automatically removed after one hour of creation.

The SAM template defines IAM Policies to allow the Lambda functions to access both GameLift to request matchmaking as well as DynamoDB to access the player data. It is best practice to never allow game clients to access these resources directly as this can open different attack vectors to your resources.

### GameLiftExamplePreRequirements Stack

  * an **IAM Role** for the GameLift Fleet EC2 instances that allows access to CloudWatch to push logs and custom metrics
  * a **Cognito Identity Pool** that will be used to store player identities and the associated **IAM Roles** for unauthenticated and authenticated users that clients use to access the backend API through API Gateway. We don't authenticate users in the example but you could connect their Facebook identitities for example or any custom identities to Cognito
  * a **CloudWatch Dashboard** (*GameLift-Game-Backend-Metrics*) that aggregates a number of metrics from the backend including API Gateway, Lambda and DynamoDB metrics. 

# License

This example is licensed under the Apache 2.0 License. See LICENSE file.
