using UnityEngine;
using System.Collections;
using System.Linq;
using System;

public partial class BoyController
{
    #region Variables

    private bool cornerDetected;
    private Transform corner;

    private bool boxDetected;
    private bool handOnBox;
    private bool isPulling;
    private Transform boxTransform;
    private Vector3 boxNearEdge;
    private DistanceJoint2D pullBoxJoint2D;

    private Transform machineButton;
    private bool machineBtnClickEnd = true;

    private bool ropeDetected;
    private Transform ropeTransform;
    private bool lockClimbFromRope = false;
    private bool zigzagHandMoveOnRope = true;
    public RelativeJoint2D relativeJoint2D;
    private RopeNode perviousNode,nextNode;
    private int lastRopeTag;
    private int pullRopeInput;
    #endregion

    #region Rope Methods
    public void setHandIKOnRope()
    {
        boyCollider.SetActive(false);
        if (zigzagHandMoveOnRope)
        {
            leftHandIK.position = nextNode ? nextNode.transform.position : ropeTransform.position;
            rightHandIK.position = perviousNode ? perviousNode.transform.position : ropeTransform.position;
        }
        else
        {
            leftHandIK.position = perviousNode ? perviousNode.transform.position : ropeTransform.position; 
            rightHandIK.position = nextNode ? nextNode.transform.position : ropeTransform.position;
        }

        if (pullRopeInput != 0)
        {
            if (pullRopeInput > 0)
            {
                leftFootIK.position = rightFootIK.position = transform.position + new Vector3(5, -5, 0);
            }
            else
            {
                leftFootIK.position = rightFootIK.position = transform.position + new Vector3(-2, -6, 0);
            }
            pullRopeInput = 0;
        }
        //leftFootIK.position = rightFootIK.position = pos.position;
        //neckIK.position = new Vector3(ropeTransform.position.x, ropeTransform.position.y + 3, 0);
        //bodyTransform.Rotate(new Vector3(0, 0, 1), 10);
    }
    
    private void detectRopeAction()
    {
        ropeDetected = false;
        foreach (var d in detectibles)
        {
            if (d.tag == "Rope")
            {
                ropeTransform = d.transform;
                ropeDetected = true;
                break;
            }
        }
        if (ropeDetected)
        {
            float dis = Vector3.Distance(transform.position, ropeTransform.position);

            if (dis < 5f)
            {
                int ropeTag = ropeTransform.GetComponent<RopeNode>().ropeTagNumber;
                if (ropeTag == lastRopeTag)
                    return;

                lastRopeTag = ropeTag;
                nextNode = perviousNode = null;
                rightHandCCD.damping = leftHandCCD.damping = 0.2f;

                Vector2 offset = relativeJoint2D.linearOffset;

                if (ropeTransform.position.x < transform.position.x)
                    offset.x = -Mathf.Abs(offset.x);
                else
                    offset.x = Mathf.Abs(offset.x);

                rigidBody2D.mass = 0.001f;
                rightFootCCD.damping = leftFootCCD.damping = 0.02f;

                relativeJoint2D.enabled = true;
                relativeJoint2D.linearOffset = offset;
                relativeJoint2D.connectedBody = ropeTransform.GetComponent<Rigidbody2D>();
                animator.SetBool("RopeIdle", true);
                state = State.GRABED_ROPE;
            }
        }
        else
            lastRopeTag = 0;
    }
    
    private void ropeInputAction()
    {
        if (jumpBtn)
        {
            jumpFromRope();
        }
        else if (_moveY != 0)
        {
            if (lockClimbFromRope)
                return;
            if (_moveY > 0.5f)
                climbToRopeNode(ropeTransform.GetComponent<RopeNode>().perviuosNode);
            else if (_moveY < -0.5f)
                climbToRopeNode(ropeTransform.GetComponent<RopeNode>().nextNode);
        }
        else if (_moveX != 0)
        {
            forceToRope();
        }
    }

    private void jumpFromRope()
    {
        relativeJoint2D.enabled = false;
        state = State.IDLE;
        boyCollider.SetActive(true);
        animator.SetBool("RopeIdle" ,false);

        rigidBody2D.mass = defaultMass;
        rightFootCCD.damping = leftFootCCD.damping = defaultFootsDamping;

        if (_moveX > 0)
        {
            rigidBody2D.AddForce(new Vector2(300, 200),ForceMode2D.Impulse);
            // rigidBody2D.velocity = new Vector2(rigidBody2D.velocity.x + 20, rigidBody2D.velocity.y + 20);
        }
        else
        {
            rigidBody2D.AddForce(new Vector2(-300, 200), ForceMode2D.Impulse);
           // rigidBody2D.velocity = new Vector2(rigidBody2D.velocity.x - 20, rigidBody2D.velocity.y + 20);
        }
    }

    private void climbToRopeNode(RopeNode rn)
    {
        if (!rn) return;
        lockClimbFromRope = true;
        perviousNode = rn.perviuosNode;
        nextNode = rn.nextNode;
        ropeTransform = rn.transform;
        relativeJoint2D.connectedBody = rn.GetComponent<Rigidbody2D>();
        zigzagHandMoveOnRope = !zigzagHandMoveOnRope;
        Invoke("unlockClimbFromRope", 0.4f);
    }

