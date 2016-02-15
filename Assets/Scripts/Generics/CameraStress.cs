using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class CameraStress : MonoBehaviour
{

    public Transform mainPosition;
    [Range(0, 5)]
    public float dampingHeight;
    [Range(0, 5)]
    public float dampingWidth;
    [Range(0, 10)]
    public float speed;
    [Range(-10, 10)]
    public float offsetY;
    Vector3 lastPosition;
    Vector3 v;
    Vector3 newPosition;
    float incSpeed;
    public bool isIdle = true;

    // Use this for initialization
    void Start()
    {
        transform.position = new Vector3(mainPosition.position.x, mainPosition.position.y + offsetY, transform.position.z);
        newPosition = mainPosition.position;
        lastPosition = mainPosition.position;
        createPoint();
        incSpeed = speed;
        //isShaking = false;
    }

    // Update is called once per frame


    void Update()
    {
        if (isIdle)
        {
            moveToPoint();
        }
        else
        {
            createPoint();
            transform.position = new Vector3(mainPosition.position.x, mainPosition.position.y + offsetY, transform.position.z);
            CancelInvoke("moveToPoint");
            incSpeed = speed;
        }
    }

    void createPoint()
    {
        Vector3 positionOffset;
        
        positionOffset = new Vector3(Random.Range(-dampingWidth / 2, dampingWidth / 2),
            Random.Range(-dampingHeight / 2, dampingHeight / 2),
            transform.position.z);
        positionOffset.y += offsetY;

        newPosition = mainPosition.position + positionOffset;
    }
    void Lateupdate()
    {
        lastPosition = mainPosition.position;
    }

    void moveToPoint()
    {
        if (Vector2.Distance(transform.position, newPosition) < 0.2f)
        {
            createPoint();
        }
        if (incSpeed < 10)
            incSpeed += 0.005f;
        v = Vector2.Lerp(transform.position, newPosition, Time.deltaTime * incSpeed);
        v.z = transform.position.z;
        transform.position = v;
    }


    public void setIsIdleFalse()
    {
        isIdle = false;
    }
    public void setIsIdleTrue()
    {
        isIdle = true;
    }

}
