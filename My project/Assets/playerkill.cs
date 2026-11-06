using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
public class playerkill : MonoBehaviour
{
    public GameObject red;
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
        Debug.Log("hit1");
        if (other.tag == "Player")
        {
            Debug.Log("playerhit");

            StartCoroutine(kill());
        }
    }
    IEnumerator kill()
    {
        yield return 1;
        red.SetActive(true);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
