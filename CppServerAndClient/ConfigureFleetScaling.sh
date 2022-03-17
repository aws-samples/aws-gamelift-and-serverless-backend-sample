#!/bin/bash

# Get the configuration variables
source ../configuration.sh

# Configuration for the scaling of the Fleet locations
# Set the max size to higher if you have requested a limit increase for your AWS account for instances
minsize=1
maxsize=1
desired=1
# available game sessions as percentage
availablesessions=20

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