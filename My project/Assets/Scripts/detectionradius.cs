using UnityEngine;

public class detectionradius : MonoBehaviour
{
    public Transform Target;
    public pathFinding Pathfinding;
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
        Debug.Log("hit");

       
        if (other.CompareTag("Player"))
        {
            Target = other.transform;
            Pathfinding.chase = true;
        }
        else if (other.CompareTag("Intrest"))
        {
            Target = other.transform;
            Pathfinding.pointOfInt = true;
            Debug.Log("hit");
        }
    }

}
