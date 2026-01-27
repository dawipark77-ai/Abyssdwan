using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class DungeonStatusView : MonoBehaviour
{
    [System.Serializable]
    public class CharacterSlot
    {
        [Header("UI Elements")]
        public GameObject slotRoot;        // 슬롯 전체를 끄고 켤 때 사용 (없으면 비워도 됨)
        public TextMeshProUGUI nameText;   // 캐릭터 이름
        public TextMeshProUGUI hpText;     // Max HP 표시 (기존 hpText를 Max HP 용도로 사용)
        public TextMeshProUGUI currentHpText; // Current HP 표시 (새로 추가)
        public TextMeshProUGUI mpText;     // Max MP 표시 (기존 mpText를 Max MP 용도로 사용)
        public TextMeshProUGUI currentMpText; // Current MP 표시 (새로 추가)
        public TextMeshProUGUI levelText;  // 레벨 (예: "Lv. 1")
        public TextMeshProUGUI classText;  // 직업 (예: "Class: Warrior")
        public TextMeshProUGUI expText;    // 경험치 (예: "EXP: 0/100")
        
        [Header("Stats")]
        public TextMeshProUGUI strText;    // 공격력 (STR)
        public TextMeshProUGUI defText;    // 방어력 (DEF)
        public TextMeshProUGUI magText;    // 마법력 (MAG)
        public TextMeshProUGUI agiText;    // 민첩성 (AGI)
        public TextMeshProUGUI lukText;    // 행운 (LUK)
        
        [Header("Optional")]
        public Image portraitImage;        // 초상화 (있다면)
    }

    [Header("Slot Configuration")]
    public List<CharacterSlot> slots = new List<CharacterSlot>();

    void OnEnable()
    {
        // 패널이 켜질 때마다 데이터 갱신
        Debug.Log("[DungeonStatusView] OnEnable Called!");
        UpdateUI();
    }

    public void UpdateUI()
    {
        // GameManager가 없으면 생성을 시도
        var gm = GameManager.EnsureInstance();
        if (gm == null)
        {
            Debug.LogError("[DungeonStatusView] GameManager is NULL!");
            return;
        }

        Debug.Log("[DungeonStatusView] GM Found. Party Count: " + GameManager.staticPartyData.Count);

        // PlayerStats 찾기 또는 생성
        PlayerStats playerInScene = FindFirstObjectByType<PlayerStats>();
        
        // PlayerStats가 없으면 생성
        if (playerInScene == null)
        {
            // 1순위: DungeonGridPlayer를 찾아서 PlayerStats 추가
            var dungeonPlayer = FindFirstObjectByType<DungeonGridPlayer>();
            if (dungeonPlayer != null)
            {
                playerInScene = dungeonPlayer.gameObject.AddComponent<PlayerStats>();
                Debug.Log("[DungeonStatusView] DungeonGridPlayer에 PlayerStats 컴포넌트 자동 추가됨");
            }
            else
            {
                // 2순위: "Player" 이름의 GameObject 찾기
                GameObject playerObj = GameObject.Find("Player");
                if (playerObj == null)
                {
                    playerObj = new GameObject("Player");
                    Debug.Log("[DungeonStatusView] 'Player' GameObject 생성됨");
                }
                playerInScene = playerObj.AddComponent<PlayerStats>();
                Debug.Log("[DungeonStatusView] PlayerStats 컴포넌트 자동 생성됨");
            }
        }
        
        // PlayerStats가 제대로 초기화되었는지 확인
        if (playerInScene != null)
        {
            // 직업이 적용되지 않았으면 적용
            if (!string.IsNullOrEmpty(playerInScene.jobClass))
            {
                playerInScene.ApplyCharacterClass();
            }
            
            // GameManager에 데이터가 없으면 PlayerStats에서 저장
            if (GameManager.staticPartyData.Count == 0)
            {
                Debug.Log("[DungeonStatusView] Syncing " + playerInScene.name + " with GM data...");
                gm.SaveFromPlayer(playerInScene);
                Debug.Log("[DungeonStatusView] 저장 완료! Party Count: " + GameManager.staticPartyData.Count + ", Class: " + playerInScene.jobClass);
            }
            else
            {
                // 데이터가 있으면 씬의 PlayerStats를 GameManager 데이터로 동기화 (저장된 HP/MP 반영)
                // 중요: 여기서 SaveFromPlayer를 호출하면 안됨! (전투 후 체력이 깎인 상태가 덮어씌워질 수 있음)
                gm.ApplyToPlayer(playerInScene);
                Debug.Log("[DungeonStatusView] PlayerStats sync(Load): " + playerInScene.playerName + " (HP: " + playerInScene.currentHP + "/" + playerInScene.maxHP + ")");
            }
        }
        else
        {
            Debug.LogError("[DungeonStatusView] PlayerStats를 생성할 수 없습니다!");
        }

        int index = 0;
        
        // GameManager에 저장된 파티 데이터 순회
        foreach (var kvp in GameManager.staticPartyData)
        {
            Debug.Log($"[DungeonStatusView] Processing Data: {kvp.Key}");
            if (index >= slots.Count) break;

            var data = kvp.Value;
            var slot = slots[index];

            // 슬롯 활성화
            if (slot.slotRoot != null) slot.slotRoot.SetActive(true);

            // 텍스트 갱신
            if (slot.nameText != null) slot.nameText.text = data.characterName;
            
            // HP 표시: 현재/최대 형식 + 보정치 표시
            // Max HP 표시 (기존 hpText 재활용)
            if (slot.hpText != null) 
            {
                // 보정치 계산 (절대값: Max - Base)
                int hpBonus = data.maxHP - data.baseHP;
                
                if (hpBonus != 0) 
                {
                    slot.hpText.text = data.maxHP + "(" + (hpBonus > 0 ? "+" : "") + hpBonus + ")";
                }
                else
                {
                    slot.hpText.text = data.maxHP.ToString();
                }
            }

            // Current HP 표시 (Current / Max)
            if (slot.currentHpText != null)
            {
                slot.currentHpText.text = data.currentHP + "/" + data.maxHP;
            }
            
            // MP 표시: 현재/최대 형식 + 보정치 표시
            // Max MP 표시 (기존 mpText 재활용)
            if (slot.mpText != null) 
            {
                // 보정치 계산 (절대값: Max - Base)
                int mpBonus = data.maxMP - data.baseMP;
                
                if (mpBonus != 0) 
                {
                    slot.mpText.text = $"{data.maxMP}({mpBonus:+#;-#;0})";
                }
                else
                {
                    slot.mpText.text = $"{data.maxMP}";
                }
            }

            // Current MP 표시 (Current / Max)
            if (slot.currentMpText != null)
            {
                slot.currentMpText.text = $"{data.currentMP}/{data.maxMP}";
            }

            if (slot.levelText != null) slot.levelText.text = $"{data.level}"; // 숫자만 표시
            
            // Class 표시 (직업이 없으면 "None" 표시)
            if (slot.classText != null) 
            {
                string displayClass = string.IsNullOrEmpty(data.jobClass) ? "None" : data.jobClass;
                slot.classText.text = displayClass;
                Debug.Log($"[DungeonStatusView] Class Text 업데이트: '{displayClass}' (jobClass: '{data.jobClass}')");
            }
            else
            {
                Debug.LogWarning("[DungeonStatusView] classText가 null입니다! Inspector에서 Class Text 필드에 TextMeshPro를 연결해주세요.");
            }
            
            // Class Description / Icon Logic Removed as per user request (Only Class Name)
            
            if (slot.expText != null) slot.expText.text = $"{data.exp} / {data.maxExp}"; // EXP 표시
            
            // 상세 스탯 (STR, DEF, MAG, AGI, LUK) - 보정치와 함께 표시
            if (slot.strText != null) 
            {
                int bonus = data.attack - data.baseAttack;
                if (bonus != 0)
                    slot.strText.text = $"{data.attack}({bonus:+#;-#;0})";
                else
                    slot.strText.text = $"{data.attack}";
            }
            
            if (slot.defText != null) 
            {
                int bonus = data.defense - data.baseDefense;
                if (bonus != 0)
                    slot.defText.text = $"{data.defense}({bonus:+#;-#;0})";
                else
                    slot.defText.text = $"{data.defense}";
            }
            
            if (slot.magText != null) 
            {
                int bonus = data.magic - data.baseMagic;
                if (bonus != 0)
                    slot.magText.text = $"{data.magic}({bonus:+#;-#;0})";
                else
                    slot.magText.text = $"{data.magic}";
            }
            
            if (slot.agiText != null) 
            {
                int bonus = data.agility - data.baseAgility;
                if (bonus != 0)
                    slot.agiText.text = $"{data.agility}({bonus:+#;-#;0})";
                else
                    slot.agiText.text = $"{data.agility}";
            }
            
            if (slot.lukText != null) 
            {
                int bonus = data.luck - data.baseLuck;
                if (bonus != 0)
                    slot.lukText.text = $"{data.luck}({bonus:+#;-#;0})";
                else
                    slot.lukText.text = $"{data.luck}";
            }

            index++;
        }

        // 데이터가 없는 남은 슬롯은 비활성화
        for (; index < slots.Count; index++)
        {
            if (slots[index].slotRoot != null) slots[index].slotRoot.SetActive(false);
            else 
            {
                // Root가 할당 안 됐으면 텍스트라도 지움
                if (slots[index].nameText != null) slots[index].nameText.text = "";
                if (slots[index].mpText != null) slots[index].mpText.text = "";
                if (slots[index].currentMpText != null) slots[index].currentMpText.text = ""; // Clear current MP
                if (slots[index].levelText != null) slots[index].levelText.text = "";
                if (slots[index].classText != null) slots[index].classText.text = "";
                if (slots[index].expText != null) slots[index].expText.text = "";
                if (slots[index].strText != null) slots[index].strText.text = "";
                if (slots[index].defText != null) slots[index].defText.text = "";
                if (slots[index].magText != null) slots[index].magText.text = "";
                if (slots[index].agiText != null) slots[index].agiText.text = "";
                if (slots[index].lukText != null) slots[index].lukText.text = "";
            }
        }
    }
}
