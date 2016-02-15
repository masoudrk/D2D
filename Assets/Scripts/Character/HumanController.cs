using UnityEngine;
using System.Collections;
using System;
using UnityEngine.Events;

public class HumanController : MonoBehaviour
{
    public enum State
    {
        PUSHING, CLIMBING_ROPE, GRAB_EDGE
    }

    State state;

    public bool debugMode;
    public CameraStress mainCamera;

    [Range(0, 20)]
    public float pushableEnhancedRadius;
    [Range(-20, 20)]
    public float offsetEnhancedY;
    [Range(-20, 20)]
    public float offsetEnhancedX;
    [Range(0, 100, order = 1)]
    public float minKillingDamage;

    [Range(10, 25000)]
    public float maxSpeedX;
    public bool flipFacing = false;
    bool dead;

    Animator animator;
    Rigidbody2D rigidBody;
    Vector2 velocity;

    public CharacterGroundChecker characterGroundChecker;
    public GrabEdgeBehaviour grabEdgeBehaviour;
    public RopeClimbBehaviour ropeClimbBehaviour;

    public bool pushing = false;
    public bool carryObject = false;
    public bool lockMovements = false;
    DistanceJoint2D pushableBoxJoint;

    public Transform transformHand;

    public GameObject[] bodyParts;
    public GameObject rockPrefab;
    public GameObject eyes;

    float moveX, absMoveX;

    void Awake()
    {
        rigidBody = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }
    public void FixedUpdate()
    {
        if (characterGroundChecker.isGround && !lockMovements)
        {

            if (!pushing)
                rigidBody.velocity = new Vector2(maxSpeedX * moveX, rigidBody.velocity.y);
            else
                rigidBody.velocity = new Vector2(maxSpeedX * moveX / 3, rigidBody.velocity.y);
        }
    }

    void Update()
    {
        /*
#if !UNITY_ANDROID
        moveX = Input.GetAxis("Horizontal");
#else
        */
        moveX = Input.GetAxis("Horizontal");
        absMoveX = Mathf.Abs(moveX);

        if (absMoveX < 0.2f && !ropeClimbBehaviour.sticked)
        {
            if (!mainCamera.IsInvoking("setIsIdleTrue") && !mainCamera.isIdle)
                mainCamera.Invoke("setIsIdleTrue", 1);
        }
        else
        {
            if (mainCamera.isIdle)
            {
                mainCamera.CancelInvoke("setIsIdleTrue");
                mainCamera.setIsIdleFalse();
            }
        }

        if (characterGroundChecker.isGround && !lockMovements)
        {
            bool shift = Input.GetKey(KeyCode.LeftShift);

            if ((moveX > 0 && flipFacing) || (moveX < 0 && !flipFacing))
            {
                if (pushing)
                {
                    if (!shift)
                    {
                        unPushObject();
                        flipFace();
                    }
                }
                else
                    flipFace();
            }

            if (characterGroundChecker.canPicking && !carryObject && shift && !pushing)
            {
                pickingObject();
            }
            else if (carryObject && shift)
            {
                throwingObject();
            }
        }

        checkPushableObject(moveX);

        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
        {
            Jump();
        }
        /*
        float velocityX = Mathf.Abs(rigidBody.velocity.x);
        
        if (!ropeClimbBehaviour.sticked)
            animator.SetFloat("VelocityX", velocityX);
            */

        if (!ropeClimbBehaviour.sticked)
            animator.SetFloat("VelocityX", maxSpeedX * absMoveX);

        if (absMoveX > 0.5f)
            animator.SetFloat("AnimVelocityX", absMoveX);
        else
            animator.SetFloat("AnimVelocityX", 0.1f);

        animator.SetFloat("VelocityY", rigidBody.velocity.y);
        animator.SetBool("IsGround", characterGroundChecker.isGround);


#if UNITY_ANDROID
        /*
        if(moveX > 0)
        {
            moveX -= 0.05f;
            if (moveX < 0.03f)
                moveX = 0;
        }
        */
#endif
    }

