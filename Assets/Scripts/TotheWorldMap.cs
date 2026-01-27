using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TotheWorldMap : MonoBehaviour
{
    [Header("UI")]
    public GameObject dialogBackground; // 화면 반투명 배경 Panel
    public GameObject uiPanel;          // 선택창 Panel
    public Button yesButton;
    public Button noButton;

    [Header("이동할 씬 이름")]
    public string targetScene;          // 이동할 씬 이름 (예: "WorldMap")

    void Start()
    {
        // 처음에는 UI 모두 숨기기
        dialogBackground.SetActive(false);
        uiPanel.SetActive(false);

        // 버튼 클릭 이벤트 등록
        yesButton.onClick.AddListener(OnYesButtonClicked);
        noButton.onClick.AddListener(OnNoButtonClicked);
    }

    // 플레이어가 트리거에 들어올 때
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            dialogBackground.SetActive(true); // 배경 켜기
            uiPanel.SetActive(true);          // 선택 UI 켜기
        }
    }

    // 플레이어가 트리거에서 나갈 때
    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            dialogBackground.SetActive(false); // 배경 끄기
            uiPanel.SetActive(false);          // 선택 UI 끄기
        }
    }

    // Yes 버튼 클릭
    void OnYesButtonClicked()
    {
        if (!string.IsNullOrEmpty(targetScene))
        {
            SceneManager.LoadScene(targetScene);
        }
        else
        {
            Debug.LogError("Genesis 05 World map");
        }
    }

    // No 버튼 클릭
    void OnNoButtonClicked()
    {
        dialogBackground.SetActive(false);
        uiPanel.SetActive(false);
    }
}
