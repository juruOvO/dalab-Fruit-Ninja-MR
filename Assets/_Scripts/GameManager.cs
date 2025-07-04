using System.Collections;
using System.Collections.Generic;
using Pico.Platform;
using Unity.XR.PXR;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

public class GameManager : MonoBehaviour
{
    [Header("Dev")]
    [SerializeField] private bool isDebuging;

    [Header("Experiment Settings")]
    [SerializeField] private int participantID;
    [SerializeField] private ExperimentSpace experimentSpace;
    [SerializeField] private bool passthrough;

    [Header("Env Prefabs")]
    //[SerializeField] private GameObject _VR_;
    [SerializeField] private GameObject _Passthrough_;

    [Header("Hit Reminder")]
    [SerializeField] private GameObject hitReminder;
    [SerializeField] private float reminderTime_std;
    private float reminderTime;

    [Header("XR Ray Interactor")]
    [SerializeField] private XRRayInteractor leftRay;
    [SerializeField] private XRRayInteractor rightRay;

    private bool isGameOver;
    private ParameterSetter parameterSetter;




    void Awake()
    {

        if (!isDebuging)
        {
            parameterSetter = GameObject.Find("Parameters Setter").GetComponent<ParameterSetter>();
            participantID = parameterSetter.participantID;
            experimentSpace = parameterSetter.experimentSpace;
            passthrough = parameterSetter.passthrough;
            Debug.Log(participantID + "\t" + experimentSpace + "\t" + passthrough);
        }
        else
        {
            Debug.Log("EMPTY PARAMETER SETTER");
        }

    }

    // Start is called before the first frame update
    void Start()
    {
        // Env Setting
        if (passthrough)
        {
            //_VR_.SetActive(false);
            _Passthrough_.SetActive(true);

        }
        else
        {
            //_VR_.SetActive(true);
            _Passthrough_.SetActive(false);
        }

        // Reminder
        hitReminder.SetActive(false);


        // Camera Setting
        Camera mainCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        if (mainCamera)
        {
            if (passthrough)
            {
                mainCamera.clearFlags = CameraClearFlags.SolidColor;
                mainCamera.backgroundColor = new Color(0, 0, 0, 0);
                
                // Start Pico Passthrough
                StartPassthrough();
            }
            else
            {
                mainCamera.clearFlags = CameraClearFlags.Skybox;
                
                // Stop Pico Passthrough if it was enabled
                StopPassthrough();
            }
            Debug.Log(mainCamera.transform.position);
        }
        else
        {
            Debug.LogError("Check the MAIN CAMERA");
        }

        isGameOver = false;
    }

    void OnDestroy()
    {
        // Ensure passthrough is disabled when the game object is destroyed
        if (passthrough)
        {
            StopPassthrough();
        }
    }

    void OnApplicationPause(bool pauseStatus)
    {
        // When app resumes from pause, re-enable see-through if it was enabled
        // This is necessary because the Boundary system automatically disables see-through on pause
        if (!pauseStatus && passthrough)
        {
            Debug.Log("App resumed - Re-enabling See-Through");
            StartPassthrough();
        }
    }

    // Update is called once per frame
    [System.Obsolete]
    void Update()
    {
        // When The Game Is Over
        if (isGameOver)
        {
            // Check if secondary button (B button) is pressed on either controller to quit
            List<InputDevice> inputDevices = new List<InputDevice>();
            InputDevices.GetDevices(inputDevices);
            
            foreach (var device in inputDevices)
            {
                if (device.characteristics.HasFlag(InputDeviceCharacteristics.Controller))
                {
                    bool secondaryButtonPressed = false;
                    if (device.TryGetFeatureValue(CommonUsages.secondaryButton, out secondaryButtonPressed) && secondaryButtonPressed)
                    {
                        Debug.Log("B button pressed - Quitting application");
                        Application.Quit();
                        
                        // For editor testing
                        #if UNITY_EDITOR
                        UnityEditor.EditorApplication.isPlaying = false;
                        #endif
                        break;
                    }
                }
            }
            
            // Debug.Log("GAME OVER");
            float waitTime = 6.0f;
            while (waitTime > 0.0f)
            {
                waitTime -= Time.deltaTime;
            }
            //Application.Quit();
        }

        if (hitReminder.active == true)
        {
            reminderTime += Time.deltaTime;
            if (reminderTime > reminderTime_std)
            {
                hitReminder.SetActive(false);
            }
        }
    }


    public int GetPariticipantID()
    {
        return this.participantID;
    }

    public ExperimentSpace GetExperimentSpace()
    {
        return this.experimentSpace;
    }

    public bool GetPassthrough()
    {
        return this.passthrough;
    }

    public bool IsGameOver()
    {
        return isGameOver;
    }

    public void IsGameOver(bool isGameOver)
    {
        this.isGameOver = isGameOver;
        leftRay.enabled = isGameOver;
        rightRay.enabled = isGameOver;
    }

    public bool IsDebuging()
    {
        return isDebuging;
    }

    public void IsHit()
    {
        this.hitReminder.SetActive(true);
        reminderTime = 0.0f;
    }

    public void RestartGame()
    {
        // 可选：保存当前进度
        DataSaver dataSaver = GetComponent<DataSaver>();
        if (dataSaver != null)
        {
            dataSaver.SaveCurrentData("restart");
        }

        // 重新加载当前场景
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }
    
    public void ReturnToMenu()
    {
        // 返回Menu场景
        UnityEngine.SceneManagement.SceneManager.LoadScene("Menu");
    }

    private void StartPassthrough()
    {
        // Use coroutine to add small delay as recommended by Pico
        StartCoroutine(EnableSeeThroughWithDelay());
    }

    private IEnumerator EnableSeeThroughWithDelay()
    {
        // Small delay to ensure the system is ready
        yield return new WaitForSeconds(0.1f);
        
        try
        {
            // Enable Pico See-Through using the correct property
            PXR_Manager.EnableVideoSeeThrough = true;
            Debug.Log("Pico See-Through enabled successfully");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to enable Pico See-Through: {e.Message}");
        }
    }

    private void StopPassthrough()
    {
        try
        {
            // Disable Pico See-Through using the correct property
            PXR_Manager.EnableVideoSeeThrough = false;
            Debug.Log("Pico See-Through disabled successfully");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to disable Pico See-Through: {e.Message}");
        }
    }
}
