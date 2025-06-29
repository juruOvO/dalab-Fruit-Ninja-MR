// using System;
// using System.Collections;
// using System.Collections.Generic;
// using System.IO;
// using System.Linq.Expressions;
// using UnityEngine;
// using UnityEngine.InputSystem.Controls;

// public class DataSaver : MonoBehaviour
// {

//     [Header("Experiment Settings")]
//     private int participantID;
//     private ExperimentSpace experimentSpace;
//     private bool passthrough;

//     [Header("Related Files Setting")]
//     [SerializeField] private string outputFolder;

//     [SerializeField] private string outputFile;

//     [Header("Data Observe")]
//     private DateTime gameStartTime;
//     private string gameStartTimeStamp;
//     public int currentActualScore, totalActualScore;
//     public int currentPerfectScore, totalPerfectScore;
//     public int currentFruitCutted, totalFruitCutted;
//     public int currentFruitSpawned, totalFruitSpawned;

//     public int currentBombCutted, totalBombCutted;
//     public int currentBombSpawned, totalBombSpawned;
//     public int currentObstacleHit, totalObstacleHit;
//     public int currentObstacleSpawned, totalObstacleSpawned;
//     public int currentComboCnt;
//     public int currentMaxComboCnt, totalMaxComboCnt;
    

//     public int score;

//     private GameManager gameManager;

//     private readonly string header
//     = "ID,Experiment Space,Passthrough,Record Time,Config Name,"
//     + "Actual Score,Perfect Score,"
//     + "FruitCutted,FruitSpawned,FruitCutRate,FruitMissingRate,"
//     + "BombCutted,BombSpawned,BombCutRate,BombAvoidRate,"
//     + "MaxComboCnt,"
//     + "ObstacleHit, ObstacleSpawned";


//     void Start()
//     {
//         gameManager = GameObject.FindGameObjectWithTag("GM").GetComponent<GameManager>();

//         participantID = gameManager.GetPariticipantID();
//         experimentSpace = gameManager.GetExperimentSpace();
//         passthrough = gameManager.GetPassthrough();

//         gameStartTime = DateTime.Now;
//         gameStartTimeStamp = new DateTimeOffset(gameStartTime).ToUnixTimeSeconds().ToString();

//         outputFolder = Path.Combine(Application.persistentDataPath, "Results");
//         Debug.Log(outputFolder);
//         if (!Directory.Exists(outputFolder))
//         {
//             Directory.CreateDirectory(outputFolder);
//         }

//         outputFolder = Path.Combine(outputFolder, this.participantID.ToString());
//         if (!Directory.Exists(outputFolder))
//         {
//             Directory.CreateDirectory(outputFolder);
//         }

//         outputFile = Path.Combine(outputFolder, this.experimentSpace.ToString() + "-" + (this.passthrough ? "On" : "Off") + "-" + gameStartTimeStamp + ".csv");
//         if (!File.Exists(outputFile))
//         {
//             using StreamWriter headerWriter = File.CreateText(outputFile);
//             headerWriter.WriteLine(header);
//             headerWriter.Close();
//         }
//     }

//     void Update()
//     {
//         score = totalActualScore + currentActualScore;
//     }

//     public DateTime GetStartTime()
//     {
//         return gameStartTime;
//     }

//     private void DataUpdate()
//     {
//         // Update Total
//         totalActualScore += currentActualScore;
//         totalPerfectScore += currentPerfectScore;

//         totalFruitCutted += currentFruitCutted;
//         totalFruitSpawned += currentFruitSpawned;

//         totalBombCutted += currentBombCutted;
//         totalBombSpawned += currentBombSpawned;

//         totalMaxComboCnt = Mathf.Max(totalMaxComboCnt, currentMaxComboCnt);

//         totalObstacleHit += currentObstacleHit;
//         totalObstacleSpawned += currentObstacleSpawned;

