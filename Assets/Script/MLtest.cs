using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class MLtest : Agent
{
    public GameObject sword; // Inspector에서 드래그&드롭으로 할당
    public RootMotionMover rootMotionMover;

    public float agentHealth = 100f;
    public float targetHealth = 100f;

    Rigidbody rBody;

    // --- [추가] 행동 쿨다운 관련 변수 ---
    private bool isActionLocked = false; // 현재 행동이 잠겨있는지 확인하는 플래그
    private float actionLockTimer = 0f; // 쿨다운 남은 시간

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActions = actionsOut.DiscreteActions;
        discreteActions[0] = 0;

        if (Input.GetKey(KeyCode.W))
            discreteActions[0] = 1; // Move Forward
        else if (Input.GetKey(KeyCode.S))
            discreteActions[0] = 2; // Move Backward
        else if (Input.GetKey(KeyCode.Space))
            discreteActions[0] = 3; // Dodge
        else if (Input.GetKey(KeyCode.Q))
            discreteActions[0] = 4; // Q_Attack
        else if (Input.GetKey(KeyCode.E))
            discreteActions[0] = 5; // E_Kick
        else if (Input.GetKey(KeyCode.R))
            discreteActions[0] = 6; // R_Attack
        else if (Input.GetKey(KeyCode.LeftShift))
            discreteActions[0] = 7; // Defend
    }

    Vector3 lastSwordVelocity;
    Vector3 lastSwordPosition;
    Vector3 swordAcceleration;

    void Start()
    {
        rBody = GetComponent<Rigidbody>();
        lastSwordPosition = sword.transform.position;
        lastSwordVelocity = Vector3.zero;

        // Sword에 소유자 등록
        Sword swordScript = sword.GetComponent<Sword>();
        if (swordScript != null)
        {
            swordScript.owner = this.gameObject;
            Collider swordCollider = sword.GetComponent<Collider>();
            Collider bodyCollider = GetComponent<Collider>();
            if (swordCollider != null && bodyCollider != null)
            {
                Physics.IgnoreCollision(swordCollider, bodyCollider);
            }
        }
    }

    public Transform Target;

    float episodeTimer = 0f;
    float maxEpisodeTime = 40f;

    // --- [수정] Update 메서드에 쿨다운 타이머 로직 추가 ---
    void Update()
    {
        episodeTimer += Time.deltaTime;
        if (episodeTimer > maxEpisodeTime)
        {
            Debug.LogWarning("에피소드가 너무 오래 걸려 강제 종료!");
            EndEpisode();
        }

        // 행동 잠금 타이머를 감소시킴
        if (isActionLocked)
        {
            actionLockTimer -= Time.deltaTime;
            if (actionLockTimer <= 0f)
            {
                isActionLocked = false; // 타이머가 끝나면 잠금 해제
            }
        }
    }

    public override void OnEpisodeBegin()
    {
        agentHealth = 100f;
        targetHealth = 100f;
        episodeTimer = 0f;
        isActionLocked = false; // 에피소드 시작 시 잠금 상태 초기화
        actionLockTimer = 0f;

        TestUIController.Instance.SetLeftHealth(agentHealth, 100f);
        TestUIController.Instance.SetRightHealth(targetHealth, 100f);

        Vector3 startPosition = new Vector3(-216.0f, 0.0f, -0.1f);
        this.transform.localPosition = startPosition;

        Target.localPosition = new Vector3(-213.0f, 0.0f, -0.1f);
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(Target.localPosition);
        sensor.AddObservation(this.transform.localPosition);

        Vector3 relativePosition = Target.localPosition - transform.localPosition;
        sensor.AddObservation(relativePosition);

        Vector3 currentPos = sword.transform.position;
        Vector3 currentVelocity = (currentPos - lastSwordPosition) / Time.fixedDeltaTime;
        Vector3 swordAcceleration = (currentVelocity - lastSwordVelocity) / Time.fixedDeltaTime;

        sensor.AddObservation(swordAcceleration.x);
        sensor.AddObservation(swordAcceleration.z);

        lastSwordPosition = currentPos;
        lastSwordVelocity = currentVelocity;
    }

    public void OnDefendSuccess(float attackAccel)
    {
        float baseReward = 0.05f;
        if (attackAccel >= 5f)
        {
            float scaledReward = baseReward * (attackAccel / 5f);
            AddReward(scaledReward);
            Debug.Log($"방어 성공! 공격 가속도: {attackAccel}, 보상: {scaledReward}");
        }
        else
        {
            AddReward(baseReward);
            Debug.Log($"방어 성공! 기본 보상만 지급: {baseReward}");
        }
    }

    public void OnDodgeSuccess(float attackAccel)
    {
        float baseReward = 0.05f;
        if (attackAccel >= 5f)
        {
            float scaledReward = baseReward * (attackAccel / 3f);
            AddReward(scaledReward);
            Debug.Log($"회피 성공! 공격 가속도: {attackAccel}, 보상: {scaledReward}");
        }
        else
        {
            AddReward(baseReward);
            Debug.Log($"회피 성공! 기본 보상만 지급: {baseReward}");
        }
    }

    public float forceMultiplier = 10;

    // --- [추가] 행동 잠금을 시작하는 헬퍼 메서드 ---
    /// <summary>
    /// 지정된 시간 동안 에이전트의 행동을 잠급니다.
    /// </summary>
    /// <param name="duration">잠글 시간(초)</param>
    private void LockAction(float duration)
    {
        isActionLocked = true;
        actionLockTimer = duration;
    }


    // --- [수정] OnActionReceived 메서드에 쿨다운 로직 적용 ---
    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        // 행동이 잠겨있는 동안에는 새로운 행동을 받지 않고 즉시 반환
        if (isActionLocked)
        {
            return;
        }

        int discreteAction = actionBuffers.DiscreteActions[0];
        float distanceToTarget = Vector3.Distance(transform.localPosition, Target.localPosition);

        if (distanceToTarget < 4.0f)
        {
            AddReward(0.03f);
        }

        if (distanceToTarget > 5.0f)
        {
            AddReward(-0.05f);
        }

        RootMotionMover opponentRootMotion = null;
        if (Target != null)
        {
            opponentRootMotion = Target.GetComponent<RootMotionMover>();
        }
        bool opponentIsAttacking = (opponentRootMotion != null && opponentRootMotion.isAttacking);

        rootMotionMover.animator.SetFloat("v", 0.0f);

        switch (discreteAction)
        {
            case 0:
                AddReward(-0.01f);
                break;
            case 1:
                rootMotionMover.animator.SetFloat("v", 1.0f);
                AddReward(0.05f);
                break;
            case 2:
                rootMotionMover.animator.SetFloat("v", -2.0f);
                AddReward(0.05f);
                break;
            case 3:
                rootMotionMover.Dodge();
                TestUIController.Instance.leftDodge.TriggerCooldown();
                AddReward(0.05f);
                LockAction(1.5f); // 회피 후 1.5초 동안 행동 잠금
                break;
            case 4:
                rootMotionMover.StartAttack(RootMotionMover.AttackType.Q_Attack);
                TestUIController.Instance.leftAttack.TriggerCooldown();
                LockAction(2.0f); // Q 공격 후 2초 동안 행동 잠금
                break;
            case 5:
                rootMotionMover.StartAttack(RootMotionMover.AttackType.E_Kick);
                TestUIController.Instance.leftAttack.TriggerCooldown();
                LockAction(2.0f); // E 발차기 후 2초 동안 행동 잠금
                break;
            case 6:
                rootMotionMover.StartAttack(RootMotionMover.AttackType.R_Attack);
                TestUIController.Instance.leftAttack.TriggerCooldown();
                LockAction(2.5f); // R 공격 후 2.5초 동안 행동 잠금
                break;
            case 7:
                bool isDefending = Input.GetKey(KeyCode.LeftShift);
                rootMotionMover.SetDefend(isDefending);
                TestUIController.Instance.leftDefend.TriggerCooldown();
                Debug.Log("방어 상태: " + isDefending);
                AddReward(0.05f);
                break;
        }

        if (targetHealth <= 0f)
        {
            float healthRatio = agentHealth / 100f;
            SetReward(1.0f + healthRatio);
            EndEpisode();
        }
        else if (agentHealth <= 0f)
        {
            SetReward(-1.5f);
            EndEpisode();
        }
    }

    public void TakeDamage(float damage)
    {
        agentHealth -= damage;
        TestUIController.Instance.SetLeftHealth(agentHealth, 100f); // UI 업데이트
        if (agentHealth <= 0f)
        {
            SetReward(-1.5f); // SetReward(-1.0f) 보다 패배 페널티를 더 강하게 줌
            EndEpisode();
        }
    }

    public void OnSuccessfulAttack(float damageDefault)
    {
        float scaledReward = damageDefault * 0.2f;
        AddReward(scaledReward);
        Debug.Log($"공격 성공! 데미지: {damageDefault}, 보상: {scaledReward}");
    }

    public void OnEffectiveCounterAttack(float damageDefault)
    {
        AddReward(0.2f + damageDefault * 0.01f);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Boundary"))
        {
            Debug.Log("Boundary 충돌: 음의 보상 주기");
            AddReward(-0.07f);
        }
    }
}