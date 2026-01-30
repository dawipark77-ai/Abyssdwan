using UnityEngine;
using System.Collections.Generic;
using System;

public class PlayerStats : MonoBehaviour
{
    [Header("1. 저장 금고 (에셋 파일 연결)")]
    [Tooltip("PlayerStatData 에셋을 연결하세요. HP/MP가 실시간으로 이 에셋에 저장됩니다.")]
    public PlayerStatData statData;

    // [FIX] 런타임 fallback 변수 (statData가 없을 때만 사용)
    private int _fallbackCurrentHP;
    private int _fallbackCurrentMP;
    private int _fallbackLevel = 1;
    private int _fallbackExp = 0;
    private bool _isInitialized = false; // 초기화 완료 플래그

    // [NEW] 이벤트 시스템 - HP/MP/스탯이 변경될 때마다 발동
    public static event Action OnStatusChanged;

    [Header("2. 캐릭터 정보")]
    public string playerName = "Hero";

    // GameManager 호환용: 현재 직업 에셋의 이름을 반환
    public string jobClass => (characterClass != null) ? characterClass.className : "None";

    [Header("3. 태초의 기본 수치 (고정값)")]
    public int baseHP = 100;
    public int baseMP = 10;      // 기본 MP 10
    public int baseAttack = 5;   // 기본 스탯 5, 5, 5, 5, 3
    public int baseDefense = 5;
    public int baseMagic = 5;
    public int baseAgility = 5;
    public int baseLuck = 3;

    [Header("4. 장착된 직업 데이터")]
    public CharacterClass characterClass; 

    // --- [실시간 조립 계산식 - 대소문자 모두 대응] ---
    public int maxHP => (characterClass != null) ? characterClass.GetFinalMaxHP(baseHP) : baseHP;
    public int maxMP => (characterClass != null) ? characterClass.GetFinalMaxMP(baseMP) : baseMP;
    
    // 대문자 버전 (BattleManager 등 최신 스크립트용)
    public int Attack  => (characterClass != null) ? characterClass.GetFinalAttack(baseAttack) : baseAttack;
    public int Defense => (characterClass != null) ? characterClass.GetFinalDefense(baseDefense) : baseDefense;
    public int Magic   => (characterClass != null) ? characterClass.GetFinalMagic(baseMagic) : baseMagic;
    public int Agility => (characterClass != null) ? characterClass.GetFinalAgility(baseAgility) : baseAgility;
    public int Luck    => (characterClass != null) ? characterClass.GetFinalLuck(baseLuck) : baseLuck;

    // 소문자 버전 (GameManager 등 기존 레거시 스크립트 호환용)
    public int attack  => Attack;
    public int defense => Defense;
    public int magic   => Magic;
    public int agility => Agility;
    public int luck    => Luck;

