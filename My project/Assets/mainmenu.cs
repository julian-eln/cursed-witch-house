using UnityEngine;
using UnityEngine.SceneManagement;

public class mainmenu : MonoBehaviour
{
    public void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void main()
    {
        SceneManager.LoadScene(0);

    }
    public void credits() 
    {
        SceneManager.LoadScene(2);
    }
    public void play()
    {
        SceneManager.LoadScene(1);
    }
    public void exit()
    {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
}
