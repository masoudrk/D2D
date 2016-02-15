using UnityEngine;
using System.Collections;

public class RopeClimbBehaviour : MonoBehaviour {

    public HumanController humanController;
    public CharacterGroundChecker characterGroundChecker;
    public Transform boyTransform;
    public Animator animator;
    public Rigidbody2D rigidBody;

    RopeNode connectedRopeNode;
    public bool sticked;

    RopeSystem lastRopeSystem;

    public float climbStageTime;
    public float lerpPosition;

    void Start () {
        sticked = false;
	}

    void Update()
    {
        if (sticked)
        {
            float moveX = Input.GetAxis("Horizontal");
            float moveRawX = Input.GetAxisRaw("Horizontal");
            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
            {
                if (moveRawX != 0)
                {
                    animator.SetBool("ClimbRope", false);
                    boyTransform.parent = null;
                    if (moveRawX > 0)
                    {
                        rigidBody.velocity += new Vector2(15, 15);
                        if (humanController.flipFacing)
                            humanController.flipFace();
                    }
                    else
                    {
                        rigidBody.velocity += new Vector2(-15, 15);
                        humanController.flipFace();
                    }
                    rigidBody.isKinematic = false;
                    connectedRopeNode.GetComponent<Rigidbody2D>().mass -= rigidBody.mass;
                    sticked = false;

                    if (lastRopeSystem)
                    {
                        lastRopeSystem.enable = false;
                        Invoke("reEnableLastRope", 1f);
                    }
                    return;
                }
                else
                    climbUp();
            }
            else if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
            {
                climbDown();
            }

            if (moveX != 0)
                forceToRope(moveX);

            Vector3 offset = transform.position - boyTransform.position;
            Vector3 charFinalPos = connectedRopeNode.transform.position - offset;

            Vector2 v = Vector2.Lerp(boyTransform.position, charFinalPos, Time.deltaTime * 25);
            boyTransform.rotation = Quaternion.RotateTowards(boyTransform.rotation 
                , connectedRopeNode.transform.rotation , Time.deltaTime * 25);

            if (Vector2.Distance(v, boyTransform.position) > 0.005f)
            {
                boyTransform.position = v;
            }
            animator.SetFloat("VelocityX", connectedRopeNode.GetComponent<Rigidbody2D>().velocity.x);
        }
    }
    
    void reEnableLastRope()
    {
        lastRopeSystem.enable = true;
    }

     void OnTriggerEnter2D(Collider2D col)
    {
        if (col.tag == "Rope" && !sticked)
        {
            if(connectedRopeNode)
                lastRopeSystem = connectedRopeNode.GetComponentInParent<RopeSystem>();

            connectedRopeNode = col.GetComponent<RopeNode>();

            if (!connectedRopeNode.GetComponentInParent<RopeSystem>().enable)
                return;

            connectedRopeNode.GetComponent<Rigidbody2D>().mass += rigidBody.mass;
            forceToRope(rigidBody.velocity.x );
            
            boyTransform.parent = connectedRopeNode.transform;
            lerpPosition = 0;
            rigidBody.isKinematic = true;
            sticked = true;
            animator.SetBool("ClimbRope", true);

            if(humanController.pushing){
                humanController.unPushObject();
            }
        }
    }

    public void climbUp()
    {
        if (animator.GetInteger("ClimbRopeDirection") != 1 && connectedRopeNode.perviuosNode)
        {
            connectedRopeNode.GetComponent<Rigidbody2D>().mass -= rigidBody.mass;
            connectedRopeNode = connectedRopeNode.perviuosNode;
            connectedRopeNode.GetComponent<Rigidbody2D>().mass += rigidBody.mass;
            boyTransform.parent = connectedRopeNode.transform;

            animator.SetInteger("ClimbRopeDirection", 1);
        }
    }
    public void climbDown()
    {
        if (animator.GetInteger("ClimbRopeDirection") != -1 && connectedRopeNode.nextNode)
        {
            connectedRopeNode.GetComponent<Rigidbody2D>().mass -= rigidBody.mass;
            connectedRopeNode = connectedRopeNode.nextNode;
            connectedRopeNode.GetComponent<Rigidbody2D>().mass += rigidBody.mass;
            boyTransform.parent = connectedRopeNode.transform;
            animator.SetInteger("ClimbRopeDirection", -1);
        }
    }

    public void forceToRope(float moveX)
    {
        Vector2 s = connectedRopeNode.GetComponent<Rigidbody2D>().velocity;
        if (s.x <5 && s.x > -5)
            connectedRopeNode.GetComponent<Rigidbody2D>().velocity += new Vector2(moveX * Time.deltaTime * 5, 0);
    }
}
