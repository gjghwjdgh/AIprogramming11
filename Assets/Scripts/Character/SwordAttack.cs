// SwordAttack.cs
using UnityEngine;

public class SwordAttack : MonoBehaviour
{
    public int damageAmount = 20; // 칼 공격 데미지
    public float attackCooldown = 0.5f; // 공격 쿨타임
    private float lastAttackTime;

    // 이 칼을 소유한 에이전트의 최상위 부모 오브젝트 (예: Player 또는 Enemy)
    private GameObject ownerAgent;

    void Start()
    {
        // Start 시점에 이 칼의 최상위 부모 오브젝트를 찾아서 ownerAgent에 저장
        // 이렇게 하면 칼을 가진 에이전트 자신은 공격 대상에서 제외할 수 있습니다.
        ownerAgent = transform.root.gameObject;
    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log(other.name + " 와(과) 충돌 시작!"); // 1️⃣ 일단 충돌이 되는지 확인

        if (Time.time < lastAttackTime + attackCooldown)
        {
            Debug.Log("쿨타임 때문에 공격 실패!"); // 2️⃣ 쿨타임에 걸리는지 확인
            return;
        }

        if (other.gameObject == ownerAgent)
        {
            Debug.Log("내 자신을 공격함! 무시!"); // 3️⃣ 자신을 공격하는지 확인
            return;
        }

        CharacterStatus health = other.gameObject.GetComponent<CharacterStatus>();
        if (health == null)
        {
            Debug.LogError(other.name + "에게서 Health 스크립트를 찾을 수 없음!"); // 4️⃣ Health 스크립트가 없는지 확인
            return; // 여기서 return을 추가해서 아래 코드가 실행되지 않게 함
        }
        
        // 여기까지 모든 검사를 통과했다면 데미지를 줍니다.
        Debug.Log(other.name + "에게 " + damageAmount + " 데미지 적용!"); // 5️⃣ 최종 데미지 적용 확인
        health.TakeDamage(damageAmount);
        lastAttackTime = Time.time;
    }
}