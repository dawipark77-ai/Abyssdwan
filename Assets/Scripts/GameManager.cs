using UnityEngine;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    // Static instance
    private static GameManager _instance;
    public static GameManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<GameManager>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("GameManager (Auto Created)");
                    _instance = go.AddComponent<GameManager>();
                }
            }
            return _instance;
        }
    }

    [System.Serializable]
    public class PartyMemberData
    {
        public string characterName;
        public int level;
        public string jobClass;
        public int exp;
        public int maxExp;
        public int maxHP;
        public int currentHP;
        public int maxMP;
        public int currentMP;
        public int attack;
        public int defense;
        public int magic;
        public int agility;
        public int luck;
        public bool isIgnited;
        public int igniteTurnsRemaining;

        public int baseHP;
        public int baseMP;
        public int baseAttack;
        public int baseDefense;
        public int baseMagic;
        public int baseAgility;
        public int baseLuck;

        public PartyMemberData(PlayerStats stats)
        {
            characterName = stats.playerName;
            level = stats.level;
            jobClass = stats.jobClass;
            exp = stats.exp;
            maxExp = stats.maxExp;
            maxHP = stats.maxHP;
            currentHP = stats.currentHP;
            maxMP = stats.maxMP;
            currentMP = stats.currentMP;
            attack = stats.attack;
            defense = stats.defense;
            magic = stats.magic;
            agility = stats.Agility;
            luck = stats.luck;
            isIgnited = stats.isIgnited;
            igniteTurnsRemaining = stats.igniteTurnsRemaining;
            
            baseHP = stats.baseHP;
            baseMP = stats.baseMP;
            baseAttack = stats.baseAttack;
            baseDefense = stats.baseDefense;
            baseMagic = stats.baseMagic;
            baseAgility = stats.baseAgility;
            baseLuck = stats.baseLuck;
        }
    }

    // CRITICAL: Using STATIC dictionary to ensure data persists even if component is re-created or missing GUID
    public static Dictionary<string, PartyMemberData> staticPartyData = new Dictionary<string, PartyMemberData>();

    // Backward-compatible alias for older code paths.
    public Dictionary<string, PartyMemberData> partyData => staticPartyData;

    // For Inspector debugging (Optional)
    [SerializeField]
    private List<PartyMemberData> debugPartyList = new List<PartyMemberData>();

    public bool hasPlayerSnapshot { get { return staticPartyData.Count > 0; } }

    public static GameManager EnsureInstance()
    {
        return Instance;
    }

    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void ClearAllData()
    {
        staticPartyData.Clear();
        debugPartyList.Clear();
        Debug.Log("[GameManager] All party data cleared!");
    }

    void OnValidate()
    {
        if (Application.isPlaying) SyncDebugList();
    }

    void SyncDebugList()
    {
        debugPartyList.Clear();
        foreach (var data in staticPartyData.Values)
        {
            debugPartyList.Add(data);
        }
    }

    public void SaveFromPlayer(PlayerStats player)
    {
        if (player == null) return;

        if (string.IsNullOrEmpty(player.playerName))
        {
            Debug.LogWarning("[GameManager] Cannot save player with empty name!");
            return;
        }

        if (staticPartyData.ContainsKey(player.playerName))
        {
            staticPartyData[player.playerName] = new PartyMemberData(player);
            Debug.Log("[SERIALIZATION_FIX] Saved UPDATED: " + player.playerName + " HP: " + player.currentHP + "/" + player.maxHP);
        }
        else
        {
            staticPartyData.Add(player.playerName, new PartyMemberData(player));
            Debug.Log("[SERIALIZATION_FIX] Saved ADDED: " + player.playerName + " HP: " + player.currentHP + "/" + player.maxHP);
        }

        SyncDebugList();
    }

    public void ApplyToPlayer(PlayerStats player)
    {
        if (player == null) return;

        if (staticPartyData.TryGetValue(player.playerName, out PartyMemberData data))
        {
            player.level = data.level;
            player.jobClass = data.jobClass;
            player.exp = data.exp;
            player.maxExp = data.maxExp;
            player.maxHP = data.maxHP;
            player.currentHP = Mathf.Clamp(data.currentHP, 0, data.maxHP);
            player.maxMP = data.maxMP;
            player.currentMP = Mathf.Clamp(data.currentMP, 0, data.maxMP);
            player.attack = data.attack;
            player.defense = data.defense;
            player.magic = data.magic;
            player.Agility = data.agility;
            player.luck = data.luck;
            player.SetIgnited(data.isIgnited, data.igniteTurnsRemaining);

            Debug.Log("[SERIALIZATION_FIX] Loaded: " + player.playerName + " HP: " + player.currentHP + "/" + player.maxHP);
        }
        else
        {
            Debug.Log("[SERIALIZATION_FIX] No data for: " + player.playerName + ". Initializing...");
            SaveFromPlayer(player);
        }
    }
}
