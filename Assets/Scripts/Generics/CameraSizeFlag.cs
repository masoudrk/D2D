using UnityEngine;
using System.Collections;

public class CameraSizeFlag : MonoBehaviour {

    [Range(12 , 25 , order =1)]
    public float cameraSize; 
    float cameraDefultSize = 18;
    bool ready = true;
    [Range(0, 0.5f)]
    public float changeSizeTime = 0.02f;

    void OnTriggerEnter2D(Collider2D col)
    {
        if(col.tag == "Character" && ready)
        {
            HumanController hc = col.transform.parent.GetComponent<HumanController>();
            if (hc)
            {
                if (!hc.flipFacing)
                {
                    cameraDefultSize = Camera.main.orthographicSize;
                    StartCoroutine(movetToSize(cameraSize));
                    ready = false;
                    Invoke("setReady", 1);
                }
                else
                {
                    StartCoroutine(movetToSize(cameraDefultSize));
                    ready = false;
                    Invoke("setReady", 1);
                }
            }
        }

    }

    void setReady()
    {
        ready = true;
    }

    public IEnumerator movetToSize(float size)
    {
        float part = (size - Camera.main.orthographicSize) / 100;
        if (part > 0)
        {
            for (int i = 0; i < 100 && Camera.main.orthographicSize < size; i++)
            {
                Camera.main.orthographicSize += part;
                yield return new WaitForSeconds(changeSizeTime);
            }
        }
        else
        {
            for (int i = 0; i < 100 && Camera.main.orthographicSize > size; i++)
            {
                Camera.main.orthographicSize += part;
                yield return new WaitForSeconds(changeSizeTime);
            }
        }

    }
}
