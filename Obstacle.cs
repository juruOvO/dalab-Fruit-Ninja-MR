using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Obstacle : MonoBehaviour
{

    [Header("Setting")]
    [SerializeField] private int award;
    [SerializeField] private int punnishment;
    [SerializeField] private GameManager gameManager = null;
    [SerializeField] private DataSaver dataSaver = null;
    private bool canHit;
    // Start is called before the first frame update
    void Start()
    {
        gameManager = GameObject.FindGameObjectWithTag("GM").GetComponent<GameManager>();
        dataSaver = GameObject.FindGameObjectWithTag("GM").GetComponent<DataSaver>();

        canHit = true;

        dataSaver.currentObstacleSpawned++;

    }

    void Update()
    {
        if (transform.position.z < -5.0f && canHit)
        {
            canHit = false;

            dataSaver.currentActualScore += award;

            Destroy(gameObject);
        }
       // Debug.Log(this.GetComponent<Rigidbody>().velocity.ToString());
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag.Equals("Player") && canHit)
        {
            //Debug.Log("HIT HEAD");
            canHit = false;
            
            dataSaver.currentActualScore -= punnishment;
            dataSaver.currentObstacleHit++;

            gameManager.IsHit();

            Destroy(gameObject);
        }
    }
}