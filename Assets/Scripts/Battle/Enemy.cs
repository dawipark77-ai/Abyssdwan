using UnityEngine;

public class Enemy : Character
{
    // Character의 Die()를 재정의
    protected override void Die()
    {
        base.Die(); // 기본 사망 처리 유지
        Debug.Log("Enemy-specific death logic"); // 적 전용 처리
    }
}
