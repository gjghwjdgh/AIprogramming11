using UnityEngine;
using System.Collections;

public class RootMotionMover : MonoBehaviour
{
    private Animator animator;
    public TrailRenderer swordTrail;

    public enum AttackType { Q_Attack = 0, E_Kick = 1, R_Attack = 2 }

    public float qAttackTrailDuration = 0.4f;
    public float rAttackTrailDuration = 0.6f;
    public float qAttackTrailDelay = 0.05f;
    public float rAttackTrailDelay = 0.1f;

    private bool isAttacking = false;
    // float v_input_value = 0.0f; // Update 함수 내의 지역 변수 v를 활용하므로 멤버 변수 불필요

    // --- 사운드 관련 변수 ---
    public SoundManager soundManager;       // SoundManager 참조 (인스펙터에서 연결)
    public AudioClip qAttackSfx;            // Q 공격 시 재생할 오디오 클립
    public AudioClip rAttackSfx;            // R 공격 시 재생할 오디오 클립
    public AudioClip eKickSfx;              // E 발차기 공격 시 재생할 오디오 클립
    public AudioClip wKeySfx;               // W 키 누를 때 (걷기) 재생할 오디오 클립
    public AudioClip sKeySfx;               // S 키 누를 때 재생할 오디오 클립
    // ---------------------------

    void Start()
    {
        animator = GetComponent<Animator>();
        animator.applyRootMotion = true;

        if (swordTrail != null)
            swordTrail.enabled = false;

        if (soundManager == null)
        {
            Debug.LogError("RootMotionMover: SoundManager가 연결되지 않았습니다! 인스펙터에서 SoundManager 오브젝트를 할당해주세요.");
            // 필요하다면 여기서 FindObjectOfType<SoundManager>() 등으로 찾아볼 수 있습니다.
            // soundManager = FindObjectOfType<SoundManager>();
        }
    }

    void Update()
    {
        float v = 0.0f; // 현재 프레임의 이동 입력 값

        // 공격 중이 아닐 때만 이동 입력 처리
        if (!isAttacking)
        {
            if (Input.GetKey(KeyCode.W)) v = 1.0f;
            else if (Input.GetKey(KeyCode.S)) v = -1.0f;
        }
        // 공격 중일 때는 v = 0.0f (위에서 초기화된 값 그대로 사용)
        animator.SetFloat("v", v);


        // --- 걷는 소리(W 키) 및 S 키 사운드 제어 ---
        if (soundManager != null)
        {
            // 조건: 공격 중이 아니고, W키를 누르고 있고, 실제로 앞으로 이동할 때 (v > 0)
            bool shouldPlayWalkingSound = !isAttacking && Input.GetKey(KeyCode.W) && v > 0;

            if (shouldPlayWalkingSound && wKeySfx != null)
            {
                soundManager.PlayWalkingSound(wKeySfx); // SoundManager에 구현된 반복 재생 함수 호출
            }
            else
            {
                // 위 조건이 아닐 경우 (W키를 떼거나, S키를 누르거나, 가만히 있거나, 공격 중일 때 등)
                // 걷는 소리는 멈춤
                soundManager.StopWalkingSound(); // SoundManager에 구현된 정지 함수 호출
            }

            // S 키는 이전처럼 단발성 효과음으로 유지 (공격 중이 아닐 때만)
            if (!isAttacking && Input.GetKeyDown(KeyCode.S) && sKeySfx != null)
            {
                soundManager.PlaySoundEffect(sKeySfx);
            }
        }
        // -------------------------------------------


        // 공격 입력 처리
        if (Input.GetKeyDown(KeyCode.Q) && !isAttacking)
        {
            StartAttack(AttackType.Q_Attack, qAttackTrailDuration, qAttackTrailDelay);
        }
        if (Input.GetKeyDown(KeyCode.E) && !isAttacking)
        {
            // E키는 발차기이므로 궤적 시간과 딜레이는 0으로 전달 (또는 StartAttack 오버로딩)
            StartAttack(AttackType.E_Kick);
        }
        if (Input.GetKeyDown(KeyCode.R) && !isAttacking)
        {
            StartAttack(AttackType.R_Attack, rAttackTrailDuration, rAttackTrailDelay);
        }
    }

    void StartAttack(AttackType attackType, float trailDuration = 0f, float trailDelay = 0f)
    {
        isAttacking = true;

        // 공격 시작 시 걷는 소리 즉시 멈춤
        if(soundManager != null)
        {
            soundManager.StopWalkingSound();
        }

        animator.SetTrigger("attackTrigger");
        animator.SetInteger("attackIndex", (int)attackType);

        // 사운드 재생 로직
        if (soundManager != null)
        {
            switch (attackType)
            {
                case AttackType.Q_Attack:
                    if (qAttackSfx != null) soundManager.PlaySoundEffect(qAttackSfx);
                    StartCoroutine(PlayTrail(trailDuration, trailDelay)); // Q 공격은 궤적 재생
                    break;
                case AttackType.E_Kick:
                    if (eKickSfx != null) soundManager.PlaySoundEffect(eKickSfx); // E 발차기 사운드 재생
                    // 발차기는 궤적 없음
                    break;
                case AttackType.R_Attack:
                    if (rAttackSfx != null) soundManager.PlaySoundEffect(rAttackSfx);
                    StartCoroutine(PlayTrail(trailDuration, trailDelay)); // R 공격은 궤적 재생
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
    }

    float GetAnimationLength(AttackType attackType)
    {
        switch (attackType)
        {
            case AttackType.Q_Attack:
                return qAttackTrailDelay + qAttackTrailDuration + 0.1f;
            case AttackType.E_Kick:
                return 0.7f; // E 발차기 애니메이션 예상 길이 (사운드 길이에 맞춰 조절 가능)
            case AttackType.R_Attack:
                return rAttackTrailDelay + rAttackTrailDuration + 0.1f;
            default:
                return 0.5f;
        }
    }

    void OnAnimatorMove()
    {
        transform.position += animator.deltaPosition;
        transform.rotation *= animator.deltaRotation;
    }

    public void AnimationFinished_SetAttackingFalse()
    {
        isAttacking = false;
    }
}