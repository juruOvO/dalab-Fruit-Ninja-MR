using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Anchor : MonoBehaviour
{
    public Text LabelText;
    public void UpdateLabel(string str)
    {
        string textStr = str;
        LabelText.text = textStr;
    }
}
