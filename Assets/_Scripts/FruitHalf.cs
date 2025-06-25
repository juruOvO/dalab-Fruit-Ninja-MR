using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FruitHalf : MonoBehaviour
{
    void Update(){
        if (transform.position.y < -0.5f )
        {
            Destroy(gameObject);
        }
    }
    
}