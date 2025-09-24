using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.XR.PXR;

public class EyeTrackingCalibrator : MonoBehaviour
{
    [Header("Calibration Settings")]
    [SerializeField] private bool forceCalibration = false; // 强制重新校准的选项
    
    private bool support = false;
    private EyeTrackingMode eyeTrackingModes;
    private bool calibrationCompleted = false;
    
    // PlayerPrefs键名
    private const string CALIBRATION_COMPLETED_KEY = "EyeTrackingCalibrationCompleted";
    private const string CALIBRATION_TIME_KEY = "EyeTrackingCalibrationTime";

    void Start()
    {
        // 检查是否在Menu场景
        if (!IsMenuScene())
        {
            Debug.Log("EyeTrackingCalibrator: Not in Menu scene, skipping calibration.");
            return;
        }

        // 检查是否已经完成校准
        if (!forceCalibration && IsCalibrationCompleted())
        {
            Debug.Log("EyeTrackingCalibrator: Calibration already completed, skipping.");
            return;
        }

        // 开始校准流程
        StartCalibration();
    }

    private bool IsMenuScene()
    {
        return UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "Menu";
    }

    private bool IsCalibrationCompleted()
    {
        return PlayerPrefs.GetInt(CALIBRATION_COMPLETED_KEY, 0) == 1;
    }

    private void StartCalibration()
    {
        Debug.Log("EyeTrackingCalibrator: Starting eye tracking calibration...");
        
        int supportModesCount = 0;
        PXR_MotionTracking.GetEyeTrackingSupported(ref support, ref supportModesCount, ref eyeTrackingModes);
        
        if (support)
        {
            EyeTrackingStartInfo eyeTrackingStartInfo = new EyeTrackingStartInfo();
            eyeTrackingStartInfo.needCalibration = 1;
            eyeTrackingStartInfo.mode = EyeTrackingMode.PXR_ETM_BOTH;
            
            int startResult = PXR_MotionTracking.StartEyeTracking(ref eyeTrackingStartInfo);
            
            if (startResult == 1)
            {
                Debug.Log("EyeTrackingCalibrator: Eye tracking calibration started successfully.");
                calibrationCompleted = true;
                
                // 记录校准完成状态和时间
                PlayerPrefs.SetInt(CALIBRATION_COMPLETED_KEY, 1);
                PlayerPrefs.SetString(CALIBRATION_TIME_KEY, System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                PlayerPrefs.Save();
                
                Debug.Log($"EyeTrackingCalibrator: Calibration completed at {PlayerPrefs.GetString(CALIBRATION_TIME_KEY)}");
            }
            else
            {
                Debug.LogError("EyeTrackingCalibrator: Failed to start eye tracking calibration.");
            }
        }
        else
        {
            Debug.LogWarning("EyeTrackingCalibrator: Eye tracking not supported on this device.");
        }
    }

    // 公共方法：重置校准状态（用于调试或强制重新校准）
    public void ResetCalibrationStatus()
    {
        PlayerPrefs.DeleteKey(CALIBRATION_COMPLETED_KEY);
        PlayerPrefs.DeleteKey(CALIBRATION_TIME_KEY);
        PlayerPrefs.Save();
        Debug.Log("EyeTrackingCalibrator: Calibration status reset.");
    }

    // 公共方法：获取校准状态
    public bool GetCalibrationStatus()
    {
        return IsCalibrationCompleted();
    }

    // 公共方法：获取校准时间
    public string GetCalibrationTime()
    {
        return PlayerPrefs.GetString(CALIBRATION_TIME_KEY, "Not calibrated");
    }

    void Update()
    {
        // 可以在这里添加校准进度检查或其他逻辑
    }

    private void OnDisable()
    {
        if (support && calibrationCompleted)
        { 
            EyeTrackingStopInfo eyeTrackingStopInfo = new EyeTrackingStopInfo();
            PXR_MotionTracking.StopEyeTracking(ref eyeTrackingStopInfo);
            Debug.Log("EyeTrackingCalibrator: Eye tracking stopped.");
        }
    }

    private void OnApplicationQuit()
    {
        // 确保在应用退出时停止眼动追踪
        if (support)
        {
            EyeTrackingStopInfo eyeTrackingStopInfo = new EyeTrackingStopInfo();
            PXR_MotionTracking.StopEyeTracking(ref eyeTrackingStopInfo);
        }
    }
}
