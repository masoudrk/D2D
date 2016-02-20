using UnityEngine;
using System.Collections;
using System.Linq;

public partial class BoyController
{
    private bool cornerDetected;
    private Transform edge;
    private Transform machineButton;
    private bool machineBtnClickEnd = true;
    private bool boxDetected;
    private Transform box;

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

    /// <summary>
    /// Called in BoyController.Update() method.
    /// called when state == IDLE , GRABING_EDGE , NEAR_EDGE ,EDGE_DETECTED
    /// </summary>
    public void detectCornersAction()
    {
        cornerDetected = false;
        foreach (var d in detectibles.Where(d => d.tag == "Corner"))
        {
            edge = d.transform;
            cornerDetected = true;
            break;
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
    }

    public void detectBoxesAction()
    {
        boxDetected = false;
        foreach (var d in detectibles.Where(d => d.tag == "Corner"))
        {
            box = d.transform;
            cornerDetected = true;
            break;
        }

        if (boxDetected)
        {
            
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
            state = State.IDLE;;
        }
        //rigidBody2D.isKinematic = false;
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
