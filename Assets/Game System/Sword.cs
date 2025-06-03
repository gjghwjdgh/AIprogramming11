using UnityEngine;

public class Sword : MonoBehaviour
{
    public float damage = 10f;

    private void OnTriggerEnter(Collider other)
    {
        // ���п� ���� ��Ҵ��� üũ
        Shield shield = other.GetComponent<Shield>();
        if (shield != null && shield.isShieldActive)
        {
            // ���а� Ȱ��ȭ�Ǿ� ������ ������ ����
            Debug.Log("���з� ����!");
            return;
        }

        // Body�� ��Ҵ��� üũ
        IDamageable target = other.GetComponent<IDamageable>();
        if (target != null)
        {
            target.TakeDamage(damage);
        }
    }
}
