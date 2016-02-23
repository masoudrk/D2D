using UnityEngine;
using System.Collections;
[ExecuteInEditMode]
public class ControlMachine : MonoBehaviour {

    public bool outPut;
    bool ready;
    [Range(0,10)]
    public float timeOffset;
    [Range(0, 20)]
    public float durationTime;
    Animator animator;
    Transform timingLamp;
    public Sprite[] numbers;
    public SpriteRenderer panelSprite;
    private int i;
    void Start()
    {
        animator = GetComponent<Animator>();
        timingLamp = transform.GetChild(0);
        int panelNum = (int) timeOffset;
        panelSprite.sprite = numbers[panelNum];
        i = panelNum;
    }

    void Update()
    {
        if(ready && Input.GetKeyDown(KeyCode.LeftShift) && !outPut)
        {

            if (!IsInvoking("turnOnTheMachine"))
            {
                Invoke("turnOnTheMachine", timeOffset);
                StartCoroutine(CountDown(1f));
            }
        }

        if(timingLamp.localScale.x <= 0)
        {
            outPut = false;
            animator.SetBool("On", outPut);
            i = (int)timeOffset;
            panelSprite.sprite = numbers[i];
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

    IEnumerator CountDown(float waitTime)
    {
        while (i > 0)
        {
            panelSprite.sprite = numbers[--i];
            yield return new WaitForSeconds(waitTime);
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
