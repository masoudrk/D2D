

using UnityEngine;
using System.Collections;
using System;

public partial class BoyController : MonoBehaviour
{
    public enum State
    {
        IDLE, EDGE_DETECTED, NEAR_EDGE, GRABING_EDGE, GRABED_EDGE, CLIMBING_UP_FROM_EDGE, CLIMBING_DOWN_FROM_EDGE, NEAR_BUTTON
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

    private bool lockFliping, lockMovement, lockJump;
    private CameraStress cameraStress;

    private bool _isGround;
    private float _moveX, _moveY;

    private Collider2D[] detectibles;
    public float detectDetectibleCirleOffsetY = 1;
    public float detectDetectibleCirleRadius = 5;
    private Vector3 detectDetectibleCirle;

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

            case State.NEAR_BUTTON:
                setHandIKOnButton();
                break;
            default:
                break;
        }
    }

    private void setHandIKOnButton()
    {
        if (machineButton != null && isFront(machineButton.position))
        {
            if (Input.GetKey(KeyCode.LeftShift))
            {
                machineBtnClickEnd = false;
                rightHandIK.position = machineButton.position;
                Invoke("cancleClickingMachineBtn" , 0.5f);
            }
            else
            {
                if(!machineBtnClickEnd)
                    rightHandIK.position = machineButton.position;
            }
        }
    }

    void cancleClickingMachineBtn()
    {
        machineBtnClickEnd = true;
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
                detectDetectibles();
                detectCornersAction();
                detectBoxesAction();
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
            case State.NEAR_BUTTON:
                xMovementsAction();
                jumpAction();
                faceFlipingAction();
                break;
            default:
                break;
        }

        print(state);

        animator.SetBool("IsGround", _isGround);
        animator.SetFloat("VelocityX", Mathf.Abs(_moveX * maxSpeedX));
        animator.SetFloat("VelocityY", rigidBody2D.velocity.y);
        //float MoveAnimSpeed = Mathf.Abs(_moveX*maxSpeedX)/maxSpeedX;
        //animator.SetFloat("MoveAnimSpeed", (MoveAnimSpeed < 0.5f)? 1 - MoveAnimSpeed : 1);
    }

    public void detectDetectibles()
    {
        detectDetectibleCirle = transform.position;
        detectDetectibleCirle.y += detectDetectibleCirleOffsetY;
        detectibles = Physics2D.OverlapCircleAll(detectDetectibleCirle,
            detectDetectibleCirleRadius, 1 << LayerMask.NameToLayer("Detectible"));
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
        else if (_moveY < 0)
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
            rigidBody2D.velocity = new Vector2(_moveX * maxSpeedX, rigidBody2D.velocity.y);
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
        v.y += detectDetectibleCirleOffsetY;
        Gizmos.DrawWireSphere(v, detectDetectibleCirleRadius);
    }
    
    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Machine")
        {
            state = State.NEAR_BUTTON;
            machineButton = collision.transform.GetChild(0);
        }
    }

    public void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.tag == "Machine")
        {
            state = State.IDLE;
        }
    }
}
