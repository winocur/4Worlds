using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    public Transform platform;
    public Transform point_0;
    public Transform point_1;

    public float speed = 2f;
    public Transform target;

    Rigidbody rigidBody;
    // Start is called before the first frame update
    void Start()
    {
        target = point_0;
        this.rigidBody = this.GetComponentInChildren<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        // target change
        if((target.position - platform.position).magnitude < 0.1f)
        {
            target = (target == point_0) ? point_1 : point_0; 
        }
        rigidBody.MovePosition(platform.position +
                        ((target.position - platform.position).normalized * speed * Time.deltaTime)
                        );
    }
}
