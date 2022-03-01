#!/bin/bash

# 0. Check that we have the tools installed

if ! [ -x "$(command -v nuget)" ]; then
  echo 'Error: nuget is not installed. See README for Mono installation details'
  exit 1
fi

if ! [ -x "$(command -v msbuild)" ]; then
  echo 'Error: msbuild is not installed. See README for Mono installation details'
  exit 1
fi

############################

# 1. Download and build the GameLift Server SDK
if [ ! -d "GameLift_06_03_2021.zip" ]; then
    # script statements if $DIR doesn't exist.
    echo "Download GameLift Server SDK"
    curl -O https://gamelift-release.s3-us-west-2.amazonaws.com/GameLift_06_03_2021.zip
else
    echo "GameLift Server SDK already downloaded"
fi


# Unzip, build and copy the files to the correct folder in the Unity project
echo "Unzipping to temporary folder..."
mkdir aws-gamelift-sdk-temp
unzip GameLift_06_03_2021.zip -d aws-gamelift-sdk-temp

# Download dependencies and Build the SDK in Release
echo "Building the SDK..."
cd aws-gamelift-sdk-temp/GameLift-SDK-Release-4.0.2/GameLift-CSharp-ServerSDK-4.0.2/
nuget restore
msbuild GameLiftServerSDKNet45.sln -property:Configuration=Release

# Copy the output files to the project
echo "Copying files.."
cp Net45/bin/Release/* ../../../GameLiftExampleUnityProject/Assets/Dependencies/GameLiftServerSDK/
cd ../../..
rm -rf aws-gamelift-sdk-temp

# Fix the Newtonsoft JSON dll name because Unity 2020 has overlapping dll
mv GameLiftExampleUnityProject/Assets/Dependencies/GameLiftServerSDK/Newtonsoft.Json.dll GameLiftExampleUnityProject/Assets/Dependencies/GameLiftServerSDK/Newtonsoft.Json.GameLift.dll 

echo "Done!"

############################

# 2. Download the AWS .NET SDK for .NET Standard 2.0 (that works with Unity)
if [ ! -d "aws-sdk-netstandard2.0.zip" ]; then
    # script statements if $DIR doesn't exist.
    echo "Download and AWS .NET SDK for .NET Standard 2.0"
    curl -O https://sdk-for-net.amazonwebservices.com/latest/v3/aws-sdk-netstandard2.0.zip
else
    echo "AWS .NET SDK for .NET Standard 2.0 already downloaded."
fi

# Unzip and copy the files to the correct folder in the Unity project
echo "Unzipping to temporary folder..."
mkdir aws-sdk-temp
unzip aws-sdk-netstandard2.0.zip -d aws-sdk-temp

echo "Copying files to the Unity project..."
mkdir GameLiftExampleUnityProject/Assets/Dependencies/AWSSDK
cp aws-sdk-temp/AWSSDK.CognitoIdentity.dll GameLiftExampleUnityProject/Assets/Dependencies/AWSSDK/
cp aws-sdk-temp/AWSSDK.CognitoIdentityProvider.dll GameLiftExampleUnityProject/Assets/Dependencies/AWSSDK/
cp aws-sdk-temp/AWSSDK.Core.dll GameLiftExampleUnityProject/Assets/Dependencies/AWSSDK/
cp aws-sdk-temp/AWSSDK.SecurityToken.dll GameLiftExampleUnityProject/Assets/Dependencies/AWSSDK/
cp aws-sdk-temp/Microsoft.Bcl.AsyncInterfaces.dll GameLiftExampleUnityProject/Assets/Dependencies/AWSSDK/
cp aws-sdk-temp/System.Threading.Tasks.Extensions.dll GameLiftExampleUnityProject/Assets/Dependencies/AWSSDK/
echo "Removing the temporary files..."
rm -rf aws-sdk-temp

echo "Done!"

############################

# 3. Download the Signature calculation example from AWS docs for SigV4 API request signing
if [ ! -d "AmazonS3SigV4_Samples_CSharp.zip" ]; then
    # script statements if $DIR doesn't exist.
    echo "Download the Signature calculation example from AWS docs"
    curl -O https://docs.aws.amazon.com/AmazonS3/latest/API/samples/AmazonS3SigV4_Samples_CSharp.zip
else
    echo "Signature calculation example already downloaded."
fi

# Unzip and copy the files to the correct folder in the Unity project
echo "Unzipping to temporary folder..."
mkdir signaturecalculation-temp
unzip AmazonS3SigV4_Samples_CSharp.zip -d signaturecalculation-temp

echo "Copying files to the Unity project..."
mkdir GameLiftExampleUnityProject/Assets/Dependencies/Signers/
mkdir GameLiftExampleUnityProject/Assets/Dependencies/Util/
cp -r signaturecalculation-temp/AWSSignatureV4-S3-Sample/Signers/ GameLiftExampleUnityProject/Assets/Dependencies/Signers/
cp -r signaturecalculation-temp/AWSSignatureV4-S3-Sample/Util/ GameLiftExampleUnityProject/Assets/Dependencies/Util/
echo "Removing the temporary files..."
rm -rf signaturecalculation-temp

echo "Done!"