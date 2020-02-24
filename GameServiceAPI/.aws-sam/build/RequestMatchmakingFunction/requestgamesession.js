// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

const AWS = require('aws-sdk');

const GameLift = new AWS.GameLift({region: process.env.AWS_REGION});

//NOTE: This could be set as environment variable
const AliasID = "alias-bea3576b-497c-44c9-8ba6-bd4b1a881e5b";

exports.requestGameSession = async (event) => {
    let response;
    let gameSessions;

    // find any sessions that have available players
    await GameLift.searchGameSessions({
        AliasId: AliasID,
        FilterExpression: "hasAvailablePlayerSessions=true"
    }).promise().then(data => {
        gameSessions = data.GameSessions;
    }).catch(err => {
        response = {
            "statusCode": 500,
            "headers": {
            },
            "body": JSON.stringify({
                message: err
            }),
            "isBase64Encoded": false
        }; 
    });

    // if the response object has any value at any point before the end of
    // the function that indicates a failure condition so return the response
    if(response != null) 
    {
        return response;
    }

    // if there are no sessions, then we need to create a game session
    let selectedGameSession;
    if(gameSessions.length == 0)
    {
        console.log("No game session detected, creating a new one");
        await GameLift.createGameSession({
            MaximumPlayerSessionCount: 2,   // only two players allowed per game
            AliasId: AliasID
        }).promise().then(data => {
            selectedGameSession = data.GameSession;
        }).catch(err => {
            response = {
                "statusCode": 500,
                "headers": {
                },
                "body": JSON.stringify({
                    message: err
                }),
                "isBase64Encoded": false
            };
        });

        if(response != null)
        {
            return response;
        }
    }
    else
    {
        // we grab the first session we find and join it
        selectedGameSession = gameSessions[0];
        console.log("Game session exists, will join session ", selectedGameSession.GameSessionId);
    }
    
    // there isn't a logical way selectedGameSession could be null at this point
    // but it's worth checking for in case other logic is added
    if(selectedGameSession != null) 
    {
        // now we have a game session one way or the other, create a session for this player
        await GameLift.createPlayerSession({
            GameSessionId : selectedGameSession.GameSessionId ,
            PlayerId: event.requestContext.identity.cognitoIdentityId
        }).promise().then(data => {
            console.log("Created player session ID: ", data.PlayerSession.PlayerSessionId);
            response =  {
                "statusCode": 200,
                "headers": {
                },
                "body": JSON.stringify(data.PlayerSession),
                "isBase64Encoded": false
            }
        }).catch(err => {
           response = {
                "statusCode": 500,
                "headers": {
                },
                "body": JSON.stringify({
                    message: err
                }),
                "isBase64Encoded": false
            };
        });

    }
    else
    {
        response = {
            "statusCode": 500,
            "headers": {
            },
            "body": JSON.stringify({
                message: "Unable to find game session, check GameLift API status"
            }),
            "isBase64Encoded": false
        };
    }

    return response;
};
