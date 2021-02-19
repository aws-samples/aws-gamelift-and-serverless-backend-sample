// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

const AWS = require('aws-sdk');

exports.handler = async (event, context, callback) => {

    var ddb = new AWS.DynamoDB({apiVersion: '2012-08-10'});

    //Parse the FlexMatch message
    var message = event.Records[0].Sns.Message;
    message = JSON.parse(message);
    console.log('Message received from SNS:', message);
    console.log('Event type:' + message.detail.type)
    var type = message.detail.type
    // Only process if matchmaking succeeded
    if(type == 'MatchmakingSucceeded')
    {
      console.log("Succeeded matchmaking.")
      var ip = message.detail.gameSessionInfo.ipAddress
      var port = message.detail.gameSessionInfo.port
      var dnsName = message.detail.gameSessionInfo.dnsName

      // Get Epoch for TTL, we expire in 1 hour
      var date = new Date();
      date.setHours(date.getHours() + 1);
      var epochDate = parseInt(date.getTime()/1000);

      // Go through the tickets and write to DynamoDB
      for (var i = 0; i < message.detail.tickets.length; i++)
      {
        var ticketId = message.detail.tickets[i].ticketId
        // Note: We know there's only one player session in each ticket, this might be different based on your implementation!
        var playerSessionId = message.detail.tickets[i].players[0].playerSessionId
        console.log("Ticket: " + ticketId)
        console.log("PlayerSessionId: " + playerSessionId)
        // Write to DynamoDB
        var params = {
          TableName: 'GameLiftExampleMatchmakingTickets',
          Item: {
            'TicketID' : {S: ticketId},
            'playerSessionId' : {S: playerSessionId},
            'ip' : {S: ip},
            'port' : {S: port.toString()},
            'dnsName' : {S: dnsName},
            'TTL' : {N: epochDate.toString()}
          }
        };
        // Call DynamoDB to add the item to the table
        await ddb.putItem(params).promise().then(data => {
            console.log("Success", data);
        }).catch(err => {
          console.log("Error in put item:", err);
        });
      }
    }

    callback(null, "Success");
};