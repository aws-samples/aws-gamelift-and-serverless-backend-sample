#!/bin/bash

sudo yum install -y gcc-c++ gdb cmake3 git libcurl-devel

if [ ! -d "AWSCPPSDK" ]; then
    echo "Clone the AWS C++ SDK, this will take some time..."
    mkdir AWSCPPSDK
    cd AWSCPPSDK
    git clone --recurse-submodules https://github.com/aws/aws-sdk-cpp
    mkdir sdk_build
    cd sdk_build
else
    echo "AWS C++ SDK already downloaded, update the repository"
    cd AWSCPPSDK
    git pull
    cd sdk_build
fi


echo "Build and install the SDK, libraries will be then available for any application on the instance"
cmake3 ../aws-sdk-cpp/ -DCMAKE_BUILD_TYPE=Debug -DBUILD_ONLY="cognito-identity"
make
sudo make install

echo "Done!"

cd ../..
