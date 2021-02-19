// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

const AWS = require('aws-sdk');

const GameLift = new AWS.GameLift({region: process.env.AWS_REGION});

exports.requestMatchStatus = async (event) => {
  
    let response;

    //Get the ticket from request querystring
    var ticketId = event.queryStringParameters.ticketId;

    console.log("Ticket id: " + ticketId);

    var ddb = new AWS.DynamoDB({apiVersion: '2012-08-10'});

    // Try to get the ticket data from DynamoDB
    var params = {
      TableName: 'GameLiftExampleMatchmakingTickets',
      Key: {
        'TicketID': {S: ticketId }
      }
    };
    await ddb.getItem(params).promise().then(data => {
      if(data.Item != null)
      {
        console.log("Found Ticket")
        ip = data.Item["ip"].S
        port = parseInt(data.Item["port"].S)
        playerSessionId = data.Item["playerSessionId"].S
        dnsName = data.Item["dnsName"].S

        var responsedata = {
          IpAddress :ip,
          Port : port,
          PlayerSessionId : playerSessionId,
          DnsName: dnsName
        }
      }
      else
      {
        console.log("Matchmaking not succeeded yet");
        var responsedata = {
          IpAddress : "",
          Port : 0,
          PlayerSessionId : "NotPlacedYet"
        }
      }

      response = {
        "statusCode": 200,
        "headers": {
        },
        "body": JSON.stringify(
            responsedata
        ),
        "isBase64Encoded": false
      };

    }).catch(err => {
      console.log("Couldn't access ticket data in DynamoDB: " + err);
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
