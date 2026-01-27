using UnityEngine;

/// <summary>
/// 직업 테스트용 컴포넌트
/// 런타임에서 직업을 쉽게 변경할 수 있도록 도와줍니다
/// </summary>
public class ClassTester : MonoBehaviour
{
    [Header("테스트 설정")]
    [Tooltip("직업을 변경할 PlayerStats 컴포넌트 (비워두면 자동으로 찾음)")]
    public PlayerStats targetPlayer;

    [Header("직업 선택")]
    [Tooltip("현재 선택된 직업")]
    public string selectedClass = "Warrior";

    void Start()
    {
        // 타겟이 없으면 자동으로 찾기
        if (targetPlayer == null)
        {
            targetPlayer = FindObjectOfType<PlayerStats>();
            if (targetPlayer != null)
            {
                Debug.Log($"[ClassTester] PlayerStats 자동 발견: {targetPlayer.playerName}");
            }
        }

        // 초기 직업 설정
        if (targetPlayer != null && !string.IsNullOrEmpty(selectedClass))
        {
            ChangeClass(selectedClass);
        }
    }

    /// <summary>
    /// 직업 변경 (코드에서 호출 가능)
    /// </summary>
    public void ChangeClass(string className)
    {
        if (targetPlayer == null)
        {
            Debug.LogWarning("[ClassTester] PlayerStats가 없습니다!");
            return;
        }

        selectedClass = className;
        targetPlayer.SetClass(className);
        
        // GameManager에도 저장 (스탯창 업데이트용)
        var gm = GameManager.EnsureInstance();
        if (gm != null)
        {
            gm.SaveFromPlayer(targetPlayer);
        }

        Debug.Log($"[ClassTester] 직업 변경됨: {className}");
    }

    /// <summary>
    /// 워리어로 변경
    /// </summary>
    [ContextMenu("직업 변경: Warrior")]
    public void SetWarrior() => ChangeClass("Warrior");

    /// <summary>
    /// 도적으로 변경
    /// </summary>
    [ContextMenu("직업 변경: Thief")]
    public void SetThief() => ChangeClass("Thief");

    /// <summary>
    /// 위저드로 변경
    /// </summary>
    [ContextMenu("직업 변경: Wizard")]
    public void SetWizard() => ChangeClass("Wizard");

    void OnValidate()
    {
        // 인스펙터에서 selectedClass가 변경되면 자동 적용
        if (Application.isPlaying && targetPlayer != null && !string.IsNullOrEmpty(selectedClass))
        {
            ChangeClass(selectedClass);
        }
    }
}



