using UnityEngine;

public class Sword : MonoBehaviour
{
    public float damage = 10f;

    private void OnTriggerEnter(Collider other)
    {
        // 방패에 먼저 닿았는지 체크
        Shield shield = other.GetComponent<Shield>();
        if (shield != null && shield.isShieldActive)
        {
            // 방패가 활성화되어 있으면 데미지 무시
            Debug.Log("방패로 막음!");
            return;
        }

        // Body에 닿았는지 체크
        IDamageable target = other.GetComponent<IDamageable>();
        if (target != null)
        {
            target.TakeDamage(damage);
        }
    }
}