    private void forceToRope()
    {
        pullRopeInput = (_moveX > 0) ? 1 : -1;
        ropeTransform.GetComponent<Rigidbody2D>().AddForce(new Vector2(_moveX , 0),ForceMode2D.Impulse);
    }
    
    public void unlockClimbFromRope()
    {
        lockClimbFromRope = false;
    }
    #endregion

    #region Box Methods
    public void setHandIKOnBox()
    {
        if (!isFront(boxTransform.position))
        {
            if (btn2.visible)
                btn2.visible = false;

            state = State.IDLE;
            return;
        }

        leftHandIK.position = boxNearEdge;
        rightHandIK.position = boxNearEdge;

        if (state == State.PULLING_BOX && isPulling)
        {
            bodyTransform.Rotate(new Vector3(0, 0, 1), 15f);
            neckIK.position = boxTransform.position;
        }
    }
    private void connectBoxJoint(bool en)
    {
        pullBoxJoint2D.enabled = en;
    }

    public void detectBoxesAction()
    {
        if (isDetected(ref boxTransform, "Box"))
        {
            findBoxNearEdge();

            if (!btn2.visible)
                btn2.visible = true;

            state = State.NEAR_BOX;
        }
        else
        {
            if (handOnBox)
            {
                if (state == State.NEAR_BOX)
                {
                    state = State.IDLE;
                }
            }
        }
    }
    private void findBoxNearEdge()
    {
        Vector3 v = transform.position;
        v.y += 1.3f;
        RaycastHit2D r = Physics2D.Linecast(v, boxTransform.position,
            1 << LayerMask.NameToLayer("Detectible"));

        Debug.DrawLine(v, r.point, Color.cyan);
        boxNearEdge = r.point;
        handOnBox = true;
    }
    #endregion

    #region Corner Methods

    /// <summary> 
    /// Called in BoyController.LateUpdate() method.
    /// when character detect near corner , he try connect hands 
    /// on detected corner with this method.
    /// </summary>
    private void setHandIKOnCorner()
    {
        if (characterGroundChecker.isGround || !isFront(corner.position))
            return;

        neckIK.position = corner.position;
        leftHandIK.position = corner.position;
        rightHandIK.position = corner.position;
    }


    /// <summary>
    /// Called in BoyController.Update() method.
    /// provice sticking on corner . this method stick character on corner.
    /// called when state == GRABED_CORNER
    /// </summary>
    public void stickOnCornerAction()
    {
        if (rigidBody2D.isKinematic)
        {
            return;
        }
        
        if (transform.position.y < corner.GetChild(0).position.y)
        {
            transform.position = Vector3.Lerp(corner.GetChild(0).position, transform.position, Time.deltaTime * 0.001f);

            if (Vector3.Distance(corner.GetChild(0).position, transform.position) < 0.001f)
            {
                rigidBody2D.isKinematic = true;
            }
        }
    }
    private void cornerClimbUpDownInputAction()
    {
        if (jumpBtn)
        {
            state = State.CLIMBING_UP_FROM_EDGE;
        }
        else if (_moveY < -0.5f)
        {
            state = State.CLIMBING_DOWN_FROM_EDGE;
        }
    }

    /// <summary>
    /// Called in BoyController.Update() method.
    /// called when state == IDLE , GRABING_EDGE , NEAR_CORNER ,CORNER_DETECTED
    /// </summary>
    public bool detectCornersAction()
    {
        cornerDetected = false;
        foreach (var d in detectibles)
        {
            if (d.tag == "Corner")
            {
                corner = d.transform;
                cornerDetected = true;
                break;
            }
        }
        bool grabCorner = false;

        if (cornerDetected)
        {
            float dis = Vector2.Distance(handTransform.position, corner.position);

            if (dis < 0.7f)
            {
                state = State.GRABED_CORNER;
                grabCorner = true;
            }
            else if (dis < 2)
            {
                state = State.NEAR_CORNER;
            }
            else
            {
                state = State.CORNER_DETECTED;
            }
        }
        else
            state = State.IDLE;

        animator.SetBool("GrabEdge", grabCorner);
        return cornerDetected;
    }
    private void climbDownAction()
    {
        rigidBody2D.isKinematic = false;
        animator.SetBool("GrabEdge", false);
        if (_isGround)
            state = State.IDLE;
    }

    private void climbUpAction()
    {
        if (!animator.GetCurrentAnimatorStateInfo(0).IsName("ClimbUpFinish"))
        {
            animator.SetBool("ClimbingUp", true);
            //animator.SetBool("GrabEdge", false);
        }
        else
        {
            transform.position = bodyTransform.position;
            bodyTransform.localPosition = Vector3.zero;
            animator.SetBool("ClimbingUp", false);
            animator.SetBool("GrabEdge", false);

            rigidBody2D.isKinematic = false;
            state = State.IDLE;
        }
    }

    #endregion
    
    #region Machine Methods

    private void setHandIKOnButton()
    {
        if (machineButton != null && isFront(machineButton.position))
        {
            if (Input.GetKey(KeyCode.LeftShift))
            {
                machineBtnClickEnd = false;
                rightHandIK.position = machineButton.position;
                Invoke("cancleClickingMachineBtn", 0.5f);
            }
            else
            {
                if (!machineBtnClickEnd)
                    rightHandIK.position = machineButton.position;
            }
        }
    }



    public void cancleClickingMachineBtn()
    {
        machineBtnClickEnd = true;
    }

    #endregion

}
