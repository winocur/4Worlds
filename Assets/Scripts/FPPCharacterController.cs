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
    float slideTime = .4f;
    public float forwardMomentum = 1f;

    public Vector3 lastMovement;

    public CapsuleCollider collider;

    public PlayerState playerState;

    public GameCamera camera;

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
            isGrounded = true;
        }
        else
        {
            isGrounded = false;
        }

        // Only during movement
        if(playerState != PlayerState.moving) return;

        if(Input.GetButtonDown("Jump")) {
            if(this.isGrounded) {
                this.rigidBody.AddForce( (Vector3.up + 
                                    (lastMovement.normalized / 2))
                                     * forwardMomentum 
                                     * jumpForce, ForceMode.Impulse);
            }
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

    }

    public void MoveCharacter (float verticalInput, float horizontalInput)
    {
        Vector3 movement;
        Vector3 currentPosition = this.transform.position;
        float rotationY = this.transform.rotation.eulerAngles.y;

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

    public void PushBounce ()
    {

    }

    public void PushInertia ()
    {

    }
}
