using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FPPCharacterController : MonoBehaviour
{
    public Transform feet;


    Rigidbody rigidBody;

    // jumping
    float firstJumpForce = 5.8f;
    float continuousJumpForce = 2500f;

    float accelerationSpeed = 5f;
    float horizontalSpeed = 4.5f;
    float groundedDistance = 0.3f;
    float airControlMovement = 1.5f;

    float momentumModifier = 0.14f;
    float momentumDeacceleration = 0.03f;
    float slideTime = .7f;
    public float forwardMomentum = 1f;
    float gravity = -13f;
    float currentGravity = 0f;
    float jumpTime = 0.17f;
    float momentumThreshold = 0.04f;

    float wallrunCheckerDistance = 1.5f;
    float wallrunGravity = -7f;
    public float wallrunExitTimer = -1f;
    float wallrunExitJumpTime = 0.6f;
    WallrunHit lastWallrunHit;

    float mouseSensitivityX = 4f;


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
        this.currentGravity = gravity;
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

        if(wallrunExitTimer >= 0) {
            wallrunExitTimer += Time.deltaTime;
        }

        CheckLanding();

        lastGrounded = isGrounded;

        // Only during movement
        if(playerState == PlayerState.moving) {
            if(Input.GetButton("Jump")) {
                this.EnterJump();
            }    

            if(Input.GetButtonDown("Slide")) {
                if(this.isGrounded) {
                    StartCoroutine(SlideCoroutine());
                }
            }

            verticalInput = Input.GetAxisRaw("Vertical");
            horizontalInput = Input.GetAxisRaw("Horizontal");

            // X mouse look
            float xRotation = Input.GetAxis("Mouse X") * mouseSensitivityX;
            Quaternion xQuaternion = Quaternion.AngleAxis (xRotation, Vector3.up);
            this.transform.rotation *= xQuaternion;
    
        } else if (playerState == PlayerState.wallrunning) {
            if(!Input.GetButton("Jump")) {
                this.ExitWallrunning();
            }    

            WallrunHit unused;
            if(!CheckWallrun(out unused)) {
                this.ExitWallrunning();
            }
        }
       
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if(playerState != PlayerState.moving &&
        playerState != PlayerState.wallrunning) return;
        
        MoveCharacter(verticalInput, horizontalInput);        
    
        lastPosition = this.transform.position;
    }

    public void EnterJump () {
        float braceTime = 0.3f;
        float impulseTime = 0.1f;

        this.camera.AnimateBraceForJump(braceTime);
        LeanTween.delayedCall(braceTime, () => this.camera.AnimateJump(impulseTime));
        LeanTween.delayedCall(braceTime = impulseTime, () => Jump());
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
            
        } else if (wallrunExitTimer >= 0 && wallrunExitTimer <= wallrunExitJumpTime) {
            Debug.Log("Entering Wallrun Jump");
            EnterWallrunJump ();
        } else if (airTimer <= jumpTime) {
            // timed follow up
            this.rigidBody.AddForce(Vector3.up
                                    * forwardMomentum 
                                    * continuousJumpForce * multiplier * Time.deltaTime, 
                                    ForceMode.Acceleration);

        } else {

            // check for wallrun
            if(this.playerState == PlayerState.moving)  {
                WallrunHit hit;
                if(CheckWallrun(out hit)) {
                    Debug.DrawRay(this.transform.position, hit.moveVector, Color.magenta, 5f);
                    EnterWallrunning(hit);
                }       
            }
            
        }

    }

    private bool CheckWallrun (out WallrunHit hitObject) {
        RaycastHit hit;
        
        Debug.DrawRay(feet.position, this.transform.rotation * Vector3.left * wallrunCheckerDistance, Color.green);
        Debug.DrawRay(feet.position, this.transform.rotation * Vector3.right * wallrunCheckerDistance, Color.cyan);
        
        if(Physics.Raycast(feet.position, this.transform.rotation * Vector3.left, out hit, wallrunCheckerDistance, LayerMask.GetMask("Wall"))) {
            hitObject = new WallrunHit {
                obj = hit.transform.gameObject,
                isRight = false,
                moveVector = Quaternion.Euler(0, -90f, 0) * hit.normal,
                normalVector = hit.normal,
            };
            
            return true;
        } 
        
        
        if (Physics.Raycast(feet.position, this.transform.rotation * Vector3.right, out hit, wallrunCheckerDistance, LayerMask.GetMask("Wall"))) {
            hitObject = new WallrunHit {
                obj = hit.transform.gameObject,
                isRight = true,
                moveVector = Quaternion.Euler(0, 90f, 0) * hit.normal,
                normalVector = hit.normal,
            };
            return true;
        }


        hitObject = new WallrunHit {};
        return false;
    }

    private void EnterWallrunning (WallrunHit wallrunHit) {

        if(wallrunHit.obj == lastWallrunHit.obj) return;

        this.playerState = PlayerState.wallrunning;
        Vector3 currentVelocity = this.rigidBody.velocity;
        this.rigidBody.velocity = new Vector3(currentVelocity.x, 6.2f, currentVelocity.z);
        this.camera.AnimateEnterWallrun(wallrunHit.isRight);
        
        LeanTween.rotate(this.gameObject, Quaternion.LookRotation(wallrunHit.moveVector, Vector3.up).eulerAngles, 0.3f);

        this.currentGravity = wallrunGravity;
        lastWallrunHit = wallrunHit;

    }

    private void ExitWallrunning () {
        Debug.Log("Exit wallrunning");
        
        this.currentGravity = gravity;
        this.camera.AnimateExitWallrun();
        this.playerState = PlayerState.moving;
        wallrunExitTimer = 0.01f;
    }

    private void EnterWallrunJump () {
        wallrunExitTimer = -1;

        

        WallrunHit hit;
        if(this.CheckWallrun(out hit)) {
            Vector3 outJumpDirection = hit.normalVector + hit.normalVector + lastMovement.normalized;
            LeanTween.rotate(this.gameObject, Quaternion.LookRotation(outJumpDirection, Vector3.up).eulerAngles, 0.22f).setEaseOutCirc(); //.setOnComplete(
            //() => {
                this.isGrounded = true;
                this.airTimer = 0f;
                lastMovement = outJumpDirection;
                Jump(1f * forwardMomentum);
            //});
        }
    }

        

    private Vector3 lastPosition;

    public void MoveCharacter (float verticalInput, float horizontalInput)
    {
        float actualLandMovement = GetActualHorizontalMovementMagnitude();
        float momentumAcceleration = (actualLandMovement * momentumModifier * Time.fixedDeltaTime);

        // momentum calculation
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

        // regular movement
        if(isGrounded || this.playerState == PlayerState.wallrunning)
        {
            movement = (Quaternion.AngleAxis(rotationY, Vector3.up) * 
                    new Vector3(horizontalInput * horizontalSpeed * Time.fixedDeltaTime
                            ,0
                            ,verticalInput * accelerationSpeed * forwardMomentum * Time.fixedDeltaTime)
                    );

        }

        // Air control
        else
        {
           movement = (Quaternion.AngleAxis(rotationY, Vector3.up) * 
                    new Vector3(horizontalInput * airControlMovement * Time.fixedDeltaTime
                            ,0
                            ,verticalInput * airControlMovement * Time.fixedDeltaTime)
                    ); 

        }

        // aplies gravity
        this.rigidBody.AddForce(new Vector3(0, currentGravity, 0), ForceMode.Acceleration);

        this.rigidBody.MovePosition(currentPosition + movement);
       
        lastMovement = movement;
    }

    private void CheckLanding ()
    {
        // landed
        if (lastGrounded == false && isGrounded == true)
        {
            airTimer = 0f;
            wallrunExitTimer = -1f;
            Debug.Log("Landed");
            lastWallrunHit.obj = null;
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

struct WallrunHit {
    public bool isRight;
    public GameObject obj;
    public Vector3 moveVector;
    public Vector3 normalVector;
}
