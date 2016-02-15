using UnityEngine;
using System.Collections;
using UnityEngine.Events;

public class AnimatorHelper : MonoBehaviour
{
    public Animator animator;
    public bool onceOccured;
    public string sensitiveObjectTag;
    public string animatorVarName;
    public UnityEvent m_MyEvent;
    bool occured;

    public void setAnimatorInt(int value)
    {
        if (animator)
        {
            animator.SetInteger(animatorVarName, value);
        }
        else
        {
            print("Animator Helper: animator is null!");
        }
    }

    public void setAnimatorBool(bool value)
    {
        if (animator)
        {
            animator.SetBool(animatorVarName, value);
        }
        else
        {
            print("Animator Helper: animator is null!");
        }
    }

    public void setAnimatorFloat(float value)
    {
        if (animator)
        {
            animator.SetFloat(animatorVarName, value);
        }
        else
        {
            print("Animator Helper: animator is null!");
        }
    }

    public void setAnimatorTrigger()
    {
        if (animator)
        {
            animator.SetTrigger(animatorVarName);
        }
        else
        {
            print("Animator Helper: animator is null!");
        }
    }


    public void Awake()
    {
        occured = false;
    }
    
    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (occured || collision.tag != sensitiveObjectTag)
            return;
        if (onceOccured)
            occured = true;

        m_MyEvent.Invoke();
    }
}
