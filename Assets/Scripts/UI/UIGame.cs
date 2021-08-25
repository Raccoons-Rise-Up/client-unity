using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using KRU.Game;

public class UIGame : MonoBehaviour
{
    public GameObject goGoldText;

    public GameObject panelUpperLeft;
    public GameObject panelMiddle;

    public GameObject gameTransform;
    private static KRUGame gameScript;

    //private SectionMiddle sectionMiddle;

    private static TextMeshProUGUI tmpGoldText;

    private enum SectionMiddle
    {
        Store,
        Research,
        None
    }

    private void Start()
    {
        tmpGoldText = goGoldText.GetComponent<TextMeshProUGUI>();
        gameScript = gameTransform.GetComponent<KRUGame>();

        //sectionMiddle = SectionMiddle.None;
        panelMiddle.SetActive(false);
    }

    public void BtnStore() 
    {
        if (panelMiddle.activeSelf)
        {
            panelMiddle.SetActive(false);
        }
        else
        {
            // Make sure upper left panel is closed
            //if (panelUpperLeft.activeSelf)
                //panelUpperLeft.SetActive(false);

            panelMiddle.SetActive(true);
        }
    }

    public void BtnCivilization() 
    {
        if (panelUpperLeft.activeSelf)
        {
            // Upper panel is open, close it
            panelUpperLeft.SetActive(false);
        }
        else 
        {
            // Upper panel is not open, open it

            // Make sure middle panel is closed
            if (panelMiddle.activeSelf)
                panelMiddle.SetActive(false);

            panelUpperLeft.SetActive(true);
        }
    }

    public static void UpdateGoldText() 
    {
        tmpGoldText.text = gameScript.Player.Gold.ToString();
    }
}
