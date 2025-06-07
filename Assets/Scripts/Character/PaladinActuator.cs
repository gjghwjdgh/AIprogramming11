// 파일 이름: PaladinActuator.cs
using UnityEngine;

// BT 시스템이 사용할 고유한 이름의 '몸통' 클래스입니다.
public class PaladinActuator : MonoBehaviour 
{
    private Animator animator;

    // 공격 타입을 정의하는 Enum
    public enum AttackType { None = 0, Q_Attack = 1, E_Kick = 2, R_Attack = 3 }

    void Awake()
    {
        animator = GetComponent<Animator>();
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
}