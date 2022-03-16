#!/bin/bash

# Get the configuration variables
source ../configuration.sh

# Create deployment bucket if it doesn't exist
if [ $region == "us-east-1" ]
then
    aws s3api create-bucket --bucket $deploymentbucketname --region $region
else
    aws s3api create-bucket --bucket $deploymentbucketname --region $region --create-bucket-configuration LocationConstraint=$region
fi

# Build, package and deploy the backend
sam build
sam package --region $region --s3-bucket $deploymentbucketname --output-template-file gameservice.yaml
sam deploy --template-file gameservice.yaml --region $region --capabilities CAPABILITY_IAM --stack-name GameLiftExampleServerlessGameBackend