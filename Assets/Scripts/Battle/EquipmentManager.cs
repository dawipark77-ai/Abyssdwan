using UnityEngine;
using System.Collections.Generic;
using AbyssdawnBattle;

/// <summary>
/// 플레이어의 장비 장착을 관리하는 클래스
/// </summary>
public class EquipmentManager : MonoBehaviour
{
    [Header("Equipment Slots")]
    [Tooltip("오른손 장비")]
    public EquipmentData rightHand;
    [Tooltip("왼손 장비")]
    public EquipmentData leftHand;
    [Tooltip("몸통 장비")]
    public EquipmentData body;
    [Tooltip("장신구 1")]
    public EquipmentData accessory1;
    [Tooltip("장신구 2")]
    public EquipmentData accessory2;

    private PlayerStats playerStats;

    void Awake()
    {
        playerStats = GetComponent<PlayerStats>();
        if (playerStats == null)
        {
            Debug.LogError("[EquipmentManager] PlayerStats 컴포넌트를 찾을 수 없습니다!");
        }
    }

    void Start()
    {
        // 시작 시 장비 보정치 반영
        RefreshStats();
    }

    /// <summary>
    /// 장비를 장착합니다.
    /// </summary>
    /// <param name="equipment">장착할 장비 데이터</param>
    /// <returns>장착 성공 여부</returns>
    public bool EquipItem(EquipmentData equipment)
    {
        if (equipment == null)
        {
            Debug.LogWarning("[EquipmentManager] 장착하려는 장비가 null입니다.");
            return false;
        }

        switch (equipment.equipmentType)
        {
            case EquipmentType.RightHand:
                if (rightHand != null)
                {
                    Debug.Log($"[EquipmentManager] {rightHand.equipmentName}을(를) 해제하고 {equipment.equipmentName}을(를) 장착합니다.");
                }
                rightHand = equipment;
                break;
            case EquipmentType.LeftHand:
                if (leftHand != null)
                {
                    Debug.Log($"[EquipmentManager] {leftHand.equipmentName}을(를) 해제하고 {equipment.equipmentName}을(를) 장착합니다.");
                }
                leftHand = equipment;
                break;
            case EquipmentType.Body:
                if (body != null)
                {
                    Debug.Log($"[EquipmentManager] {body.equipmentName}을(를) 해제하고 {equipment.equipmentName}을(를) 장착합니다.");
                }
                body = equipment;
                break;
            case EquipmentType.Accessory:
                // Accessory는 빈 슬롯에 자동으로 장착
                if (accessory1 == null)
                {
                    accessory1 = equipment;
                }
                else if (accessory2 == null)
                {
                    accessory2 = equipment;
                }
                else
                {
                    Debug.LogWarning("[EquipmentManager] 장신구 슬롯이 모두 찼습니다. 먼저 해제해주세요.");
                    return false;
                }
                break;
        }

        RefreshStats();
        Debug.Log($"[EquipmentManager] {equipment.equipmentName} 장착 완료!");
        return true;
    }

    /// <summary>
    /// 장비를 해제합니다.
    /// </summary>
    /// <param name="equipmentType">해제할 장비 타입</param>
    /// <param name="slotIndex">Accessory의 경우 슬롯 인덱스 (1 또는 2)</param>
    /// <returns>해제 성공 여부</returns>
    public bool UnequipItem(EquipmentType equipmentType, int slotIndex = 1)
    {
        EquipmentData unequippedItem = null;

        switch (equipmentType)
        {
            case EquipmentType.RightHand:
                unequippedItem = rightHand;
                rightHand = null;
                break;
            case EquipmentType.LeftHand:
                unequippedItem = leftHand;
                leftHand = null;
                break;
            case EquipmentType.Body:
                unequippedItem = body;
                body = null;
                break;
            case EquipmentType.Accessory:
                if (slotIndex == 1)
                {
                    unequippedItem = accessory1;
                    accessory1 = null;
                }
                else if (slotIndex == 2)
                {
                    unequippedItem = accessory2;
                    accessory2 = null;
                }
                break;
        }

        if (unequippedItem != null)
        {
            RefreshStats();
            Debug.Log($"[EquipmentManager] {unequippedItem.equipmentName} 해제 완료!");
            return true;
        }

        return false;
    }

    /// <summary>
    /// 장착된 모든 장비의 보정치를 합산하여 PlayerStats에 반영합니다.
    /// </summary>
    public void RefreshStats()
    {
        if (playerStats == null) return;

        // PlayerStats의 GetEquipmentAgilityBonus, GetEquipmentLuckBonus, GetEquipmentAccuracyBonus가
        // 이 메서드를 통해 장비 보정치를 가져오도록 구현되어 있습니다.
        // 실제 계산은 PlayerStats의 해당 메서드에서 수행됩니다.

        // 스탯 변경 이벤트 발동 (UI 업데이트용)
        playerStats.NotifyStatusChanged();
    }

    /// <summary>
    /// 장착된 모든 장비 리스트를 반환합니다.
    /// </summary>
    public List<EquipmentData> GetEquippedItems()
    {
        List<EquipmentData> items = new List<EquipmentData>();
        if (rightHand != null) items.Add(rightHand);
        if (leftHand != null) items.Add(leftHand);
        if (body != null) items.Add(body);
        if (accessory1 != null) items.Add(accessory1);
        if (accessory2 != null) items.Add(accessory2);
        return items;
    }

    /// <summary>
    /// 특정 타입의 장비가 장착되어 있는지 확인합니다.
    /// </summary>
    public bool HasEquipment(EquipmentType type)
    {
        switch (type)
        {
            case EquipmentType.RightHand:
                return rightHand != null;
            case EquipmentType.LeftHand:
                return leftHand != null;
            case EquipmentType.Body:
                return body != null;
            case EquipmentType.Accessory:
                return accessory1 != null || accessory2 != null;
            default:
                return false;
        }
    }
}

