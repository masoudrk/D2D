using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class SectionChooser : MonoBehaviour {

    public static int loadSectionIndex= 200;
    public bool load;

    public InputField inputMission;
    
    public void loadSection()
    {
        int num;
        bool itsRight = int.TryParse( inputMission.text.ToString() , out num ) ;
        if (itsRight)
            loadSectionIndex = num;
        else
            loadSectionIndex = 0;

        Application.LoadLevel("Game");
    }
}
