

using UnityEngine;
using System.Collections;
using System;

public partial class BoyController : MonoBehaviour
{ 
    public enum State
    {  
        IDLE , EDGE_DETECTED ,NEAR_EDGE , GRABING_EDGE , GRABED_EDGE , CLIMBING_UP_FROM_EDGE, CLIMBING_DOWN_FROM_EDGE
    }

    public CharacterGroundChecker characterGroundChecker;

    private State state;

    public Transform neckIK;
    public Transform rightHandIK;
    public Transform rightFootIK;
    public Transform leftHandIK;
    public Transform leftFootIK;

    public Transform handTransform;
    public Transform bodyTransform;

    public float maxSpeedX;
    public bool flipFacing;

    private Rigidbody2D rigidBody2D;
    private Animator animator;
    
    private bool lockFliping,lockMovement, lockJump;
    private CameraStress cameraStress;

    private bool _isGround;
    private float _moveX , _moveY;

    public void Start()
    {
        lockJump = lockMovement = lockFliping = false;
        state = State.IDLE;
        rigidBody2D = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    public void LateUpdate()
    {
        switch (state)
        {
            case State.EDGE_DETECTED:
            case State.NEAR_EDGE:
            case State.GRABING_EDGE:
            case State.GRABED_EDGE:
                setHandIKOnEdge();
                break;
            default:
                break;
        }
    }

    public void Update()
    {
        _isGround = characterGroundChecker.isGround;
        _moveX = Input.GetAxis("Horizontal");
        _moveY = Input.GetAxisRaw("Vertical");

        switch (state)
        {
            case State.NEAR_EDGE:
            case State.EDGE_DETECTED:
            case State.GRABING_EDGE:
            case State.IDLE:
                xMovementsAction();
                jumpAction();
                faceFlipingAction();
                detectEdgesAction();
                break;
            case State.GRABED_EDGE:
                detectClimbUpDownAction();
                stickOnEdgeAction();
                break;
            case State.CLIMBING_UP_FROM_EDGE:
                climbUpAction();
                break;
            case State.CLIMBING_DOWN_FROM_EDGE:
                climbDownAction();
                break;
            default:
                break;
        }

        print(state);

        animator.SetFloat("VelocityX", Mathf.Abs(_moveX*maxSpeedX));
        animator.SetFloat("VelocityY", rigidBody2D.velocity.y);
    }


    private void faceFlipingAction()
    {
        if ((_moveX > 0 && flipFacing) || (_moveX < 0 && !flipFacing) && !lockFliping)
            flipFace();
    }

    private void detectClimbUpDownAction()
    {
        if (_moveY > 0)
        {
            state = State.CLIMBING_UP_FROM_EDGE;
        }
        else if(_moveY < 0)
        {
            state = State.CLIMBING_DOWN_FROM_EDGE;
        }
    }

    private void jumpAction()
    {
        if (_moveY > 0 && _isGround && !lockJump)
        {
            if (rigidBody2D.velocity.y < 5)
                rigidBody2D.velocity = new Vector2(rigidBody2D.velocity.x, 15);
        }
    }

    private void xMovementsAction()
    {
        if (_isGround && !lockMovement)
            rigidBody2D.velocity = new Vector2(_moveX*maxSpeedX, rigidBody2D.velocity.y);
    }
    
    public bool isFront(Vector3 objPos)
    {
        return (!flipFacing && transform.position.x < objPos.x) | (flipFacing && transform.position.x > objPos.x);
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
        Vector3 v = transform.position;
        v.y += detectEdgeCirleOffsetY;
        Gizmos.DrawWireSphere(v, detectEdgeCirleRadius);
    }
}
