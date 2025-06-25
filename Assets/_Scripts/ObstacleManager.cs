using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleManager : MonoBehaviour
{
    [Header("Generator's Settings")]
    [SerializeField] private float zMinVelocity;
    [SerializeField] private float zMaxVelocity;
    [SerializeField][Range(0.0f, 10.0f)] private float genrateIntetval;

    [SerializeField] private GameObject prefab;

    private GameManager gameManager = null;
    private DataLoader dataLoader = null;
    private DataSaver dataSaver = null;



    void Start()
    {
        gameManager = GameObject.FindGameObjectWithTag("GM").GetComponent<GameManager>();
        dataLoader = GameObject.FindGameObjectWithTag("GM").GetComponent<DataLoader>();
        dataSaver = GameObject.FindGameObjectWithTag("GM").GetComponent<DataSaver>();

        StartCoroutine(GenerateObstacle());
    }

    IEnumerator GenerateObstacle()
    {

        while (!gameManager.IsGameOver())
        {
            GameObject instance = Instantiate(prefab);
            if (instance.GetComponent<Rigidbody>() == null)
            {
                instance.AddComponent<Rigidbody>();
            }
            Rigidbody instanceRigidBody = instance.GetComponent<Rigidbody>();
            instanceRigidBody.velocity = new Vector3(0.0f,0.0f,Random.Range(zMinVelocity, zMaxVelocity));

            instance.transform.position = this.transform.position;



            yield return new WaitForSeconds(genrateIntetval);
            //StopCoroutine(GenerateObject());
        }
        Debug.Log("-----Stop Generate Obstacle-----");
    }


}