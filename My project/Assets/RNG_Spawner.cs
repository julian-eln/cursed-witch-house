using UnityEngine;
using Random = UnityEngine.Random;
public class RNG_Spawner : MonoBehaviour
{
    public GameObject[] options; // maintain an array field, plug in all options in the Inspector
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (options == null || options.Length == 0) return;

    }


    // Update is called once per frame
    void Update()
    {
        
    }
    public void spawn()
    {
        int randomInt = Random.Range(1, 101);
        GameObject go;
        //15%
        if (randomInt is >= 41 and <= 81)
            //gold 
            go = options[2];
        //40%
        else if (randomInt is >= 85 and <= 100)
            //iron
            go = options[1];
        //5% draw
        else if (randomInt<40)
            //unobtaniumn
            go = options[0];
        //50%
        else
           //lead
            go = options[3];
        


        Instantiate(go, transform.position, transform.rotation);
    }

}
