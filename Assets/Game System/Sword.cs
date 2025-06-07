using UnityEngine;

public class Sword : MonoBehaviour
{
    public float damage = 10f;
    public GameObject owner;

    private Vector3 lastPosition;
    public Vector3 velocity { get; private set; }
    public Vector3 acceleration { get; private set; }



    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Sword �浹��! other = " + other.name);

        // ���п� ���� ��Ҵ��� üũ
        Shield shield = other.GetComponent<Shield>();
        if (shield != null && shield.isShieldActive)
        {
            Debug.Log("���з� ����!");
            return;
        }

        // �ڱ� �˰� �ε��� ��� ����
        if (other.GetComponent<Sword>() != null && other.GetComponent<Sword>().owner == this.owner)
        {
            Debug.Log("�ڱ� �˰� �浹 ����");
            return;
        }

        // �ڱ� �ڽ��� ��ü ����
        if (other.transform.root.gameObject == owner)
        {
            Debug.Log("�ڱ� �ڽ��� ��ü�� �浹 ����");
            return;
        }

        // ��� ���� Ȯ��
        RootMotionMover rootMotion = other.GetComponentInParent<RootMotionMover>();
        if (rootMotion != null)
        {
            bool isDefending = rootMotion.animator.GetBool("isDefending");
            Debug.Log($"{other.name}�� ��� ����: {isDefending}");

            if (isDefending)
            {
                Debug.Log($"{other.name}�� ��� ��! ������ ����.");
                return; // ��� ���̸� ������ �� ��
            }
        }

        // Body�� ��Ҵ��� üũ (��� ���� �ƴ� ��츸!)
        IDamageable target = other.GetComponent<IDamageable>();
        if (target != null)
        {
            Debug.Log($"{owner.name}�� ���� {other.name}���� ������!");
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
