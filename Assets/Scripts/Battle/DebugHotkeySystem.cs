using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 디버그 핫키 시스템: F1~F10으로 게임 상태 전환, 적 소환, 테스트 기능 실행
/// </summary>
public class DebugHotkeySystem : MonoBehaviour
{
    [Header("References")]
    public BattleManager battleManager;
    public PlayerStats playerStats;
    public List<EnemyStats> enemyPrefabs = new List<EnemyStats>(); // 인스펙터에서 할당할 적 프리팹들
    
    [Header("Spawn Settings")]
    public Transform spawnCenter; // 적 생성 중심 위치
    public float spawnOffset = 2f; // 적 간격
    
    [Header("Debug Settings")]
    public bool enableDebugMode = true; // 디버그 모드 켜기/끄기
    
    private List<EnemyStats> spawnedEnemies = new List<EnemyStats>();
    
    // 게임 상태 열거형
    public enum GameState
    {
        Exploration,  // 탐색 모드
        Battle,       // 전투 모드
        Menu,         // 메뉴(스테이터스/장비/스킬 창)
        Paused        // 일시정지
    }
    
    private GameState currentState = GameState.Exploration;
    
    void Awake()
    {
        if (battleManager == null)
        {
            battleManager = Object.FindObjectOfType<BattleManager>();
        }
    }

