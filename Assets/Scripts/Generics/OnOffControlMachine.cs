using UnityEngine;
using System.Collections;

public class OnOffControlMachine : MonoBehaviour {


    public bool outPut;
    bool ready;
    [Range(0, 10)]
    public float timeOffset;
    Animator animator;
    Transform timingLamp;
    void Start()
    {
        animator = GetComponent<Animator>();
        timingLamp = transform.GetChild(0);
    }

    void Update()
    {
        if (ready && Input.GetKeyDown(KeyCode.LeftShift))
        {
            if(!outPut)
            {
                if (timeOffset != 0)
                {
                    if(!IsInvoking("startCount"))
                    {
                        timingLamp.gameObject.SetActive(true);
                        InvokeRepeating("startCount", 0, timeOffset / 100);
                    }
                }
                else
                {
                    timingLamp.gameObject.SetActive(true);
                    turnOnTheMachine();
                }
                
            }
            else
            {
                timingLamp.gameObject.SetActive(false);
                outPut = false;
                animator.SetBool("On", outPut);
            }
        }

        if (timingLamp.localScale.y <= 0 )
        {
            timingLamp.localScale = new Vector3(1, 1, 1);
            CancelInvoke("startCount");
            timingLamp.gameObject.SetActive(false);
            turnOnTheMachine();
        }
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.tag == "Character")
        {
            ready = true;
        }
    }


    void OnTriggerStay2D(Collider2D col)
    {
        if (col.tag == "Character")
        {
            ready = true;
        }
    }

    void OnTriggerExit2D(Collider2D col)
    {
        if (col.tag == "Character")
        {
            ready = false;
        }
    }
    void turnOnTheMachine()
    {
        outPut = true;
        animator.SetBool("On", outPut);
        
    }

    public void startCount()
    {
        timingLamp.localScale += new Vector3(0, -0.01f, 0);
    }
}
