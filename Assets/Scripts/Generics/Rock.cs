using UnityEngine;
using System.Collections;

public class Rock : MonoBehaviour {

    public float lifeTime;
	void Start ()
    {
        if(lifeTime > 0)
            Destroy(gameObject,lifeTime);
    }
}
