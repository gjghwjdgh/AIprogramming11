// 파일 이름: PaladinActuator.cs
using UnityEngine;

// BT 시스템이 사용할 고유한 이름의 '몸통' 클래스입니다.
public class PaladinActuator : MonoBehaviour 
{
    private Animator animator;

    public bool IsActionInProgress { get; private set; } // 현재 행동이 진행 중인지 여부

    public bool IsCurrentlyDefending { get; private set; } // 현재 방어 중인지 여부

    // 공격 타입을 정의하는 Enum
    public enum AttackType { None = 0, Q_Attack = 1, E_Kick = 2, R_Attack = 3 }

    void Awake()
    {
        animator = GetComponent<Animator>();
        IsActionInProgress = false; // 초기 상태는 행동이 진행 중이 아님
    }

    //     // PaladinActuator.cs에 이 Update 함수를 임시로 추가하세요.
    // private void Update()
    // {
    //     // W 키를 누르고 있는 동안 v 파라미터를 1로 설정
    //     if (Input.GetKey(KeyCode.W))
    //     {
    //         animator.SetFloat("v", 1f);
    //         // 콘솔에 현재 상태를 직접 출력해서 확인
    //         Debug.Log("W Key Pressed: Setting v = 1");
    //     }
    //     else
    //     {
    //         // W 키를 떼면 v 파라미터를 0으로 설정
    //         animator.SetFloat("v", 0f);
    //     }
    // }





    public void OnActionStart()
    {
        IsActionInProgress = true; // 행동이 시작되면 true로 설정
    }
    public void OnActionEnd()
    {
        IsActionInProgress = false; // 행동이 끝나면 false로 설정
    }

    public void StartDefense()
    {
        IsCurrentlyDefending = true; // 방어 시작
        animator.SetTrigger("startDefend");
    }
    public void StopDefense()
    {
        IsCurrentlyDefending = false; // 방어 중지
        animator.SetTrigger("endDefend");
    }

    // --- 행동 실행을 위한 공개 함수들 ---

    public void StartAttack(AttackType attackType)
    {
        if (attackType != AttackType.None)
        {
            animator.SetTrigger("attackTrigger");
            animator.SetInteger("attackIndex", (int)attackType - 1);
        }
    }
    
    public void SetDefend(bool isDefending)
    {
        animator.SetBool("isDefending", isDefending);
    }
    
    public void Dodge(string direction)
    {
        animator.SetTrigger("Dodge_" + direction);
    }

    public void FeintStep(string direction)
    {
        animator.SetTrigger("FeintStep_" + direction);
    }

    public void Die()
    {
        animator.SetTrigger("Die");
    }

    public void SetMovement(float vertical)
    {
        animator.SetFloat("v", vertical);
    }

    public void SetRotation(Quaternion rotation)
    {
        transform.rotation = rotation;
    }

    public void Strafe(string direction, System.Action onComplete, float duration = 3.3f)
    {
        StartCoroutine(StrafeCoroutine(direction, duration, onComplete));
    }

    private System.Collections.IEnumerator StrafeCoroutine(string direction, float duration, System.Action onComplete)
    {
        Quaternion originalRotation = transform.rotation;
        float angle = (direction == "Left") ? -90f : 90f;
        transform.Rotate(0, angle, 0);
        SetMovement(1.0f);
        yield return new WaitForSeconds(duration);
        SetMovement(0);
        transform.rotation = originalRotation;

        // 동작이 모두 끝났으므로, 약속했던 콜백 함수를 실행해서 알려줌
        onComplete?.Invoke();
    }
}