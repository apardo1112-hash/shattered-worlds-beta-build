using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SplashDetector : MonoBehaviour
{
    public GameObject SplashEffect;
    // Start is called before the first frame update
    void Start()
    {
        Instantiate(SplashEffect, transform);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
     void OnCollisionEnter(Collision collision)
    {
        
        if(collision.gameObject.CompareTag("water")){
            Debug.Log("splash");




        }



    } 
}
