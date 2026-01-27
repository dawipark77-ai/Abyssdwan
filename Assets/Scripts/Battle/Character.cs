using UnityEngine;

public class Character : MonoBehaviour
{
    public string characterName;
    public int maxHP = 100;
    public int currentHP;
    public bool isDead = false;

    void Start()
    {
        currentHP = maxHP;
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHP -= damage;
        if (currentHP <= 0)
        {
            currentHP = 0;
            Die();
        }
    }

    // ↓ override를 허용하려면 virtual 붙이기
    protected virtual void Die()
    {
        isDead = true;
        Debug.Log(characterName + " has been defeated!");
        gameObject.SetActive(false); // 임시 처리
    }
}
