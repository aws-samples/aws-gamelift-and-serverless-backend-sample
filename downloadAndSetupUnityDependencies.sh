#!/bin/bash

# 1. ownload the AWS .NET SDK for .NET Standard 2.0 (that works with Unity)
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
cp aws-sdk-temp/AWSSDK.CognitoIdentity.dll GameLiftExampleUnityProject/Assets/Dependencies/
cp aws-sdk-temp/AWSSDK.CognitoIdentityProvider.dll GameLiftExampleUnityProject/Assets/Dependencies/
cp aws-sdk-temp/AWSSDK.Core.dll GameLiftExampleUnityProject/Assets/Dependencies/
cp aws-sdk-temp/AWSSDK.SecurityToken.dll GameLiftExampleUnityProject/Assets/Dependencies/
cp aws-sdk-temp/Microsoft.Bcl.AsyncInterfaces.dll GameLiftExampleUnityProject/Assets/Dependencies/
cp aws-sdk-temp/System.Threading.Tasks.Extensions.dll GameLiftExampleUnityProject/Assets/Dependencies/
echo "Removing the temporary files..."
rm -rf aws-sdk-temp

echo "Done!"

# 2. Download the Signature calculation example from AWS docs for SigV4 API request signing
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