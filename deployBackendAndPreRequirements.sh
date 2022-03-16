#!/bin/bash

# 0. Check that we have the tools installed

if ! [ -x "$(command -v aws)" ]; then
  echo 'Error: AWS CLI is not installed'
  exit 1
fi

if ! [ -x "$(command -v sam)" ]; then
  echo 'Error: SAM is not installed'
  exit 1
fi

if ! [ -x "$(command -v node)" ]; then
  echo 'Error: Node is not installed'
  exit 1
fi

echo "1. Deploy Serverless Backend"
cd GameServiceAPI
./deploy.sh
cd ..

echo "2. Deploy Pre-Requirements (IAM & Cognito resources)"
cd FleetDeployment
./deployPreRequirements.sh
cd ..
