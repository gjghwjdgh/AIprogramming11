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
    }

    void OnAnimatorMove()
    {
        // Animator의 Root Motion 이동 데이터를 Transform에 누적 적용
        transform.position += animator.deltaPosition;
        transform.rotation *= animator.deltaRotation;
    }
}