    // --- [런타임 데이터 프로퍼티 - SO 실시간 저장] ---
    // HP/MP가 변경될 때마다 SO 에셋에 즉시 저장됩니다
    public int currentHP
    {
        get
        {
            int value = (statData != null) ? statData.currentHP : _fallbackCurrentHP;
            Debug.Log($"[StatusCheck] {playerName} Current HP: {value}/{maxHP}");
            return value;
        }
        set
        {
            int oldValue = (statData != null) ? statData.currentHP : _fallbackCurrentHP;

            // [FIX] 실시간 저장: SO 에셋에 즉시 기록
            if (statData != null)
            {
                statData.currentHP = value;
                Debug.Log($"[PlayerStats] ✓ SO 에셋 저장: currentHP = {value}");
            }
            else
            {
                _fallbackCurrentHP = value;
                Debug.LogWarning($"[PlayerStats] statData가 없어서 fallback 변수에 저장: currentHP = {value}");
            }

            // [DEBUG] HP 변경 로그
            if (oldValue != value)
            {
                Debug.Log($"[PlayerStats] {playerName} currentHP 변경: {oldValue} -> {value} (maxHP: {maxHP})");

                // [DEBUG] 이벤트 구독자 수 확인
                if (OnStatusChanged != null)
                {
                    int subscriberCount = OnStatusChanged.GetInvocationList().Length;
                    Debug.Log($"[PlayerStats] OnStatusChanged 이벤트 발동! (구독자 수: {subscriberCount})");
                }
                else
                {
                    Debug.LogWarning($"[PlayerStats] OnStatusChanged 이벤트에 구독자가 없습니다!");
                }
            }

            // [NEW] HP 변경 시 이벤트 발동
            OnStatusChanged?.Invoke();
        }
    }
    public int currentMP
    {
        get => (statData != null) ? statData.currentMP : _fallbackCurrentMP;
        set
        {
            int oldValue = (statData != null) ? statData.currentMP : _fallbackCurrentMP;

            // [FIX] 실시간 저장: SO 에셋에 즉시 기록
            if (statData != null)
            {
                statData.currentMP = value;
                Debug.Log($"[PlayerStats] ✓ SO 에셋 저장: currentMP = {value}");
            }
            else
            {
                _fallbackCurrentMP = value;
            }

            // [DEBUG] MP 변경 로그
            if (oldValue != value)
            {
                Debug.Log($"[PlayerStats] {playerName} currentMP 변경: {oldValue} -> {value} (maxMP: {maxMP})");
            }

            // [NEW] MP 변경 시 이벤트 발동
            OnStatusChanged?.Invoke();
        }
    }
    public int level
    {
        get => (statData != null) ? statData.level : _fallbackLevel;
        set
        {
            if (statData != null)
                statData.level = value;
            else
                _fallbackLevel = value;

            // [NEW] 레벨 변경 시 이벤트 발동
            OnStatusChanged?.Invoke();
        }
    }
    public int exp
    {
        get => (statData != null) ? statData.exp : _fallbackExp;
        set
        {
            if (statData != null)
                statData.exp = value;
            else
                _fallbackExp = value;

            // [NEW] 경험치 변경 시 이벤트 발동
            OnStatusChanged?.Invoke();
        }
    }

    // --- [UI용 보너스 계산 메서드] ---
    // HP/MP 보너스 (최종값 - 기본값)
    public int GetHPBonus() => maxHP - baseHP;
    public int GetMPBonus() => maxMP - baseMP;

    // 스탯 보너스 (직업 보정치)
    public int GetAttackBonus() => (characterClass != null) ? characterClass.attackBonus : 0;
    public int GetDefenseBonus() => (characterClass != null) ? characterClass.defenseBonus : 0;
    public int GetMagicBonus() => (characterClass != null) ? characterClass.magicBonus : 0;
    public int GetAgilityBonus() => (characterClass != null) ? characterClass.agilityBonus : 0;
    public int GetLuckBonus() => (characterClass != null) ? characterClass.luckBonus : 0;

    [Header("5. 레벨 시스템")]
    public int maxExp = 100;

    [Header("6. 배틀 상태 (휘발성)")]
    public bool isDefending = false;
    public float defenceReduction = 0.4f;
    public float defenseBuffAmount = 0f;
    public bool isIgnited = false;
    public int igniteTurnsRemaining = 0;

