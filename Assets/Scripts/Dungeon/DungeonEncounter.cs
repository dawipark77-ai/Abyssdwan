using UnityEngine;
using UnityEngine.SceneManagement;

public class DungeonEncounter : MonoBehaviour
{
    public static DungeonEncounter Instance { get; private set; }

    [Header("Encounter Settings")]
    [Range(0f, 1f)]
    public float encounterChance = 0.15f; // ?대룞 1?뚮떦 ?꾪닾 ?뺣쪧 (15%)

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
    /// ?뚮젅?댁뼱媛 ??移??대룞?덉쓣 ???몄텧
    /// </summary>
    public void CheckEncounter(Vector2Int pos)
    {
        float roll = UnityEngine.Random.value;
        Debug.Log("[DungeonEncounter] Checking encounter at " + pos + ". Roll: " + roll.ToString("F2") + ", Chance: " + encounterChance.ToString("F2"));
        if (roll < encounterChance)
        {
            StartEncounter();
        }
    }

    void StartEncounter()
    {
        Debug.Log("[DungeonEncounter] >>> STARTING ENCOUNTER! <<<");
        
        // ?꾩슂?섎떎硫??꾩옱 痢? 醫뚰몴, 紐ъ뒪???뚯씠釉??깆쓣 ???
        lastDungeonScene = SceneManager.GetActiveScene().name;
        
        // ?뚮젅?댁뼱 ?꾩튂 諛??곹깭 ???
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


