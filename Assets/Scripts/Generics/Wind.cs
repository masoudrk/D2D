using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Wind : MonoBehaviour
{
    [Range(0,1)]
    public float variety;
    public Vector2 Force = Vector2.zero;
    [Range (0,5)]
    public float threshold;
    Vector2 finalForce;
    private List <Collider2D> objects = new List <Collider2D>();

    void Start()
    {
        finalForce = Force;
    }
    void FixedUpdate()
    {

        for (int i = 0; i < objects.Count; i++)
        {
            Rigidbody2D body = objects[i].attachedRigidbody;
            if (Vector2.Distance(finalForce, Force) < threshold)
                finalForce = Force * variety * ((Random.Range(0, 2) > 1) ? 1 : -1) + Force;
            if(body)
                body.AddForce(Force);
        }
        Vector2.Lerp(Force, finalForce, 0.2f);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        objects.Add(other);
    }

    void OnTriggerExit2D(Collider2D other)
    {
        objects.Remove(other);
    }
}