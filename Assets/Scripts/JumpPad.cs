using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpPad : MonoBehaviour
{
    public float multiplier = 2.4f;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnCollisionEnter (Collision other)
    {
        if(other.gameObject.CompareTag("Player"))
        {
            FPPCharacterController controller = other.gameObject.GetComponent<FPPCharacterController>();
            controller.PushBounce(multiplier);
        }
    }
}
