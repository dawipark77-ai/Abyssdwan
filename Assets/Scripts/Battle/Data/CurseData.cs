using UnityEngine;

namespace AbyssdawnBattle
{
    /// <summary>
    /// 저주 타입 열거형
    /// </summary>
    public enum CurseType
    {
        Ignite,      // 화상
        Poison,      // 독
        Stun,        // 기절
        Weakness,    // 약화
        Bleed,       // 출혈
        Curse,       // 저주
        Slow,        // 둔화
        Silence      // 침묵
    }

    /// <summary>
    /// 저주 데이터를 정의하는 ScriptableObject
    /// </summary>
    [CreateAssetMenu(fileName = "New Curse", menuName = "Abyssdawn/Battle/Curse Data", order = 3)]
    public class CurseData : ScriptableObject
    {
        [Header("기본 정보")]
        [Tooltip("저주 이름")]
        public string curseName;

        [Tooltip("저주 타입")]
        public CurseType type;

        [Header("효과")]
        [Tooltip("지속 턴 수")]
        public int duration = 3;

        [Tooltip("매 턴 데미지 비율 (타겟 최대 HP 대비 %)")]
        [Range(0f, 100f)]
        public float dotDamagePercent = 5f;

        [Header("시각 효과")]
        [Tooltip("저주 이펙트 프리팹")]
        public GameObject curseVFX;

        [Header("추가 효과 (선택사항)")]
        [Tooltip("공격력 감소 비율 (%)")]
        [Range(0f, 100f)]
        public float attackDebuff = 0f;

        [Tooltip("방어력 감소 비율 (%)")]
        [Range(0f, 100f)]
        public float defenseDebuff = 0f;

        [Tooltip("스킬 사용 불가 여부 (Silence)")]
        public bool preventSkillUse = false;

        [Tooltip("행동 불가 여부 (Stun)")]
        public bool preventAction = false;
    }
}
