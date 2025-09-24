using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class MenuController : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private Slider participantIDSlider;
    [SerializeField] private TextMeshProUGUI participantIDValue;
    [SerializeField] private Toggle conditionToggle;
    [SerializeField] private TMP_Dropdown locationDropdown;

    private void Start()
    {
        // Initialize UI with current values from the ParameterSetter singleton
        if (ParameterSetter.Instance != null)
        {
            participantIDSlider.value = ParameterSetter.Instance.participantID;
            participantIDValue.text = ParameterSetter.Instance.participantID.ToString("00");
            conditionToggle.isOn = ParameterSetter.Instance.passthrough;
            locationDropdown.value = (int)ParameterSetter.Instance.experimentSpace;
        }

        // Add listeners to UI elements
        participantIDSlider.onValueChanged.AddListener(delegate { OnParticipantIDChanged(); });
        conditionToggle.onValueChanged.AddListener(delegate { OnConditionChanged(); });
        locationDropdown.onValueChanged.AddListener(delegate { OnLocationChanged(); });
    }

    public void OnParticipantIDChanged()
    {
        int id = (int)participantIDSlider.value;
        participantIDValue.text = id.ToString("00");
        ParameterSetter.Instance.participantID = id;
    }

    public void OnConditionChanged()
    {
        ParameterSetter.Instance.passthrough = conditionToggle.isOn;
    }

    public void OnLocationChanged()
    {
        ParameterSetter.Instance.experimentSpace = (ExperimentSpace)locationDropdown.value;
    }

    public void StartExperiment()
    {
        Debug.Log("Starting Experiment with:");
        Debug.Log("Participant ID: " + ParameterSetter.Instance.participantID);
        Debug.Log("Experiment Space: " + ParameterSetter.Instance.experimentSpace);
        Debug.Log("Passthrough: " + (ParameterSetter.Instance.passthrough ? "On" : "Off"));

        switch (ParameterSetter.Instance.experimentSpace)
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