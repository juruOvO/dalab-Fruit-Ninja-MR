using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class UIManager : MonoBehaviour
{
    private ObjectSpawner objectSpawner;
    private GameManager gameManager;
    private DataSaver dataSaver;
    [Header("Components")]
    // [SerializeField] private GameObject info;
    // [SerializeField] private GameObject info2;
    // [SerializeField] private GameObject info3;

    [Header("Items")]
    // [SerializeField] private TextMeshProUGUI turns;
    // [SerializeField] private TextMeshProUGUI rounds;
    // [SerializeField] private TextMeshProUGUI time;
    [SerializeField] private TextMeshProUGUI score;
    [SerializeField] private GameObject gameOver;
    [SerializeField] private TextMeshProUGUI combo;
    [SerializeField] private GameObject startGamePanel; // 新增：开始游戏面板
    [SerializeField] private Button startGameButton; // 新增：开始游戏按钮
    // [SerializeField] private TextMeshProUGUI gameParams;

    // [SerializeField] private TextMeshProUGUI endTime;
    // [SerializeField] private TextMeshProUGUI endScore;


    void Start()
    {
        objectSpawner = GameObject.FindGameObjectWithTag("Generator").GetComponent<ObjectSpawner>();
        gameManager = GameObject.FindGameObjectWithTag("GM").GetComponent<GameManager>();
        dataSaver = GameObject.FindGameObjectWithTag("GM").GetComponent<DataSaver>();

        // 初始化开始游戏按钮
        if (startGameButton != null)
        {
            startGameButton.onClick.AddListener(OnStartGameButtonClicked);
        }

        // 确保开始游戏面板在游戏开始时显示
        if (startGamePanel != null)
        {
            startGamePanel.SetActive(true);
        }

        // info.SetActive(true);
        // info2.SetActive(true);
        // info3.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        // 如果游戏还没开始，显示开始游戏面板，隐藏其他UI
        if (!gameManager.IsGameStarted())
        {
            if (startGamePanel != null)
            {
                startGamePanel.SetActive(true);
            }
            if (gameOver != null)
            {
                gameOver.SetActive(false);
            }
            return; // 游戏未开始时不更新其他UI
        }

        if (!gameManager.IsGameOver())
        {
            gameOver.SetActive(false);
            // 游戏开始后隐藏开始游戏面板
            if (startGamePanel != null)
            {
                startGamePanel.SetActive(false);
            }
            
            // GAME INFO
            // turns.text = objectSpawner.TurnRatio[0].ToString() + " / " + objectSpawner.TurnRatio[1].ToString();
            // rounds.text = objectSpawner.RoundRatio[0].ToString("00") + " / " + objectSpawner.RoundRatio[1].ToString("00");

            TimeSpan elapsedTime = DateTime.Now - dataSaver.GetStartTime();
            // time.text = string.Format("{0:D2}:{1:D2}:{2:D4}", elapsedTime.Minutes, elapsedTime.Seconds, elapsedTime.Milliseconds);

            score.text = dataSaver.score.ToString("0000");

            combo.text = dataSaver.currentComboCnt != 0 ? ("x " + dataSaver.currentComboCnt.ToString("000") + "!") : "";

            // GAME PARAMS
            // gameParams.text = gameManager.GetPariticipantID() + "\t" + (gameManager.GetPassthrough() ? "On" : "Off");
        }
        else
        {
            // info.SetActive(false);
            // info2.SetActive(false);
            // info3.SetActive(true);

            // endTime.text = time.text;
            // endScore.text = score.text;
            gameOver.SetActive(true);
        }
    }

    /// <summary>
    /// 开始游戏按钮点击事件
    /// </summary>
    private void OnStartGameButtonClicked()
    {
        gameManager.StartGame();
        Debug.Log("Start game button clicked!");
    }
}
