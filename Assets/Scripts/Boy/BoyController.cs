

using UnityEngine;
using System.Collections;
using System;

public partial class BoyController : MonoBehaviour
{ 
    public enum State
    {  
        IDLE , EDGE_DETECTED ,NEAR_EDGE , GRABING_EDGE , GRABED_EDGE 
    }

    public CharacterGroundChecker characterGroundChecker;

    State state;

    public Transform neckIK;
    public Transform rightHandIK;
    public Transform rightFootIK;
    public Transform leftHandIK;
    public Transform leftFootIK;

    public Transform handTransform;

    public float maxSpeedX;
    public bool flipFacing;

    private Rigidbody2D rigidBody2D;
    private Animator animator;
    
    private bool lockFliping,lockMovement, lockJump;


    void Start()
    {
        lockJump = lockMovement = lockFliping = false;
        state = State.IDLE;
        rigidBody2D = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }


    void Update()
    {
        bool _isGround = characterGroundChecker.isGround;
        float moveX = Input.GetAxis("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");

        if (_isGround && !lockMovement) 
            rigidBody2D.velocity = new Vector2(moveX * maxSpeedX, rigidBody2D.velocity.y);

        if (moveY > 0 && _isGround && !lockJump)
        {
            if (rigidBody2D.velocity.y < 5)
                rigidBody2D.velocity = new Vector2(rigidBody2D.velocity.x, 15);
        }

        if ((moveX > 0 && flipFacing) || (moveX < 0 && !flipFacing) && !lockFliping)
            flipFace();

        animator.SetFloat("VelocityX", Mathf.Abs(moveX * maxSpeedX));
        animator.SetFloat("VelocityY", rigidBody2D.velocity.y);
        
        if (state == State.GRABED_EDGE)
        {
            comeToEdge();
        }
        else
        {
            detectEdges();
        }
    }

    public void LateUpdate()
    {
        if (state == State.EDGE_DETECTED || state == State.NEAR_EDGE || state == State.GRABED_EDGE)
        {
            setHandIKOnEdge();
        }
        /*
        if (state == State.GRABED_EDGE)
        {
            comeToEdge();
        }*/

        if (state == State.NEAR_EDGE)
        {/*
            Vector3 v = new Vector3(Mathf.Abs(handTransform.position.x - edgePos.x), 3f) + transform.position;
            leftFootIK.position = rightFootIK.position = -v;*/
        }
    }


    public bool isFront(Vector3 objPos)
    {
        return (!flipFacing && transform.position.x < objPos.x) |
                (flipFacing && transform.position.x > objPos.x);
    }

    public void flipFace()
    {
        flipFacing = !flipFacing;
        Vector3 localScale = transform.localScale;
        localScale.x *= -1;
        transform.localScale = localScale;
    }

    public void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, 10);
        Gizmos.DrawWireSphere(handTransform.position, circleRadius);
    }
}
