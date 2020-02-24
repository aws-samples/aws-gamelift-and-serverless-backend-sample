// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

const AWS = require('aws-sdk');

const GameLift = new AWS.GameLift({region: process.env.AWS_REGION});

exports.requestMatchStatus = async (event) => {
    let response;

    //Get the ticket from request querystring
    var ticketId = event.queryStringParameters.ticketId;

    //Params for the matchmaking status check
    var params = {
      TicketIds: [ 
        ticketId,
      ]
    };

    console.log("Ticket id: " + ticketId);

    // Request matchmaking status
    await GameLift.describeMatchmaking(params).promise().then(data => {

      console.log(data);
      var matchTicket = data.TicketList[0];
      console.log("Match status: " + matchTicket.Status);
      if(matchTicket.Status == "COMPLETED")
      {
          var responsedata = {
            IpAddress : matchTicket.GameSessionConnectionInfo.IpAddress,
            Port : matchTicket.GameSessionConnectionInfo.Port,
            PlayerSessionId : matchTicket.GameSessionConnectionInfo.MatchedPlayerSessions[0].PlayerSessionId,
            DnsName: matchTicket.GameSessionConnectionInfo.DnsName
          }
      }
      else
      {
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
