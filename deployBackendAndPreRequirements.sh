#!/bin/bash

echo "1. Deploy Serverless Backend"
cd GameServiceAPI
./deploy.sh
cd ..

echo "2. Deploy Pre-Requirements (IAM & Cognito resources)"
cd FleetDeployment
./deployPreRequirements.sh
cd ..
