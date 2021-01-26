$region="us-east-1"

# Returns the status of a stack
Function Get-Status-Of-Stack {
  param($name)
	aws cloudformation describe-stacks --region $region --stack-name $name --query Stacks[].StackStatus --output text 2> Out-Null
}

# Deploy the build to GameLift (Expecting that it was built from Unity already)
Write-Host "Deploying build (Expecting it is prebuilt in LinuxServerBuild folder)"
$buildversion = Get-Date -UFormat "%y-%m-%d.%H%M%S"
aws gamelift upload-build --operating-system AMAZON_LINUX_2 --build-root ../LinuxServerBuild --name "Unity Game Server Example" --build-version $buildversion --region $region

# Get the build version for fleet deployment
$query = """Builds[?Version==``$buildversion``].BuildId"""
$buildid = aws gamelift list-builds --query $query --output text --region $region

# Deploy rest of the resources with CloudFormation
$stackstatus=Get-Status-Of-Stack GameliftExampleResources
if ($null -eq $stackstatus) {
  Write-Host "Creating stack for example fleet (this will take some time)..."
  aws cloudformation --region $region create-stack --stack-name GameliftExampleResources `
      --template-body file://gamelift.yaml `
      --parameters ParameterKey=BuildId,ParameterValue=$buildid `
      --capabilities CAPABILITY_IAM
  aws cloudformation --region $region wait stack-create-complete --stack-name GameliftExampleResources
  Write-Host "Done creating stack!"
} else {
  Write-Host "Updating stack for example fleet (this will take some time)..."
  aws cloudformation --region $region update-stack --stack-name GameliftExampleResources `
     --template-body file://gamelift.yaml `
     --parameters ParameterKey=BuildId,ParameterValue=$buildid `
     --capabilities CAPABILITY_IAM
  aws cloudformation --region $region wait stack-update-complete --stack-name GameliftExampleResources
  Write-Host "Done updating stack!"
}