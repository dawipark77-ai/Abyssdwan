using UnityEngine;

public class BattleSystem : MonoBehaviour
{
    public PlayerStats player;
    public EnemyStats enemy;
    public bool battleEnded = false;

    [Header("Item Settings")]
    public int potionCount = 3;
    public int potionHealAmount = 20;

    [Header("Defence System")]
    public bool isDefending = false;        // 방어 중인지 체크
    public float defenceReduction = 0.5f;   // 방어 시 받는 데미지 비율 (50%)

    // -------------------- 회피 / 크리티컬 --------------------
    private bool CheckHit(int attackerAgility, int targetAgility)
    {
        int hitChance = attackerAgility * 2 - targetAgility; // 간단 공식
        hitChance = Mathf.Clamp(hitChance, 10, 100);         // 최소10% 최대100%
        int roll = Random.Range(0, 100);
        return roll <= hitChance;
    }

    private bool CheckCritical(int luck)
    {
        float critChance = luck * 0.05f; // 1 luck = 5%
        return Random.value < critChance;
    }

    // -------------------- 플레이어 공격 --------------------
    public int PlayerAttack()
    {
        if (battleEnded) return 0;

        if (!CheckHit(player.Agility, enemy.Agility))
        {
            Debug.Log("Player attack missed!");
            return 0;
        }

        int damage = Mathf.Max(1, player.attack - enemy.defense);

        if (CheckCritical(player.luck))
        {
            damage = Mathf.RoundToInt(damage * 1.5f);
            Debug.Log("Critical hit!");
        }

        enemy.TakeDamage(damage);
        if (enemy.currentHP <= 0) battleEnded = true;

        return damage;
    }

    // -------------------- 적 공격 --------------------
    public int EnemyAttack()
    {
        if (battleEnded) return 0;

        if (!CheckHit(enemy.Agility, player.Agility))
        {
            Debug.Log("Enemy attack missed!");
            return 0;
        }

        int damage = Mathf.Max(1, enemy.attack - player.defense);

        if (CheckCritical(enemy.luck))
        {
            damage = Mathf.RoundToInt(damage * 1.5f);
            Debug.Log("Enemy critical hit!");
        }

        if (isDefending)
        {
            damage = Mathf.RoundToInt(damage * defenceReduction);
            isDefending = false; // 한 턴만 적용
            Debug.Log("Player defended! Damage reduced.");
        }

        player.TakeDamage(damage);
        if (player.currentHP <= 0) battleEnded = true;

        return damage;
    }

    // -------------------- 아이템 사용 --------------------
    public int UsePotion()
    {
        if (battleEnded || potionCount <= 0) return 0;

        potionCount--;
        player.Heal(potionHealAmount);
        return potionHealAmount;
    }

    // -------------------- 방어 --------------------
    public void Defend()
    {
        if (battleEnded) return;
        isDefending = true;
    }
}
