using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UITerminal : MonoBehaviour
{
    public GameObject content;
    public GameObject textOutputPrefab;

    public void Log(string message) 
    {
        var textGo = Instantiate(textOutputPrefab, content.transform);
        var textComponent = textGo.GetComponent<TextMeshProUGUI>();
        textComponent.text = message;
    }
}
