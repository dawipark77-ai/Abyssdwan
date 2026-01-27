using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class Title : MonoBehaviour
{
    // Use this for initialization
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Click()
    {
        // ¾À ÀÎµ¦½º 1¹øÀ» ºÒ·¯¿È
        SceneManager.LoadScene(1);
    }
}
