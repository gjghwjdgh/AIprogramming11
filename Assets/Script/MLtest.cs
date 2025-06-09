// 파일 이름: MLtest.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class MLtest : Agent, IDamageable
{
    public GameObject sword;
    public RootMotionMover rootMotionMover;
    public Transform Target;

    [Header("Agent Side")]
    public bool isLeftAgent;

    public float agentHealth = 100f;
    public float targetHealth = 100f;

    private Rigidbody rBody;
    private Vector3 lastSwordVelocity;
    private Vector3 lastSwordPosition;
    private bool isActionLocked = false;

    // --- [수정] 칼의 콜라이더를 직접 제어하기 위한 변수 ---
    private Collider swordCollider;

    [Header("Cooldowns")]
    private float qAttackCooldown = 2.5f;
    private float eKickCooldown = 2.5f;
    private float rAttackCooldown = 2.5f;
    private float defendCooldown = 5.0f;
    private float dodgeCooldown = 5.0f;

    private float qAttackTimer = 0f;
    private float eKickTimer = 0f;
    private float rAttackTimer = 0f;
    private float defendTimer = 0f;
    private float dodgeTimer = 0f;

    [Header("Action Durations")]
    private float qAttackDuration = 2.5f;
    private float eKickDuration = 2.5f;
    private float rAttackDuration = 2.5f;
    private float defendDuration = 5.0f;
    private float dodgeDuration = 5.0f;

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActions = actionsOut.DiscreteActions;
        discreteActions[0] = 0;

        if (Input.GetKey(KeyCode.W)) discreteActions[0] = 1;
        else if (Input.GetKey(KeyCode.S)) discreteActions[0] = 2;
        else if (Input.GetKey(KeyCode.Space)) discreteActions[0] = 3;
        else if (Input.GetKey(KeyCode.Q)) discreteActions[0] = 4;
        else if (Input.GetKey(KeyCode.E)) discreteActions[0] = 5;
        else if (Input.GetKey(KeyCode.R)) discreteActions[0] = 6;
        else if (Input.GetKey(KeyCode.LeftShift)) discreteActions[0] = 7;
    }

    void Start()
    {
        rBody = GetComponent<Rigidbody>();
        lastSwordPosition = sword.transform.position;
        lastSwordVelocity = Vector3.zero;

        // --- [수정] swordCollider 변수 초기화 ---
        if (sword != null)
        {
            swordCollider = sword.GetComponent<Collider>();
            // 게임 시작 시 칼의 콜라이더는 항상 비활성화 상태로 시작
            if (swordCollider != null) swordCollider.enabled = false;
        }

        Sword swordScript = sword.GetComponent<Sword>();
        if (swordScript != null)
        {
            swordScript.owner = this.gameObject;
            Collider bodyCollider = GetComponent<Collider>();
            if (swordCollider != null && bodyCollider != null)
            {
                Physics.IgnoreCollision(swordCollider, bodyCollider);
            }
        }
    }

    float episodeTimer = 0f;
    float maxEpisodeTime = 40f;

    void Update()
    {
        episodeTimer += Time.deltaTime;
        if (episodeTimer > maxEpisodeTime)
        {
            Debug.LogWarning("에피소드가 너무 오래 걸려 강제 종료!");
            SetReward(-1.0f);
            EndEpisode();
        }

        if (qAttackTimer > 0) qAttackTimer -= Time.deltaTime;
        if (eKickTimer > 0) eKickTimer -= Time.deltaTime;
        if (rAttackTimer > 0) rAttackTimer -= Time.deltaTime;
        if (defendTimer > 0) defendTimer -= Time.deltaTime;
        if (dodgeTimer > 0) dodgeTimer -= Time.deltaTime;
    }

    public override void OnEpisodeBegin()
    {
        agentHealth = 100f;
        targetHealth = 100f;
        episodeTimer = 0f;
        isActionLocked = false;
        CancelInvoke();

        // --- [수정] 에피소드 시작 시 칼 콜라이더 비활성화 확실히 ---
        if (swordCollider != null) swordCollider.enabled = false;

        BattleUIController.Instance?.UpdateHealth(isLeftAgent, agentHealth, 100f);
        BattleUIController.Instance?.UpdateHealth(!isLeftAgent, targetHealth, 100f);
        BattleUIController.Instance?.HideWinMessage();

        qAttackTimer = 0f;
        eKickTimer = 0f;
        rAttackTimer = 0f;
        defendTimer = 0f;
        dodgeTimer = 0f;

        this.transform.localPosition = new Vector3(isLeftAgent ? -216.0f : -213.0f, 0.0f, -0.1f);
        Target.localPosition = new Vector3(isLeftAgent ? -213.0f : -216.0f, 0.0f, -0.1f);
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(Target.localPosition);
        sensor.AddObservation(this.transform.localPosition);
        sensor.AddObservation(Target.localPosition - transform.localPosition);

        Vector3 currentPos = sword.transform.position;
        Vector3 currentVelocity = (currentPos - lastSwordPosition) / Time.fixedDeltaTime;
        Vector3 swordAcceleration = (currentVelocity - lastSwordVelocity) / Time.fixedDeltaTime;
        sensor.AddObservation(swordAcceleration.x);
        sensor.AddObservation(swordAcceleration.z);
        lastSwordPosition = currentPos;
        lastSwordVelocity = currentVelocity;

        sensor.AddObservation(isActionLocked);
        sensor.AddObservation(qAttackTimer / qAttackCooldown);
        sensor.AddObservation(eKickTimer / eKickCooldown);
        sensor.AddObservation(rAttackTimer / rAttackCooldown);
        sensor.AddObservation(defendTimer / defendCooldown);
        sensor.AddObservation(dodgeTimer / dodgeCooldown);
    }

    // --- [추가] 칼을 활성화/비활성화하는 함수들 ---
    void ActivateSword()
    {
        if (swordCollider != null) swordCollider.enabled = true;
    }

    void DeactivateSword()
    {
        if (swordCollider != null) swordCollider.enabled = false;
    }


    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        if (isActionLocked)
        {
            AddReward(-0.001f);
            return;
        }

        int discreteAction = actionBuffers.DiscreteActions[0];
        float distanceToTarget = Vector3.Distance(transform.localPosition, Target.localPosition);
        if (distanceToTarget < 4.0f) { AddReward(0.03f); }
        if (distanceToTarget > 5.0f) { AddReward(-0.05f); }

        rootMotionMover.animator.SetFloat("v", 0.0f);

        switch (discreteAction)
        {
            case 0: break;
            case 1: rootMotionMover.animator.SetFloat("v", 1.0f); break;
            case 2: rootMotionMover.animator.SetFloat("v", -1.0f); break;
            case 3: // Dodge
                if (dodgeTimer <= 0f)
                {
                    isActionLocked = true;
                    rootMotionMover.Dodge();
                    dodgeTimer = dodgeCooldown;
                    BattleUIController.Instance?.TriggerCooldown(isLeftAgent, "Dodge", dodgeCooldown);
                    Invoke(nameof(ReleaseActionLock), dodgeDuration);
                    AddReward(0.05f);
                }
                else { AddReward(-0.1f); }
                break;

            // --- [수정] 모든 공격에 칼 활성화/비활성화 로직 추가 ---
            case 4: // Q_Attack
                if (qAttackTimer <= 0f)
                {
                    isActionLocked = true;
                    rootMotionMover.StartAttack(RootMotionMover.AttackType.Q_Attack);
                    qAttackTimer = qAttackCooldown;
                    BattleUIController.Instance?.TriggerCooldown(isLeftAgent, "Attack1", qAttackCooldown);

                    // 0.2초 뒤 칼을 활성화하고, 0.7초 뒤에 비활성화
                    Invoke(nameof(ActivateSword), 0.2f);
                    Invoke(nameof(DeactivateSword), 0.7f);

                    Invoke(nameof(ReleaseActionLock), qAttackDuration);
                }
                else { AddReward(-0.1f); }
                break;
            case 5: // E_Kick
                if (eKickTimer <= 0f)
                {
                    isActionLocked = true;
                    rootMotionMover.StartAttack(RootMotionMover.AttackType.E_Kick);
                    eKickTimer = eKickCooldown;
                    BattleUIController.Instance?.TriggerCooldown(isLeftAgent, "Attack2", eKickCooldown);

                    Invoke(nameof(ActivateSword), 0.2f);
                    Invoke(nameof(DeactivateSword), 0.7f);

                    Invoke(nameof(ReleaseActionLock), eKickDuration);
                }
                else { AddReward(-0.1f); }
                break;
            case 6: // R_Attack
                if (rAttackTimer <= 0f)
                {
                    isActionLocked = true;
                    rootMotionMover.StartAttack(RootMotionMover.AttackType.R_Attack);
                    rAttackTimer = rAttackCooldown;
                    BattleUIController.Instance?.TriggerCooldown(isLeftAgent, "Attack3", rAttackCooldown);

                    Invoke(nameof(ActivateSword), 0.2f);
                    Invoke(nameof(DeactivateSword), 0.7f);

                    Invoke(nameof(ReleaseActionLock), rAttackDuration);
                }
                else { AddReward(-0.1f); }
                break;
            case 7: // Defend
                if (defendTimer <= 0f)
                {
                    isActionLocked = true;
                    rootMotionMover.SetDefend(true);
                    defendTimer = defendCooldown;
                    BattleUIController.Instance?.TriggerCooldown(isLeftAgent, "Defend", defendCooldown);
                    Invoke(nameof(ResetDefendState), defendDuration);
                    AddReward(0.05f);
                }
                break;
        }

        if (targetHealth <= 0f)
        {
            float healthRatio = agentHealth / 100f;
            SetReward(1.0f + healthRatio);
            string winner = isLeftAgent ? "Left Agent" : "Right Agent";
            BattleUIController.Instance?.ShowWinMessage(winner);
            EndEpisode();
        }
        else if (agentHealth <= 0f)
        {
            SetReward(-1.5f);
            string winner = isLeftAgent ? "Right Agent" : "Left Agent";
            BattleUIController.Instance?.ShowWinMessage(winner);
            EndEpisode();
        }
    }

    private void ReleaseActionLock()
    {
        isActionLocked = false;
    }

    private void ResetDefendState()
    {
        rootMotionMover.SetDefend(false);
        isActionLocked = false;
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

    public void TakeDamage(float damage)
    {
        agentHealth -= damage;
        // 데미지를 입을 때마다 즉시 UI 업데이트
        BattleUIController.Instance?.UpdateHealth(isLeftAgent, agentHealth, 100f);
        if (agentHealth <= 0f)
        {
            SetReward(-1.0f);
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