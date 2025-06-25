using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataLoader : MonoBehaviour
{
    [Header("Related Files Setting")]
    [SerializeField] private List<TextAsset> inputFiles;
    private List<(string name, int[][] data)> dataList;

    void Awake(){
        if(inputFiles.Count!=0){
            dataList = new List<(string name, int[][] data)>();
            LoadDatas();
        }
    }

    public void LoadDatas(){
        // Shuffle the Order of the Datas
        while(inputFiles.Count>0){
            int idx = Random.Range(0, inputFiles.Count); // Next loaded sequence
            int[][] dataTmp = LoadData(idx);
            
            if(dataTmp!=null){
                dataList.Add((inputFiles[idx].name,dataTmp));
                Debug.Log("[NOTE] LOAD "+inputFiles[idx].name +" SUCCESSFULLY");
            }
            inputFiles.RemoveAt(idx);
        }
    }

    private int[][] LoadData(int idx){
        string[] rows = inputFiles[idx].text.Trim().Split("\n");
        string header = rows[0];

        int rowNum = rows.Length-1;// Exclude the header
        int colNum = header.Split(";").Length;

        int[][] data = new int[rowNum][];

        for (int i = 1; i < rows.Length; i++)
        {
            data[i-1] = new int[colNum];
            string[] colTmp = rows[i].Split(';'); ;
            for (int j = 0; j < colNum; j++)
            {
                int value = int.Parse(colTmp[j]);
                data[i-1][j] = value;
            }
        }

        return data;
    }

    public List<(string name, int[][] data)> GetDataList()
    {
        return dataList;
    }


    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
