using UnityEngine;
using System.Collections;

public class ControlMachine : MonoBehaviour {

    public bool outPut;
    bool ready;
    [Range(0,10)]
    public float timeOffset;
    [Range(0, 20)]
    public float durationTime;
    Animator animator;
    Transform timingLamp;
    void Start()
    {
        animator = GetComponent<Animator>();
        timingLamp = transform.GetChild(0);
    }

    void Update()
    {
        if(ready && Input.GetKeyDown(KeyCode.LeftShift) && !outPut)
        {

                if(!IsInvoking("turnOnTheMachine"))
                    Invoke("turnOnTheMachine", timeOffset);
        }

        if(timingLamp.localScale.x <= 0)
        {
            outPut = false;
            animator.SetBool("On", outPut);
            timingLamp.localScale = new Vector3(1, 1, 1);
            CancelInvoke("startCount");
            timingLamp.gameObject.SetActive(false);
        }
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if(col.tag == "Character")
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
        timingLamp.gameObject.SetActive(true);
        animator.SetBool("On", outPut);
        InvokeRepeating("startCount", 0, durationTime / 100);
    }

    public void startCount()
    {
        timingLamp.localScale += new Vector3(-0.01f,0,0 );
    }
}
