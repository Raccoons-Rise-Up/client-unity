using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UITerminal : MonoBehaviour
{
    public GameObject m_Content;
    public GameObject m_TextOutputPrefab;

    public void Log(string message) 
    {
        var textGo = Instantiate(m_TextOutputPrefab, m_Content.transform);
        var textComponent = textGo.GetComponent<TextMeshProUGUI>();
        textComponent.text = message;
    }
}
