using UnityEngine;

public class StatusMenuToggle : MonoBehaviour
{
    public GameObject menuPanel;

    public void Toggle()
    {
        if (menuPanel == null) return;
        bool next = !menuPanel.activeSelf;
        menuPanel.SetActive(next);
    }

    public void Hide()  // 닫기 버튼 쓰고 싶을 때
    {
        if (menuPanel == null) return;
        menuPanel.SetActive(false);
    }
}