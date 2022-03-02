# Deploys the backend SAM application as well as Cognito resources and IAM role for the Fleet

# 0. Check that we have the tools installed

if (Get-Command aws) {
    Write-Host "AWS CLI installed"
}
else {
    Write-Host "aWS CLI not installed, exit."
    exit
}

if (Get-Command sam) {
    Write-Host "SAM installed"
}
else {
    Write-Host "SAM not installed, exit."
    exit
}

if (Get-Command node) {
    Write-Host "Node installed"
}
else {
    Write-Host "Node not installed, exit."
    exit
}

Write-Host  "1. Deploy Serverless Backend"
cd GameServiceAPI
./deploy.ps1
cd ..

Write-Host  "2. Deploy Pre-Requirements (IAM & Cognito resources)"
cd FleetDeployment
./deployPreRequirements.ps1
cd ..