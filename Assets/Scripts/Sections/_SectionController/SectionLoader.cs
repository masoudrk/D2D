using UnityEngine;
using System.Collections;
using System;

public class SectionLoader : MonoBehaviour
{
    public bool testMode;

    public float currentLength;

    Transform transformPerviousSection;
    Transform transformCurrentSection;
    Transform transformNextSection;

    public Transform[] transformBgLayer;

    public GameObject boy;
    public void Awake()
    {
        Section.bgLayer = new float[3];
        currentLength = Section.bgLayer[0] = Section.bgLayer[1] = Section.bgLayer[2] = 0;

        transformPerviousSection = transform.GetChild(0);
        transformCurrentSection = transform.GetChild(1);
        transformNextSection = transform.GetChild(2);
    }

    public void Start()
    {
        if (!testMode)
        {
            loadSection(SectionChooser.loadSectionIndex);
        }
        else
        {
            GameObject currentSection = transformCurrentSection.GetChild(0).gameObject;
            setupBoyPos(currentSection);
            currentLength = transformCurrentSection.GetChild(0).GetComponent<Section>().length;
            currentSection.GetComponent<Section>().loadBgs();

            if (transformNextSection.childCount > 0)
            {
                currentLength += transformNextSection.GetChild(0).GetComponent<Section>().length;
                transformNextSection.GetChild(0).GetComponent<Section>().loadBgs();
            }
        }
    }

    public void loadNextSection(GameObject nextSectionPrefab )
    {
        if (nextSectionPrefab)
        {
            // shift sections
            if (transformPerviousSection.childCount > 0)
            {
                Section s = transformPerviousSection.GetChild(0).GetComponent<Section>();
                s.removeBackgrounds();
                Destroy(transformPerviousSection.GetChild(0).gameObject);
            }

            transformCurrentSection.GetChild(0).parent = transformPerviousSection;

            if (transformNextSection.childCount > 0)
                transformNextSection.GetChild(0).parent = transformCurrentSection;


            //load next section
            GameObject section = Instantiate(nextSectionPrefab) as GameObject;
            if (!testMode)
            {
                section.transform.parent = transformNextSection;
            }
            else
            {
                section.transform.parent = transformCurrentSection;
            }
            section.transform.position = new Vector3(currentLength, 0, 0);

            Section _section = nextSectionPrefab.GetComponent<Section>();
            currentLength += _section.length;
            _section.loadBgs();
        }
    }

    public Transform getBgLayerByIndex(int index)
    {
        return transformBgLayer[index];
    }

    public void loadSection(int index)
    {
        UnityEngine.Object sectionPerviousPrefab, sectionCurrentPrefab, sectionNextPrefab;
        GameObject sectionPervious = null, sectionCurrent, sectionNext;
        
        sectionPerviousPrefab = Resources.Load("Sections/Section" + (index - 1), typeof(GameObject));
        sectionCurrentPrefab = Resources.Load("Sections/Section" + index, typeof(GameObject));
        sectionNextPrefab = Resources.Load("Sections/Section" + (index + 1), typeof(GameObject));

        if (sectionCurrentPrefab)
        {
            if (sectionPerviousPrefab)
            {
                sectionPervious = Instantiate(sectionPerviousPrefab) as GameObject;
                Section s = sectionPervious.GetComponent<Section>();
                sectionPervious.transform.position = new Vector3(-s.length, 0 , 0);
                sectionPervious.transform.parent = transformPerviousSection;

                Section.Background [] bgs = s.backgroundPrefabs;
                foreach (Section.Background b in bgs)
                {
                    float size = b.sprite.bounds.size.x;
                    Section.bgLayer[b.layer] = -size ;
                }
                s.loadBgs();
            }

            sectionCurrent = Instantiate(sectionCurrentPrefab) as GameObject;
            sectionCurrent.transform.parent = transformCurrentSection;
            sectionCurrent.transform.position = new Vector3();//sectionCurrent.GetComponent<Section>().position;
            setupBoyPos(sectionCurrent);
            Section secCurrent = sectionCurrent.GetComponent<Section>();
            currentLength = secCurrent.length;
            secCurrent.loadBgs();

            if (sectionNextPrefab)
            {
                sectionNext = Instantiate(sectionNextPrefab) as GameObject;
                sectionNext.transform.parent = transformNextSection;
                sectionNext.transform.position = new Vector3(currentLength, 0, 0);//sectionNext.GetComponent<Section>().position;

                Section secNext = sectionNext.GetComponent<Section>();
                currentLength += secNext.length;
                secNext.loadBgs();
            }
        }
        else
        {
            print("(Section" + index + ") is not Exists! " + "Please enter a valid section!");
            Application.LoadLevel("Menu");
        }
    }

    private void setupBoyPos(GameObject sectionCurrent)
    {
        boy.transform.position = sectionCurrent.transform.GetChild(0).position;
    }
}
