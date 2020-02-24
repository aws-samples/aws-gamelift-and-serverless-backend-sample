// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using UnityEngine;

public class UIManager : MonoBehaviour
{
    public UnityEngine.UI.Text textBox;

    public void SetTextBox(string text)
    {
        this.textBox.text = text;
    }
}
