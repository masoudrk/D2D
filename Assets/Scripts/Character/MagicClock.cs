using UnityEngine;
using System.Collections;

public class MagicClock : MonoBehaviour {

    Vector3[] clockTransforms;
    int count = 0;

    [Range(0, 10, order = 1)]
    public int MAX = 4;
    public float captureTime = 0.5f;
    Rigidbody2D rigidBody;

    
    void Start()
    {
        count = 0;
        clockTransforms = new Vector3[MAX];
        rigidBody = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (count == 0)
            InvokeRepeating("captureTransform" , captureTime, captureTime);

        if(Input.GetKeyDown(KeyCode.T))
        {
            moveBack();
        }

    }

    void captureTransform()
    {
            if (count < MAX - 1)
            {
                addTransform();
            }
            else
            {
                Vector3 temp = new Vector3();
                temp = clockTransforms[count];
                for (int i = count -1; i >= 0 ; i--)
                {
                    Vector3 t = new Vector3();
                    t = temp;
                    temp = clockTransforms[i];
                    clockTransforms[i] = t;
                }
                addTransform();
            }
    }

    bool addTransform()
    {
        if (count > MAX)
            return false;
        else
        {
            clockTransforms[count] = transform.position;
            if (count < MAX - 1)
                count++;
            return true;
        }
        
    }


    bool moveBack()
    {
        if (count == 0)
            return false;
        else
        {
            if (count == MAX - 1)
            {
                transform.position = clockTransforms[0];
                rigidBody.velocity = Vector2.zero;
                CancelInvoke("captureTransform");
                count = 0;
                return true;
            }
            else
                return false;
        }
    }
}
