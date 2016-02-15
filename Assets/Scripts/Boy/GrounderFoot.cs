using UnityEngine;
using System.Collections;

public class GrounderFoot : MonoBehaviour
{
    RaycastHit2D[] hits;
    public Transform IK;
    void Start()
    {
    }

    void LateUpdate()
    {
        hits = Physics2D.RaycastAll(transform.position, Vector2.down , 500,LayerMask.NameToLayer("Ground"));
        if (hits.Length > 1)
        {
            IK.position = hits[1].point;
            Debug.DrawRay(hits[1].point , hits[1].normal , Color.red);
            print("dis : "  + hits[1].distance);
            //transform.up = Vector3.Lerp(transform.up , hits[1].normal - new Vector2(1,1) , Time.deltaTime * 5) ;
        }
    }
}
