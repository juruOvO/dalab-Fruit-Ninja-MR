using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
public class DataSaver : MonoBehaviour
{
    [Header("Related Files Setting")]
    [SerializeField] private string outputFolder;
    [SerializeField] private string outputFile;

    [Header("Data Observe")]
    private DateTime gameStartTime;
    private string gameStartTimeStamp;
    public int currentActualScore, totalActualScore;
    public int currentPerfectScore, totalPerfectScore;
    public int currentFruitCutted, totalFruitCutted;
    public int currentFruitSpawned, totalFruitSpawned;

    public int currentBombCutted, totalBombCutted;
    public int currentBombSpawned, totalBombSpawned;
    public int currentObstacleHit, totalObstacleHit;
    public int currentObstacleSpawned, totalObstacleSpawned;
    public int currentComboCnt;
    public int currentMaxComboCnt, totalMaxComboCnt;

    public int score;
    private int participantID;
    private ExperimentSpace experimentSpace;
    private bool passthrough;
    private GameManager gameManager;

    // 新的表头设计 - PID + 4个条件 * 4个指标 = 17列
    private readonly string header = "PID," +
        "ActualScore_Baseline,ActualScore_Character,ActualScore_Object,ActualScore_Abstract," +
        "FruitCutRate_Baseline,FruitCutRate_Character,FruitCutRate_Object,FruitCutRate_Abstract," +
        "BombAvoidRate_Baseline,BombAvoidRate_Character,BombAvoidRate_Object,BombAvoidRate_Abstract," +
        "MaxCombo_Baseline,MaxCombo_Character,MaxCombo_Object,MaxCombo_Abstract";

    void Start()
    {
        gameManager = GameObject.FindGameObjectWithTag("GM").GetComponent<GameManager>();

        participantID = gameManager.GetPariticipantID();
        experimentSpace = gameManager.GetExperimentSpace();
        passthrough = gameManager.GetPassthrough();
        Debug.Log(experimentSpace);

        gameStartTime = DateTime.Now;
        gameStartTimeStamp = new DateTimeOffset(gameStartTime).ToUnixTimeSeconds().ToString();

        // 统一的输出文件
        outputFolder = Path.Combine(Application.persistentDataPath, "Results");
        if (!Directory.Exists(outputFolder))
        {
            Directory.CreateDirectory(outputFolder);
        }

        outputFile = Path.Combine(outputFolder, "ExperimentResults.csv");
        
        // 如果文件不存在，创建并写入表头
        if (!File.Exists(outputFile))
        {
            using StreamWriter headerWriter = File.CreateText(outputFile);
            headerWriter.WriteLine(header);
            headerWriter.Close();
        }
    }

    void Update()
    {
        score = totalActualScore + currentActualScore;
    }

    public DateTime GetStartTime()
    {
        return gameStartTime;
    }

    private void DataUpdate()
    {
        // Update Total
        totalActualScore += currentActualScore;
        totalPerfectScore += currentPerfectScore;

        totalFruitCutted += currentFruitCutted;
        totalFruitSpawned += currentFruitSpawned;

        totalBombCutted += currentBombCutted;
        totalBombSpawned += currentBombSpawned;

        totalMaxComboCnt = Mathf.Max(totalMaxComboCnt, currentMaxComboCnt);

        totalObstacleHit += currentObstacleHit;
        totalObstacleSpawned += currentObstacleSpawned;

        // Reset Current
        currentActualScore = 0;
        currentPerfectScore = 0;

        currentFruitCutted = 0;
        currentFruitSpawned = 0;

        currentBombCutted = 0;
        currentBombSpawned = 0;

        currentComboCnt = 0;
        currentMaxComboCnt = 0;

        currentObstacleHit = 0;
        currentObstacleSpawned = 0;
    }

    public void SaveCurrentData(string configName)
    {
        // 只做数据更新，不保存到文件
        DataUpdate();
    }

    public void SaveTotalData()
    {
        // 计算当前条件的数据
        double fruitCutRate = totalFruitSpawned > 0 ? (double)totalFruitCutted / totalFruitSpawned : 0;
        double bombAvoidRate = totalBombSpawned > 0 ? 1.0 - (double)totalBombCutted / totalBombSpawned : 1.0;

        // 读取现有CSV文件
        List<string> lines = new List<string>();
        if (File.Exists(outputFile))
        {
            lines = File.ReadAllLines(outputFile).ToList();
        }

        // 查找是否已存在该PID的数据行
        int targetRowIndex = -1;
        for (int i = 1; i < lines.Count; i++) // 跳过表头
        {
            string[] values = lines[i].Split(',');
            if (values.Length > 0 && values[0] == participantID.ToString())
            {
                targetRowIndex = i;
                break;
            }
        }

        int conditionOffset = GetColumnOffset(experimentSpace);

        string[] rowData;
        if (targetRowIndex == -1)
        {
            // 新建行：PID + 16个空值
            rowData = new string[17];
            rowData[0] = participantID.ToString();
            for (int i = 1; i < 17; i++)
            {
                rowData[i] = "";
            }
            targetRowIndex = lines.Count;
            lines.Add("");
        }
        else
        {
            // 解析现有行
            rowData = lines[targetRowIndex].Split(',');
            // 确保数组长度足够
            if (rowData.Length < 17)
            {
                Array.Resize(ref rowData, 17);
            }
        }

        // 按新的列排列填入数据
        // ActualScore组 (列1-4)
        rowData[1 + conditionOffset] = totalActualScore.ToString();
        
        // FruitCutRate组 (列5-8)  
        rowData[5 + conditionOffset] = fruitCutRate.ToString("F4");
        
        // BombAvoidRate组 (列9-12)
        rowData[9 + conditionOffset] = bombAvoidRate.ToString("F4");
        
        // MaxCombo组 (列13-16)
        rowData[13 + conditionOffset] = totalMaxComboCnt.ToString();

        // 重新组装行数据
        lines[targetRowIndex] = string.Join(",", rowData);

        // 写回文件
        File.WriteAllLines(outputFile, lines);

        Debug.Log($"数据已保存：PID={participantID}, 条件={experimentSpace}");
    }

    private int GetColumnOffset(ExperimentSpace space)
    {
        // 返回对应条件的列索引偏移
        switch (space)
        {
            case ExperimentSpace.Baseline:
                return 0; // Baseline在每组的第1个位置
            case ExperimentSpace.Character:
                return 1; // Character在每组的第2个位置
            case ExperimentSpace.Object:
                return 2; // Object在每组的第3个位置
            case ExperimentSpace.Abstract:
                return 3; // Abstract在每组的第4个位置
            default:
                return 0;
        }
    }
}