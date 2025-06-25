using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectSpawner : MonoBehaviour
{
    [Header("Generator's Settings")]
    [SerializeField] private float xMinVelocity;
    [SerializeField] private float xMaxVelocity;
    [SerializeField] private float yMinVelocity;
    [SerializeField] private float yMaxVelocity;
    

    [SerializeField] private float zVelocity;

    [SerializeField] private float angularVelocityRange;

    [SerializeField][Range(0.0f,10.0f)] private float genrateIntetval;

    [SerializeField] private GameObject[] prefabs;

    private GameManager gameManager = null;
    private DataLoader dataLoader  = null;
    private DataSaver dataSaver = null;

    private List<(string name, int[][] data)> dataList;


    private int totalTurns;
    private int currentTurn;

    private int totalRounds;
    private int currentRound;

    void Start(){
        gameManager = GameObject.FindGameObjectWithTag("GM").GetComponent<GameManager>();
        dataLoader = GameObject.FindGameObjectWithTag("GM").GetComponent<DataLoader>();
        dataSaver = GameObject.FindGameObjectWithTag("GM").GetComponent<DataSaver>();

        dataList = dataLoader.GetDataList();
        if(dataList!=null){
            Debug.Log("Data List Get Successfully!");

            totalTurns = dataList.Count;
            currentTurn = 0;
            Debug.Log(dataList.Count);

            StartCoroutine(GenerateObject());
        }
    }
    

    IEnumerator GenerateObject(){
        while(!gameManager.IsGameOver()){
            // Game Turn -> Configs Number
            if(currentTurn < totalTurns){
                // 
                Debug.Log("CURRENT GAME TURN "+currentTurn);

                int[][] data = dataList[currentTurn].data;

                totalRounds = data.Length;
                currentRound = 0;

                while(currentRound<totalRounds){    
                    for(int col = 0; col< data[0].Length;col++){
                        int id = col; // Prefab's Index
                        int num = data[currentRound][col]; // Instance Number
                        for(int i = 0; i< num;i++){
                            // Debug.Log(col+"---"+ i+"/"+num);
                            InstantiateObject(id);
                            // Debug.Log(prefabs[id].name=="Bomb"?"Bomb":"");
                            // Offset in one round
                            yield return new WaitForSeconds(Random.Range(0.0f,0.2f));
                        }
                    }

                    currentRound++;
                    yield return new WaitForSeconds(genrateIntetval);
                }

                string configName = dataList[currentTurn].name;
                dataSaver.SaveCurrentData(configName);
                Debug.Log("-----Save: " + configName);
                currentTurn++;
                yield return new WaitForSeconds(genrateIntetval);
            }
            else{
                yield return new WaitForSeconds(genrateIntetval * 2);

                gameManager.IsGameOver(true);
                dataSaver.SaveTotalData();
                Debug.Log("-----Game Over-----");
                //StopCoroutine(GenerateObject());
            }
        }
    }

    private void InstantiateObject(int id){
        GameObject objectInstance = Instantiate(prefabs[id]);
        // Add Rigidbody
        if (objectInstance.GetComponent<Rigidbody>() == null)
        {
            objectInstance.AddComponent<Rigidbody>();
        }
        Rigidbody objectInstanceRigidBody = objectInstance.GetComponent<Rigidbody>();
        
        // Add Velocity 
        objectInstanceRigidBody.velocity = new Vector3(Random.Range(xMinVelocity, xMaxVelocity), Random.Range(yMinVelocity, yMaxVelocity), zVelocity);

//        Debug.Log(prefabs[id].name=="Bomb"?objectInstanceRigidBody.velocity.ToString()+"zVelocity:"+zVelocity:"");

        objectInstanceRigidBody.angularVelocity = new Vector3(Random.Range(-angularVelocityRange, angularVelocityRange), 0.0f, Random.Range(-angularVelocityRange, angularVelocityRange));

        objectInstanceRigidBody.useGravity = true;

        objectInstanceRigidBody.transform.position = this.transform.position;
    }

    public int[] RoundRatio => new int[2]{currentRound, totalRounds};
    public int[] TurnRatio => new int[2]{currentTurn, totalTurns};
}