    void Awake()
    {
        // [DEBUG] 인스턴스 ID 출력 (어떤 PlayerStats가 초기화되는지 확인)
        Debug.Log($"[PlayerStats] Awake() 호출: GameObject={gameObject.name}, InstanceID={GetInstanceID()}, playerName={playerName}");

        // [FIX] 이미 초기화되었으면 다시 초기화하지 않음 (중복 방지)
        if (_isInitialized)
        {
            Debug.Log($"[PlayerStats] {playerName} 이미 초기화되어 있습니다. 스킵합니다. (InstanceID={GetInstanceID()})");
            return;
        }

        Debug.Log($"[PlayerStats] {playerName} 초기화 시작... (InstanceID={GetInstanceID()})");

        // [FIX] statData 연결 확인
        if (statData == null)
        {
            Debug.LogWarning($"[PlayerStats] {playerName}의 statData가 할당되지 않았습니다! fallback 변수를 사용합니다.");
            _fallbackCurrentHP = 0;
            _fallbackCurrentMP = 0;
            _fallbackLevel = 1;
            _fallbackExp = 0;
        }
        else
        {
            Debug.Log($"[PlayerStats] ✓ statData 에셋 연결됨: {statData.name}");
            Debug.Log($"[PlayerStats] SO 현재값: HP={statData.currentHP}, MP={statData.currentMP}, Level={statData.level}");
        }

        // [FIX] 직업이 설정되지 않았으면 기본 Warrior 직업 강제 설정
        if (characterClass == null)
        {
            Debug.LogWarning($"[PlayerStats] {playerName}의 직업이 설정되지 않았습니다! 기본 Warrior 직업을 설정합니다.");
            SetClass("Warrior");

            if (characterClass != null)
            {
                Debug.Log($"[PlayerStats] ✓ {playerName}에 Warrior 직업이 자동 설정되었습니다. Attack: {Attack} (base: {baseAttack} + bonus: {GetAttackBonus()})");
            }
            else
            {
                Debug.LogError($"[PlayerStats] ✗ Warrior 직업 설정 실패! CharacterClassDatabase.asset 파일이 Resources 폴더에 있는지 확인하세요.");
            }
        }
        else
        {
            Debug.Log($"[PlayerStats] {playerName} 직업 확인: {characterClass.className}, Attack: {Attack} (base: {baseAttack} + bonus: {GetAttackBonus()})");
        }

        // [FIX] 자동 계산: BaseHP * JobModifier = MaxHP
        Debug.Log($"[PlayerStats] 자동 계산: BaseHP({baseHP}) * JobModifier({characterClass?.hpMultiplier ?? 1.0f}) = MaxHP({maxHP})");

        // [FIX] 조건부 초기화: 에셋에 저장된 HP가 0이거나 비정상일 때만 maxHP로 초기화
        int currentHPFromAsset = (statData != null) ? statData.currentHP : 0;
        int currentMPFromAsset = (statData != null) ? statData.currentMP : 0;

        // HP 조건: 0 이하이거나 maxHP보다 크면 초기화
        if (currentHPFromAsset <= 0 || currentHPFromAsset > maxHP)
        {
            Debug.Log($"[PlayerStats] SO의 HP가 비정상({currentHPFromAsset}) → maxHP({maxHP})로 초기화");
            currentHP = maxHP;
        }
        else
        {
            Debug.Log($"[PlayerStats] SO에서 로드한 HP 유지: {currentHPFromAsset}/{maxHP}");
            // currentHP는 이미 SO에서 getter로 읽어오므로 별도 설정 불필요
        }

        // MP 조건: 0 미만이거나 maxMP보다 크면 초기화
        if (currentMPFromAsset < 0 || currentMPFromAsset > maxMP)
        {
            Debug.Log($"[PlayerStats] SO의 MP가 비정상({currentMPFromAsset}) → maxMP({maxMP})로 초기화");
            currentMP = maxMP;
        }
        else
        {
            Debug.Log($"[PlayerStats] SO에서 로드한 MP 유지: {currentMPFromAsset}/{maxMP}");
        }

        _isInitialized = true; // 초기화 완료 플래그

        Debug.Log($"[PlayerStats] ===== {playerName} 초기화 완료! =====");
        Debug.Log($"[PlayerStats] InstanceID: {GetInstanceID()}");
        Debug.Log($"[PlayerStats] HP: {currentHP}/{maxHP}");
        Debug.Log($"[PlayerStats] MP: {currentMP}/{maxMP}");
        Debug.Log($"[PlayerStats] Attack: {Attack} (base: {baseAttack} + bonus: {GetAttackBonus()})");
        Debug.Log($"[PlayerStats] ==============================");
    }

    void Start()
    {
        // [FIX] 초기화 확인만 수행 (값 수정 안 함)
        Debug.Log($"[PlayerStats] Start() - {playerName} 상태 확인 (InstanceID={GetInstanceID()}):");
        Debug.Log($"[PlayerStats] Start() - HP {currentHP}/{maxHP}, MP {currentMP}/{maxMP}");

        // [FIX] 이벤트 발동 (UI 갱신)
        OnStatusChanged?.Invoke();
    }

    // --- [직업 관련 함수] ---

