using UnityEngine;

public class NonPlayerCharacter : MonoBehaviour
{
    [Header("Dialog Settings")]
    public GameObject dialogBox;   // 대화 UI (Canvas 안에 Panel/Text 등)
    public float displayTime = 4f; // 대화창 유지 시간

    private float timerDisplay = -1f;

    void Start()
    {
        if (dialogBox != null) dialogBox.SetActive(false);
        timerDisplay = -1f;
    }

    void Update()
    {
        if (timerDisplay >= 0f)
        {
            timerDisplay -= Time.deltaTime;
            if (timerDisplay < 0f && dialogBox != null)
            {
                dialogBox.SetActive(false);
            }
        }
    }

    public void DisplayDialog()
    {
        if (dialogBox != null)
        {
            dialogBox.SetActive(true);
            timerDisplay = displayTime;
        }
        else
        {
            Debug.LogWarning("DialogBox가 NPC에 연결되지 않았습니다: " + gameObject.name);
        }
    }
}
