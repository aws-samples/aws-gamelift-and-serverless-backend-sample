#!/bin/bash

# Get the configuration variables
source ../configuration.sh

# Configuration for the scaling of the Fleet locations
minsize=1
maxsize=2
desired=1
# available game sessions as percentage
availablesessions=20

# Returns the status of a stack
getstatusofstack() {
	aws cloudformation describe-stacks --region $region --stack-name $1 --query Stacks[].StackStatus --output text 2>/dev/null
}

# Deploy the build to GameLift (Expecting that it was built from Unity already)
echo "Deploying build (Expecting it is prebuilt in LinuxServerBuild folder)"
buildversion=$(date +%Y-%m-%d.%H:%M:%S)
aws gamelift upload-build --operating-system AMAZON_LINUX_2 --build-root ../LinuxServerBuild --name "Unity Game Server Example" --build-version $buildversion --region $region

# Get the build version for fleet deployment
query='Builds[?Version==`'
query+=$buildversion
query+='`].BuildId'
buildid=$(aws gamelift list-builds --query $query --output text --region $region)
echo $buildid

# Deploy rest of the resources with CloudFromation
stackstatus=$(getstatusofstack GameliftExampleResources)
if [ -z "$stackstatus" ]; then
  echo "Creating stack for example fleet (this will take up to 40 minutes as we deploy to multiple regions)..."
  aws cloudformation --region $region create-stack --stack-name GameliftExampleResources \
      --template-body file://gamelift.yaml \
      --parameters ParameterKey=BuildId,ParameterValue=$buildid ParameterKey=SecondaryLocation,ParameterValue=$secondaryregion \
      --capabilities CAPABILITY_IAM
  aws cloudformation --region $region wait stack-create-complete --stack-name GameliftExampleResources
  echo "Done creating stack!"
else
  echo "Updating stack for example fleet (this will take up to 40 minutes as we deploy to multiple regions)..."
  aws cloudformation --region $region update-stack --stack-name GameliftExampleResources \
     --template-body file://gamelift.yaml \
     --parameters ParameterKey=BuildId,ParameterValue=$buildid ParameterKey=SecondaryLocation,ParameterValue=$secondaryregion \
     --capabilities CAPABILITY_IAM
  aws cloudformation --region $region wait stack-update-complete --stack-name GameliftExampleResources
  echo "Done updating stack!"
fi

# get the fleet ID
echo "Requesting Fleet ID for scaling configuration..."
fleetid=$(aws cloudformation --region $region describe-stacks --stack-name GameliftExampleResources --query "Stacks[0].Outputs[0].OutputValue")
# removes double quotes
fleetid=$(echo "$fleetid" | tr -d '"')
echo $fleetid

# Set the min, max and desired, as the CloudFormation deployment doesn't set this
echo "Updating the fleet scaling configuration..."
aws gamelift update-fleet-capacity --fleet-id $fleetid --min-size $minsize --max-size $maxsize --desired-instances $desired --location $region --region $region
aws gamelift update-fleet-capacity --fleet-id $fleetid --min-size $minsize --max-size $maxsize --desired-instances $desired --location $secondaryregion --region $region

# Set the scaling configuration for the Fleet to 20% available game sessions
echo 'Setting scaling policy for the fleet to 20% available game sessions...'
aws gamelift put-scaling-policy --name ExampleFleetScaling --fleet-id $fleetid --policy-type TargetBased --target-configuration TargetValue=$availablesessions --metric-name PercentAvailableGameSessions --region $region
echo 'Done'