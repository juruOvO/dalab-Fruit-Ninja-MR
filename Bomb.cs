using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bomb : MonoBehaviour
{

    [Header("Setting")]
    [SerializeField] private int punnishment;
    [SerializeField] private AudioSource[] bombAudios;
    [SerializeField] private ParticleSystem[] bombEffects;
     [SerializeField]private GameManager gameManager = null;
    [SerializeField]private DataSaver dataSaver  = null;
    private bool canCut;
    // Start is called before the first frame update
    void Start()
    {
        gameManager = GameObject.FindGameObjectWithTag("GM").GetComponent<GameManager>();
        dataSaver = GameObject.FindGameObjectWithTag("GM").GetComponent<DataSaver>();

        canCut = true;

        dataSaver.currentBombSpawned ++;
        
    }

    // Update is called once per frame
    void Update()
    {
        if (transform.position.y < -5.0f && canCut)
        {
            canCut = false;
            
            Destroy(gameObject);
        }
//        Debug.Log(this.GetComponent<Rigidbody>().velocity.ToString());
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag.Equals("Kanata")&& canCut)
        {
            canCut = false;
            dataSaver.currentActualScore -= punnishment;
            dataSaver.currentBombCutted ++;
            Debug.Log(other.tag + "bomb");
            // Rigidbody rigidbody = this.GetComponent<Rigidbody>();

            // if (rigidbody != null)
            // {
            //     rigidbody.useGravity = false;
            //     rigidbody.velocity = Vector3.zero;
            // }
            
            bombEffects[0].Stop();
            bombAudios[0].Stop();

            bombEffects[1].Play();
            bombAudios[1].Play();
            
            Destroy(gameObject,0.3f);
        }
    }
}
