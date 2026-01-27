using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class Main : MonoBehaviour
{
    // 초기화용 메서드
    void Start()
    {
        // 필요시 초기화 코드 작성
    }

    // 매 프레임마다 실행되는 메서드
    void Update()
    {
        // 매 프레임 실행할 코드가 없으면 비워둬도 됩니다
    }

    // 버튼 클릭 시 씬 전환용 메서드
    public void Click()
    {
        // Build Settings에서 0번 씬으로 전환
        SceneManager.LoadScene(0);
    }
}
