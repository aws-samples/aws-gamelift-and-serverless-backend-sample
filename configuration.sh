#!/bin/bash

# Home Region of the Backend resources, GameLift resources, and the Fleet
region="us-east-1"
# Region for the Fleet's second Location
secondaryregion="us-west-2"

# A Unique name for the bucket used for backend deployments
deploymentbucketname="<YOUR_UNIQUE_BUCKET_NAME>"

# Account ID (only needed for bot clients, no need to set when not creating bots)
accountid="123456789012"