using UnityEngine;
using System.Collections.Generic;

public class PlayerStats : MonoBehaviour
{
    public string playerName = "Hero";
    public int level = 1;
    public int exp = 0;
    public int maxExp = 100;
    public string jobClass = "Warrior";

    [Header("Base Stats")]
    public int baseHP = 100;
    public int baseMP = 10;
    public int baseAttack = 5;
    public int baseDefense = 5;
    public int baseMagic = 5;
    public int baseAgility = 5;
    public int baseLuck = 3;

    [Header("Final Stats")]
    public int maxHP = 100;
    public int currentHP;
    public int maxMP = 10;
    public int currentMP;
    public int attack = 5;
    public int defense = 5;
    public int magic = 5;
    public int Agility = 5;
    public int luck = 3;

    [Header("Character Class")]
    public CharacterClass characterClass;
    private bool classApplied = false;
    private string lastJobClass = "";

    [Header("Defense Ability")]
    public bool isDefending = false;
    public float defenceReduction = 0.4f;
    public float defenseBuffAmount = 0f;
    
    [Header("Status Effects")]
    public bool isIgnited = false;
    public int igniteTurnsRemaining = 0;

    void Awake()
    {
        Debug.Log("[PERSISTENCE_DEBUG] PlayerStats.Awake RUNNING for '" + playerName + "' (Obj: " + name + ")");
        Debug.Log("[PERSISTENCE_DEBUG] PlayerStats.Awake: INITIAL VALUES - currentHP: " + currentHP + ", maxHP: " + maxHP);

        if (GameManager.Instance != null)
        {
            if (GameManager.Instance.partyData.ContainsKey(playerName))
            {
                Debug.Log("[PERSISTENCE_DEBUG] PlayerStats.Awake: Key found for '" + playerName + "'. Loading...");
                GameManager.Instance.ApplyToPlayer(this);
                Debug.Log("[PERSISTENCE_DEBUG] PlayerStats.Awake: Loaded Result - HP: " + currentHP + "/" + maxHP);
                
                if (currentHP <= 0)
                {
                    currentHP = maxHP;
                    Debug.LogWarning("[PlayerStats] Loaded HP was 0! Force initialized to maxHP (" + maxHP + ")");
                    GameManager.Instance.SaveFromPlayer(this);
                }
                if (currentMP <= 0)
                {
                    currentMP = maxMP;
                    Debug.LogWarning("[PlayerStats] Loaded MP was 0 or negative! Force initialized to maxMP (" + maxMP + ")");
                    GameManager.Instance.SaveFromPlayer(this);
                }
                
                classApplied = true;
                return;
            }
            else
            {
                Debug.Log("[PERSISTENCE_DEBUG] PlayerStats.Awake: GM exists but NO key for '" + playerName + "'. PartyData Count: " + GameManager.Instance.partyData.Count);
            }
        }
        else
        {
            Debug.Log("[PERSISTENCE_DEBUG] PlayerStats.Awake: GameManager.Instance is NULL!");
        }

        if (string.IsNullOrEmpty(jobClass))
        {
            jobClass = "Warrior";
        }

        if (!classApplied)
        {
            int savedHP = currentHP;
            int savedMP = currentMP;
            bool hasLoadedData = GameManager.Instance != null && GameManager.Instance.partyData.ContainsKey(playerName);
            
            ApplyCharacterClass();
            
            if (hasLoadedData)
            {
                currentHP = Mathf.Clamp(savedHP, 0, maxHP);
                currentMP = Mathf.Clamp(savedMP, 0, maxMP);
                Debug.Log("[PlayerStats] Awake: Restored HP/MP after ApplyCharacterClass. HP: " + currentHP + "/" + maxHP + ", MP: " + currentMP + "/" + maxMP);
            }
        }

        bool hasLoadedDataFinal = GameManager.Instance != null && GameManager.Instance.partyData.ContainsKey(playerName);
        if (!hasLoadedDataFinal && currentHP <= 0)
        {
            currentHP = maxHP;
            currentMP = maxMP;
            Debug.Log("[PlayerStats] Awake: Initialized " + playerName + " HP/MP to max values (HP: " + currentHP + "/" + maxHP + ", MP: " + currentMP + "/" + maxMP + ")");
        }
        else if (!hasLoadedDataFinal)
        {
            Debug.Log("[PlayerStats] Awake: " + playerName + " HP was already set to " + currentHP + "/" + maxHP + " (not initializing)");
        }
        
        Debug.Log("[PERSISTENCE_DEBUG] PlayerStats.Awake COMPLETE for '" + playerName + "' - FINAL HP: " + currentHP + "/" + maxHP + ", MP: " + currentMP + "/" + maxMP);
    }

