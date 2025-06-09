// PaladinActuator.cs (수정된 버전)
using UnityEngine;
using System.Collections;

public class PaladinActuator : MonoBehaviour 
{
    private Animator animator;

    // 공격 시 사용할 콜라이더 오브젝트 참조는 유지할 수 있습니다.
    // 다른 스크립트에서 참조하거나, 수동으로 제어할 수 있기 때문입니다.
    public GameObject basicAttackColliderObject;
    public GameObject kickAttackColliderObject;
    public GameObject spinAttackColliderObject;

    public bool IsActionInProgress { get; private set; }
    public bool IsCurrentlyDefending { get; private set; }

    public enum AttackType { None = 0, Q_Attack = 1, E_Kick = 2, R_Attack = 3 }

    void Awake()
    {
        animator = GetComponent<Animator>();
        IsActionInProgress = false;

        // ※※※ 시작 시 콜라이더를 강제로 비활성화하는 코드를 제거하거나 주석 처리합니다. ※※※
        // if (basicAttackColliderObject != null) basicAttackColliderObject.SetActive(false);
        // if (kickAttackColliderObject != null) kickAttackColliderObject.SetActive(false);
        // if (spinAttackColliderObject != null) spinAttackColliderObject.SetActive(false);
    }

    public void OnActionStart()
    {
        IsActionInProgress = true;
    }
    public void OnActionEnd()
    {
        IsActionInProgress = false;
    }

    public void StartDefense()
    {
        IsCurrentlyDefending = true;
        animator.SetTrigger("startDefend");
    }
    public void StopDefense()
    {
        IsCurrentlyDefending = false;
        animator.SetTrigger("endDefend");
    }

    public void StartAttack(AttackType attackType)
    {
        if (attackType != AttackType.None)
        {
            animator.SetTrigger("attackTrigger");
            animator.SetInteger("attackIndex", (int)attackType - 1);

            // ※※※ 코루틴을 호출하여 콜라이더를 껐다 켜는 코드를 제거했습니다. ※※※
        }
    }
    
    // ※※※ AttackColliderControlCoroutine 함수 전체를 삭제했습니다. ※※※


    public void SetDefend(bool isDefending)
    {
        animator.SetBool("isDefending", isDefending);
    }
    
    public void Dodge(string direction)
    {
        if (direction == "Backward" || direction == "Left" || direction == "Right")
        {
            animator.SetTrigger("Dodge_Backward");
        }
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
        onComplete?.Invoke();
    }

    public void ExecuteTimedDefense(float duration)
    {
        StartCoroutine(TimedDefenseCoroutine(duration));
    }

    private System.Collections.IEnumerator TimedDefenseCoroutine(float duration)
    {
        OnActionStart();
        StartDefense();
        yield return new WaitForSeconds(duration);
        StopDefense();
        OnActionEnd();
    }
}