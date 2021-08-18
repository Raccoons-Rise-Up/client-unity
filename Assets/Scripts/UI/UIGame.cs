using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIGame : MonoBehaviour
{
    public GameObject panelMiddle;
    private SectionMiddle sectionMiddle;

    private enum SectionMiddle
    {
        Store,
        Research,
        None
    }

    private void Start()
    {
        sectionMiddle = SectionMiddle.None;
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
            panelMiddle.SetActive(true);
        }
    }
}
