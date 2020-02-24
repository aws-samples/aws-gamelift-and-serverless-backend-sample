// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

// A simple class for getting player input and moving a character

using UnityEngine;

public class SimpleController : MonoBehaviour
{
    float speed = 0.1f;

    // Manage player input on a fixed timestep
    void FixedUpdate()
    {
        Vector3 move = Vector3.zero;

        // Get the movement input 
        if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W))
        {
            move.z = 1;
        }
        else if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S))
        {
            move.z = -1;
        }
        if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
        {
            move.x = -1;
        }
        else if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
        {
            move.x = 1;
        }

        // Move the character
        move.Normalize();
        this.transform.LookAt(this.transform.position + move);
        this.GetComponent<CharacterController>().Move(move*speed);
    }
}
