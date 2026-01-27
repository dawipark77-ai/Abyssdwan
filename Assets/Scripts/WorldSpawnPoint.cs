using UnityEngine;

public class WorldSpawnPoint : MonoBehaviour
{
    // ✅ 다른 오브젝트가 이 위치를 스폰 지점으로 사용할 수 있음
    public Transform spawnPosition;

    // 필요하면 시작 시 초기화 코드 추가
    void Start()
    {
        // spawnPosition이 설정 안 되어 있으면, 자기 자신의 위치를 기본값으로 사용
        if (spawnPosition == null)
        {
            spawnPosition = this.transform;
        }
    }

    // 스폰 위치를 외부에서 가져올 수 있도록 메서드 제공
    public Vector3 GetSpawnPosition()
    {
        return spawnPosition.position;
    }
}
