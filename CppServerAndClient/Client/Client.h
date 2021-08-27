// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

#include <stdio.h>

#include <aws/core/Aws.h>
#include <aws/core/Region.h>

using namespace Aws;

//TODO: insert your backend API url here including /Prod/
String backendApiUrl = "https://<YOUREENDPOINT>";
//TODO: insert your identity pool ID here
String identityPoolId = "<YOURIDENTITYPOOLID>";
//TODO: insert the region of your backend services here
const char* REGION = Aws::Region::US_EAST_1;
String regionString = "us-east-1";
String secondaryRegionString = "eu-west-1"; //Secondary region location used by the GameLift Fleet