    void Update()
    {
        if (!enableDebugMode) return;
        
        // 핫키 처리
        if (Input.GetKeyDown(KeyCode.F1))
        {
            SwitchToExplorationMode();
        }
        else if (Input.GetKeyDown(KeyCode.F2))
        {
            StartBattle();
        }
        else if (Input.GetKeyDown(KeyCode.F3))
        {
            SpawnRandomEnemy();
        }
        else if (Input.GetKeyDown(KeyCode.F4))
        {
            SpawnMultipleEnemies();
        }
        else if (Input.GetKeyDown(KeyCode.F5))
        {
            RestartBattleWithRandomEnemy();
        }
        else if (Input.GetKeyDown(KeyCode.F6))
        {
            HealPlayer();
        }
        else if (Input.GetKeyDown(KeyCode.F7))
        {
            DamagePlayer(10);
        }
        else if (Input.GetKeyDown(KeyCode.F8))
        {
            ResetBattle();
        }
        else if (Input.GetKeyDown(KeyCode.F9))
        {
            TogglePause();
        }
        else if (Input.GetKeyDown(KeyCode.F10))
        {
            LogCurrentStatus();
        }
        
        // 추가: 숫자키로 특정 적 소환 (1~9)
        for (int i = 1; i <= 9; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha0 + i) && enemyPrefabs.Count >= i)
            {
                SpawnEnemyByIndex(i - 1);
            }
        }
    }
    
    // F1: 탐색 모드로 전환
    private void SwitchToExplorationMode()
    {
        currentState = GameState.Exploration;
        if (battleManager != null)
        {
            battleManager.actionPanel.SetActive(false);
            battleManager.skillPanel.SetActive(false);
            battleManager.battleEnded = true;
        }
        Debug.Log("[DEBUG] Switched to Exploration Mode");
    }
    
    // F2: 전투 시작
    private void StartBattle()
    {
        currentState = GameState.Battle;
        if (battleManager != null)
        {
            battleManager.battleEnded = false;
            battleManager.StartBattle();
        }
        Debug.Log("[DEBUG] Battle Started");
    }
    
    // F3: 랜덤 적 1마리 소환
    private void SpawnRandomEnemy()
    {
        if (enemyPrefabs == null || enemyPrefabs.Count == 0)
        {
            Debug.LogWarning("[DEBUG] No enemy prefabs assigned!");
            return;
        }
        
        EnemyStats randomEnemy = enemyPrefabs[Random.Range(0, enemyPrefabs.Count)];
        SpawnEnemyAtPosition(randomEnemy, GetSpawnPosition(0));
        Debug.Log($"[DEBUG] Spawned random enemy: {randomEnemy.enemyName}");
    }
    
    // F4: 랜덤 적 1~4마리 소환 (중앙 대칭 배치)
    private void SpawnMultipleEnemies()
    {
        if (enemyPrefabs == null || enemyPrefabs.Count == 0)
        {
            Debug.LogWarning("[DEBUG] No enemy prefabs assigned!");
            return;
        }
        
        ClearAllEnemies(); // 기존 적 제거
        
        int enemyCount = Random.Range(1, 5); // 1~4마리
        Vector3 center = spawnCenter != null ? spawnCenter.position : Vector3.zero;
        
        for (int i = 0; i < enemyCount; i++)
        {
            EnemyStats randomEnemy = enemyPrefabs[Random.Range(0, enemyPrefabs.Count)];
            Vector3 pos = GetSymmetricPosition(center, enemyCount, i);
            SpawnEnemyAtPosition(randomEnemy, pos);
        }
        
        Debug.Log($"[DEBUG] Spawned {enemyCount} enemies (symmetric around center)");
    }
    
    // F5: 모든 적 제거
    private void ClearAllEnemies()
    {
        foreach (var enemy in spawnedEnemies)
        {
            if (enemy != null)
                Destroy(enemy.gameObject);
        }
        spawnedEnemies.Clear();
        Debug.Log("[DEBUG] Cleared all enemies");
    }
    
    // F6: 플레이어 회복
    private void HealPlayer()
    {
        if (playerStats != null)
        {
            playerStats.Heal(999); // 완전 회복
            Debug.Log($"[DEBUG] Player healed. HP: {playerStats.currentHP}/{playerStats.maxHP}");
        }
    }
    
    // F7: 플레이어 데미지
    private void DamagePlayer(int damage)
    {
        if (playerStats != null)
        {
            playerStats.TakeDamage(damage);
            Debug.Log($"[DEBUG] Player took {damage} damage. HP: {playerStats.currentHP}/{playerStats.maxHP}");
        }
    }
    
    // F8: 전투 리셋
    private void ResetBattle()
    {
        ClearAllEnemies();
        if (battleManager != null)
        {
            battleManager.battleEnded = false;
            battleManager.playerTurn = true;
        }
        if (playerStats != null)
        {
            playerStats.currentHP = playerStats.maxHP;
            playerStats.currentMP = playerStats.maxMP;
        }
        Debug.Log("[DEBUG] Battle reset");
    }

    // F5: 전투 전체 리셋 후 새로운 적 랜덤 소환
    private void RestartBattleWithRandomEnemy()
    {
        Debug.Log("[DEBUG] Restarting battle (F5)");

        // 기본 리셋 로직 수행
        ResetBattle();

        if (battleManager == null)
        {
            battleManager = Object.FindObjectOfType<BattleManager>();
        }

        // BattleManager가 있으면 새 전투 시작 (EnemyDatabase를 통해 랜덤 적 소환)
        if (battleManager != null)
        {
            battleManager.StopAllCoroutines();
            battleManager.StartBattle();
        }
        else
        {
            // BattleManager가 없으면 디버그용 프리팹에서 랜덤 적 소환
            ClearAllEnemies();
            SpawnRandomEnemy();
        }
    }
    
    // F9: 일시정지 토글
    private void TogglePause()
    {
        if (Time.timeScale > 0f)
        {
            Time.timeScale = 0f;
            currentState = GameState.Paused;
            Debug.Log("[DEBUG] Game Paused");
        }
        else
        {
            Time.timeScale = 1f;
            Debug.Log("[DEBUG] Game Resumed");
        }
    }
    
    // F10: 현재 상태 로그
    private void LogCurrentStatus()
    {
        Debug.Log("=== DEBUG STATUS ===");
        Debug.Log($"Game State: {currentState}");
        if (playerStats != null)
        {
            Debug.Log($"Player HP: {playerStats.currentHP}/{playerStats.maxHP}, MP: {playerStats.currentMP}/{playerStats.maxMP}");
            Debug.Log($"Player ATK: {playerStats.attack}, DEF: {playerStats.defense}, MAG: {playerStats.magic}");
        }
        Debug.Log($"Spawned Enemies: {spawnedEnemies.Count}");
        if (battleManager != null)
        {
            Debug.Log($"Battle Ended: {battleManager.battleEnded}, Player Turn: {battleManager.playerTurn}");
        }
    }
    
    // 숫자키: 특정 적 소환 (인덱스)
    private void SpawnEnemyByIndex(int index)
    {
        if (index < 0 || index >= enemyPrefabs.Count)
        {
            Debug.LogWarning($"[DEBUG] Invalid enemy index: {index}");
            return;
        }
        
        EnemyStats enemy = enemyPrefabs[index];
        Vector3 pos = GetSpawnPosition(spawnedEnemies.Count);
        SpawnEnemyAtPosition(enemy, pos);
        Debug.Log($"[DEBUG] Spawned enemy by index {index}: {enemy.enemyName}");
    }
    
    // 적 소환 헬퍼
    private void SpawnEnemyAtPosition(EnemyStats enemyPrefab, Vector3 position)
    {
        GameObject enemyObj = Instantiate(enemyPrefab.gameObject, position, Quaternion.identity);
        EnemyStats enemy = enemyObj.GetComponent<EnemyStats>();
        if (enemy != null)
        {
            spawnedEnemies.Add(enemy);
            
            // BattleManager에 첫 번째 적 연결 (다중 적 지원 전까지 임시)
            if (battleManager != null && spawnedEnemies.Count == 1)
            {
                battleManager.enemy = enemy;
            }
        }
    }
    
    // 중앙 대칭 위치 계산 (한글처럼 중앙 기준 좌우 펼침)
    private Vector3 GetSymmetricPosition(Vector3 center, int totalCount, int index)
    {
        float offset = 0f;
        
        // 중앙 기준 대칭 오프셋 계산
        if (totalCount == 1)
        {
            offset = 0f;
        }
        else if (totalCount == 2)
        {
            offset = (index == 0) ? -spawnOffset * 0.5f : spawnOffset * 0.5f;
        }
        else if (totalCount == 3)
        {
            if (index == 0) offset = -spawnOffset;
            else if (index == 1) offset = 0f;
            else offset = spawnOffset;
        }
        else // 4마리
        {
            if (index == 0) offset = -spawnOffset * 1.5f;
            else if (index == 1) offset = -spawnOffset * 0.5f;
            else if (index == 2) offset = spawnOffset * 0.5f;
            else offset = spawnOffset * 1.5f;
        }
        
        // 카메라 기준 right 벡터 사용 (1인칭 기준)
        Vector3 right = Camera.main != null ? Camera.main.transform.right : Vector3.right;
        return center + right * offset;
    }
    
    // 단일 위치 (기본)
    private Vector3 GetSpawnPosition(int index)
    {
        Vector3 center = spawnCenter != null ? spawnCenter.position : Vector3.zero;
        Vector3 right = Camera.main != null ? Camera.main.transform.right : Vector3.right;
        return center + right * (index * spawnOffset);
    }
    
    void OnGUI()
    {
        if (!enableDebugMode) return;
        
        // 화면 좌측 상단에 디버그 정보 표시
        GUILayout.BeginArea(new Rect(10, 10, 300, 400));
        GUILayout.Label("=== DEBUG MODE ===", GUI.skin.box);
        GUILayout.Label($"State: {currentState}");
        GUILayout.Label("Hotkeys:");
        GUILayout.Label("F1: Exploration Mode");
        GUILayout.Label("F2: Start Battle");
        GUILayout.Label("F3: Spawn Random Enemy");
        GUILayout.Label("F4: Spawn 1-4 Enemies (Symmetric)");
        GUILayout.Label("F5: Clear All Enemies");
        GUILayout.Label("F6: Heal Player");
        GUILayout.Label("F7: Damage Player (10)");
        GUILayout.Label("F8: Reset Battle");
        GUILayout.Label("F9: Toggle Pause");
        GUILayout.Label("F10: Log Status");
        GUILayout.Label("1-9: Spawn Enemy by Index");
        GUILayout.EndArea();
    }
}

