#!/bin/bash

# Install all required packages
echo "Installing required packages, this will take some time... "
sudo yum install -y gcc-c++ gdb cmake3 git

# Download the GameLift SDK, NOTE: You can replace this with the latest version
if [ ! -d "GameLift-SDK-Release-4.0.2" ]; then
    # script statements if $DIR doesn't exist.
    echo "Download and unzip GameLift Server SDK"
    wget https://gamelift-release.s3-us-west-2.amazonaws.com/GameLift_06_03_2021.zip
    unzip GameLift_06_03_2021.zip
else
    echo "GameLift Server SDK already downloaded."
fi


# Build the GameLift Server SDK
echo "Build the GameLift server SDK"
cd GameLift-SDK-Release-4.0.2
cd GameLift-Cpp-ServerSDK-3.4.2
mkdir out
cd out
cmake3 ..
make
cd prefix

echo "Copy the SDK build output to the Server folder (Lib and include)"
cp -r include ../../../../Server/
cp -r lib ../../../../Server/

cd ../../../..

echo "Done!"
