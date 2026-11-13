using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;


public class exit_end : MonoBehaviour
{
    public bool inplace;
    public TextMeshProUGUI text;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (inplace==true)
        {
            text.text = "find pink key to escape!";
        }
        else 
        {
            text.text = "";
        }
    }
    private void OnTriggerEnter(Collider other)
    {

        if (other.tag == "Player")
        {
            inplace = true;
            if (Player_Man.haskey == true)
            {
                SceneManager.LoadScene(2);
            }
            
        }
        
    }
    private void OnTriggerExit(Collider other)
    {
        inplace = false;
    }
}
