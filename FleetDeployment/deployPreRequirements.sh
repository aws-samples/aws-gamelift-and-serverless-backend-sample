#!/bin/bash

# Get the configuration variables
source ../configuration.sh

# Returns the status of a stack
getstatusofstack() {
	aws cloudformation describe-stacks --region $region --stack-name $1 --query Stacks[].StackStatus --output text 2>/dev/null
}

# Deploy the resources with CloudFromation
stackstatus=$(getstatusofstack GameLiftExamplePreRequirements)
if [ -z "$stackstatus" ]; then
  echo "Creating pre-requirements stack (this will take some time)..."
  aws cloudformation --region $region create-stack --stack-name GameLiftExamplePreRequirements \
      --template-body file://prerequirements.yaml \
      --capabilities CAPABILITY_IAM
  aws cloudformation --region $region wait stack-create-complete --stack-name GameLiftExamplePreRequirements
  echo "Done creating stack!"
else
  echo "Updating pre-requirements stack (this will take some time)..."
  aws cloudformation --region $region update-stack --stack-name GameLiftExamplePreRequirements \
     --template-body file://prerequirements.yaml \
     --capabilities CAPABILITY_IAM
  aws cloudformation --region $region wait stack-update-complete --stack-name GameLiftExamplePreRequirements
  echo "Done updating stack!"
fi

echo "We need to set this Role ARN to the cloudwatch agent configuration in /LinuxServerBuild/amazon-cloudwatch-agent.json:"
echo $(aws cloudformation --region $region describe-stacks --stack-name GameLiftExamplePreRequirements --query "Stacks[0].Outputs[0].OutputValue")
echo "Configuring the role in /LinuxServerBuild/amazon-cloudwatch-agent.json..."
rolearn=$(aws cloudformation --region $region describe-stacks --stack-name GameLiftExamplePreRequirements --query "Stacks[0].Outputs[0].OutputValue")
sed -i -e "s|.*role_arn.*|        \"role_arn\": $rolearn|" ../LinuxServerBuild/amazon-cloudwatch-agent.json
echo "Done!"
echo ""
echo "You need this Identity pool ID in NetworkClient.cs:"
echo $(aws cloudformation --region $region describe-stacks --stack-name GameLiftExamplePreRequirements --query "Stacks[0].Outputs[1].OutputValue")