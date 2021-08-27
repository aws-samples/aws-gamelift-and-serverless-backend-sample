#!/bin/bash -e

g++ -o GameLiftExampleServer.x86_64 Server.h Server.cpp -Iinclude -Llib -laws-cpp-sdk-gamelift-server -lprotobuf-lite -lboost_date_time -lboost_random -lboost_system -lprotobuf -lsioclient -lprotoc -pthread -Wl,-rpath=./lib

cp -r lib/ ../ServerBuild/lib/
cp GameLiftExampleServer.x86_64 ../ServerBuild/