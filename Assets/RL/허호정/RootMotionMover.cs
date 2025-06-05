using UnityEngine;
using System.Collections;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class RootMotionRLAgent : Agent // 클래스 이름을 RootMotionRLAgent 등으로 변경
{
    // 기존 RootMotionMover의 멤버 변수들
    private Animator animator;
    public TrailRenderer swordTrail;

    public enum AttackType { None = 0, Q_Attack = 1, E_Kick = 2, R_Attack = 3 } // None 추가
    private AttackType currentAttackTypeInternal; // 내부 상태 추적용

    public float qAttackTrailDuration = 0.4f;
    public float rAttackTrailDuration = 0.6f;
    public float qAttackTrailDelay = 0.05f;
    public float rAttackTrailDelay = 0.1f;

    private bool isAttacking = false;

    public SoundManager soundManager;
    public AudioClip qAttackSfx;
    public AudioClip rAttackSfx;
    public AudioClip eKickSfx;
    public AudioClip wKeySfx;
    public AudioClip sKeySfx;

    // RL 에이전트 관련 설정 변수
    [Header("RL Settings")]
    public Transform target; // 학습 목표 (예: 공격할 대상)
    public float arenaSizeX = 10f; // 경기장 X축 절반 크기 (예: 10이면 -10 ~ 10)
    public float arenaSizeZ = 10f; // 경기장 Z축 절반 크기
    public float moveSpeedMultiplier = 1.0f; // 애니메이터 v값에 곱해질 값 (루트모션 속도 조절용)

    private Vector3 startPosition;
    private Quaternion startRotation;

    // --- Agent 기본 메서드 ---

    public override void Initialize()
    {
        animator = GetComponent<Animator>();
        animator.applyRootMotion = true; // 루트 모션 사용

        if (swordTrail != null)
            swordTrail.enabled = false;

        if (soundManager == null)
        {
            // 에디터에서 경고를 보고 SoundManager를 할당하도록 유도
            // Debug.LogError("SoundManager가 연결되지 않았습니다!", this);
        }

        startPosition = transform.localPosition;
        startRotation = transform.localRotation;

        // isAttacking은 에피소드 시작 시 확실히 false로
        isAttacking = false;
        currentAttackTypeInternal = AttackType.None;
    }

    public override void OnEpisodeBegin()
    {
        // 에이전트 위치 및 회전 리셋
        transform.localPosition = startPosition + new Vector3(Random.Range(-arenaSizeX / 2, arenaSizeX / 2), 0, Random.Range(-arenaSizeZ / 2, arenaSizeZ / 2));
        transform.localRotation = Quaternion.Euler(0, Random.Range(0, 360), 0);


        // 타겟이 있다면 타겟 위치도 리셋 (경기장 내 랜덤 위치)
        if (target != null)
        {
            target.localPosition = new Vector3(Random.Range(-arenaSizeX * 0.8f, arenaSizeX * 0.8f),
                                             target.localPosition.y, // 타겟의 Y축은 유지하거나 고정값 사용
                                             Random.Range(-arenaSizeZ * 0.8f, arenaSizeZ * 0.8f));
        }

        // 상태 변수 초기화
        isAttacking = false;
        currentAttackTypeInternal = AttackType.None;
        animator.SetFloat("v", 0f);
        animator.SetBool("isDefending", false);
        // 애니메이터 트리거 리셋 (필요하다면)
        // animator.ResetTrigger("attackTrigger");

        if (swordTrail != null)
            swordTrail.enabled = false;

        if (soundManager != null)
            soundManager.StopWalkingSound();
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // 에이전트 자신의 상태
        sensor.AddObservation(transform.localPosition.normalized); // 정규화된 위치 (3 floats)
        sensor.AddObservation(transform.forward); // 정규화된 전방 벡터 (3 floats) - 회전 대신 사용 가능
        sensor.AddObservation(isAttacking); // 현재 공격 중인지 (1 float: 0 or 1)
        sensor.AddObservation(animator.GetBool("isDefending")); // 현재 방어 중인지 (1 float: 0 or 1)
        sensor.AddObservation(animator.GetFloat("v")); // 현재 이동 값 (1 float)

        // 타겟과의 관계 (타겟이 설정되어 있을 경우)
        if (target != null)
        {
            Vector3 dirToTarget = (target.position - transform.position).normalized;
            sensor.AddObservation(dirToTarget); // 타겟 방향 (3 floats)
            sensor.AddObservation(Vector3.Distance(transform.position, target.position)); // 타겟과의 거리 (1 float)
        }
        else // 타겟이 없는 경우 0으로 채움
        {
            sensor.AddObservation(Vector3.zero); // 3 floats
            sensor.AddObservation(0f); // 1 float
        }
        // 총 관찰 수: 3 + 3 + 1 + 1 + 1 + 3 + 1 = 13 floats (타겟 있을 시)
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        // 행동 배열 정의 (Behavior Parameters에서 설정한 대로)
        // Discrete Actions:
        // Branch 0: 이동 (0: 정지, 1: 앞으로, 2: 뒤로)
        // Branch 1: 공격 (0: 안함, 1: Q, 2: E, 3: R)
        // Branch 2: 방어 (0: 안함, 1: 방어)

        int moveAction = actions.DiscreteActions[0];
        int attackAction = actions.DiscreteActions[1]; // AttackType enum과 값 일치 (0: None, 1:Q, 2:E, 3:R)
        int defenseAction = actions.DiscreteActions[2];

        // 1. 이동 처리
        float v = 0.0f;
        if (!isAttacking) // 공격 중이 아닐 때만 이동/방어 가능
        {
            if (moveAction == 1) v = 1.0f * moveSpeedMultiplier;
            else if (moveAction == 2) v = -1.0f * moveSpeedMultiplier;
        }
        animator.SetFloat("v", v);

        // 2. 방어 처리
        if (!isAttacking)
        {
            animator.SetBool("isDefending", defenseAction == 1);
        }

        // 3. 공격 처리
        if (!isAttacking && attackAction != (int)AttackType.None)
        {
            AttackType selectedAttack = (AttackType)attackAction;
            StartAttackInternal(selectedAttack);
        }

        // --- 보상 로직 ---
        // 살아있는 것에 대한 작은 패널티 (시간 경과 패널티)
        AddReward(-0.0005f);

        // (예시) 타겟과의 거리에 따른 보상 (가까워지면 보상)
        if (target != null)
        {
            float distanceToTarget = Vector3.Distance(transform.position, target.position);
            // AddReward((1.0f / (1.0f + distanceToTarget)) * 0.001f); // 거리가 가까울수록 큰 보상 (미세 조정 필요)

            // (예시) 타겟을 공격했을 때 큰 보상
            // 이 부분은 실제 공격 판정 시스템과 연동해야 합니다.
            // 여기서는 간단히 공격 애니메이션 중에 타겟이 특정 범위 내에 있으면 보상
            if (isAttacking && distanceToTarget < 2.0f) // Q, E 공격 범위 대략 2m 가정
            {
                // 어떤 공격이냐에 따라 보상 차등 가능
                switch (currentAttackTypeInternal)
                {
                    case AttackType.Q_Attack:
                        AddReward(0.5f); // Q공격 성공
                        Debug.Log("Q Attack Hit Target! Reward: 0.5");
                        break;
                    case AttackType.E_Kick:
                        AddReward(0.3f); // E공격 성공
                        Debug.Log("E Kick Hit Target! Reward: 0.3");
                        break;
                    case AttackType.R_Attack: // R공격은 범위가 더 넓거나 효과가 강할 수 있음
                        if (distanceToTarget < 3.0f) AddReward(1.0f); // R공격 성공
                        Debug.Log("R Attack Hit Target! Reward: 1.0");
                        break;
                }
                // 성공적으로 공격했으므로 에피소드 종료 또는 타겟 리셋
                // EndEpisode();
                // 또는 타겟 위치를 재설정해서 계속 학습
                // target.localPosition = new Vector3(Random.Range(-arenaSizeX * 0.8f, arenaSizeX * 0.8f), target.localPosition.y, Random.Range(-arenaSizeZ * 0.8f, arenaSizeZ * 0.8f));
            }
        }

        // (예시) 너무 오래 공격하지 않으면 작은 패널티
        // if (!isAttacking && GetCumulativeReward() < -0.5f) // 특정 시간 동안 공격 안하면
        // {
        //    AddReward(-0.01f);
        // }


        // --- 사운드 처리 (선택적) ---
        HandleSounds(v, moveAction);
    }


    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActionsOut = actionsOut.DiscreteActions;
        discreteActionsOut.Clear();

        // 이동 (W, S 키)
        if (Input.GetKey(KeyCode.W)) discreteActionsOut[0] = 1;
        else if (Input.GetKey(KeyCode.S)) discreteActionsOut[0] = 2;
        else discreteActionsOut[0] = 0;

        // 공격 (Q, E, R 키)
        if (Input.GetKeyDown(KeyCode.Q)) discreteActionsOut[1] = (int)AttackType.Q_Attack;
        else if (Input.GetKeyDown(KeyCode.E)) discreteActionsOut[1] = (int)AttackType.E_Kick;
        else if (Input.GetKeyDown(KeyCode.R)) discreteActionsOut[1] = (int)AttackType.R_Attack;
        else discreteActionsOut[1] = (int)AttackType.None;

        // 방어 (LeftShift 키)
        if (Input.GetKey(KeyCode.LeftShift)) discreteActionsOut[2] = 1;
        else discreteActionsOut[2] = 0;
    }

    // --- 기존 RootMotionMover의 메서드들 ---
    // (StartAttack은 StartAttackInternal로 변경하여 RL의 Action과 구분)

    void StartAttackInternal(AttackType attackType)
    {
        if (isAttacking) return; // 이미 공격 중이면 실행 안함

        isAttacking = true;
        currentAttackTypeInternal = attackType; // 현재 공격 타입 기록

        if (soundManager != null) soundManager.StopWalkingSound();

        animator.SetTrigger("attackTrigger");
        animator.SetInteger("attackIndex", (int)attackType - 1); // enum None 때문에 -1 (기존 Q=0, E=1, R=2)

        if (soundManager != null)
        {
            switch (attackType)
            {
                case AttackType.Q_Attack:
                    if (qAttackSfx != null) soundManager.PlaySoundEffect(qAttackSfx);
                    if (swordTrail != null) StartCoroutine(PlayTrail(qAttackTrailDuration, qAttackTrailDelay));
                    break;
                case AttackType.E_Kick:
                    if (eKickSfx != null) soundManager.PlaySoundEffect(eKickSfx);
                    break;
                case AttackType.R_Attack:
                    if (rAttackSfx != null) soundManager.PlaySoundEffect(rAttackSfx);
                    if (swordTrail != null) StartCoroutine(PlayTrail(rAttackTrailDuration, rAttackTrailDelay));
                    break;
            }
        }
        StartCoroutine(ResetAttackStateAfterAnimation(GetAnimationLength(attackType)));
    }

    IEnumerator PlayTrail(float duration, float delay)
    {
        if (swordTrail == null) yield break;
        if (delay > 0) yield return new WaitForSeconds(delay);
        swordTrail.enabled = true;
        swordTrail.Clear();
        yield return new WaitForSeconds(duration);
        swordTrail.enabled = false;
    }

    IEnumerator ResetAttackStateAfterAnimation(float animationLength)
    {
        yield return new WaitForSeconds(animationLength);
        isAttacking = false;
        currentAttackTypeInternal = AttackType.None; // 공격 끝났으므로 None으로

        // R 공격 후 Y 위치 보정 (선택적)
        if (animator.GetCurrentAnimatorStateInfo(0).IsName("R_Attack_Animation_State_Name")) // 실제 애니메이션 상태 이름으로 확인
        {
            Vector3 fixedPos = transform.position;
            fixedPos.y = startPosition.y; // 초기 Y 값으로 고정 또는 0.0f
            transform.position = fixedPos;
        }
    }

    float GetAnimationLength(AttackType attackType)
    {
        // 이 값들은 실제 애니메이션 클립 길이에 맞춰 정확히 설정해야 합니다.
        switch (attackType)
        {
            case AttackType.Q_Attack: return qAttackTrailDelay + qAttackTrailDuration + 0.1f; // 애니메이션 전체 길이
            case AttackType.E_Kick: return 0.7f; // E_Kick 애니메이션 전체 길이
            case AttackType.R_Attack: return rAttackTrailDelay + rAttackTrailDuration + 0.1f; // 애니메이션 전체 길이
            default: return 0.5f;
        }
    }

    void OnAnimatorMove()
    {
        if (Time.deltaTime == 0) return; // 일시정지 등 상황 방지

        // R 공격 애니메이션은 Y축 이동을 허용하고, 나머지는 Y축을 고정 (루트모션 특성 따라 조절)
        // currentAttackTypeInternal을 사용하여 현재 진행 중인 공격에 따라 다르게 처리 가능
        if (isAttacking && currentAttackTypeInternal == AttackType.R_Attack)
        {
            transform.position += animator.deltaPosition;
        }
        else
        {
            Vector3 deltaPos = animator.deltaPosition;
            deltaPos.y = 0.0f; // 일반 이동이나 다른 공격 시 Y축 고정
            transform.position += deltaPos;
        }
        transform.rotation *= animator.deltaRotation;
    }

    // 애니메이션 이벤트에서 isAttacking = false 호출 (선택적, 코루틴과 중복 가능성)
    public void AnimationFinished_SetAttackingFalse()
    {
        isAttacking = false;
        currentAttackTypeInternal = AttackType.None;
    }

    // 벽 충돌 감지 (경기장 벽에 "Wall" 태그를 붙여야 함)
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Wall"))
        {
            AddReward(-0.1f); // 벽에 부딪히면 패널티
            // Debug.Log("벽 충돌! 패널티: -0.1");
            // EndEpisode(); // 벽에 부딪히면 에피소드 종료 (선택적)
        }
    }

    private void HandleSounds(float vValue, int moveActionValue)
    {
        if (soundManager == null) return;

        bool shouldPlayWalkingSound = !isAttacking && moveActionValue == 1 && vValue > 0;
        if (shouldPlayWalkingSound && wKeySfx != null)
        {
            soundManager.PlayWalkingSound(wKeySfx);
        }
        else
        {
            soundManager.StopWalkingSound();
        }

        // S키(뒤로가기) 사운드 (단발성)
        // ML-Agents에서는 GetKeyDown과 같은 효과를 내려면 이전 상태 비교 필요
        // 여기서는 단순화를 위해 '뒤로가기 액션이 선택되었고, isAttacking이 아닐 때' 정도로만 표현
        if (!isAttacking && moveActionValue == 2 && sKeySfx != null /* && previousMoveAction != 2 (구현 필요) */)
        {
            // soundManager.PlaySoundEffect(sKeySfx); // 연속 재생될 수 있으므로 SoundManager에서 중복 방지 필요
        }
    }
}