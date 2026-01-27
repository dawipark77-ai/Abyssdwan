using UnityEngine;
using UnityEngine.SceneManagement;

public class WorldToTown : MonoBehaviour
{
    public string townSceneName = "Genesis 05 town"; // 이동할 씬 이름

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            SceneManager.LoadScene(townSceneName); // 플레이어가 트리거에 들어가면 씬 로드
        }
    }
}
