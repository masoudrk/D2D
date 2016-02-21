using UnityEngine;
using System.Collections;
using System.Linq;

public partial class BoyController
{
    private bool cornerDetected;
    private Transform edge;

    private bool boxDetected;
    private bool handOnBox;
    private Transform box;
    private Vector3 boxNearEdge;

    private Transform machineButton;
    private bool machineBtnClickEnd = true;
    /// <summary> 
    /// Called in BoyController.LateUpdate() method.
    /// when character detect near edge , he try connect hands 
    /// on detected edge with this method.
    /// </summary>
    private void setHandIKOnEdge()
    {
        if (characterGroundChecker.isGround || !isFront(edge.position))
            return;

        neckIK.position = edge.position;
        leftHandIK.position = edge.position;
        rightHandIK.position = edge.position; 
    }

    public void setHandIKOnBox()
    {
        if (!isFront(box.position))
        {
            state = State.IDLE;
            return;
        }

        leftHandIK.position = boxNearEdge;
        rightHandIK.position = boxNearEdge;
    }

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

    /// <summary>
    /// Called in BoyController.Update() method.
    /// called when state == IDLE , GRABING_EDGE , NEAR_EDGE ,EDGE_DETECTED
    /// </summary>
    public bool detectCornersAction()
    {
        cornerDetected = false;
        foreach (var d in detectibles)
        {
            if (d.tag == "Corner")
            {
                edge = d.transform;
                cornerDetected = true;
                break;
            }
        }
        bool grabEdge = false;

        if (cornerDetected)
        {
            float dis = Vector2.Distance(handTransform.position, edge.position);

            if (dis < 0.7f)
            {
                state = State.GRABED_EDGE;
                grabEdge = true;
            }
            else if (dis < 2)
            {
                state = State.NEAR_EDGE;
            }
            else
            {
                state = State.EDGE_DETECTED;
            }
        }
        else
            state = State.IDLE;
        
        animator.SetBool("GrabEdge", grabEdge);
        return cornerDetected;
    }

    public void detectBoxesAction()
    {
        boxDetected = false;
        foreach (var d in detectibles)
        {
            if (d.tag == "Box")
            {
                box = d.transform;
                boxDetected = true;
                break;
            }
        }

        if (boxDetected)
        {
            Vector3 v = transform.position;
            v.y += 1.3f;
            RaycastHit2D r = Physics2D.Linecast(v, box.position,
                1 << LayerMask.NameToLayer("Detectible"));

            Debug.DrawLine(v, r.point, Color.cyan);
            boxNearEdge = r.point;
            handOnBox = true;
            state = State.PUSHING_BOX;
        }
        else if (handOnBox)
        {
            if(state == State.PUSHING_BOX)
                state = State.IDLE;
        }
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
            animator.SetBool("GrabEdge", false);
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
    public void cancleClickingMachineBtn()
    {
        machineBtnClickEnd = true;
    }

    /// <summary>
    /// Called in BoyController.Update() method.
    /// provice sticking on edge . this method stick character on edge.
    /// called when state == GRABED_EDGE
    /// </summary>
    public void stickOnEdgeAction()
    {
        if (rigidBody2D.isKinematic)
            return;

        if (transform.position.y < edge.GetChild(0).position.y)
        {
            transform.position = Vector3.Lerp(edge.GetChild(0).position, transform.position , Time.deltaTime * 0.001f);

            if (Vector3.Distance(edge.GetChild(0).position, transform.position) < 0.001f)
            {
                rigidBody2D.isKinematic = true;
            }
        }
    }
}
