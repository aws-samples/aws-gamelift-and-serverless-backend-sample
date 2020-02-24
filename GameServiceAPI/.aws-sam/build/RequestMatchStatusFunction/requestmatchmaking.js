// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

const AWS = require('aws-sdk');

const GameLift = new AWS.GameLift({region: process.env.AWS_REGION});

//TODO: Could be environment variable
const matchmakingConfigurationName = "ExampleGameConfiguration";

exports.requestMatchMaking = async (event) => {

    let response;
    var playerSkill = null;
    var ddb = new AWS.DynamoDB({apiVersion: '2012-08-10'});

    // 1. Get the player data from DynamoDB
    var params = {
      TableName: 'GameLiftExamplePlayerData',
      Key: {
        'ID': {S: event.requestContext.identity.cognitoIdentityId }
      }
    };
    await ddb.getItem(params).promise().then(data => {
      if(data.Item != null && data.Item["Skill"] != null)
      {
        console.log("Success", data.Item["Skill"].N);
        playerSkill = parseInt(data.Item["Skill"].N);
      }
      else
      {
        console.log("Player skill Data doesn't exist yet, will create");
      }
    }).catch(err => {
      console.log("Couldn't access player data in DynamoDB: " + err);
    });

    // 2. Create player data if it doesn't exist
    if(playerSkill == null)
    {
      console.log("Adding player to database");
      playerSkill = 10; //The initial skill level
      var params = {
        TableName: 'GameLiftExamplePlayerData',
        Item: {
          'ID' : {S: event.requestContext.identity.cognitoIdentityId},
          'Skill' : {N: playerSkill.toString()}
        }
      };
      // Call DynamoDB to add the item to the table
      await ddb.putItem(params).promise().then(data => {
          console.log("Success", data);
      }).catch(err => {
        console.log("Error in put item:", err);
      });
    }

    // Use the player's Cognito Identity as the ID
    console.log("Cognito ID: " + event.requestContext.identity.cognitoIdentityId);
    var playerId  = event.requestContext.identity.cognitoIdentityId;

    //Params for the matchmaking request
    var params = {
        ConfigurationName: matchmakingConfigurationName, 
        Players: [ 
          {
            PlayerAttributes: {
              skill : {
                N: playerSkill
              }
            },
            PlayerId: playerId
          }
        ]
      };
    
    // 3. Request matchmaking
    await GameLift.startMatchmaking(params).promise().then(data => {
      console.log(data);
      response = {
        "statusCode": 200,
        "headers": {
        },
        "body": JSON.stringify(
            data.MatchmakingTicket
        ),
        "isBase64Encoded": false
      };
    }).catch(err => {
        console.log(err);
    });

    //Return response if we got one
    if(response != null)
    {
      return response;
    }

    //Send error response if not successful
    response = {
            "statusCode": 500,
            "headers": {
            },
            "body": JSON.stringify({
                message: "Unable to do matchmaking"
            }),
            "isBase64Encoded": false
    };

    return response;
};
