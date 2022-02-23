# Deploys the backend SAM application as well as Cognito resources and IAM role for the Fleet

Write-Host  "1. Deploy Serverless Backend"
cd GameServiceAPI
./deploy.ps1
cd ..

Write-Host  "2. Deploy Pre-Requirements (IAM & Cognito resources)"
cd FleetDeployment
./deployPreRequirements.ps1
cd ..