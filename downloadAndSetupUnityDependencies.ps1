# 0. Check that we have the tools installed

if (Get-Command ./nuget) {
    Write-Host "Nuget installed"
}
else {
    Write-Host "Nuget not installed, exit. Please copy nuget.exe directly to this directory"
    exit
}

if (Get-Command msbuild) {
    Write-Host "MsBuild installed"
}
else {
    Write-Host "MSBuild not installed, exit."
    exit
}

$ProgressPreference = 'SilentlyContinue'

############################

# 1. Download and build the GameLift Server SDK.
if (Test-Path("$PsScriptRoot\GameLift_06_03_2021.zip")) {
    Write-Host 'Already downloaded GameLift Server SDK'
}
else {
    Write-Host 'Downloading GameLift Server SDK...'
    Invoke-WebRequest https://gamelift-release.s3-us-west-2.amazonaws.com/GameLift_06_03_2021.zip -OutFile GameLift_06_03_2021.zip
}

# Unzip, build and copy the files to the correct folder in the Unity project
Write-Host "Unzipping to temporary folder..."
New-Item -Path "./" -Name "glsdk" -ItemType "directory" -Force
Expand-Archive -Path GameLift_06_03_2021.zip -DestinationPath "$PsScriptRoot\glsdk\" -Force

#  Build the SDK in Release
Write-Host "Building the SDK..."
cd glsdk/GameLift-SDK-Release-4.0.2/GameLift-CSharp-ServerSDK-4.0.2/

# Retarget to 4.7.2 which is installed with Visual Studio Build Tools 2022
$con = Get-Content ./Net45/GameLiftServerSDKNet45.csproj
$con | % { $_.Replace("v4.5", "v4.7.2") } | Set-Content ./Net45/GameLiftServerSDKNet45.csproj

# Restore packages and Build the SDK
../../../nuget restore
msbuild GameLiftServerSDKNet45.sln -property:Configuration=Release

# Copy the output files to the project
Write-Host "Copying files.."
Copy-Item "Net45/bin/Release/*" -Destination "../../../GameLiftExampleUnityProject/Assets/Dependencies/GameLiftServerSDK" -Force
cd ../../..
Remove-Item -Recurse -Force glsdk

# Fix the Newtonsoft JSON dll name because Unity 2020 has overlapping dll
Move-Item -Path "GameLiftExampleUnityProject/Assets/Dependencies/GameLiftServerSDK/Newtonsoft.Json.dll" -Destination "GameLiftExampleUnityProject/Assets/Dependencies/GameLiftServerSDK/Newtonsoft.Json.GameLift.dll" -Force 

Write-Host "Done!"

############################

# 2. Download the AWS .NET SDK for .NET Standard 2.0 (that works with Unity)
if (Test-Path("$PsScriptRoot\aws-sdk-netstandard2.0.zip")) {
    Write-Host "AWS .NET SDK for .NET Standard 2.0 already downloaded."
}
else {
    Write-Host "Download AWS .NET SDK for .NET Standard 2.0"
    Invoke-WebRequest https://sdk-for-net.amazonwebservices.com/latest/v3/aws-sdk-netstandard2.0.zip -OutFile aws-sdk-netstandard2.0.zip
}

# Unzip and copy the files to the correct folder in the Unity project
Write-Host "Unzipping to temporary folder..."
New-Item -Path "./" -Name "aws-sdk-temp" -ItemType "directory" -Force
Expand-Archive -Path aws-sdk-netstandard2.0.zip -DestinationPath "$PsScriptRoot\aws-sdk-temp\" -Force

Write-Host "Copying files to the Unity project..."
New-Item -Path "GameLiftExampleUnityProject/Assets/Dependencies/" -Name "AWSSDK" -ItemType "directory" -Force
Copy-Item "aws-sdk-temp/AWSSDK.CognitoIdentity.dll" -Destination "GameLiftExampleUnityProject/Assets/Dependencies/AWSSDK/" -Force
Copy-Item "aws-sdk-temp/AWSSDK.CognitoIdentityProvider.dll*" -Destination "GameLiftExampleUnityProject/Assets/Dependencies/AWSSDK/" -Force
Copy-Item "aws-sdk-temp/AWSSDK.Core.dll" -Destination "GameLiftExampleUnityProject/Assets/Dependencies/AWSSDK/" -Force
Copy-Item "aws-sdk-temp/AWSSDK.SecurityToken.dll" -Destination "GameLiftExampleUnityProject/Assets/Dependencies/AWSSDK/" -Force
Copy-Item "aws-sdk-temp/Microsoft.Bcl.AsyncInterfaces.dll" -Destination "GameLiftExampleUnityProject/Assets/Dependencies/AWSSDK/" -Force
Copy-Item "aws-sdk-temp/System.Threading.Tasks.Extensions.dll" -Destination "GameLiftExampleUnityProject/Assets/Dependencies/AWSSDK/" -Force
Write-Host "Removing the temporary files..."
Remove-Item -Recurse -Force aws-sdk-temp

echo "Done!"

############################

# 3. Download the Signature calculation example from AWS docs for SigV4 API request signing
if (Test-Path("$PsScriptRoot\AmazonS3SigV4_Samples_CSharp.zip")) {
    Write-Host "Signature calculation example already downloaded."
}
else {
    Write-Host "Download the Signature calculation example from AWS docs"
    Invoke-WebRequest https://docs.aws.amazon.com/AmazonS3/latest/API/samples/AmazonS3SigV4_Samples_CSharp.zip -OutFile AmazonS3SigV4_Samples_CSharp.zip
}

# Unzip and copy the files to the correct folder in the Unity project
Write-Host "Unzipping to temporary folder..."
New-Item -Path "./" -Name "signaturecalculation-temp" -ItemType "directory" -Force
Expand-Archive -Path AmazonS3SigV4_Samples_CSharp.zip -DestinationPath "$PsScriptRoot\signaturecalculation-temp\" -Force

Write-Host "Copying files to the Unity project..."
New-Item -Path "GameLiftExampleUnityProject/Assets/Dependencies//" -Name "Signers" -ItemType "directory" -Force
New-Item -Path "GameLiftExampleUnityProject/Assets/Dependencies/" -Name "Util" -ItemType "directory" -Force
Copy-Item "signaturecalculation-temp/AWSSignatureV4-S3-Sample/Signers/*" -Destination "GameLiftExampleUnityProject/Assets/Dependencies/Signers/" -Force -Recurse
Copy-Item "signaturecalculation-temp/AWSSignatureV4-S3-Sample/Util/*" -Destination "GameLiftExampleUnityProject/Assets/Dependencies/Util/" -Force -Recurse
Write-Host "Removing the temporary files..."
Remove-Item -Recurse -Force signaturecalculation-temp

echo "Done!"