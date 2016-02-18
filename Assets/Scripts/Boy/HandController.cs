using UnityEngine;
using System.Collections;

public partial class BoyController : MonoBehaviour
{

    public int circleRadius = 5;
    public int distanceRay = 0;
    public Vector2 dirRay;

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
    }

    Collider2D[] edges;
    Vector2 edgePos;

    public void detectEdges()
    {
        edges = Physics2D.OverlapCircleAll(transform.position,
            10, 1 << LayerMask.NameToLayer("Corner"));

        if (edges != null && edges.Length > 0)
        {
            edgePos = edges[0].transform.position;
            float dis = Vector2.Distance(handTransform.position, edgePos);
            print(dis);
            if (dis < 1)
                state = State.GRABED_EDGE;
            else if (dis < 2)
                state = State.NEAR_EDGE;
            else
                state = State.EDGE_DETECTED;
        }
        else
            state = State.IDLE;
    }

    public void comeToEdge()
    {/*
        Vector2 min = edgePos - new Vector2(handTransform.position.x , handTransform.position.y) ;
        Vector3 lerp = Vector2.Lerp(Vector2.zero, min, Time.deltaTime * 40);
        transform.position += lerp;*/
        Vector2 min = handTransform.position - transform.position;
        transform.position = edgePos + min;
    }

}
