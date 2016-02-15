using UnityEngine;
using System.Collections;

public class SurfaceEffector : MonoBehaviour {
    [Range(-15, 15, order = 1)]
    public float speed;
    Rigidbody2D rigidBody;
    void OnTriggerEnter2D(Collider2D col)
    {

        if (col.tag == "Character")
        {

            if (rigidBody = col.transform.parent.gameObject.GetComponent<Rigidbody2D>())
            {
                speedUp();
            }

            col.transform.parent.SendMessage("Slide");

        }
    }
    void OnTriggerStay2D(Collider2D col)
    {
        if (col.tag == "Character")
        {
            if (rigidBody.velocity.magnitude < 40)
                speedUp();
            col.transform.parent.SendMessage("Slide");
        }
    }

    void OnTriggerExit2D(Collider2D col)
    {

        if (col.tag == "Character")
        {
            col.transform.parent.SendMessage("stopSliding");
        }
    }

    void speedUp()
    {
        Vector3 newFwd = new Vector3(Mathf.Cos(transform.rotation.eulerAngles.z * Mathf.Deg2Rad),
            Mathf.Sin(transform.rotation.eulerAngles.z * Mathf.Deg2Rad), 0);
        rigidBody.velocity += new Vector2(newFwd.x, newFwd.y) * speed;
    }
}
