#!/bin/bash

# The number of tasks to run (each task will have 4 bots)
numberoftasks=10

# Get the configuration variables
source ../configuration.sh

# Returns the status of a stack
getstatusofstack() {
	aws cloudformation describe-stacks --region $region --stack-name $1 --query Stacks[].StackStatus --output text 2>/dev/null
}

# 1. Create ECR repository if it doesn't exits
aws ecr create-repository --repository-name gamelift-example-bot-client --region $region --output text

# 2. Login to ECR (AWS CLI V2)
aws ecr get-login-password --region $region | docker login --username AWS --password-stdin $accountid.dkr.ecr.$region.amazonaws.com/gamelift-example-bot-client
#eval $(aws ecr get-login --region $region --no-include-email) #This if for CLI V1

# 3. Create Docker Image from latest build (expected to be already created from Unity)
build_id=$(date +%Y-%m-%d.%H%M%S)
docker build ./Build/ -t $accountid.dkr.ecr.$region.amazonaws.com/gamelift-example-bot-client:$build_id

# 4. Push the image to ECR
docker push $accountid.dkr.ecr.$region.amazonaws.com/gamelift-example-bot-client:$build_id

# 5. Deploy an updated task definition with the new image
stackstatus=$(getstatusofstack gamelift-example-bot-resources)
if [ -z "$stackstatus" ]; then
  echo "Creating gamelift-example-bot-resources stack (this will take some time)..."
  aws cloudformation --region $region create-stack --stack-name gamelift-example-bot-resources \
      --template-body file://gamelift-example-bot-resources.yaml \
      --parameters ParameterKey=Image,ParameterValue=$accountid.dkr.ecr.$region.amazonaws.com/gamelift-example-bot-client:$build_id ParameterKey=TaskCount,ParameterValue=$numberoftasks\
      --capabilities CAPABILITY_IAM
  aws cloudformation --region $region wait stack-create-complete --stack-name gamelift-example-bot-resources
  echo "Done creating stack!"
else
  echo "Updating gamelift-example-bot-resources stack (this will take some time)..."
  aws cloudformation --region $region update-stack --stack-name gamelift-example-bot-resources \
     --template-body file://gamelift-example-bot-resources.yaml \
     --parameters ParameterKey=Image,ParameterValue=$accountid.dkr.ecr.$region.amazonaws.com/gamelift-example-bot-client:$build_id ParameterKey=TaskCount,ParameterValue=$numberoftasks \
     --capabilities CAPABILITY_IAM
  aws cloudformation --region $region wait stack-update-complete --stack-name gamelift-example-bot-resources
  echo "Done updating stack!"
fi

echo "Your bots will be running as an ECS service as long as you keep them running! Use destroyBots.sh to destroy the stack"