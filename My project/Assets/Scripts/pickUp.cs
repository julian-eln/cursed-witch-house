using UnityEngine;

public class pickUp : MonoBehaviour
{
    //calls inventory management 
    public inven_Man inven_man;
    public int weapon_Ind;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnTriggerEnter(Collider other)
    {

        if (weapon_Ind == 1)
        {
            inven_man.batEquip = true;
        }
        else if (weapon_Ind == 2)
        {
            inven_man.swordEquip = true;

        }
    }
}
