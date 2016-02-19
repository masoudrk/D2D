using UnityEngine;
using System.Collections;

public partial class BoyController : MonoBehaviour
{

    public int circleRadius = 5;
    public int distanceRay = 0;
    public Vector2 dirRay;

    Collider2D[] edges;
    Transform edge;
    
    public void detectEdges()
    {
        edges = Physics2D.OverlapCircleAll(transform.position,
            10, 1 << LayerMask.NameToLayer("Corner"));

        bool grabEdge = false;
        bool _lockFliping = false;

        if (edges != null && edges.Length > 0)
        {
            edge = edges[0].transform;
            float dis = Vector2.Distance(handTransform.position, edge.position);

            if (dis < 0.7f)
            {
                state = State.GRABED_EDGE;
                grabEdge = true;
                _lockFliping = true;
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

        lockFliping = _lockFliping;
        animator.SetBool("GrabEdge", grabEdge);
    }

    public void comeToEdge()
    {
        //Vector2 min = handTransform.position - transform.position;
        //transform.position = edgePos - min;
        //handTransform.position = edgePos;

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

    private void setHandIKOnEdge()
    {
        if (characterGroundChecker.isGround || !isFront(edge.position))
            return;

        neckIK.position = edge.position;
        leftHandIK.position = edge.position;//Vector3.Lerp(edgePos, leftHandIK.position, Time.deltaTime);
        rightHandIK.position = edge.position; //Vector3.Lerp(edgePos, rightHandIK.position, Time.deltaTime);
    }

    /*
    private void stickHandOnEdge()
    {
        var _edges = Physics2D.CircleCastAll(transform.position, circleRadius, dirRay, distanceRay,
            1 << LayerMask.NameToLayer("Ground"));

        if (_edges != null && _edges.Length > 0)
        {
            foreach (var e in _edges)
            {
                //float dis = Vector2.Distance(handTransform.position, edgePos);

                Debug.DrawLine(handTransform.position, e.point, Color.cyan);
            }
        }
    }*/

}
