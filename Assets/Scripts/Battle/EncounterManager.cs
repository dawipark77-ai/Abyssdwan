using UnityEngine;
using UnityEngine.SceneManagement;

public class EncounterManager : MonoBehaviour
{
    public static EncounterManager Instance;
    public string nextScene;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
    }

    public void StartBattle(string battleSceneName)
    {
        nextScene = battleSceneName;
        SceneManager.LoadScene("BattleScene");
    }
}
