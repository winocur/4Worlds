using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FPPCharacterController : MonoBehaviour
{
    public Transform feet;

    Rigidbody rigidBody;
    float jumpForce = 6f;
    float accelerationSpeed = 5f;
    float horizontalSpeed = 3f;
    float groundedDistance = 0.3f;
    float airControlMovement = 1f;

    float momentumModifier = 0.06f;
    float momentumDeacceleration = 0.03f;
    public float forwardMomentum = 1f;

    public Vector3 lastMovement;

    [SerializeField]
    private bool isGrounded = true; 

    // Start is called before the first frame update
    void Start()
    {
        this.rigidBody = this.GetComponent<Rigidbody>();
    }

    void OnDrawGizmos()
    {
           Gizmos.DrawWireSphere(feet.position - new Vector3(0, 0.55f, 0), 0.55f);
     
    }

    void Update ()
    {
        RaycastHit hit;
        if(Physics.SphereCast(feet.position, 0.55f, Vector3.down, out hit, 0.55f, LayerMask.GetMask("Floor")))
        {
            isGrounded = true;
        }
        else
        {
            isGrounded = false;
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        
        Vector3 currentPosition = this.transform.position;

        float rotationY = this.transform.rotation.eulerAngles.y;
        float verticalInput = Input.GetAxisRaw("Vertical");
        float horizontalInput = Input.GetAxisRaw("Horizontal");

        Vector3 movement;

        forwardMomentum = Mathf.Clamp(forwardMomentum + (verticalInput * momentumModifier * Time.fixedDeltaTime) 
                                    - momentumDeacceleration * Time.fixedDeltaTime,
                                    1f, 2.5f);

        if(isGrounded)
        {
            movement = (Quaternion.AngleAxis(rotationY, Vector3.up) * 
                    new Vector3(horizontalInput * horizontalSpeed * Time.fixedDeltaTime
                            ,0
                            ,verticalInput * accelerationSpeed * forwardMomentum * Time.fixedDeltaTime)
                    );

        }
        else
        {
           movement = (Quaternion.AngleAxis(rotationY, Vector3.up) * 
                    new Vector3(horizontalInput * airControlMovement * Time.fixedDeltaTime
                            ,0
                            ,verticalInput * airControlMovement * Time.fixedDeltaTime)
                    ); 
        }

        this.rigidBody.MovePosition(currentPosition + movement);
        
        lastMovement = movement;

        if(Input.GetButtonDown("Jump")) {
            if(this.isGrounded) {
                this.rigidBody.AddForce( (Vector3.up + 
                                    (lastMovement.normalized / 2))
                                     * forwardMomentum 
                                     * jumpForce, ForceMode.Impulse);
            }
        }

    }

    public void PushBounce ()
    {

    }

    public void PushInertia ()
    {

    }
}
