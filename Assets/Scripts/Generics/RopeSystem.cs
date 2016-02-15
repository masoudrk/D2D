using UnityEngine;
using System.Collections;

public class RopeSystem : MonoBehaviour {

    public bool enable;


    /*
    [Header("Rope Settings")]
    public int nodesCount;
    public Sprite sprite;

    public GameObject ropePrefab;

	void Start ()
    {
        Rigidbody2D connectedBody = null;
        JointMotor2D m = new JointMotor2D();
        m.maxMotorTorque = 1;

        for (int i = 0; i < nodesCount; i++)
        {
            GameObject obj = new GameObject("Node " + i);//Instantiate(ropePrefab, transform.position, Quaternion.identity) as GameObject;
            obj.transform.parent = transform;

            SpriteRenderer s = obj.AddComponent<SpriteRenderer>();
            s.sprite = sprite;
            obj.transform.position = new Vector3( s.bounds.size.x,0,0);

            BoxCollider2D col = obj.AddComponent<BoxCollider2D>();
            col.isTrigger = true;

            connectedBody = obj.GetComponent<Rigidbody2D>();

            HingeJoint2D joint = obj.AddComponent<HingeJoint2D>();
            joint.motor = m;
            joint.connectedBody = connectedBody;
            joint.enableCollision = false;

        }
    }*/
    /*
    void Update()
    {
        #if UNITY_EDITOR
        if (Application.isPlaying)
            return;
        #endif

        for (int i = 0; i < nodesCount; i++)
        {
            GameObject obj = new GameObject("Node " + i);//Instantiate(ropePrefab, transform.position, Quaternion.identity) as GameObject;
            obj.transform.parent = transform;
        }
    }*/
}
