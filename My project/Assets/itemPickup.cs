using UnityEngine;
using TMPro;
using System.Collections;

public class itemPickup : MonoBehaviour
{
    //you name what the item is to get the script to work
    public string itemname = "";
    //public TextMeshProUGUI text;
    public GameObject item;
    //this object has everything for the Player_Man script which handles item pickups (besides weapons)
    public Player_Man ui;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //finds object with player_man script then assigns it so you can call things from that script
        ui = FindFirstObjectByType<Player_Man>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            //calls a function that gives a notification to the player of what they picked up
            ui.textchange(itemname);
            item.SetActive(false);
        }
        
    }

}
