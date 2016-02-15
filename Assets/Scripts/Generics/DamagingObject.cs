using UnityEngine;
using System.Collections;

public class DamagingObject : MonoBehaviour {

    [Range(0, 10)]
    public float killingSpeed;
    Vector2 objectSpeed;
    Rigidbody2D rigidBody;
    void Start()
    {
        rigidBody = GetComponent<Rigidbody2D>();
    }
    void Update()
    {
        objectSpeed = rigidBody.velocity;
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if (col.gameObject.tag == "Character")
        {
            if (GetComponent<Rigidbody2D>().velocity.magnitude > killingSpeed)
            {
                col.gameObject.GetComponent<HumanController>()
                .getDamageFromObjects(GetComponent<Rigidbody2D>().mass, objectSpeed);
            }
        }
    }
}
