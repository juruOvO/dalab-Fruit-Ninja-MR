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
    // [SerializeField] private TextMeshProUGUI gameParams;

    // [SerializeField] private TextMeshProUGUI endTime;
    // [SerializeField] private TextMeshProUGUI endScore;


    void Start()
    {
        objectSpawner = GameObject.FindGameObjectWithTag("Generator").GetComponent<ObjectSpawner>();
        gameManager = GameObject.FindGameObjectWithTag("GM").GetComponent<GameManager>();
        dataSaver = GameObject.FindGameObjectWithTag("GM").GetComponent<DataSaver>();

        // info.SetActive(true);
        // info2.SetActive(true);
        // info3.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (!gameManager.IsGameOver())
        {
            gameOver.SetActive(false);
            // GAME INFO
            // turns.text = objectSpawner.TurnRatio[0].ToString() + " / " + objectSpawner.TurnRatio[1].ToString();
            // rounds.text = objectSpawner.RoundRatio[0].ToString("00") + " / " + objectSpawner.RoundRatio[1].ToString("00");

            // TimeSpan elapsedTime = DateTime.Now - dataSaver.GetStartTime();
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
}
