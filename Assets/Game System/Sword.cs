using UnityEngine;

public class Sword : MonoBehaviour
{
    public float damage = 10f;

    private Vector3 lastPosition;
    public Vector3 velocity { get; private set; }
    public Vector3 acceleration { get; private set; }

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

    private void Update()
    {
        Vector3 currentVelocity = (transform.position - lastPosition) / Time.deltaTime;
        acceleration = (currentVelocity - velocity) / Time.deltaTime;
        velocity = currentVelocity;
        lastPosition = transform.position;
    }
}
