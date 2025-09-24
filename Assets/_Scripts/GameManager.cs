using System.Collections;
using System.Collections.Generic;
using Pico.Platform;
using Unity.XR.PXR;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

public class GameManager : MonoBehaviour
{
    private Camera mainCamera;

    [Header("Gaze Point Visualization")]
    [SerializeField] private GameObject gazeSpherePrefab;   // 预制体：仅需一个带MeshRenderer的Sphere，默认半径0.025m
    [SerializeField] private Color gazeSphereColor = Color.red;
    [SerializeField] private bool gazeSphereVisible = true;
    private GameObject gazeSphereInstance;

    [Header("Eye Tracking Visualization")]
    [SerializeField] private bool visualizeGazeRay = true; // 是否可视化视线
    [SerializeField] private Color gazeRayColor = Color.green; // 视线颜色
    [SerializeField] private float gazeRayLength = 50f; // 视线长度
    [SerializeField] private float gazeRayWidth = 0.01f; // 视线宽度
    private LineRenderer gazeRayRenderer; // 用于绘制视线的组件

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
    private bool isGameStarted = false; // 新增：控制游戏是否已开始
    private ParameterSetter parameterSetter;
    private DataSaver dataSaver;
    private bool isEyeTrackingActive = false;




    void Awake()
    {
        dataSaver = GameObject.FindGameObjectWithTag("GM").GetComponent<DataSaver>();
    }

    void OnEnable()
    {
        isGameOver = false;
    }

    // Start is called before the first frame update
    void Start()
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
        mainCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
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


        // 初始化视线可视化（但不启动眼动追踪，等待游戏开始）
        InitializeGazeVisualization();
        InitializeGazePointVisualization();
        
