#!/bin/bash

# Get the configuration variables
source ../configuration.sh

echo "Delete The Bots Stack.."
aws cloudformation --region $region delete-stack --stack-name gamelift-example-bot-resources
aws cloudformation --region $region wait stack-delete-complete --stack-name gamelift-example-bot-resources
echo "Done deleting stack!"