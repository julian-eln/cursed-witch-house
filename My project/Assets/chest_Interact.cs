using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;
using System;

public class chest_Interact : MonoBehaviour
{
    public RNG_Spawner rng_Spawner;
    public GameObject ebutton;
    public bool collected=false;
    public bool playerinrange;

    void Update()
    {
        if (Input.GetKeyDown("e") && collected == false && playerinrange == true)
        {
            rng_Spawner.spawn();
            collection();
        }


    }
    private void OnTriggerEnter(Collider other)
    {
        playerinrange = true;
 
        if(collected==false)
            ebutton.SetActive(true);

    }
    private void OnTriggerExit(Collider other)
    {
        ebutton.SetActive(false);
        playerinrange = false;

    }
    void collection()
    {
        collected = true;
        ebutton.SetActive(false);
    }
    
}
