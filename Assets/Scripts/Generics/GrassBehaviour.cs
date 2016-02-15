using UnityEngine;
using System.Collections;

public class GrassBehaviour : MonoBehaviour {

    Animator animator;
    bool isRight;
    void Start()
    {
        animator = GetComponent<Animator>();
    }

    // Use this for initialization
    void OnTriggerEnter2D(Collider2D other)
    {
        Rigidbody2D rb;
        if ( rb = other.attachedRigidbody)
            {
            if (rb.velocity.x > 0)
            {
                animator.SetBool("HitRight" ,true);
                isRight = true;
            }
            else 
            {
                animator.SetBool("HitLeft" , true);
                isRight = false;
            }
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        animator.SetBool("HitRight", false);
        animator.SetBool("HitLeft", false);
    }

    void OnTriggerStay2D(Collider2D other)
    {
        if (isRight)
        {
            animator.SetBool("HitRight", true);
        }
        else
        {
            animator.SetBool("HitLeft", true);
        }
    }
}
