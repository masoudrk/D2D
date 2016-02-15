using UnityEngine;
using System.Collections;

public class EndSection : MonoBehaviour {

    Section section;
    bool isTrigged = false;

    public void Start()
    {
        section = GetComponentInParent<Section>();
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (!isTrigged)
        {
            section.endOfSection();
            isTrigged = true;
        }
    }
}
