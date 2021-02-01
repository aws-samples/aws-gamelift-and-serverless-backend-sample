$region="us-east-1"

# Returns the status of a stack
Function Get-Status-Of-Stack {
  param($name)
	aws cloudformation describe-stacks --region $region --stack-name $name --query Stacks[].StackStatus --output text 2> Out-Null
}

# Deploy the resources with CloudFormation
$stackstatus = Get-Status-Of-Stack GameLiftExamplePreRequirements

if ($null -eq $stackstatus) {
  Write-Host "Creating pre-requirements stack (this will take some time)..."
  aws cloudformation --region $region create-stack --stack-name GameLiftExamplePreRequirements `
      --template-body file://prerequirements.yaml `
      --capabilities CAPABILITY_IAM
  aws cloudformation --region $region wait stack-create-complete --stack-name GameLiftExamplePreRequirements
  Write-Host "Done creating stack!"
} else {
  Write-Host "Updating pre-requirements stack (this will take some time)..."
  aws cloudformation --region $region update-stack --stack-name GameLiftExamplePreRequirements `
     --template-body file://prerequirements.yaml `
     --capabilities CAPABILITY_IAM
  aws cloudformation --region $region wait stack-update-complete --stack-name GameLiftExamplePreRequirements
  Write-Host "Done updating stack!"
}

Write-Host "You need to set this Role ARN to the cloudwatch agent configuration in /LinuxServerBuild/amazon-cloudwatch-agent.json:"
Write-Host $(aws cloudformation --region $region describe-stacks --stack-name GameLiftExamplePreRequirements --query "Stacks[0].Outputs[0].OutputValue")
Write-Host ""
Write-Host "You need this Identity pool ID in NetworkClient.cs:"
Write-Host $(aws cloudformation --region $region describe-stacks --stack-name GameLiftExamplePreRequirements --query "Stacks[0].Outputs[1].OutputValue")