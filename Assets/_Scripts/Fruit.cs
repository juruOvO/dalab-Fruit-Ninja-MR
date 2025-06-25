using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fruit : MonoBehaviour
{
    [Header("Setting")]
    [SerializeField] private int award;
    [SerializeField] private int punnishment;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private GameObject brokenPrefab;

    [SerializeField]private GameManager gameManager = null;
    [SerializeField]private DataSaver dataSaver  = null;
    
    private bool canCut;

    // Start is called before the first frame update
    void Start()
    {
        gameManager = GameObject.FindGameObjectWithTag("GM").GetComponent<GameManager>();
        dataSaver = GameObject.FindGameObjectWithTag("GM").GetComponent<DataSaver>();

        canCut = true;

        dataSaver.currentPerfectScore += award;
        dataSaver.currentFruitSpawned ++;
    }

    // Update is called once per frame
    void Update()
    {
        if (transform.position.y < -0.5f && canCut)
        {
            canCut = false;
            
            dataSaver.currentComboCnt = 0;
            dataSaver.currentActualScore -= punnishment;

            Destroy(gameObject);
        }
    }


    private void OnTriggerEnter(Collider other){
        // Debug.Log(other.tag);
        if(other.tag.Equals("Kanata") && canCut){
            canCut = false;
            Debug.Log(other.tag);
            // Update Game Data
            dataSaver.currentActualScore += award;
            dataSaver.currentFruitCutted ++;
            dataSaver.currentComboCnt++;
            dataSaver.currentMaxComboCnt = Mathf.Max(dataSaver.currentComboCnt, dataSaver.currentMaxComboCnt);

            // Generate Broken Prefabs And 
            // Record Trigger Transform
            Vector3 originalPos = transform.position;
            Quaternion originalRot = transform.rotation;
            Vector3 originalVel = GetComponent<Rigidbody>().velocity;
            Vector3 originalAV = GetComponent<Rigidbody>().angularVelocity;

            // Play Related SFX
            audioSource.Play();

            // Destroy Old Model
            Destroy(gameObject, 0.1f);

            // Left Piece
            GameObject leftInstance = Instantiate(brokenPrefab);
            if (leftInstance.GetComponent<Rigidbody>() == null)
            {
                leftInstance.AddComponent<Rigidbody>();
            }
            Rigidbody leftInstanceRigidBody = leftInstance.GetComponent<Rigidbody>();
            leftInstanceRigidBody.velocity = new Vector3(-0.3f, 0.0f, originalVel.z);
            leftInstanceRigidBody.angularVelocity = originalAV;
            leftInstance.transform.position = originalPos;
            leftInstance.transform.rotation = originalRot;

            // Right Piece
            GameObject rightInstance = Instantiate(brokenPrefab);
            if (rightInstance.GetComponent<Rigidbody>() == null)
            {
                rightInstance.AddComponent<Rigidbody>();
            }
            Rigidbody rightInstanceRigidBody = rightInstance.GetComponent<Rigidbody>();
            rightInstanceRigidBody.velocity = new Vector3(0.3f, 0.0f, originalVel.z);
            rightInstanceRigidBody.angularVelocity = originalAV;
            rightInstance.transform.position = originalPos;
            rightInstance.transform.rotation = new Quaternion(originalRot.x, originalRot.y + 180.0f, originalRot.z, originalRot.w);
        }
    }
}
