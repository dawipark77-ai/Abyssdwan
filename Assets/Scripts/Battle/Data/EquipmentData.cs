using UnityEngine;

namespace AbyssdawnBattle
{
    public enum EquipmentType { RightHand, LeftHand, Body, Accessory }

    [CreateAssetMenu(fileName = "Equipment_", menuName = "Abyssdawn/Equipment Data", order = 2)]
    public class EquipmentData : ScriptableObject
    {
        [Header("Basic Info")]
        public string equipmentName;
        [TextArea(2, 4)]
        public string description;
        public EquipmentType equipmentType;

        [Header("Stat Bonuses")]
        [Tooltip("민첩 보정치")]
        public int agiBonus = 0;
        [Tooltip("행운 보정치")]
        public int luckBonus = 0;
        [Tooltip("명중률 보정치 (0.0 ~ 1.0, 예: 0.1 = 10% 증가)")]
        [Range(0f, 1f)]
        public float accuracyBonus = 0f;

        [Header("Weapon Effect (Optional)")]
        [Tooltip("장비 시 자동으로 사용 가능한 스킬 (선택 사항)")]
        public SkillData weaponEffect;
    }
}


