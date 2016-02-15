using UnityEngine;
using System.Collections;
using System;

public class Section : MonoBehaviour
{

    public static float[] bgLayer;

    [Serializable]
    public class Background
    {
        public bool usePrefab;
        public GameObject backgroundPrefabObject;
        public Sprite sprite;
        public int layer;
        public int orderInLayer;
        [System.NonSerialized]
        public GameObject bgObject;
    }

    SectionLoader sectionLoader;
    [Range(0, 500)]
    public int sectionIndex;
    public GameObject nextSectionPrefab;
    public int length;

    public Background[] backgroundPrefabs;

    public void Start()
    {
        //sectionLoader = FindObjectOfType<SectionLoader>();
    }

    public void loadBgs()
    {
        sectionLoader = FindObjectOfType<SectionLoader>();
        foreach (Background b in backgroundPrefabs)
        {
            if (b.layer >= 0)
            {
                GameObject bg = null;
                
                if (!b.usePrefab)
                {
                    bg = new GameObject();
                    bg.name = "ParralaxBg_s" + sectionIndex;

                    SpriteRenderer sr = bg.AddComponent<SpriteRenderer>();
                    sr.sprite = b.sprite;
                    sr.sortingOrder = b.orderInLayer;
                    b.bgObject = bg;
                }
                else
                {
                    if (b.backgroundPrefabObject != null)
                        bg = Instantiate(b.backgroundPrefabObject) as GameObject;
                }

                if (bg)
                {
                    Transform layer = sectionLoader.getBgLayerByIndex(b.layer); ;
                    bg.transform.position = new Vector3(bgLayer[b.layer] + layer.position.x, 0, 0);

                    bg.transform.parent = layer;
                    float bgLength = b.sprite.bounds.size.x;
                    bgLayer[b.layer] += bgLength;
                }
            }
        }
    }

    internal void removeBackgrounds()
    {
        foreach (Background b in backgroundPrefabs)
        {
            Destroy(b.bgObject);
        }
    }

    public void endOfSection()
    {
        sectionLoader = FindObjectOfType<SectionLoader>();
        sectionLoader.loadNextSection(nextSectionPrefab);
    }

    public void shakeCamera(AnimationEvent ae)
    {
        //Camera.main.GetComponent<CameraStress>().shake(ae.floatParameter, 0 , 0);
        //Camera.main.GetComponent<CameraShake>().Shake(ae.floatParameter);
    }
}
