// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using UnityEngine;

public class UIManager : MonoBehaviour
{
    public UnityEngine.UI.Text textBox;
    public UnityEngine.UI.Text latency1;
    public UnityEngine.UI.Text latency2;
    public UnityEngine.UI.Text gameServerInfo;

    public void SetTextBox(string text)
    {
        this.textBox.text = text;
    }

    public void SetLatencyInfo(string latency1, string latency2)
    {
        this.latency1.text = latency1;
        this.latency2.text = latency2;
    }

    public void SetGameServerInfo(string text)
    {
        gameServerInfo.text = text;
    }
}
