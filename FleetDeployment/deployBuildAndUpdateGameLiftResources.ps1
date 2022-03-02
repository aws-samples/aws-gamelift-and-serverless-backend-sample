# Get the configuration variables
if (-not (Test-Path -Path "$PsScriptRoot\..\configuration.xml")) {
    throw 'The configuration file does not exist'
} else {
    Write-Host 'Loading configuration file'
    [xml]$Config = Get-Content "$PsScriptRoot\..\configuration.xml"
}

# Configuration for the scaling of the Fleet locations
$minsize = 1
$maxsize = 2
$desired = 1
# available game sessions as percentage
$availablesessions = 20

# Returns the status of a stack
Function Get-Status-Of-Stack {
  param($name)
	aws cloudformation describe-stacks --region $Config.Settings.AccountSettings.Region --stack-name $name --query Stacks[].StackStatus --output text 2> Out-Null
}

# Deploy the build to GameLift (Expecting that it was built from Unity already)
Write-Host "Deploying build (Expecting it is prebuilt in LinuxServerBuild folder)"
$buildversion = Get-Date -UFormat "%y-%m-%d.%H%M%S"
aws gamelift upload-build --operating-system AMAZON_LINUX_2 --build-root ../LinuxServerBuild --name "Unity Game Server Example" --build-version $buildversion --region $Config.Settings.AccountSettings.Region

# Get the build version for fleet deployment
$query = """Builds[?Version==``$buildversion``].BuildId"""
$buildid = aws gamelift list-builds --query $query --output text --region $Config.Settings.AccountSettings.Region

# Deploy rest of the resources with CloudFormation
$stackstatus=Get-Status-Of-Stack GameliftExampleResources
$secondaryregion = $Config.Settings.AccountSettings.SecondaryRegion
if ($null -eq $stackstatus) {
  Write-Host "Creating stack for example fleet (this will take some time, up to 40 minutes)..."
  aws cloudformation --region $Config.Settings.AccountSettings.Region create-stack --stack-name GameliftExampleResources `
      --template-body file://gamelift.yaml `
      --parameters ParameterKey=BuildId,ParameterValue=$buildid ParameterKey=SecondaryLocation,ParameterValue=$secondaryregion `
      --capabilities CAPABILITY_IAM
  aws cloudformation --region $Config.Settings.AccountSettings.Region wait stack-create-complete --stack-name GameliftExampleResources
  Write-Host "Done creating stack!"
} else {
  Write-Host "Updating stack for example fleet (this will take some time, up to 40 minutes)..."
  aws cloudformation --region $Config.Settings.AccountSettings.Region update-stack --stack-name GameliftExampleResources `
     --template-body file://gamelift.yaml `
     --parameters ParameterKey=BuildId,ParameterValue=$buildid ParameterKey=SecondaryLocation,ParameterValue=$secondaryregion `
     --capabilities CAPABILITY_IAM
  aws cloudformation --region $Config.Settings.AccountSettings.Region wait stack-update-complete --stack-name GameliftExampleResources
  Write-Host "Done updating stack!"
}

# get the fleet ID
Write-Host "Requesting Fleet ID for scaling configuration..."
$fleetid = aws cloudformation --region $Config.Settings.AccountSettings.Region describe-stacks --stack-name GameliftExampleResources --query "Stacks[0].Outputs[0].OutputValue"

# Set the min, max and desired, as the CloudFormation deployment doesn't set this
Write-Host "Updating the fleet scaling configuration..."
aws gamelift update-fleet-capacity --fleet-id $fleetid --min-size $minsize --max-size $maxsize --desired-instances $desired --location $Config.Settings.AccountSettings.Region --region $Config.Settings.AccountSettings.Region
aws gamelift update-fleet-capacity --fleet-id $fleetid --min-size $minsize --max-size $maxsize --desired-instances $desired --location $Config.Settings.AccountSettings.SecondaryRegion --region $Config.Settings.AccountSettings.Region

# Set the scaling configuration for the Fleet to 20% available game sessions
Write-Host 'Setting scaling policy for the fleet to 20% available game sessions...'
aws gamelift put-scaling-policy --name ExampleFleetScaling --fleet-id $fleetid --policy-type TargetBased --target-configuration TargetValue=$availablesessions --metric-name PercentAvailableGameSessions --region $Config.Settings.AccountSettings.Region
Write-Host 'Done'