using UnityEngine;
using UnityEngine.SceneManagement;

public class TownEntrance : MonoBehaviour
{
    public string townSceneName = "Town"; // 전환할 씬 이름

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Player")) // 플레이어 태그 확인
        {
            if (Input.GetKeyDown(KeyCode.E)) // E키로 진입
            {
                SceneManager.LoadScene(townSceneName);
            }
        }
    }
}
