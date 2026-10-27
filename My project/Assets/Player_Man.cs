/*
 This script is different from inven_Man as it only handles item pick up and interacts with itemPickup script
*/
using UnityEngine;
using TMPro;
using System.Collections;

public class Player_Man : MonoBehaviour
{
    public TextMeshProUGUI text;
    public string itemname;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    //this will change the text when called
    public  void textchange(string name) 
    {
        itemname = name;
        StartCoroutine(pickedup()); 
        
    }
    IEnumerator pickedup()
    {
        //will add one more +1 (wtv item) into the text field showing the player how much and what they picked up
        text.text=text.text + "+1 " + itemname+" ";
        yield return new WaitForSeconds(3);
        //resets text
        text.text = "";
        

    }
}
