// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

#include <aws/gamelift/server/model/GameSession.h>

class Server
{
public:
    Server();

    bool InitializeGameLift(int listenPort, std::string logfile);
    void FinalizeGameLift();
    bool AcceptPlayerSession(const std::string& playerSessionId);
    void OnStartGameSession(Aws::GameLift::Server::Model::GameSession myGameSession);
    void OnProcessTerminate();
    bool OnHealthCheck() { return mActivated; }
    void TerminateGameSession();

private:
    bool mActivated;

};