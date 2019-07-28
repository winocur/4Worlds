using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpPad : MonoBehaviour
{
    public float multiplier = 5f;

    // Jump pad sends the player flying in the direction of their current movement
    void OnCollisionEnter (Collision other)
    {
        if(other.gameObject.CompareTag("Player"))
        {
            FPPCharacterController controller = other.gameObject.GetComponent<FPPCharacterController>();
            controller.PushBounce(multiplier, 2f);
        }
    }
}
