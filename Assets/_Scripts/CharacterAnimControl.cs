using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class CharacterAnimControl : MonoBehaviour
{
    private GameManager gameManager;
    private DataSaver dataSaver;

    private int comboCnt;
    public int threshold = 3;

    public List<CharacterEvent> characterEvents = new List<CharacterEvent>();
    void Start()
    {
        gameManager = GameObject.FindGameObjectWithTag("GM").GetComponent<GameManager>();
        if (gameManager)
        {
            dataSaver = gameManager.GetComponent<DataSaver>();
        }
        else
        { 
            Debug.LogError("GameManager not found!");
        }
    }

    void Update()
    {
        comboCnt = dataSaver.currentComboCnt % threshold;
        if (comboCnt==0&&dataSaver.currentComboCnt>0)
        {
            foreach (CharacterEvent characterEvent in characterEvents)
            {
                if (characterEvent._index != -1)
                {
                    if (characterEvent.DoOnce == false || characterEvent.isTriggered == false)
                    { 
                        characterEvent.TriggerEvent.Invoke();
                    }
                }
            }
        }
    }
}

[Serializable]
public class CharacterEvent
{
    public int _index = -1;
    public UnityEvent TriggerEvent;
    public bool isTriggered = false;
    public bool DoOnce = false;
};