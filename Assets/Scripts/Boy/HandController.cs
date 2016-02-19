using UnityEngine;
using System.Collections;

public partial class BoyController 
{
    Collider2D[] edges;
    Transform edge;
    public float detectEdgeCirleOffsetY = 1;
    public float detectEdgeCirleRadius = 5;
    private Vector3 detectEdgeCirle;
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
    public void detectEdgesAction()
    {
        detectEdgeCirle = transform.position;
        detectEdgeCirle.y += detectEdgeCirleOffsetY;
        edges = Physics2D.OverlapCircleAll(detectEdgeCirle,
            detectEdgeCirleRadius, 1 << LayerMask.NameToLayer("Corner"));

        bool grabEdge = false;

        if (edges != null && edges.Length > 0)
        {
            edge = edges[0].transform;
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
