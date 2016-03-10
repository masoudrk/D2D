using UnityEngine;
using System.Collections;

public class Box : MonoBehaviour
{
    public enum Action
    {
        CAN_JUMP, CANNOT_JUMP , GRAB_AND_JUMP
    }

    public Action action;
    //public bool canJumpUpFrom;
    public Transform jumpCorner;
}
