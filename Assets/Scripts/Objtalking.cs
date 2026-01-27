using UnityEngine;

public class Objtalking : MonoBehaviour
{
    [Header("대화창 연결 (Canvas)")]
    public GameObject dialogBox; // NPC의 Canvas를 할당
    public float displayTime = 4f; // 대화창 표시 시간(초)

    private float timerDisplay = -1f;

    void Start()
    {
        // 시작 시 대화창 숨기기
        if (dialogBox != null)
            dialogBox.SetActive(false);
    }

    void Update()
    {
        // 대화창 표시 시간 카운트
        if (timerDisplay >= 0f)
        {
            timerDisplay -= Time.deltaTime;
            if (timerDisplay < 0f && dialogBox != null)
            {
                dialogBox.SetActive(false);
            }
        }
    }

    // 플레이어가 트리거 안에 들어왔을 때 대화 시작
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            DisplayDialog();
        }
    }

    // 대화창 표시 함수
    public void DisplayDialog()
    {
        if (dialogBox != null)
        {
            dialogBox.SetActive(true);
            timerDisplay = displayTime;
        }
    }
}
