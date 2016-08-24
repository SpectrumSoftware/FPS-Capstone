using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CharacterController))]

public class playerMovement : MonoBehaviour {
    
    //Movement Variables//
    public float walkSpeed = 6.0f;
    public float runSpeed = 11.0f;
    public bool limitDiagonalSpeed = true;
    public bool toggleRun = true;
    public float jumpSpeed = 8.0f;
    public float gravity = 20.0f;
    public float fallingDamageThreshold = 10.0f;
    public bool slideWhenOverSlopeLimit = false;
    public bool slideOnTaggedObjects = false;
    public float slideSpeed = 12.0f;
    public bool airControl = false;
    public float antiBumpFactor = .75f;
    public int antiBunnyHopFactor = 1;

    //Private Variables//
    private Vector3 moveDirection = Vector3.zero;
    private bool grounded = false;
    private CharacterController controller;
    private Transform myTransform;
    private float speed;
    private RaycastHit hit;
    private float fallStartLevel;
    private bool falling;
    private float slideLimit;
    private float rayDistance;
    private Vector3 contactPoint;
    private bool playerControl = false;
    private int jumpTimer;
 
    void Start() {
        controller = GetComponent<CharacterController>();
        myTransform = transform;
        speed = walkSpeed;
        rayDistance = controller.height * .5f + controller.radius;
        slideLimit = controller.slopeLimit - .1f;
        jumpTimer = antiBunnyHopFactor;
    }
 
    void FixedUpdate() {
        float inputX = Input.GetAxis("Horizontal");
        float inputY = Input.GetAxis("Vertical");
        //If both horizontal and vertical are used simultaneously, limit speed (if allowed), so the total doesn't exceed normal move speed//
        float inputModifyFactor = (inputX != 0.0f && inputY != 0.0f && limitDiagonalSpeed)? .7071f : 1.0f;
 
        if (grounded) {
            bool sliding = false;
            //See if surface immediately below should be slid down//
            if (Physics.Raycast(myTransform.position, -Vector3.up, out hit, rayDistance)) {
                if (Vector3.Angle(hit.normal, Vector3.up) > slideLimit)
                    sliding = true;
            }
            else {
                Physics.Raycast(contactPoint + Vector3.up, -Vector3.up, out hit);
                if (Vector3.Angle(hit.normal, Vector3.up) > slideLimit)
                    sliding = true;
            }
 
            //If we were falling, and we fell a vertical distance greater than the threshold, run a falling damage routine//
            if (falling) {
                falling = false;
                if (myTransform.position.y < fallStartLevel - fallingDamageThreshold)
                    FallingDamageAlert (fallStartLevel - myTransform.position.y);
            }
 
            //If running isn't on a toggle, then use the appropriate speed depending on whether the run button is down//
            if (toggleRun)
                speed = Input.GetButton("Run")? runSpeed : walkSpeed;
 
            //If sliding (and it's allowed), or if we're on an object tagged "Slide", get a vector pointing down the slope we're on//
            if ( (sliding && slideWhenOverSlopeLimit) || (slideOnTaggedObjects && hit.collider.tag == "Slide") ) {
                Vector3 hitNormal = hit.normal;
                moveDirection = new Vector3(hitNormal.x, -hitNormal.y, hitNormal.z);
                Vector3.OrthoNormalize (ref hitNormal, ref moveDirection);
                moveDirection *= slideSpeed;
                playerControl = false;
            }
            else {
                moveDirection = new Vector3(inputX * inputModifyFactor, -antiBumpFactor, inputY * inputModifyFactor);
                moveDirection = myTransform.TransformDirection(moveDirection) * speed;
                playerControl = true;
            }
 
            //Jump! But only if the jump button has been released and player has been grounded for a given number of frames//
            if (!Input.GetButton("Jump"))
                jumpTimer++;
            else if (jumpTimer >= antiBunnyHopFactor) {
                moveDirection.y = jumpSpeed;
                jumpTimer = 0;
            }
        }
        else {
            //If we stepped over a cliff or something, set the height at which we started falling//
            if (!falling) {
                falling = true;
                fallStartLevel = myTransform.position.y;
            }
 
            //If air control is allowed, check movement but don't touch the y component//
            if (airControl && playerControl) {
                moveDirection.x = inputX * speed * inputModifyFactor;
                moveDirection.z = inputY * speed * inputModifyFactor;
                moveDirection = myTransform.TransformDirection(moveDirection);
            }
        }
 
        //Apply gravity//
        moveDirection.y -= gravity * Time.deltaTime;
 
        //Move the controller, and set grounded true or false depending on whether we're standing on something//
        grounded = (controller.Move(moveDirection * Time.deltaTime) & CollisionFlags.Below) != 0;
    }
 
    void Update () {
        transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);
        //If the run button is set to toggle, then switch between walk/run speed//
        if (toggleRun && grounded && Input.GetButtonDown("Run"))
            speed = (speed == walkSpeed? runSpeed : walkSpeed);
    }
 
    //Store point that we're in contact with for use in FixedUpdate if needed//
    void OnControllerColliderHit (ControllerColliderHit hit) {
        contactPoint = hit.point;
    }
 
    //If falling damage occured//
    void FallingDamageAlert (float fallDistance) {
        print ("You Fell " + fallDistance + " units!");   
    }
}

