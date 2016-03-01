

using UnityEngine;
using System.Collections;
using System;
using System.Runtime.Remoting.Messaging;

public partial class BoyController : MonoBehaviour
{
    public enum State
    {
        IDLE, CORNER_DETECTED, NEAR_CORNER, GRABING_EDGE, GRABED_CORNER, CLIMBING_UP_FROM_EDGE, CLIMBING_DOWN_FROM_EDGE,
        NEAR_BOX, PULLING_BOX, JUMP_FROM_BOX, NEAR_BUTTON, NEAR_ROPE , GRABED_ROPE
    }

    private float defaultMass , defaultFootsDamping , defaultHandsDamping;

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

    public SimpleCCD rightHandCCD;
    public SimpleCCD leftHandCCD;
    public SimpleCCD rightFootCCD;
    public SimpleCCD leftFootCCD;

    public GameObject boyCollider;

    public float maxSpeedX;
    public bool flipFacing;

    private Rigidbody2D rigidBody2D;
    private Animator animator;

    private bool lockFliping, lockMovement, lockJump;
    private CameraStress cameraStress;

    private bool _isGround;
    private bool jumpBtn, helpBtnPress;
    public float _moveX , _moveY;

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

        defaultFootsDamping = leftFootCCD.damping;
        defaultHandsDamping = rightFootCCD.damping;
        defaultMass = rigidBody2D.mass;
    }

    public void LateUpdate()
    {
        switch (state)
        {
            case State.CORNER_DETECTED:
            case State.NEAR_CORNER:
            case State.GRABING_EDGE:
            case State.GRABED_CORNER:
                setHandIKOnCorner();
                break;
            case State.NEAR_BOX:
            case State.PULLING_BOX:
                setHandIKOnBox();
                break;
            case State.NEAR_BUTTON:
                setHandIKOnButton();
                break;
            case State.GRABED_ROPE:
                setHandIKOnRope();
                break;
            default:
                break;
        }
    }
    public void Update()
    {
        _isGround = characterGroundChecker.isGround;
        _moveX = joystick.axisX.axisValue;
        _moveY = joystick.axisY.axisValue;
        jumpBtn = btn1.axis.axisValue > 0;
        helpBtnPress = btn2.axis.axisValue > 0;

        switch (state)
        {
            case State.NEAR_CORNER:
            case State.CORNER_DETECTED:
            case State.GRABING_EDGE:
            case State.IDLE:
                xMovementsAction();
                jumpAction();
                faceFlipingAction();
                detectDetectibles();
                detectCornersAction();
                detectBoxesAction();
                detectRopeAction();
                animator.SetFloat("MoveAnimSpeed", Math.Abs(_moveX));
                setAnimator();
                break;
            case State.GRABED_CORNER:
                cornerClimbUpDownInputAction();
                stickOnCornerAction();
                setAnimator();
                break;
            case State.CLIMBING_UP_FROM_EDGE:
                climbUpAction();
                setAnimator();
                break;
            case State.CLIMBING_DOWN_FROM_EDGE:
                climbDownAction();
                setAnimator();
                break;
            case State.PULLING_BOX:
                pullingBoxAction();
                setAnimator();
                break;
            case State.NEAR_BOX:
                xMovementsAction();
                faceFlipingAction();
                detectDetectibles();
                detectBoxesAction();
                detectPullingBox();
                detectJumpFromBoxAction();
                animator.SetFloat("MoveAnimSpeed", .5f);
                setAnimator();
                break;
            case State.JUMP_FROM_BOX:
                jumpFromBoxAction();
                setAnimator();
                break;
            case State.NEAR_BUTTON:
                xMovementsAction();
                jumpAction();
                faceFlipingAction();
                setAnimator();
                break; 
            case State.GRABED_ROPE:
                ropeInputAction();
                resetAnimator();
                break;
            default:
                break;
        }

        //print(state.ToString());
    }

    private void setAnimator()
    {
        animator.SetBool("IsGround", _isGround);
        animator.SetFloat("VelocityX", Mathf.Abs(_moveX * maxSpeedX));
        animator.SetFloat("VelocityY", rigidBody2D.velocity.y);
    }
    private void resetAnimator()
    {
        animator.SetBool("IsGround", false);
        animator.SetFloat("VelocityX", 0);
        animator.SetFloat("VelocityY", 0);
    }
    
    #region Box Methods

    private void pullingBoxAction()
    {
        detectDetectibles();
        if (!isDetected("Box"))
        {
            connectBoxJoint(false);
            state = State.IDLE;
        }

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
            pullBoxJoint2D = boxTransform.GetComponent<DistanceJoint2D>();
            state = State.PULLING_BOX;
        }
    }
    private void detectJumpFromBoxAction()
    {
        if (jumpBtn)
        {
            Box box = boxTransform.GetComponent<Box>();
            bool canDo = box.canJumpUpFrom;

            if (canDo)
            {
                state = State.JUMP_FROM_BOX;
            }
            else
            {
                corner = box.jumpCorner;
                animator.SetBool("GrabEdge", true);
                animator.SetBool("ClimbingUp", true);
                state = State.CLIMBING_UP_FROM_EDGE;
            }
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

    #region Generic Methods
    private bool isDetected(ref Transform fillIt, string tagStr)
    {
        foreach (var d in detectibles)
        {
            if (d.tag != tagStr) continue;
            fillIt = d.transform;
            return true;
        }
        return false;
    }
    private Transform isDetected(string tagStr)
    {
        foreach (var d in detectibles)
            if (d.tag == tagStr)
                return d.transform;
        return null;
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