    public void Jump()
    {
        if (pushing)
        {
            unPushObject();
        }

        if (!grabEdgeBehaviour.grabbed && characterGroundChecker.isGround && !lockMovements)
        {
            if (absMoveX < 0.1f)
            {
                lockMovement();
                animator.SetBool("Jump", true);
            }
            else
            {
                animator.SetTrigger("RunningJump");
                rigidBody.velocity += new Vector2(moveX * maxSpeedX, 18);
            }
        }
    }
    public void startJumping()
    {
        unLockMovement();
        if (characterGroundChecker.isGround)
        {
            animator.SetBool("Jump", false);
            rigidBody.velocity += new Vector2(0, 15);
        }
    }

    private void pickingObject()
    {
        lockMovement();
        animator.SetBool("Picking", true);
    }

    public void pickedObject()
    {
        animator.SetBool("Picking", false);
        GameObject rock = Instantiate(rockPrefab, transformHand.position, Quaternion.identity) as GameObject;
        rock.transform.parent = transformHand;

        float r = UnityEngine.Random.Range(0.7f, 1f);
        rock.transform.localScale = rock.transform.localScale * r;
        rock.transform.GetChild(0).Rotate(new Vector3(0, 0, UnityEngine.Random.Range(0, 360)));

        carryObject = true;
    }


    public void throwingObject()
    {
        lockMovement();
        rigidBody.velocity = new Vector2();
        animator.SetBool("Throwing", true);
    }
    public void throwObjectNow()
    {
        throwObjectNow(30, UnityEngine.Random.Range(4, 7));
    }

    public void throwObjectNow(float vx, float vy)
    {
        animator.SetBool("Throwing", false);
        Rigidbody2D r = transformHand.GetComponentInChildren<Rigidbody2D>();
        r.isKinematic = false;
        r.transform.parent = null;

        if (flipFacing)
        {
            Vector3 s = r.transform.localScale;
            s.x = -s.x;
            r.transform.localScale = s;
            r.velocity = new Vector2(-vx, vy);
        }
        else
        {
            r.velocity = new Vector2(vx, vy);
        }
        carryObject = false;
    }

    public void unLockMovement()
    {
        lockMovements = false;
    }

    public void lockMovement()
    {
        lockMovements = true;
        rigidBody.velocity = new Vector2();
    }

