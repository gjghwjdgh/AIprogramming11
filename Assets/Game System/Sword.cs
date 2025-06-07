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
        Debug.Log("Sword 충돌됨! other = " + other.name);

        // 방패 충돌 체크
        Shield shield = other.GetComponent<Shield>();
        if (shield != null && shield.isShieldActive)
        {
            Debug.Log("방패로 막음!");
            return;
        }

        // 자기 검과 충돌 무시
        Sword otherSword = other.GetComponent<Sword>();
        if (otherSword != null && otherSword.owner == this.owner)
        {
            Debug.Log("자기 검과 충돌 무시");
            return;
        }

        // 자기 자신의 몸체와 충돌 무시
        if (other.transform.root.gameObject == owner)
        {
            Debug.Log("자기 자신의 몸체와 충돌 무시");
            return;
        }

        //  [중요!] 공격자가 공격 중인지 체크
        RootMotionMover myRootMotion = owner.GetComponent<RootMotionMover>();
        if (myRootMotion != null && !myRootMotion.isAttacking)
        {
            Debug.Log($"{owner.name}가 공격 중이 아님! 데미지 무시");
            return;
        }

        // 상대방의 방어 여부만 확인
        RootMotionMover rootMotion = other.GetComponentInParent<RootMotionMover>();
        if (rootMotion != null)
        {
            bool isDefending = rootMotion.animator.GetBool("isDefending");
            Debug.Log($"{other.name}의 방어 상태: {isDefending}");

            if (isDefending)
            {
                Debug.Log($"{other.name}가 방어 중! 데미지 무시.");
                return;
            }
        }

        // 데미지 주기
        IDamageable target = other.GetComponent<IDamageable>();
        if (target != null)
        {
            Debug.Log($"{owner.name}의 검이 {other.name}에게 데미지!");
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
