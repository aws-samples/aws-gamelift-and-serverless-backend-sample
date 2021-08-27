#!/bin/bash -e

#g++ -o servertest Server.h Server.cpp -Iinclude -Llib -laws-cpp-sdk-gamelift-server -lprotobuf-lite -lboost_date_time -lboost_random -lboost_system -lprotobuf -lsioclient -lprotoc -pthread -Wl,-rpath=./lib
 g++ -o client Client.h Client.cpp -laws-cpp-sdk-core -laws-cpp-sdk-cognito-identity -Wl,-rpath=/usr/local/lib64