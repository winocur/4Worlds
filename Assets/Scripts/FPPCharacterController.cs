using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FPPCharacterController : MonoBehaviour
{
    public Transform feet;


    Rigidbody rigidBody;

    // jumping
    float firstJumpForce = 3.6f;
    float continuousJumpForce = 2500f;

    float accelerationSpeed = 5f;
    float horizontalSpeed = 4.5f;
    float groundedDistance = 0.3f;
    float airControlMovement = 1.5f;

    float momentumModifier = 0.14f;
    float momentumDeacceleration = 0.03f;
    float slideTime = .7f;
    public float forwardMomentum = 1f;
    float jumpTime = 0.17f;
    float momentumThreshold = 0.04f;

    public Vector3 lastMovement;
    public bool lastGrounded;

    public CapsuleCollider collider;

    public PlayerState playerState;

    public GameCamera camera;

    public float airTimer = 0f;

    public enum PlayerState {
        moving,
        sliding,
        wallrunning,
    }

    [SerializeField]
    private bool isGrounded = true; 

    // Start is called before the first frame update
    void Start()
    {
        this.rigidBody = this.GetComponent<Rigidbody>();
        this.playerState = PlayerState.moving;
    }

    void OnDrawGizmos()
    {
           Gizmos.DrawWireSphere(feet.position - new Vector3(0, 0.55f, 0), 0.55f);
     
    }

    float verticalInput;
    float horizontalInput;

    void Update ()
    {

        RaycastHit hit;
        if(Physics.SphereCast(feet.position, 0.55f, Vector3.down, out hit, 0.55f, LayerMask.GetMask("Floor")))
        {
            if(airTimer > jumpTime || airTimer == 0)
            {
                isGrounded = true;
            }
            else
            {
                isGrounded = false;
                airTimer += Time.deltaTime;
            }
        }
        else
        {
            isGrounded = false;
            airTimer += Time.deltaTime;
        }

        CheckLanding();

        lastGrounded = isGrounded;

        // Only during movement
        if(playerState != PlayerState.moving) return;

        if(Input.GetButton("Jump")) {
           this.Jump();
        } 

        if(Input.GetButtonDown("Slide")) {
            if(this.isGrounded) {
                StartCoroutine(SlideCoroutine());
            }
        }

        verticalInput = Input.GetAxisRaw("Vertical");
        horizontalInput = Input.GetAxisRaw("Horizontal");
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if(playerState != PlayerState.moving) return;
        
        MoveCharacter(verticalInput, horizontalInput);        
    
        lastPosition = this.transform.position;
    }

    public void Jump (float multiplier = 1, float directionDivider = 1f)
    {
        if(this.isGrounded && airTimer == 0) {
            Debug.Log("Initial jump at: " + multiplier);
            // initial jump
            airTimer = 0f;
            this.rigidBody.AddForce( (Vector3.up + 
                                (lastMovement.normalized / directionDivider))
                                    * forwardMomentum 
                                    * firstJumpForce
                                    * multiplier, ForceMode.Impulse);

            airTimer += Time.deltaTime;
            
        } else if (airTimer <= jumpTime) {
            // timed follow up
            this.rigidBody.AddForce(Vector3.up
                                    * forwardMomentum 
                                    * continuousJumpForce * multiplier * Time.deltaTime, 
                                    ForceMode.Acceleration);

        } 

    }

    private Vector3 lastPosition;

    public void MoveCharacter (float verticalInput, float horizontalInput)
    {
        float actualLandMovement = GetActualHorizontalMovementMagnitude();
        float momentumAcceleration = (actualLandMovement * momentumModifier * Time.fixedDeltaTime);
  
        if(actualLandMovement > momentumThreshold)
        {
            forwardMomentum = Mathf.Clamp(forwardMomentum + momentumAcceleration * Time.fixedDeltaTime,
                                    1f, 2.5f);
        } else {
            // if not moved, reset momentum
            forwardMomentum = 1f;
        }
        
        Vector3 movement;
        Vector3 currentPosition = this.transform.position;
    
        float rotationY = this.transform.rotation.eulerAngles.y;

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
    }

    private void CheckLanding ()
    {
        // landed
        if (lastGrounded == false && isGrounded == true)
        {
            airTimer = 0f;
            Debug.Log("Landed");
        }
    }

    public IEnumerator SlideCoroutine ()
    {
        this.playerState = PlayerState.sliding;
        float slideTimer = 0f;
    
        this.collider.height = 0.3f;
        this.collider.center = new Vector3 (0, -0.70f, 0);
        camera.AnimateEnterSlide();

        yield return 0;

        while(slideTimer < slideTime)
        {
            MoveCharacter(1, 0);
            slideTimer += Time.deltaTime;
            yield return 0;
        }

        camera.AnimateExitSlide();
        this.collider.height = 2f;
        this.collider.center = new Vector3 (0, 0, 0);
        this.playerState = PlayerState.moving;
    }

    public float GetActualHorizontalMovementMagnitude ()
    {
        return (new Vector3(this.transform.position.x, 0 ,this.transform.position.z) -
                new Vector3(lastPosition.x, 0, lastPosition.z)).magnitude;
    }

    public void PushBounce (float multiplier, float directionDivider = 1)
    {
        this.Jump(multiplier, directionDivider);
    }

    public void PushInertia ()
    {

    }
}