    void Start()
    {
        if (!classApplied)
        {
            if (GameManager.Instance != null && GameManager.Instance.partyData.ContainsKey(playerName))
            {
                 GameManager.Instance.ApplyToPlayer(this);
                 Debug.Log("[PlayerStats.Start] Loaded from GameManager: " + playerName + " (HP: " + currentHP + "/" + maxHP + ")");
                 classApplied = true;
            }
            else if (!string.IsNullOrEmpty(jobClass))
            {
                Debug.Log("[PlayerStats.Start] No data found for " + playerName + ". Applying base class: " + jobClass);
                ApplyCharacterClass();
                
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.SaveFromPlayer(this);
                }
            }
        }
        else
        {
             Debug.Log("[PlayerStats.Start] Already initialized via load/awake for " + playerName + ". Skipping class application.");
        }
        
        lastJobClass = jobClass;
    }

    void OnValidate()
    {
        if (Application.isPlaying && !string.IsNullOrEmpty(jobClass) && jobClass != lastJobClass)
        {
            Debug.Log("[PlayerStats] Inspector class change detected: " + lastJobClass + " -> " + jobClass);
            SetClass(jobClass);
            lastJobClass = jobClass;
        }
    }

    public void SetClass(string className)
    {
        if (string.IsNullOrEmpty(className))
        {
            Debug.LogWarning("[PlayerStats] SetClass: Class name is empty!");
            return;
        }
        
        Debug.Log("[PlayerStats] SetClass called: '" + className + "'");
        
        jobClass = className;
        classApplied = false;
        
        if (CharacterClassDatabase.Instance != null)
        {
            CharacterClassDatabase.Instance.BuildDictionary();
            characterClass = CharacterClassDatabase.Instance.GetClassByName(className);
            
            if (characterClass == null)
            {
                Debug.LogWarning("[PlayerStats] Class '" + className + "' not found!");
            }
        }
        
        ApplyCharacterClass();
        AutoSaveToGameManager();
    }

    public void SetClass(CharacterClass newClass)
    {
        if (newClass == null) return;
        characterClass = newClass;
        jobClass = newClass.className;
        ApplyCharacterClass();
    }

    public void ApplyCharacterClass()
    {
        if (characterClass == null && !string.IsNullOrEmpty(jobClass))
        {
            if (CharacterClassDatabase.Instance != null)
            {
                CharacterClassDatabase.Instance.BuildDictionary();
                characterClass = CharacterClassDatabase.Instance.GetClassByName(jobClass);
                
                if (characterClass == null && CharacterClassDatabase.Instance.allClasses != null)
                {
                    foreach (var cls in CharacterClassDatabase.Instance.allClasses)
                    {
                        if (cls != null && string.Equals(cls.className, jobClass, System.StringComparison.OrdinalIgnoreCase))
                        {
                            characterClass = cls;
                            break;
                        }
                    }
                }
            }
        }

        if (characterClass != null)
        {
            jobClass = characterClass.className;
            maxHP = characterClass.GetFinalMaxHP(baseHP);
            maxMP = characterClass.GetFinalMaxMP(baseMP);
            attack = characterClass.GetFinalAttack(baseAttack);
            defense = characterClass.GetFinalDefense(baseDefense);
            magic = characterClass.GetFinalMagic(baseMagic);
            Agility = characterClass.GetFinalAgility(baseAgility);
            luck = characterClass.GetFinalLuck(baseLuck);
            classApplied = true;
            
            Debug.Log("[PlayerStats] Applied class [" + characterClass.className + "] to " + playerName);
            AutoSaveToGameManager();
        }
        else
        {
            maxHP = baseHP;
            maxMP = baseMP;
            attack = baseAttack;
            defense = baseDefense;
            magic = baseMagic;
            Agility = baseAgility;
            luck = baseLuck;
        }

        bool hasLoadedData = GameManager.Instance != null && GameManager.Instance.partyData.ContainsKey(playerName);
        
        if (!hasLoadedData || currentHP <= 0 || currentMP <= 0)
        {
            if (currentHP <= 0 || (!hasLoadedData && currentHP > maxHP))
            {
                currentHP = maxHP;
                Debug.Log("[PlayerStats] ApplyCharacterClass: HP initialized to " + currentHP);
            }
            if (currentMP <= 0 || (!hasLoadedData && currentMP > maxMP))
            {
                currentMP = maxMP;
                Debug.Log("[PlayerStats] ApplyCharacterClass: MP initialized to " + currentMP);
            }
        }
        else
        {
            currentHP = Mathf.Clamp(currentHP, 0, maxHP);
            currentMP = Mathf.Clamp(currentMP, 0, maxMP);
            Debug.Log("[PlayerStats] ApplyCharacterClass: Preserved loaded HP/MP. HP: " + currentHP + "/" + maxHP + ", MP: " + currentMP + "/" + maxMP);
        }
        
        AutoSaveToGameManager();
    }

    public string GetClassBonusInfo()
    {
        if (characterClass == null) return "No Class";
        return characterClass.className;
    }

    public int TakeDamage(int damage)
    {
        int finalDamage = damage;
        if (isDefending || defenseBuffAmount > 0)
        {
            float totalReduction = 0f;
            if (isDefending) totalReduction += defenceReduction;
            if (defenseBuffAmount > 0) totalReduction += (defenseBuffAmount / 100f);
            totalReduction = Mathf.Min(totalReduction, 0.9f);
            finalDamage = Mathf.Max(1, Mathf.FloorToInt(damage * (1f - totalReduction)));
            isDefending = false;
            defenseBuffAmount = 0f;
        }

        currentHP -= finalDamage;
        currentHP = Mathf.Clamp(currentHP, 0, maxHP);
        Debug.Log(playerName + " took " + finalDamage + " damage. HP: " + currentHP + "/" + maxHP);
        AutoSaveToGameManager();
        if (currentHP <= 0) Die();
        return finalDamage;
    }

    public void Heal(int amount)
    {
        currentHP = Mathf.Min(currentHP + amount, maxHP);
        Debug.Log(playerName + " healed " + amount + ". HP: " + currentHP + "/" + maxHP);
        AutoSaveToGameManager();
    }
    
    private void AutoSaveToGameManager()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SaveFromPlayer(this);
        }
    }

    public void Defend()
    {
        isDefending = true;
        Debug.Log(playerName + " is defending.");
    }

    void Die()
    {
        Debug.Log(playerName + " has been defeated!");
        isIgnited = false;
        igniteTurnsRemaining = 0;
    }
    
    public void SetIgnited(bool ignited, int turns = 5)
    {
        isIgnited = ignited;
        if (ignited)
        {
            igniteTurnsRemaining = Mathf.Max(igniteTurnsRemaining, turns);
        }
        else
        {
            igniteTurnsRemaining = 0;
        }
    }
    
    public void ProcessIgniteDamage()
    {
        if (isIgnited && igniteTurnsRemaining > 0 && currentHP > 0)
        {
            int igniteDamage = Mathf.Max(1, Mathf.FloorToInt(maxHP * 0.05f));
            currentHP -= igniteDamage;
            currentHP = Mathf.Max(0, currentHP);
            igniteTurnsRemaining--;
            AutoSaveToGameManager();
            if (igniteTurnsRemaining <= 0) isIgnited = false;
            if (currentHP <= 0) Die();
        }
    }

    public void AddExp(int amount)
    {
        exp += amount;
        CheckLevelUp();
    }

    private void CheckLevelUp()
    {
        while (exp >= maxExp)
        {
            exp -= maxExp;
            LevelUp();
        }
    }

    private void LevelUp()
    {
        level++;
        maxExp = level * 100;
        currentHP = maxHP;
        currentMP = maxMP;
        Debug.Log("LEVEL UP! " + playerName + " reached Level " + level);
        AutoSaveToGameManager();
    }
}
