# Bot Client Implementation for testing the Unity deployment

This additional component is designed to help test the Unity deployment option by running headless client bots on AWS Fargate, a fully managed serverless platform for running containerized applications.

# Setup

You need to have the backend, GameLift resources and client all set up and deployed first.

1. Make sure you have Docker installed on your system
2. Make sure you have AWS CLI configured on your system
3. Open the GameLiftExampleUnityProject and configure bots by selecting Gamelift -> SetAsBotClientBuild
4. Build the bot client by selecting Gamelift -> BuildLinuxBotClient. Select the folder UnityBotCLient/Build as the output folder
5. Run ./buildSetupAndRunBots.sh to start the bots. Modify the script if you want to change the bot count. Each Fargate Task will run 4 bots, so the default configuration 10 will run 40 bots on a continuous fashion as a Fargate Service, until you terminate them
6. When you're done, run ./destroyBots.sh to destroy the whole infrastructure for the bots