        // 初始化射线交互状态（游戏开始前启用）
        if (leftRay != null && rightRay != null)
        {
            leftRay.enabled = true;
            rightRay.enabled = true;
        }
    }

    void OnDestroy()
    {
        // Ensure passthrough is disabled when the game object is destroyed
        if (passthrough)
        {
            StopPassthrough();
        }
        
        EyeTrackingStopInfo eyeTrackingStopInfo = new EyeTrackingStopInfo();
        PXR_MotionTracking.StopEyeTracking(ref eyeTrackingStopInfo);
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

                        EyeTrackingStopInfo eyeTrackingStopInfo = new EyeTrackingStopInfo();
                        PXR_MotionTracking.StopEyeTracking(ref eyeTrackingStopInfo);

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



        if (!isGameOver && isEyeTrackingActive && isGameStarted)
        {
            ProcessEyeTracking();
        }

        if (visualizeGazeRay && isEyeTrackingActive && isGameStarted)
        {
            UpdateGazeVisualization();
        }

        if (gazeSphereVisible && isEyeTrackingActive && isGameStarted)
        {
            UpdateGazePointVisualization();
        }

        if (hitReminder.activeSelf == true)
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
        // 射线交互在游戏开始前和游戏结束时都可用，只在游戏进行中禁用
        bool shouldEnableRays = isGameOver || !isGameStarted;
        leftRay.enabled = shouldEnableRays;
        rightRay.enabled = shouldEnableRays;
    }

    public bool IsDebuging()
    {
        return isDebuging;
    }

    public bool IsGameStarted()
    {
        return isGameStarted;
    }

    public void StartGame()
    {
        isGameStarted = true;
        Debug.Log("Game started by player!");
        
        // 游戏开始时禁用射线交互
        leftRay.enabled = false;
        rightRay.enabled = false;
        
        // 游戏开始时启动眼动追踪
        StartEyeTrackingForExperiment();
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
            // Enable Pico See-Through using the old version API
            PXR_Boundary.EnableSeeThroughManual(true);
            Debug.Log("Pico See-Through enabled successfully (old API)");
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
            // Disable Pico See-Through using the old version API
            PXR_Boundary.EnableSeeThroughManual(false);
            Debug.Log("Pico See-Through disabled successfully (old API)");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to disable Pico See-Through: {e.Message}");
        }
    }

    private void ProcessEyeTracking()
    {


        // Debug.Log("Processing Eye Tracking frame..."); // For debugging, uncomment to see if this function is called.
        if (PXR_EyeTracking.GetCombineEyeGazePoint(out Vector3 localGazeOrigin) &&
            PXR_EyeTracking.GetCombineEyeGazeVector(out Vector3 localGazeDirection))
        {
            Vector3 worldGazeOrigin = mainCamera.transform.TransformPoint(localGazeOrigin);
            Vector3 worldGazeDirection = mainCamera.transform.TransformDirection(localGazeDirection);

            if (Physics.Raycast(worldGazeOrigin, worldGazeDirection, out RaycastHit hit, Mathf.Infinity))
            {
                // If the raycast hits a game object, check its tag.
                if (hit.collider.CompareTag("Core"))
                {
                    // Record as time spent looking at core game elements.
                    dataSaver.RecordGameCoreTime(Time.deltaTime);
                }
                else if (hit.collider.CompareTag("Kanata"))
                {
                    dataSaver.RecordGameCoreTime(Time.deltaTime);
                }
                else if (hit.collider.CompareTag("Env"))
                {
                    // If it's not a core element, record it as environment time.
                    dataSaver.RecordGameEnvTime(Time.deltaTime);
                }
                else
                {
                    dataSaver.RecordRealWorldTime(Time.deltaTime);
                }
            }
            else
            {
                // If the raycast hits nothing, record as time spent looking at the real world.
                dataSaver.RecordRealWorldTime(Time.deltaTime);
            }
        }
        else
        {
            // For debugging, uncomment to see if eye tracking data is unavailable.
            // Debug.Log("Eye tracking data not available this frame.");
        }
    }

    private void StartEyeTrackingForExperiment()
    {
        // 检查是否在Menu场景，如果是则跳过（由EyeTrackingCalibrator处理）
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "Menu")
        {
            Debug.Log("GameManager: In Menu scene, eye tracking handled by EyeTrackingCalibrator.");
            return;
        }

        // 在实验场景中启动眼动追踪（不进行校准）
        bool support = false;
        int supportModesCount = 0;
        EyeTrackingMode eyeTrackingModes = EyeTrackingMode.PXR_ETM_BOTH;

        PXR_MotionTracking.GetEyeTrackingSupported(ref support, ref supportModesCount, ref eyeTrackingModes);

        if (support)
        {
            EyeTrackingStartInfo eyeTrackingStartInfo = new EyeTrackingStartInfo();
            eyeTrackingStartInfo.needCalibration = 1;
            eyeTrackingStartInfo.mode = EyeTrackingMode.PXR_ETM_BOTH;

            int startResult = PXR_MotionTracking.StartEyeTracking(ref eyeTrackingStartInfo);

            if (startResult == 0)
            {
                Debug.Log("GameManager: Eye tracking started for experiment (no calibration needed).");
                isEyeTrackingActive = true;
            }
            else
            {
                Debug.LogError("GameManager: Failed to start eye tracking for experiment.");
                isEyeTrackingActive = false;
            }
        }
        else
        {
            Debug.LogWarning("GameManager: Eye tracking not supported on this device.");
            isEyeTrackingActive = false;
        }
    }
    /// <summary>
    /// 初始化视线可视化组件
    /// </summary>
    private void InitializeGazeVisualization()
    {
        // 创建新的游戏对象用于视线渲染
        GameObject gazeRayObject = new GameObject("GazeRayVisualizer");
        gazeRayRenderer = gazeRayObject.AddComponent<LineRenderer>();

        // 设置视线渲染器属性
        gazeRayRenderer.material = new Material(Shader.Find("Unlit/Color"));
        gazeRayRenderer.material.color = gazeRayColor;
        gazeRayRenderer.startWidth = gazeRayWidth;
        gazeRayRenderer.endWidth = gazeRayWidth;
        gazeRayRenderer.positionCount = 2;

        // 初始化为不可见
        gazeRayRenderer.enabled = false;
    }

    /// <summary>
    /// 更新视线可视化
    /// </summary>
    private void UpdateGazeVisualization()
    {
       if (PXR_EyeTracking.GetCombineEyeGazePoint(out Vector3 localGazeOrigin) &&
        PXR_EyeTracking.GetCombineEyeGazeVector(out Vector3 localGazeDirection))
        {
            // 坐标转换
            Vector3 worldGazeOrigin = mainCamera.transform.TransformPoint(localGazeOrigin);
            Vector3 worldGazeDirection = mainCamera.transform.TransformDirection(localGazeDirection);
            
            Vector3 gazeEnd = worldGazeOrigin + worldGazeDirection * gazeRayLength;
            
            gazeRayRenderer.SetPosition(0, worldGazeOrigin);
            gazeRayRenderer.SetPosition(1, gazeEnd);
            gazeRayRenderer.enabled = true;
        }
        else
        {
            gazeRayRenderer.enabled = false;
        }
    }
    
    /// <summary>
    /// 初始化注视点可视化（独立于 GazeRay）
    /// </summary>
    private void InitializeGazePointVisualization()
    {
        if (gazeSpherePrefab == null)
        {
            // 运行时动态创建球体
            gazeSphereInstance = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            gazeSphereInstance.name = "GazePointVisualizer";
            gazeSphereInstance.transform.localScale = Vector3.one * 0.05f; // 5cm 直径
            gazeSphereInstance.GetComponent<Renderer>().material = new Material(Shader.Find("Unlit/Color"));
            gazeSphereInstance.GetComponent<Renderer>().material.color = gazeSphereColor;
        }
        else
        {
            // 使用预制体
            gazeSphereInstance = Instantiate(gazeSpherePrefab);
        }

        gazeSphereInstance.SetActive(false);
    }

    /// <summary>
    /// 更新注视点位置
    /// </summary>
    private void UpdateGazePointVisualization()
    {
        if (PXR_EyeTracking.GetCombineEyeGazePoint(out Vector3 localGazeOrigin) &&
        PXR_EyeTracking.GetCombineEyeGazeVector(out Vector3 localGazeDirection))
        {
            // 坐标转换
            Vector3 worldGazeOrigin = mainCamera.transform.TransformPoint(localGazeOrigin);
            gazeSphereInstance.transform.position = worldGazeOrigin;
            gazeSphereInstance.SetActive(true);
        }
        else
        {
            gazeSphereInstance.SetActive(false);
        }
    }
}