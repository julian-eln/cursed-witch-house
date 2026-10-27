using UnityEngine;

public class inven_Man : MonoBehaviour
{
    //controls the weapon in fps controller 1
    public GameObject Sword1;
    //controls the weapon in fps controller 2
   //public GameObject Sword2;
    public GameObject Bat1;
    //public GameObject bat2;
    public bool swordEquip;
    public bool batEquip;
    //this array keeps track of every weapon in inventory(0=bat,1=sword)
    bool[] weap = new bool[2];

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //if the sword has been picked up it will reflect in the array 
        if(swordEquip==true)
        {
            weap[1] = true;
        }
        //if bat is picked up it will reflect in the array
        if (batEquip==true)
        {
            weap[0] = true;
        }
        //when one key is pressed equip bat
        if(Input.GetKeyDown("1")&&weap[0]==true)
        {
            //calls function that equips bat
            batEquipped();
        }
        //when two key is pressed equip sword
        if (Input.GetKeyDown("2") && weap[1] == true)
        {
            //calls function that equips sword
            swordEquipped();
        }

    }
    //equips sword disables everything else
    void swordEquipped()
    {
        Bat1.SetActive(false);
        //bat2.SetActive(false);
        Sword1.SetActive(true);
        //Sword2.SetActive(true);
    }

    //equips bat disables everything else 
    void batEquipped()
    {
        Bat1.SetActive(true);
        //bat2.SetActive(true);
        Sword1.SetActive(false);
        //Sword2.SetActive(false);

    }
}