    private void checkPushableObject(float moveX)
    {
        if (!pushing && !ropeClimbBehaviour.sticked)
        {
            pushObject();
        }
        else if (!ropeClimbBehaviour.sticked)
        {
            animator.SetInteger("PushDirection", (moveX > 0) ? 1 : -1);

            float disY = Mathf.Abs(pushableBoxJoint.transform.position.y - transform.position.y);
            if (!forwardWithPushableObject(pushableBoxJoint.transform) || disY > 2.2f)
                unPushObject();
            else
            {
                RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 50, 1 << LayerMask.NameToLayer("Ground"));
                if (hit.collider != null)
                {
                    transform.up = Vector2.Lerp(transform.up, hit.normal, 1f);
                }
            }
        }
    }

    public bool handPermissiveDis()
    {
        Collider2D col = Physics2D.OverlapCircle(transformHand.position, 1, 1 << LayerMask.NameToLayer("PushableObject"));
        if (col)
            return col.tag == "Box";
        return false;
    }
    public void unPushObject()
    {
        animator.SetBool("Carry", false);
        pushableBoxJoint.enabled = false;
        pushing = false;
    }

    private void pushObject()
    {
        Vector3 v = transform.position;
        v.y += offsetEnhancedY;
        v.x += (flipFacing) ? -offsetEnhancedX : offsetEnhancedX;
        Collider2D[] cols = Physics2D.OverlapCircleAll(v, pushableEnhancedRadius, 1 << LayerMask.NameToLayer("PushableObject"));

        if (cols.Length > 0)
        {
            float minDistance = float.MaxValue;
            int selectedObjectIndex = -1;

            for (int i = 0; i < cols.Length; i++)
            {
                if (forwardWithPushableObject(cols[i].transform))
                {
                    float distance = Vector2.Distance(cols[i].transform.position, transform.position);

                    if (distance < minDistance && distance < pushableEnhancedRadius
                        && Mathf.Abs(cols[i].GetComponent<Rigidbody2D>().velocity.y) < 0.2f)
                    {
                        minDistance = distance;
                        selectedObjectIndex = i;
                    }
                }
            }

            if (selectedObjectIndex != -1)
            {
                if (carryObject)
                    throwObjectNow();
                animator.SetBool("Carry", true);
                pushableBoxJoint = cols[selectedObjectIndex].GetComponent<DistanceJoint2D>();
                pushableBoxJoint.enabled = pushing = true;
            }
        }
    }

    private bool forwardWithPushableObject(Transform pushableTransfrom)
    {
        if (transform.position.x > pushableTransfrom.position.x && flipFacing)
        {
            return true;
        }
        else if (transform.position.x < pushableTransfrom.position.x && !flipFacing)
        {
            return true;
        }
        return false;
    }

    public void flipFace()
    {
        flipFacing = !flipFacing;
        Vector3 localScale = transform.localScale;
        localScale.x *= -1;
        transform.localScale = localScale;
    }


    void OnGUI()
    {
        if (!debugMode)
            return;

        int sw3 = Screen.width / 3;

        if (GUI.Button(new Rect(0, 0, sw3, 30), "Reset"))
            Application.LoadLevel("Game");

        if (GUI.RepeatButton(new Rect(sw3, 0, sw3, 30), "Go"))
        {
            if (moveX < 1)
            {
                moveX += 0.1f;
                moveX = Mathf.Clamp01(moveX);
            }
        }

        if (GUI.Button(new Rect(sw3 * 2, 0, sw3, 30), "Jump"))
            Jump();
    }

    void OnDrawGizmos()
    {
        Vector3 v = transform.position;
        v.y += offsetEnhancedY;
        v.x += (flipFacing) ? -offsetEnhancedX : offsetEnhancedX;
        Gizmos.DrawWireSphere(v, pushableEnhancedRadius);
    }


    void Slide()
    {
        animator.SetBool("Slide", true);
    }

    void stopSliding()
    {
        animator.SetBool("Slide", false);
    }

    public void getDamageFromObjects(float mass, Vector2 objectVelocity)
    {
        float damage = (float)(mass / 2) * objectVelocity.magnitude * objectVelocity.magnitude;
        //print("damage : " + damage + "  mass : " + mass + "  velocity " + objectVelocity.magnitude);
        if (damage > minKillingDamage * 500 && !dead && !lockMovements)
        {

            dead = true;
            if (flipFacing)
                flipFace();
            animator.enabled = false;
            rigidBody.AddRelativeForce(new Vector2(-(damage / 100), 0), ForceMode2D.Impulse);
            transform.GetChild(0).gameObject.SetActive(false);
            makeBodySceleton();
        }
    }

    public void getDamageFromFalling()
    {
        float damage = (float)(rigidBody.mass / 2) * rigidBody.velocity.magnitude * rigidBody.velocity.magnitude;

        if (damage > minKillingDamage * 1000 && !dead && !lockMovements)
        {
            //print("damage : " + damage + "  mass : " + rigidBody.mass + "  velocity " + rigidBody.velocity.magnitude);
            dead = true;
            if (flipFacing)
                flipFace();
            animator.enabled = false;
            rigidBody.AddRelativeForce(new Vector2((damage / 100), 0), ForceMode2D.Impulse);
            transform.GetChild(0).gameObject.SetActive(false);
            makeBodySceleton();
        }
    }
    // make body sceleton for dying
    public void makeBodySceleton()
    {
        lockMovement();
        JointMotor2D motor = new JointMotor2D();
        motor.maxMotorTorque = 20;
        motor.motorSpeed = 0;
        JointAngleLimits2D lim = new JointAngleLimits2D();
        Rigidbody2D[] bodyPartsRigidBody = new Rigidbody2D[14];
        HingeJoint2D[] bodyPartsHingeJoint = new HingeJoint2D[14];

        for (int i = 0; i < 14; i++)
        {
            //create body parts rigidbodys
            bodyParts[i].GetComponent<Collider2D>().enabled = true;
            bodyPartsRigidBody[i] = bodyParts[i].AddComponent<Rigidbody2D>();
            if (bodyPartsRigidBody[i] == null)
                print(i);
            bodyPartsRigidBody[i].mass = 10;
            //create body parts joints
            bodyPartsHingeJoint[i] = bodyParts[i].AddComponent<HingeJoint2D>();
            bodyPartsHingeJoint[i].useLimits = true;
            bodyPartsHingeJoint[i].enableCollision = true;
            bodyPartsHingeJoint[i].useMotor = true;
            bodyPartsHingeJoint[i].motor = motor;
        }
        bodyPartsRigidBody[7].mass = 50;
        bodyPartsRigidBody[6].mass = 50;
        //part 1
        bodyParts[0].transform.rotation = Quaternion.identity;
        lim.min = -25;
        lim.max = 25;
        bodyPartsHingeJoint[0].limits = lim;
        bodyPartsHingeJoint[0].connectedBody = rigidBody;
        bodyPartsHingeJoint[0].connectedAnchor = new Vector2(0.41f, -0.112f);

        //part 2
        bodyParts[1].transform.rotation = Quaternion.identity;
        lim.min = 0;
        lim.max = 100;
        bodyPartsHingeJoint[1].limits = lim;
        bodyPartsHingeJoint[1].connectedBody = bodyPartsRigidBody[0];
        bodyPartsHingeJoint[1].connectedAnchor = new Vector2(0.04f, -1.45f);

        //part 3
        lim.min = -10;
        lim.max = 30;
        bodyPartsHingeJoint[2].limits = lim;
        bodyPartsHingeJoint[2].connectedBody = bodyPartsRigidBody[1];
        bodyPartsHingeJoint[2].connectedAnchor = new Vector2(-0.03f, -0.858f);

        //part 4
        bodyParts[3].transform.rotation = Quaternion.identity;
        lim.min = -25;
        lim.max = 25;
        bodyPartsHingeJoint[3].limits = lim;
        bodyPartsHingeJoint[3].connectedBody = rigidBody;
        bodyPartsHingeJoint[3].connectedAnchor = new Vector2(-0.3f, -0.233f);

        //part 5
        bodyParts[4].transform.rotation = Quaternion.identity;
        lim.min = 0;
        lim.max = 100;
        bodyPartsHingeJoint[4].limits = lim;
        bodyPartsHingeJoint[4].connectedBody = bodyPartsRigidBody[3];
        bodyPartsHingeJoint[4].connectedAnchor = new Vector2(-0.399f, -1.36f);

        //part 6
        lim.min = -10;
        lim.max = 30;
        bodyPartsHingeJoint[5].limits = lim;
        bodyPartsHingeJoint[5].connectedBody = bodyPartsRigidBody[4];
        bodyPartsHingeJoint[5].connectedAnchor = new Vector2(-0.024f, -0.92f);

        //part 7
        lim.min = -20;
        lim.max = 20;
        bodyPartsHingeJoint[6].limits = lim;
        bodyPartsHingeJoint[6].connectedBody = rigidBody;
        bodyPartsHingeJoint[6].connectedAnchor = new Vector2(0f, 0f);

        //part 8
        lim.min = -20;
        lim.max = +20;
        bodyPartsHingeJoint[7].limits = lim;
        bodyPartsHingeJoint[7].connectedBody = bodyPartsRigidBody[6];
        bodyPartsHingeJoint[7].connectedAnchor = new Vector2(-0.12f, 2.12f);
        bodyPartsRigidBody[7].AddRelativeForce(new Vector2(-50000, 0));

        //part 9
        bodyParts[8].transform.rotation = Quaternion.identity;
        lim.min = -150;
        lim.max = +20;
        bodyPartsHingeJoint[8].limits = lim;
        bodyPartsHingeJoint[8].connectedBody = bodyPartsRigidBody[6];
        bodyPartsHingeJoint[8].connectedAnchor = new Vector2(0.31f, 1.69f);

        //part 10
        lim.min = -150;
        lim.max = 90;
        bodyPartsHingeJoint[9].limits = lim;
        bodyPartsHingeJoint[9].connectedBody = bodyPartsRigidBody[8];
        bodyPartsHingeJoint[9].connectedAnchor = new Vector2(0.6f, -1.02f);

        //part 11
        lim.min = -10;
        lim.max = 10;
        bodyPartsHingeJoint[10].limits = lim;
        bodyPartsHingeJoint[10].connectedBody = bodyPartsRigidBody[9];
        bodyPartsHingeJoint[10].connectedAnchor = new Vector2(0.549f, -0.369f);

        //part 12
        bodyParts[11].transform.rotation = Quaternion.identity;
        lim.min = -150;
        lim.max = -20;
        bodyPartsHingeJoint[11].limits = lim;
        bodyPartsHingeJoint[11].connectedBody = bodyPartsRigidBody[6];
        bodyPartsHingeJoint[11].connectedAnchor = new Vector2(-0.649f, 1.61f);

        //part 13
        lim.min = -150;
        lim.max = 90;
        bodyPartsHingeJoint[12].limits = lim;
        bodyPartsHingeJoint[12].connectedBody = bodyPartsRigidBody[11];
        bodyPartsHingeJoint[12].connectedAnchor = new Vector2(-1f, -0.97f);

        //part 14
        lim.min = -10;
        lim.max = 10;
        bodyPartsHingeJoint[13].limits = lim;
        bodyPartsHingeJoint[13].connectedBody = bodyPartsRigidBody[12];
        bodyPartsHingeJoint[13].connectedAnchor = new Vector2(0.58f, -0.413f);

        GetComponent<Collider2D>().enabled = false;
        rigidBody.constraints = RigidbodyConstraints2D.None;

        StartCoroutine(Fade());
        Invoke("moveArmsAndLegs", 0.3f);

    }

    void moveArmsAndLegs()
    {
        JointMotor2D motor = new JointMotor2D();
        motor.maxMotorTorque = 20000;

        motor.motorSpeed = -100;
        bodyParts[1].GetComponent<HingeJoint2D>().motor = motor;
        bodyParts[4].GetComponent<HingeJoint2D>().motor = motor;
        bodyParts[1].GetComponent<Rigidbody2D>().AddRelativeForce(new Vector2(5000, 0));
        bodyParts[4].GetComponent<Rigidbody2D>().AddRelativeForce(new Vector2(5000, 0));

        motor.motorSpeed = 100;
        bodyParts[8].GetComponent<HingeJoint2D>().motor = motor;

    }

    IEnumerator Fade()
    {
        for (float f = 1f; f >= 0; f -= 0.08f)
        {
            Color c = eyes.transform.GetChild(0).GetComponent<SpriteRenderer>().material.color;
            c.a = f;
            eyes.transform.GetChild(0).GetComponent<SpriteRenderer>().material.color = c;
            eyes.transform.GetChild(1).GetComponent<SpriteRenderer>().material.color = c;
            yield return new WaitForSeconds(.1f);
        }
    }
}
