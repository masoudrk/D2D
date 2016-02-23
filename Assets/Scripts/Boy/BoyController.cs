

using UnityEngine;
using System.Collections;
using System;
using System.Runtime.Remoting.Messaging;

public partial class BoyController : MonoBehaviour
{
    public enum State
    {
        IDLE, EDGE_DETECTED, NEAR_EDGE, GRABING_EDGE, GRABED_EDGE, CLIMBING_UP_FROM_EDGE, CLIMBING_DOWN_FROM_EDGE,
        NEAR_BOX, PULLING_BOX, JUMP_FROM_BOX, NEAR_BUTTON
    }

    public ETCJoystick joystick;
    public ETCButton btn1;
    public ETCButton btn2;
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
    private bool jumpBtn, helpBtnPress;
    public float _moveX;

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
            case State.NEAR_BOX:
            case State.PULLING_BOX:
                setHandIKOnBox();
                break;
            case State.NEAR_BUTTON:
                setHandIKOnButton();
                break;
            default:
                break;
        }
    }
    public void Update()
    {
        _isGround = characterGroundChecker.isGround;
        _moveX = joystick.axisX.axisValue;
        jumpBtn = btn1.axis.axisValue > 0;
        helpBtnPress = btn2.axis.axisValue > 0;

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
                animator.SetFloat("MoveAnimSpeed", Math.Abs(_moveX));
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
            case State.PULLING_BOX:
                pullingBoxAction();
                break;
            case State.NEAR_BOX:
                xMovementsAction();
                faceFlipingAction();
                detectDetectibles();
                detectBoxesAction();
                detectJumpFromBoxAction();
                detectPullingBox();
                animator.SetFloat("MoveAnimSpeed", .5f);
                break;
            case State.JUMP_FROM_BOX:
                jumpFromBoxAction();
                break;
            case State.NEAR_BUTTON:
                xMovementsAction();
                jumpAction();
                faceFlipingAction();
                break;
            default:
                break;
        }

        animator.SetBool("IsGround", _isGround);
        animator.SetFloat("VelocityX", Mathf.Abs(_moveX * maxSpeedX));
        animator.SetFloat("VelocityY", rigidBody2D.velocity.y);
    }


    #region Box Methods

    private void pullingBoxAction()
    {
        if (!helpBtnPress)
        {
            connectBoxJoint(false);
            state = State.NEAR_BOX;
            return;
        }

        findBoxNearEdge();
        if ((flipFacing && _moveX > 0) || (!flipFacing && _moveX < 0))
        {
            //pulling box
            connectBoxJoint(true);
            rigidBody2D.velocity = new Vector2(_moveX * maxSpeedX, rigidBody2D.velocity.y);
            animator.SetFloat("MoveAnimSpeed", -.5f);
            isPulling = true;
        }
        else if (_moveX != 0)
        {
            //pushing box

            /*
            connectBoxJoint(false);
            state = State.NEAR_BOX;
            */
            connectBoxJoint(false);
            rigidBody2D.velocity = new Vector2(_moveX * maxSpeedX, rigidBody2D.velocity.y);
            animator.SetFloat("MoveAnimSpeed", .5f);
            isPulling = false;
        }

    }
    private void detectPullingBox()
    {
        if (helpBtnPress)
        {
            pullBoxJoint2D = box2D.GetComponent<DistanceJoint2D>();
            state = State.PULLING_BOX;
        }
    }
    private void detectJumpFromBoxAction()
    {
        if (jumpBtn)
        {
            state = State.JUMP_FROM_BOX;
        }
    }
    private void jumpFromBoxAction()
    {
        if (!animator.GetCurrentAnimatorStateInfo(0).IsName("JumpFromBoxFinished"))
        {
            animator.SetBool("JumpFromBox", true);
        }
        else
        {
            transform.position = bodyTransform.position;
            bodyTransform.localPosition = Vector3.zero;
            animator.SetBool("JumpFromBox", false);

            rigidBody2D.isKinematic = false;
            state = State.IDLE;
        }
    }
    #endregion

    #region Corner Methods
    private void detectClimbUpDownAction()
    {
        if (jumpBtn)
        {
            state = State.CLIMBING_UP_FROM_EDGE;
        }
        else if (jumpBtn)
        {
            state = State.CLIMBING_DOWN_FROM_EDGE;
        }
    }
    #endregion

    #region Generic Methods
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
    private void jumpAction()
    {
        if (jumpBtn && _isGround && !lockJump)
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
    #endregion

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
    public void OnDrawGizmos()
    {
        Vector3 v = transform.position;
        v.y += detectDetectibleCirleOffsetY;
        Gizmos.DrawWireSphere(v, detectDetectibleCirleRadius);
    }
    public void OnGUI()
    {
        GUI.Box(new Rect(0, 0, 200, 20), state.ToString());
    }
}
