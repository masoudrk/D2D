using UnityEngine;
using System.Collections;

public class CharacterGroundChecker : MonoBehaviour {

    public bool isGround;
    public bool canPicking;
    public HumanController humanController;

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.tag == "Ground" || col.tag == "Box")
        {
            isGround = true;
            humanController.getDamageFromFalling();
            humanController.GetComponent<Rigidbody2D>().velocity = new Vector2();
        }

        if (col.tag == "Slider")
        {
            isGround = true;
            humanController.getDamageFromFalling();
            float speed = col.GetComponent<SurfaceEffector>().speed;
            if (!humanController.flipFacing && speed < 0 || humanController.flipFacing && speed > 0)
                humanController.flipFace();
        }
    }
    void OnTriggerStay2D(Collider2D col)
    {
        switch (col.tag)
        {
            case "Ground":
            case "Box":
            case "Slider":
                isGround = true;
                break;
            case "Pickable":
                canPicking = true;
                break;
        }
    }

    void OnTriggerExit2D(Collider2D col)
    {
        if (col.tag == "Ground" || col.tag == "Box")
        {
            isGround = false;
        }

        if(col.tag == "Pickable")
            canPicking = false;
    }
}
