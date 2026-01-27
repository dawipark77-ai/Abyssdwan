using UnityEngine;
using UnityEngine.SceneManagement;

public class DungeonEncounter : MonoBehaviour
{
    public static DungeonEncounter Instance { get; private set; }

    [Header("Encounter Settings")]
    [Range(0f, 1f)]
    public float encounterChance = 0.15f; // 이동 1회당 전투 확률 (15%)

    public string battleSceneName = "Abyssdawn_Battle 01";
    public static string lastDungeonScene;

    // private DungeonPlayer player; // Not used currently

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        // player = GetComponent<DungeonPlayer>();
    }

    /// <summary>
    /// 플레이어가 한 칸 이동했을 때 호출
    /// </summary>
    public void CheckEncounter(Vector2Int pos)
    {
        float roll = Random.value;
        Debug.Log("[DungeonEncounter] Checking encounter at " + pos + ". Roll: " + roll.ToString("F2") + ", Chance: " + encounterChance.ToString("F2"));
        if (roll < encounterChance)
        {
            StartEncounter();
        }
    }

    void StartEncounter()
    {
        Debug.Log("[DungeonEncounter] >>> STARTING ENCOUNTER! <<<");
        
        // 필요하다면 현재 층, 좌표, 몬스터 테이블 등을 저장
        lastDungeonScene = SceneManager.GetActiveScene().name;
        
        // 플레이어 위치 및 상태 저장
        DungeonGridPlayer dPlayer = FindFirstObjectByType<DungeonGridPlayer>();
        if (dPlayer != null)
        {
            DungeonPersistentData.lastPlayerGridPos = dPlayer.gridPos;
            DungeonPersistentData.lastPlayerFacing = dPlayer.facing;
            DungeonPersistentData.hasSavedState = true;
            Debug.Log("[DungeonEncounter] Saving grid state: " + dPlayer.gridPos + ", facing " + dPlayer.facing);
        }

        PlayerStats stats = FindFirstObjectByType<PlayerStats>();
        if (stats != null)
        {
            var gm = GameManager.EnsureInstance();
            gm.SaveFromPlayer(stats);
            Debug.Log("[DungeonEncounter] Saved " + stats.playerName + " stats to GM. HP: " + stats.currentHP + "/" + stats.maxHP);
        }

        Debug.Log("[DungeonEncounter] Loading battle scene: " + battleSceneName);
        SceneManager.LoadScene(battleSceneName);
    }
}
