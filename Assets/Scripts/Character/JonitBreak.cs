using UnityEngine;
using System.Collections;

public class JonitBreak : MonoBehaviour {
    
    HingeJoint2D joint;
    // Use this for initialization
    void Start () {
        joint = GetComponent<HingeJoint2D>();
	}
	
	// Update is called once per frame
	void Update () {
        if (joint)
        {
            Vector2 jointForce = joint.GetReactionForce(Time.deltaTime);
            print(jointForce);
            if (jointForce.magnitude > 2000)
            {
                Destroy(joint);
            }
        }
        
    }

    void OnJointBreak()
    {
        print("break");
    }
}
