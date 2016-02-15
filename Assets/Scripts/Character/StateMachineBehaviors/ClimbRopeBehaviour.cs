using UnityEngine;
using System.Collections;

public class ClimbRopeBehaviour : StateMachineBehaviour {
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.SetInteger("ClimbRopeDirection", 0); 
    }
}
