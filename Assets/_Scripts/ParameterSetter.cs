using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.XR;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.UI;
using TMPro;
using System.Text.RegularExpressions;
using UnityEngine.SceneManagement;

public class ParameterSetter : MonoBehaviour
{
    [SerializeField] private int m_participantID;
    [SerializeField] private ExperimentSpace m_experimentSpace;
    [SerializeField] private bool m_passthrough;

    [SerializeField] private Slider participantIDSlider;
    [SerializeField] private TextMeshProUGUI participantIDValue;
    [SerializeField] private Toggle condition;
    [SerializeField] private TMP_Dropdown location;

    public int participantID
    {
        get => m_participantID;
        set => m_participantID = value;
    }

    public ExperimentSpace experimentSpace
    {
        get => m_experimentSpace;
        set => m_experimentSpace = value;
    }

    public bool passthrough
    {
        get => m_passthrough;
        set => m_passthrough = value;
    }

    public void SetParticipantID()
    {
        this.participantID = (int)participantIDSlider.value;
        this.participantIDValue.text = this.participantID.ToString("00");
    }

    public void SetConditionData(bool isOn)
    {
        m_passthrough = isOn;
    }

    public void SetLocationData(int index)
    {
        string selectedOption = location.options[index].text;
        //Debug.Log("index:" + index + "selectedoption:" + selectedOption);
        switch (selectedOption)
        {
            case "Baseline":
                experimentSpace = ExperimentSpace.Baseline;
                break;
            case "Character":
                experimentSpace = ExperimentSpace.Character;
                break;
            case "Object":
                experimentSpace = ExperimentSpace.Object;
                break;
            case "Abstract":
                experimentSpace = ExperimentSpace.Abstract;
                break;
            default:
                experimentSpace = ExperimentSpace.Baseline; // 默认值
                break;
        }
    }

    public void Start(){
        GameObject.DontDestroyOnLoad(this);
    }
    public void CheckAllData()
    {

        Debug.Log("Participant ID: " + m_participantID);
        Debug.Log("Experiment Space: " + m_experimentSpace);
        Debug.Log("Passthrough: " + (m_passthrough ? "On" : "Off"));
        //SceneManager.LoadScene("Main");
        switch (m_experimentSpace)
        {
            case ExperimentSpace.Baseline:
                SceneManager.LoadScene("Baseline");
                break;
            case ExperimentSpace.Character:
                SceneManager.LoadScene("Character");
                break;
            case ExperimentSpace.Object:
                SceneManager.LoadScene("Object");
                break;
            case ExperimentSpace.Abstract:
                SceneManager.LoadScene("Abstract");
                break;
            default:
                SceneManager.LoadScene("Baseline");
                break;
        }
    }

    
}
