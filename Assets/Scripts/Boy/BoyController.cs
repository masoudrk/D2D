

using UnityEngine;
using System.Collections;
using System;

public partial class BoyController : MonoBehaviour
{ 
    public enum State
    {  
        IDLE , EDGE_DETECTED ,NEAR_EDGE , GRABING_EDGE , GRABED_EDGE 
    }

    State state;

    public Transform neckIK;
    public Transform rightHandIK;
    public Transform rightFootIK;
    public Transform leftHandIK;
    public Transform leftFootIK;

    public Transform handTransform;

    public float maxSpeedX;
    public bool flipFacing;

    private Rigidbody2D rigidBody2D;
    private Animator animator;

    void Start()
    {
        state = State.IDLE;
        rigidBody2D = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }


    void Update()
    {
        float moveX = Input.GetAxis("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");

        rigidBody2D.velocity = new Vector2(moveX * maxSpeedX, rigidBody2D.velocity.y);

        if (moveY > 0)
        {
            if (rigidBody2D.velocity.y < 5)
                rigidBody2D.velocity = new Vector2(rigidBody2D.velocity.x, 15);
        }

        if ((moveX > 0 && flipFacing) || (moveX < 0 && !flipFacing))
        {
            flipFace();
        }

        animator.SetFloat("VelocityX", Mathf.Abs(moveX * maxSpeedX));
        animator.SetFloat("VelocityY", rigidBody2D.velocity.y);

        stickHandOnEdge();

        if (state == State.GRABED_EDGE)
        {
            comeToEdge();
        }
        else
        {
            detectEdges();
        }
    }

    public void LateUpdate()
    {
        if (state == State.EDGE_DETECTED || state == State.NEAR_EDGE || state == State.GRABED_EDGE)
        {
            neckIK.position = leftHandIK.position = rightHandIK.position = edgePos;
        }

        if (state == State.NEAR_EDGE)
        {
            Vector3 v = new Vector3(Mathf.Abs(handTransform.position.x - edgePos.x), 3f) + transform.position;
            leftFootIK.position = rightFootIK.position = -v;
        }
    }
    public void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, 10);
        Gizmos.DrawWireSphere(handTransform.position, circleRadius);
    }
    
    public void flipFace()
    {
        flipFacing = !flipFacing;
        Vector3 localScale = transform.localScale;
        localScale.x *= -1;
        transform.localScale = localScale;
    }
}
