using UnityEngine;
using System;

/// <summary>
/// 던전 이벤트 관리자
/// 방에 들어갔을 때 발생하는 이벤트 처리
/// </summary>
public class DungeonEventManager : MonoBehaviour
{
    [Header("이벤트 설정")]
    [SerializeField] private bool enableEnemyEncounters = true;
    [SerializeField] private bool enableTreasureCollection = true;
    
    public static DungeonEventManager Instance { get; private set; }
    
    // 이벤트 델리게이트
    public event Action<DungeonRoom> OnRoomEntered;
    public event Action<DungeonRoom> OnEnemyEncountered;
    public event Action<DungeonRoom> OnTreasureFound;
    public event Action<DungeonRoom> OnExitReached;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    /// <summary>
    /// 방 진입 이벤트 처리
    /// </summary>
    public void HandleRoomEntered(DungeonRoom room)
    {
        if (room == null) return;
        
        OnRoomEntered?.Invoke(room);
        
        // 적 조우
        if (enableEnemyEncounters && room.hasEnemy && !room.isExplored)
        {
            OnEnemyEncountered?.Invoke(room);
            Debug.Log($"적을 만났습니다! 방 위치: {room.position}");
            // 여기에 전투 시스템 연결
        }
        
        // 보물 발견
        if (enableTreasureCollection && room.hasTreasure && !room.isExplored)
        {
            OnTreasureFound?.Invoke(room);
            Debug.Log($"보물을 발견했습니다! 방 위치: {room.position}");
            // 보물 획득 처리
            CollectTreasure(room);
        }
        
        // 출구 도달
        if (room.hasExit)
        {
            OnExitReached?.Invoke(room);
            Debug.Log($"출구에 도달했습니다! 방 위치: {room.position}");
        }
        
        room.Explore();
    }
    
    /// <summary>
    /// 보물 수집 처리
    /// </summary>
    private void CollectTreasure(DungeonRoom room)
    {
        if (room.hasTreasure)
        {
            room.hasTreasure = false;
            // 여기에 보상 시스템 연결 (골드, 아이템 등)
        }
    }
    
    /// <summary>
    /// 적 처치 처리
    /// </summary>
    public void HandleEnemyDefeated(DungeonRoom room)
    {
        if (room != null && room.hasEnemy)
        {
            room.hasEnemy = false;
            Debug.Log($"적을 처치했습니다! 방 위치: {room.position}");
        }
    }
}






