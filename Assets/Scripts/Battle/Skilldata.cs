using UnityEngine;

[System.Serializable]
public class SkillData : System.IComparable<SkillData>
{
    public string skillID;        // 스킬 고유 ID
    public string skillName;      // 스킬 이름
    public string skillType;      // "물리" / "마법" 등
    public float hpCostPercent;   // HP 소모 비율 (%)
    public float mpCost;          // MP 소모량
    public float minMultiplier;   // 최소 공격 배율
    public float maxMultiplier;   // 최대 공격 배율
    public Sprite icon;           // 스킬 아이콘

    // --- New Fields for Advanced Skills ---
    public int hitCount = 1;             // 타격 횟수 (기본 1)
    public string scalingStat = "Attack"; // 데미지 계수 스탯 ("Attack", "Magic", "Agility")
    public float selfDamageChance = 0f;  // 본인 피해 확률 (%)
    public float selfDamagePercent = 0f; // 본인 피해량 (% MaxHP)
    public bool isRecovery = false;      // 회복 스킬 여부
    public bool isDefensive = false;     // 방어 스킬 여부
    public float effectValue = 0f;       // 회복량(%) 또는 방어력 증가량(%)

    // --- 생성자 ---
    public SkillData(string id, string name, string type, float hpCostPercent, float mpCost, float minMultiplier, float maxMultiplier,
                     int hitCount = 1, string scalingStat = "Attack", float selfDamageChance = 0f, float selfDamagePercent = 0f,
                     bool isRecovery = false, bool isDefensive = false, float effectValue = 0f)
    {
        this.skillID = id;
        this.skillName = name;
        this.skillType = type;
        this.hpCostPercent = hpCostPercent;
        this.mpCost = mpCost;
        this.minMultiplier = minMultiplier;
        this.maxMultiplier = maxMultiplier;
        
        this.hitCount = hitCount;
        this.scalingStat = scalingStat;
        this.selfDamageChance = selfDamageChance;
        this.selfDamagePercent = selfDamagePercent;
        this.isRecovery = isRecovery;
        this.isDefensive = isDefensive;
        this.effectValue = effectValue;
    }


    // 정렬을 위한 비교 메서드 (HP 스킬 우선, 그 다음 코스트 오름차순)
    public int CompareTo(SkillData other)
    {
        if (other == null) return 1;

        bool isHpSkill = this.hpCostPercent > 0;
        bool isOtherHpSkill = other.hpCostPercent > 0;

        // 1. HP 스킬 우선
        if (isHpSkill && !isOtherHpSkill) return -1;
        if (!isHpSkill && isOtherHpSkill) return 1;

        // 2. 같은 타입이면 코스트 오름차순
        if (isHpSkill) // 둘 다 HP 스킬
        {
            return this.hpCostPercent.CompareTo(other.hpCostPercent);
        }
        else // 둘 다 MP 스킬 (또는 코스트 없음)
        {
            return this.mpCost.CompareTo(other.mpCost);
        }
    }

    // 코스트 가치 계산 (단순 참고용, 정렬 로직은 CompareTo에서 처리)
    public float GetCostValue()
    {
        return mpCost + hpCostPercent;
    }
}
