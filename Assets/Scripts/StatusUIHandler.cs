using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using AbyssdawnBattle;

public class StatusUIHandler : MonoBehaviour
{
    [Header("Data")]
    public PlayerStatData playerStatData;

    [Header("UI Roots")]
    public Transform statusPanel;
    public string passiveContainerName = "PassiveSkills";
    public string activeContainerName = "ActiveSkills";

    [Header("Slots (auto-filled if empty)")]
    public List<Image> passiveSlots = new List<Image>();
    public List<Image> activeSlots = new List<Image>();

    void Awake()
    {
        if (statusPanel == null) statusPanel = transform;
    }

    void OnEnable()
    {
        // 캐시 초기화: 기존 슬롯 리스트를 무조건 비우고 새로 찾기
        passiveSlots.Clear();
        activeSlots.Clear();

        AutoCacheSlotsIfNeeded();
        PlayerStats.OnStatusChanged += RefreshUI;
        RefreshUI();
    }

    void OnDisable()
    {
        PlayerStats.OnStatusChanged -= RefreshUI;
    }

    public void RefreshUI()
    {
        if (playerStatData == null)
        {
            Debug.LogWarning("[StatusUIHandler] playerStatData is null.");
            ClearSlots(passiveSlots);
            ClearSlots(activeSlots);
            return;
        }

        ApplySkillIcons(passiveSlots, playerStatData.equippedPassives);
        ApplySkillIcons(activeSlots, playerStatData.equippedSkills);
    }

    private void ApplySkillIcons(List<Image> slots, List<SkillData> skills)
    {
        if (slots == null) return;
        int count = (skills != null) ? skills.Count : 0;

        for (int i = 0; i < slots.Count; i++)
        {
            Image slot = slots[i];
            if (slot == null) continue;

            SkillData skill = (i < count) ? skills[i] : null;
            Sprite icon = (skill != null) ? skill.skillIcon : null;
            SetSlotIcon(slot, icon);
        }
    }

    private void SetSlotIcon(Image slot, Sprite icon)
    {
        slot.sprite = icon;
        Color c = slot.color;
        c.a = (icon != null) ? 1f : 0f;
        slot.color = c;

        // 아이콘을 넣은 후 명시적으로 활성화
        slot.gameObject.SetActive(true);
    }

    private void ClearSlots(List<Image> slots)
    {
        if (slots == null) return;
        foreach (var slot in slots)
        {
            if (slot == null) continue;
            SetSlotIcon(slot, null);
        }
    }

    private void AutoCacheSlotsIfNeeded()
    {
        if (statusPanel == null) return;

        // GetComponentsInChildren로 모든 Transform 가져오기
        Transform[] allTransforms = statusPanel.GetComponentsInChildren<Transform>(true);

        Transform passiveRoot = System.Array.Find(allTransforms, t => t.name == passiveContainerName);
        if (passiveRoot != null)
        {
            Debug.Log($"[StatusUIHandler] {passiveContainerName} 부모 찾음!");
            passiveSlots = CollectSlotImages(passiveRoot);
            Debug.Log($"[StatusUIHandler] Passive slots found: {passiveSlots.Count}");
        }
        else
        {
            Debug.LogWarning($"[StatusUIHandler] '{passiveContainerName}' 컨테이너를 찾을 수 없습니다.");
        }

        Transform activeRoot = System.Array.Find(allTransforms, t => t.name == activeContainerName);
        if (activeRoot != null)
        {
            Debug.Log($"[StatusUIHandler] {activeContainerName} 부모 찾음!");
            activeSlots = CollectSlotImages(activeRoot);
            Debug.Log($"[StatusUIHandler] Active slots found: {activeSlots.Count}");
        }
        else
        {
            Debug.LogWarning($"[StatusUIHandler] '{activeContainerName}' 컨테이너를 찾을 수 없습니다.");
        }
    }

    private List<Image> CollectSlotImages(Transform root)
    {
        List<Image> images = new List<Image>();
        List<Transform> slotTransforms = new List<Transform>();

        // 자식들 중에서 이름에 "Slot"을 포함하는 모든 오브젝트를 가져오기
        foreach (Transform child in root)
        {
            if (child.name.Contains("Slot"))
            {
                slotTransforms.Add(child);
            }
        }

        // 이름 순서대로 정렬 (Slot1, Slot2, Slot3...)
        slotTransforms.Sort((a, b) => string.Compare(a.name, b.name));

        // Image 컴포넌트 찾기
        foreach (Transform slotTransform in slotTransforms)
        {
            Image img = slotTransform.GetComponent<Image>();
            if (img == null)
            {
                img = slotTransform.GetComponentInChildren<Image>(true);
            }
            if (img != null)
            {
                images.Add(img);
                Debug.Log($"[StatusUIHandler] Slot 찾음: {slotTransform.name}");
            }
        }

        return images;
    }

}





