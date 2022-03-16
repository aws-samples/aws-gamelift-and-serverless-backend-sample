# Get the configuration variables
if (-not (Test-Path -Path "$PsScriptRoot\..\configuration.xml")) {
    throw 'The configuration file does not exist'
} else {
    Write-Host 'Loading configuration file'
    [xml]$Config = Get-Content "$PsScriptRoot\..\configuration.xml"
}

# Returns the status of a stack
Function Get-Status-Of-Stack {
  param($name)
	aws cloudformation describe-stacks --region $Config.Settings.AccountSettings.Region --stack-name $name --query Stacks[].StackStatus --output text 2> Out-Null
}

# Deploy the resources with CloudFormation
$stackstatus = Get-Status-Of-Stack GameLiftExamplePreRequirements

if ($null -eq $stackstatus) {
  Write-Host "Creating pre-requirements stack (this will take some time)..."
  aws cloudformation --region $Config.Settings.AccountSettings.Region create-stack --stack-name GameLiftExamplePreRequirements `
      --template-body file://prerequirements.yaml `
      --capabilities CAPABILITY_IAM
  aws cloudformation --region $Config.Settings.AccountSettings.Region wait stack-create-complete --stack-name GameLiftExamplePreRequirements
  Write-Host "Done creating stack!"
} else {
  Write-Host "Updating pre-requirements stack (this will take some time)..."
  aws cloudformation --region $Config.Settings.AccountSettings.Region update-stack --stack-name GameLiftExamplePreRequirements `
     --template-body file://prerequirements.yaml `
     --capabilities CAPABILITY_IAM
  aws cloudformation --region $Config.Settings.AccountSettings.Region wait stack-update-complete --stack-name GameLiftExamplePreRequirements
  Write-Host "Done updating stack!"
}

Write-Host "You need to set this Role ARN to the cloudwatch agent configuration in /LinuxServerBuild/amazon-cloudwatch-agent.json:"
Write-Host $(aws cloudformation --region $Config.Settings.AccountSettings.Region describe-stacks --stack-name GameLiftExamplePreRequirements --query "Stacks[0].Outputs[0].OutputValue")
Write-Host ""
Write-Host "You need this Identity pool ID for your client configuration"
Write-Host $(aws cloudformation --region $Config.Settings.AccountSettings.Region describe-stacks --stack-name GameLiftExamplePreRequirements --query "Stacks[0].Outputs[1].OutputValue")