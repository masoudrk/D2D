using UnityEngine;
using System.Collections;

public class BackgroundParallax : MonoBehaviour
{
    public Transform[] backgrounds;

    public Transform Boy;
    private Transform cam;                        // Shorter reference to the main camera's transform.

    void Awake()
    {
        cam = Boy;
    }
    
    public float[] ceo = new float[] { 0.13333333f, 0.24f, 0.346666666666f };

    public void Update()
    {
        float characterDeltaX = cam.position.x;

        for(int  i = 0 ; i < backgrounds.Length ; i++)
        {
            Vector3 v = backgrounds[i].position;
            v.x = -characterDeltaX * ceo[i];
            backgrounds[i].position = v;
        }

    }
}