    /// <summary>
    /// 직업을 이름으로 설정 (CharacterClassDatabase 사용)
    /// </summary>
    public void SetClass(string className)
    {
        // Null check for database
        if (CharacterClassDatabase.Instance == null)
        {
            Debug.LogError($"[PlayerStats] CharacterClassDatabase를 로드할 수 없습니다! Resources/CharacterClassDatabase.asset 파일을 확인하세요.");
            return;
        }

        CharacterClass newClass = CharacterClassDatabase.Instance.GetClassByName(className);

        if (newClass != null)
        {
            // [FIX] 직업 변경 전 HP/MP 비율 계산
            // currentHP가 0이거나 maxHP가 0이면 100%로 간주 (초기 설정)
            float hpRatio = (maxHP > 0 && currentHP > 0) ? (float)currentHP / maxHP : 1.0f;
            float mpRatio = (maxMP > 0 && currentMP >= 0) ? (float)currentMP / maxMP : 1.0f;

            Debug.Log($"[PlayerStats] 직업 변경 전 - HP: {currentHP}/{maxHP} ({hpRatio:P0}), MP: {currentMP}/{maxMP} ({mpRatio:P0})");

            // 직업 변경
            characterClass = newClass;

            // [FIX] 변경된 maxHP/maxMP에 동일한 비율 적용
            currentHP = Mathf.RoundToInt(maxHP * hpRatio);
            currentMP = Mathf.RoundToInt(maxMP * mpRatio);

            // 최소 1, 최대치 초과 방지
            currentHP = Mathf.Clamp(currentHP, 1, maxHP);
            currentMP = Mathf.Clamp(currentMP, 0, maxMP);

            Debug.Log($"[PlayerStats] {playerName}의 직업이 {className}으로 변경되었습니다. HP: {currentHP}/{maxHP}, MP: {currentMP}/{maxMP}");
        }
        else
        {
            Debug.LogWarning($"[PlayerStats] 직업 '{className}'을(를) 찾을 수 없습니다.");
        }
    }

    // --- [전투 관련 함수] ---

    public void Defend()
    {
        isDefending = true;
        Debug.Log($"{playerName} 방어 자세 취함!");
    }

    public int TakeDamage(int damage)
    {
        int finalDamage = damage;
        if (isDefending || defenseBuffAmount > 0)
        {
            float totalReduction = isDefending ? defenceReduction : 0f;
            totalReduction += (defenseBuffAmount / 100f);
            finalDamage = Mathf.Max(1, Mathf.FloorToInt(damage * (1f - totalReduction)));
            isDefending = false; 
            defenseBuffAmount = 0f;
        }
        currentHP = Mathf.Clamp(currentHP - finalDamage, 0, maxHP);
        if (currentHP <= 0) Die();
        return finalDamage;
    }

    public void Heal(int amount) { currentHP = Mathf.Min(currentHP + amount, maxHP); }
    public void UseMP(int amount) { currentMP = Mathf.Clamp(currentMP - amount, 0, maxMP); }
    void Die() { Debug.Log($"{playerName} 사망"); }

    public void SetIgnited(bool ignited, int turns = 5)
    {
        isIgnited = ignited;
        if (ignited) igniteTurnsRemaining = Mathf.Max(igniteTurnsRemaining, turns);
        else igniteTurnsRemaining = 0;
    }

    public void ProcessIgniteDamage()
    {
        if (isIgnited && igniteTurnsRemaining > 0 && currentHP > 0)
        {
            int igniteDamage = Mathf.Max(1, Mathf.FloorToInt(maxHP * 0.05f));
            currentHP = Mathf.Max(0, currentHP - igniteDamage);
            igniteTurnsRemaining--;
            if (igniteTurnsRemaining <= 0) isIgnited = false;
            if (currentHP <= 0) Die();
        }
    }

    public void AddExp(int amount)
    {
        exp += amount;
        while (exp >= maxExp) LevelUp();
    }

    private void LevelUp()
    {
        level++;
        exp -= maxExp;
        maxExp = level * 100; 
        currentHP = maxHP;
        currentMP = maxMP;
    }
}
