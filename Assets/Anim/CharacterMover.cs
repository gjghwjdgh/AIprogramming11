using UnityEngine;
using System.Collections;

public class RootMotionMover : MonoBehaviour
{
    public Animator animator;
    public TrailRenderer swordTrail;

    public bool isDodgging = false;
    public bool isDefending = false;

    public enum AttackType { Q_Attack = 0, E_Kick = 1, R_Attack = 2 }
    private AttackType currentAttackType;

    public float qAttackTrailDuration = 0.4f;
    public float rAttackTrailDuration = 0.6f;
    public float qAttackTrailDelay = 0.05f;
    public float rAttackTrailDelay = 0.1f;

    public bool isAttacking = false;

    public SoundManager soundManager;
    public AudioClip qAttackSfx;
    public AudioClip rAttackSfx;
    public AudioClip eKickSfx;
    public AudioClip wKeySfx;
    public AudioClip sKeySfx;

    private Rigidbody rBody;




    void Start()
    {
        animator = GetComponent<Animator>();
        rBody = GetComponent<Rigidbody>();
        animator.applyRootMotion = true;




        if (swordTrail != null)
            swordTrail.enabled = false;

        if (soundManager == null)
        {
            Debug.LogError("RootMotionMover: SoundManager가 연결되지 않았습니다! 인스펙터에서 할당해주세요.");
        }
    }

    //void Update()
    //{
        //float v = 0.0f;

        //if (!isAttacking)
        //{
        //    if (Input.GetKey(KeyCode.W)) v = 1.0f;
        //    else if (Input.GetKey(KeyCode.S)) v = -1.0f;
        //}

        //animator.SetFloat("v", v);

        //if (soundManager != null)
        //{
        //    bool shouldPlayWalkingSound = !isAttacking && Input.GetKey(KeyCode.W) && v > 0;

        //    if (shouldPlayWalkingSound && wKeySfx != null)
        //    {
        //        soundManager.PlayWalkingSound(wKeySfx);
        //    }
        //    else
        //    {
        //        soundManager.StopWalkingSound();
        //    }

        //    if (!isAttacking && Input.GetKeyDown(KeyCode.S) && sKeySfx != null)
        //    {
        //        soundManager.PlaySoundEffect(sKeySfx);
        //    }
        //}

        //if (Input.GetKeyDown(KeyCode.Q) && !isAttacking)
        //{
        //    StartAttack(AttackType.Q_Attack, qAttackTrailDuration, qAttackTrailDelay);
        //}
        //if (Input.GetKeyDown(KeyCode.E) && !isAttacking)
        //{
        //    StartAttack(AttackType.E_Kick);
        //}
        //if (Input.GetKeyDown(KeyCode.R) && !isAttacking)
        //{
        //    StartAttack(AttackType.R_Attack, rAttackTrailDuration, rAttackTrailDelay);
        //}
    //}

    public void StartAttack(AttackType attackType, float trailDuration = 0f, float trailDelay = 0f)
    {

        if (isAttacking) return;

        isAttacking = true;
        currentAttackType = attackType;

        if (soundManager != null)
        {
            soundManager.StopWalkingSound();
        }

        animator.SetTrigger("attackTrigger");
        animator.SetInteger("attackIndex", (int)attackType);

        if (soundManager != null)
        {
            switch (attackType)
            {
                case AttackType.Q_Attack:
                    if (qAttackSfx != null) soundManager.PlaySoundEffect(qAttackSfx);
                    StartCoroutine(PlayTrail(trailDuration, trailDelay));
                    break;
                case AttackType.E_Kick:
                    if (eKickSfx != null) soundManager.PlaySoundEffect(eKickSfx);
                    break;
                case AttackType.R_Attack:
                    if (rAttackSfx != null) soundManager.PlaySoundEffect(rAttackSfx);
                    StartCoroutine(PlayTrail(trailDuration, trailDelay));
                    break;
            }
        }

        StartCoroutine(ResetAttackStateAfterAnimation(GetAnimationLength(attackType)));
    }

    IEnumerator PlayTrail(float duration, float delay)
    {
        if (swordTrail != null)
        {
            if (delay > 0) yield return new WaitForSeconds(delay);
            swordTrail.enabled = true;
            swordTrail.Clear();
            yield return new WaitForSeconds(duration);
            swordTrail.enabled = false;
        }
    }

    IEnumerator ResetAttackStateAfterAnimation(float animationLength)
    {
        yield return new WaitForSeconds(animationLength);
        isAttacking = false;

        // R 공격 끝났을 때 위치 보정
        if (currentAttackType == AttackType.R_Attack)
        {
            Vector3 fixedPos = transform.position;
            fixedPos.y = 0.0f;
            transform.position = fixedPos;
        }
    }

    float GetAnimationLength(AttackType attackType)
    {
        switch (attackType)
        {
            case AttackType.Q_Attack:
                return qAttackTrailDelay + qAttackTrailDuration + 0.1f;
            case AttackType.E_Kick:
                return 0.7f;
            case AttackType.R_Attack:
                return rAttackTrailDelay + rAttackTrailDuration + 0.1f;
            default:
                return 0.5f;
        }
    }

    void OnAnimatorMove()
    {

        if(animator && rBody)
        {
            Vector3 deltaPos = animator.deltaPosition;
            deltaPos.y = 0.0f; // Y 이동은 막기 (바닥 뚫림 방지)
            transform.position += deltaPos;
            transform.rotation *= animator.deltaRotation;

            //Vector3 newPos = rBody.position + deltaPos;
            //newPos.y = 0f; // 최종 위치 Y를 강제로 지면으로 고정!

            //rBody.MovePosition(newPos);
            //rBody.MoveRotation(transform.rotation * animator.deltaRotation);
        }

        //// Rigidbody로 Physics 이동 처리
        //rBody.MovePosition(transform.position + deltaPos);
        //rBody.MoveRotation(transform.rotation * animator.deltaRotation);

        //if (currentattacktype == attacktype.r_attack && isattacking)
        //{
        //    transform.position += animator.deltaposition;
        //}
        //else
        //{
        //    vector3 deltapos = animator.deltaposition;
        //    deltapos.y = 0.0f;
        //    transform.position += deltapos;
        //}

        //transform.rotation *= animator.deltarotation;

        ////// 방어 모션 처리 (선택적)
        ////if (input.getkey(keycode.leftshift))
        ////{
        ////    animator.setbool("isdefending", true);
        ////}
        ////else
        ////{
        ////    animator.setbool("isdefending", false);
        ////}
    }




    public void SetDefend(bool isDefending)
    {
        
        animator.SetBool("isDefending", isDefending);
        Debug.Log("SetDefend 호출됨: " + isDefending);  // 상태 확인
    }

    public void Dodge()
    {
        if (isDodgging) return;  // 중복 방지

        animator.SetTrigger("dodgeTrigger");
        isDodgging = true;
    }

    // 애니메이션 이벤트로 끝났을 때 호출
    public void AnimationFinished_Dodge()
    {
        isDodgging = false;
    }



    public void AnimationFinished_SetAttackingFalse()
    {
        isAttacking = false;
    }


}
