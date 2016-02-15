using UnityEngine;
using System.Collections;
using System;

public class GrabEdgeBehaviour : MonoBehaviour {


    [System.Serializable]
    public class ClimbPaths
    {
        public float time;
        public Vector3 pathToClimb;
    }

    public ClimbPaths[] climpPaths;
    int lerpingPathNumber;
    float _timeStartedLerping;
    Vector3 _startPosition, _endPosition;

    public bool climbing;
    public bool grabbed;
    public bool grabbing;
    public bool releasing;

    public HumanController humanController;
    public CharacterGroundChecker characterGroundChecker;
    public Transform boyTransform;
    public Animator animator;
    public Rigidbody2D rigidBody; 
    
    Transform nearCornerTransfrom;
    
    void Start () {

	}

    void Update()
    {
        if (!climbing && grabbed)
        {
            if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
            {
                releaseCorner();
            }

            if (!humanController.pushing)
            {
                if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
                {
                    if (grabbed && !climbing)
                    {
                        if (!animator.GetCurrentAnimatorStateInfo(0).IsName("GrabEdge"))
                            return;
                        animator.SetTrigger("ClimbingUp");
                        startClimb();
                        return;
                    }
                }
            }

            Vector3 offset = transform.position - boyTransform.position;
            Vector3 charFinalPos = nearCornerTransfrom.position - offset;

            boyTransform.transform.position = charFinalPos;
        }
    }
    
    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.tag == "Corner" && !climbing && !grabbed && !releasing)
        {
            grabbed = true;
            nearCornerTransfrom = col.gameObject.transform;
            animator.SetTrigger("GrabEdge");
            rigidBody.isKinematic = true;
        }
        else if (releasing)
        {
            releasing = false;
        }
    }
    
    void FixedUpdate()
    {
        if (climbing)
        {
            float timeSinceStarted1 = Time.time - _timeStartedLerping;
            float percentageComplete1 = timeSinceStarted1 / climpPaths[lerpingPathNumber].time;

            boyTransform.position = Vector3.Lerp(_startPosition, _endPosition, percentageComplete1);

            if (percentageComplete1 >= 1.0f)
            {
                lerpingPathNumber++;
                if (lerpingPathNumber == climpPaths.Length)
                {
                    climbFinished();
                }
                else
                    prepareLerping();
            }
        }
    }

    private void climbFinished()
    {
        climbing = false;
        grabbed = false;
        rigidBody.isKinematic = false;
        gameObject.GetComponent<CircleCollider2D>().enabled = true;
    }

    void prepareLerping()
    {
        _timeStartedLerping = Time.time;

        _startPosition = boyTransform.position;
        if (boyTransform.position.x > nearCornerTransfrom.position.x)
        {
            Vector3 v = climpPaths[lerpingPathNumber].pathToClimb;
            v.x = -v.x;
            _endPosition = boyTransform.position + v;
        }
        else
            _endPosition = boyTransform.position + climpPaths[lerpingPathNumber].pathToClimb;
    }

    public void startClimb()
    {
        lerpingPathNumber = 0;
        prepareLerping();
        gameObject.GetComponent<CircleCollider2D>().enabled = false;
        climbing = true;
    }

    public void releaseCorner()
    {
        if (!climbing)
        {
            rigidBody.isKinematic = false;
            animator.SetTrigger("ReleaseEdge");
            grabbed = false;
            releasing = true;
        }
    }
}
