using UnityEngine;

public class RootMotionMover : MonoBehaviour
{
    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
        animator.applyRootMotion = true;  //  반드시 켜야 함
    }

    void Update()
    {
        float v = 0.0f;

        if (Input.GetKey(KeyCode.W))
            v = 1.0f;
        else if (Input.GetKey(KeyCode.S))
            v = -1.0f;

        else
        {
            v = 0;
        }


        // 애니메이터에 v 값만 전달 → 애니메이션 전환만 담당
        animator.SetFloat("v", v);

        // Q 키 → 공격 모션 1 휘두르기
        if (Input.GetKeyDown(KeyCode.Q))
        {
            animator.SetTrigger("attackTrigger");
            animator.SetInteger("attackIndex", 0);
            
        }

        // E 키 → 공격 모션 2 발차기
        if (Input.GetKeyDown(KeyCode.E))
        {
            animator.SetTrigger("attackTrigger");
            animator.SetInteger("attackIndex", 1);
            
        }

        // R 키 → 공격 모션 3 회전베기
        if (Input.GetKeyDown(KeyCode.R))
        {
            animator.SetTrigger("attackTrigger");
            animator.SetInteger("attackIndex", 2);

        }

    }

    void OnAnimatorMove()
    {
        // Animator의 Root Motion 이동 데이터를 Transform에 누적 적용
        transform.position += animator.deltaPosition;
        transform.rotation *= animator.deltaRotation;
    }
}