//         // Reset Current
//         currentActualScore = 0;
//         currentPerfectScore = 0;

//         currentFruitCutted = 0;
//         currentFruitSpawned = 0;

//         currentBombCutted = 0;
//         currentBombSpawned = 0;

//         currentComboCnt = 0;
//         currentMaxComboCnt = 0;

//         currentObstacleHit = 0;
//         currentObstacleSpawned = 0;
//     }

//     private string CurrentDataProcess(string configName)
//     {
//         // Basic Info
//         string dataTmp = participantID.ToString() + "," + experimentSpace + "," + (this.passthrough ? "On" : "Off") + "," + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:ffffff") + "," + configName + ",";

//         // Record Score
//         dataTmp += currentActualScore + "," + currentPerfectScore + ",";

//         // Fruit Related
//         double fruitCutRate = (double)currentFruitCutted / currentFruitSpawned;
//         double fruitMissingRate = 1 - fruitCutRate;

//         dataTmp += currentFruitCutted + "," + currentFruitSpawned + ",";
//         dataTmp += fruitCutRate + "," + fruitMissingRate + ",";

//         // Bomb Related
//         double bombCutRate = (double)currentBombCutted / currentBombSpawned;
//         double bombAvoidRate = 1 - bombCutRate;

//         dataTmp += currentBombCutted + "," + currentBombSpawned + ",";
//         dataTmp += bombCutRate + "," + bombAvoidRate + ",";

//         // Obstacle Related
//         double obstacleCutRate = (double)currentObstacleHit / currentObstacleSpawned;
//         double obstacleAvoidRate = 1 - obstacleCutRate;

//         dataTmp += currentObstacleHit + "," + currentObstacleSpawned + ",";
//         dataTmp += obstacleCutRate + "," + obstacleAvoidRate + ",";

//         // Max Combo
//         dataTmp += currentMaxComboCnt;

//         return dataTmp;
//     }

//     public void SaveCurrentData(string configName)
//     {
//         string currentData = CurrentDataProcess(configName);
//         DataUpdate();

//         StreamWriter contentWriter = new StreamWriter(outputFile, true);
//         contentWriter.WriteLine(currentData);
//         contentWriter.Close();
//     }

//     private string TotalDataProcess()
//     {
//         // Basic Info
//         string dataTmp = participantID.ToString() + "," + experimentSpace + "," + (this.passthrough ? "On" : "Off") + "," + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:ffffff") + "," + "total" + ",";

//         // Record Score
//         dataTmp += totalActualScore + "," + totalPerfectScore + ",";

//         // Fruit Related
//         double fruitCutRate = (double)totalFruitCutted / totalFruitSpawned;
//         double fruitMissingRate = 1 - fruitCutRate;

//         dataTmp += totalFruitCutted + "," + totalFruitSpawned + ",";
//         dataTmp += fruitCutRate + "," + fruitMissingRate + ",";

//         // Bomb Related
//         double bombCutRate = (double)totalBombCutted / totalBombSpawned;
//         double bombAvoidRate = 1 - bombCutRate;

//         dataTmp += totalBombCutted + "," + totalBombSpawned + ",";
//         dataTmp += bombCutRate + "," + bombAvoidRate + ",";

//         // Obstacle Related
//         double obstacleCutRate = (double)totalObstacleHit / totalObstacleSpawned;
//         double obstacleAvoidRate = 1 - obstacleCutRate;

//         dataTmp += totalObstacleHit + "," + totalObstacleSpawned + ",";
//         dataTmp += obstacleCutRate + "," + obstacleAvoidRate + ",";

//         // Max Combo
//         dataTmp += totalMaxComboCnt;

//         return dataTmp;
//     }

//     public void SaveTotalData()
//     {
//         string totalData = TotalDataProcess();

//         StreamWriter contentWriter = new StreamWriter(outputFile, true);
//         contentWriter.WriteLine(totalData);
//         contentWriter.Close();
//     }

// }
