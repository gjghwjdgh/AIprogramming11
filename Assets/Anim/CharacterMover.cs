using UnityEngine;
using System.Collections;

public class RootMotionMover : MonoBehaviour
{
    private Animator animator;
    public TrailRenderer swordTrail; // 인스펙터에서 연결

    void Start()
    {
        animator = GetComponent<Animator>();
        animator.applyRootMotion = true;

        if (swordTrail != null)
            swordTrail.enabled = false;
    }

    void Update()
    {
        float v = 0.0f;

        if (Input.GetKey(KeyCode.W))
            v = 1.0f;
        else if (Input.GetKey(KeyCode.S))
            v = -1.0f;

        animator.SetFloat("v", v);

        if (Input.GetKeyDown(KeyCode.Q))
        {
            animator.SetTrigger("attackTrigger");
            animator.SetInteger("attackIndex", 0);
            StartCoroutine(PlayTrail(1.0f, 0.3f)); // Q 공격 궤적
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            animator.SetTrigger("attackTrigger");
            animator.SetInteger("attackIndex", 1); // 발차기 — 검기 없음
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            animator.SetTrigger("attackTrigger");
            animator.SetInteger("attackIndex", 2);
            StartCoroutine(PlayTrail(1.5f, 0.5f)); // R 공격 궤적
        }
    }

IEnumerator PlayTrail(float duration, float delay = 0f) // 선택적 delay 파라미터 추가
{
    if (swordTrail != null)
    {
        if (delay > 0)
        {
            yield return new WaitForSeconds(delay);
        }
        swordTrail.enabled = true;
        swordTrail.Clear(); // 필요하다면 이전 궤적을 지웁니다.
        yield return new WaitForSeconds(duration);
        swordTrail.enabled = false;
    }
}

// 호출 예시 (0.1초 뒤에 0.4초 동안 궤적 활성화)
// StartCoroutine(PlayTrail(0.4f, 0.1f));

    void OnAnimatorMove()
    {
        transform.position += animator.deltaPosition;
        transform.rotation *= animator.deltaRotation;
    }
}
