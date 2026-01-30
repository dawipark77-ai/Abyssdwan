using UnityEngine;

/// <summary>
/// 플레이어의 런타임 상태 데이터를 저장하는 ScriptableObject
/// 중요: 이 에셋은 게임 실행 중 실시간으로 업데이트됩니다!
/// HP/MP가 변경될 때마다 PlayerStats의 setter에서 이 에셋에 자동 저장됩니다.
/// </summary>
[CreateAssetMenu(fileName = "NewPlayerStatData", menuName = "MyRPG/PlayerData Asset", order = 1)]
public class PlayerStatData : ScriptableObject
{
    [Header("런타임 데이터 (실시간 저장)")]
    [Tooltip("현재 HP - 실시간으로 저장됨")]
    public int currentHP;

    [Tooltip("현재 MP - 실시간으로 저장됨")]
    public int currentMP;

    [Tooltip("레벨 - 실시간으로 저장됨")]
    public int level = 1;

    [Tooltip("경험치 - 실시간으로 저장됨")]
    public int exp = 0;
}