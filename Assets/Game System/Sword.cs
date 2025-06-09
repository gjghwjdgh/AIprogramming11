// 파일 이름: Sword.cs (최종 버전)
using UnityEngine;

public class Sword : MonoBehaviour
{
    public GameObject owner; // 칼의 주인 (공격 에이전트)
    public float damageAmount = 20.0f;

    private void OnTriggerEnter(Collider other)
    {
        // 자기 자신이나 경계선과 부딪히면 무시
        if (other.gameObject == owner || other.CompareTag("Boundary"))
        {
            return;
        }

        // 상대방이 MLtest인지 확인
        if (other.TryGetComponent<MLtest>(out var opponent1))
        {
            opponent1.TakeDamage(damageAmount); // 상대에게 데미지

            // 공격자인 나에게 성공 보상 (내가 MLtest2일 수도 있으므로)
            owner.GetComponent<MLtest2>()?.OnSuccessfulAttack(damageAmount);

            // 한 번만 때리도록 콜라이더 비활성화
            GetComponent<Collider>().enabled = false;
        }
        // 상대방이 MLtest2인지 확인
        else if (other.TryGetComponent<MLtest2>(out var opponent2))
        {
            opponent2.TakeDamage(damageAmount); // 상대에게 데미지

            // 공격자인 나에게 성공 보상 (내가 MLtest일 수도 있으므로)
            owner.GetComponent<MLtest>()?.OnSuccessfulAttack(damageAmount);

            // 한 번만 때리도록 콜라이더 비활성화
            GetComponent<Collider>().enabled = false;
        }
    }
}