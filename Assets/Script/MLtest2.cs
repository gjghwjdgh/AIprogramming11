using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class MLtest2 : Agent
{
    public GameObject sword; // Inspector에서 드래그&드롭으로 할당
    public RootMotionMover rootMotionMover;
    public Transform Target;

    [Header("Agent Side")]
    public bool isLeftAgent = false; // UI 구분을 위한 플래그 (이 에이전트는 오른쪽이므로 false)

    public float agentHealth = 100f;
    public float targetHealth = 100f;

    private Rigidbody rBody;
    private Vector3 lastSwordVelocity;
    private Vector3 lastSwordPosition;

    // --- [추가] 쿨다운 시스템 변수 ---
    private float qAttackCooldown = 2.5f;
    private float eKickCooldown = 2.5f;
    private float rAttackCooldown = 3.5f;
    private float defendCooldown = 2.0f;
    private float dodgeCooldown = 4.0f;

    private float qAttackTimer = 0f;
    private float eKickTimer = 0f;
    private float rAttackTimer = 0f;
    private float defendTimer = 0f;
    private float dodgeTimer = 0f;
    // --- [추가] 쿨다운 시스템 변수 끝 ---

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActions = actionsOut.DiscreteActions;
        discreteActions[0] = 0;

        if (Input.GetKey(KeyCode.I))
            discreteActions[0] = 1; // Move Forward
        else if (Input.GetKey(KeyCode.K))
            discreteActions[0] = 2; // Move Backward
        else if (Input.GetKey(KeyCode.J))
            discreteActions[0] = 3; // Dodge
        else if (Input.GetKey(KeyCode.U))
            discreteActions[0] = 4; // Q_Attack (U키)
        else if (Input.GetKey(KeyCode.O))
            discreteActions[0] = 5; // E_Kick (O키)
        else if (Input.GetKey(KeyCode.P))
            discreteActions[0] = 6; // R_Attack (P키)
        else if (Input.GetKey(KeyCode.RightShift))
            discreteActions[0] = 7; // Defend
    }

    void Start()
    {
        rBody = GetComponent<Rigidbody>();
        lastSwordPosition = sword.transform.position;
        lastSwordVelocity = Vector3.zero;

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

    float episodeTimer = 0f;
    float maxEpisodeTime = 40f;

    void Update()
    {
        episodeTimer += Time.deltaTime;
        if (episodeTimer > maxEpisodeTime)
        {
            Debug.LogWarning("에피소드가 너무 오래 걸려 강제 종료!");
            EndEpisode();
            episodeTimer = 0f;
        }

        // 매 프레임 쿨다운 타이머 감소
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

        // --- [수정] UI 업데이트 로직 변경 ---
        // isLeftAgent가 false이므로 자신의 체력은 오른쪽 UI에, 상대 체력은 왼쪽 UI에 표시
        BattleUIController.Instance?.UpdateHealth(isLeftAgent, agentHealth, 100f);
        BattleUIController.Instance?.UpdateHealth(!isLeftAgent, targetHealth, 100f);
        BattleUIController.Instance?.HideWinMessage();

        // --- [추가] 쿨다운 타이머 리셋 ---
        qAttackTimer = 0f;
        eKickTimer = 0f;
        rAttackTimer = 0f;
        defendTimer = 0f;
        dodgeTimer = 0f;

        // 위치 초기화
        this.transform.localPosition = new Vector3(-213.0f, 0.0f, -0.1f);
        Target.localPosition = new Vector3(-216.0f, 0.0f, -0.1f);
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

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        int discreteAction = actionBuffers.DiscreteActions[0];

        float distanceToTarget = Vector3.Distance(transform.localPosition, Target.localPosition);
        if (distanceToTarget < 4.0f) { AddReward(0.03f); }
        if (distanceToTarget > 5.0f) { AddReward(-0.05f); }

        rootMotionMover.animator.SetFloat("v", 0.0f);

        switch (discreteAction)
        {
            case 0: // Idle
                AddReward(-0.01f);
                break;
            case 1: // Move Forward
                rootMotionMover.animator.SetFloat("v", 1.0f);
                AddReward(0.05f);
                break;
            case 2: // Move Backward
                rootMotionMover.animator.SetFloat("v", -2.0f);
                AddReward(0.05f);
                break;
            case 3: // Dodge
                if (dodgeTimer <= 0f)
                {
                    rootMotionMover.Dodge();
                    dodgeTimer = dodgeCooldown;
                    BattleUIController.Instance?.TriggerCooldown(isLeftAgent, "Dodge", dodgeCooldown);
                }
                else { AddReward(-0.1f); }
                break;
            case 4: // Q_Attack (U키)
                if (qAttackTimer <= 0f)
                {
                    rootMotionMover.StartAttack(RootMotionMover.AttackType.Q_Attack);
                    qAttackTimer = qAttackCooldown;
                    BattleUIController.Instance?.TriggerCooldown(isLeftAgent, "Attack1", qAttackCooldown);
                }
                else { AddReward(-0.1f); }
                break;
            case 5: // E_Kick (O키)
                if (eKickTimer <= 0f)
                {
                    rootMotionMover.StartAttack(RootMotionMover.AttackType.E_Kick);
                    eKickTimer = eKickCooldown;
                    BattleUIController.Instance?.TriggerCooldown(isLeftAgent, "Attack2", eKickCooldown);
                }
                else { AddReward(-0.1f); }
                break;
            case 6: // R_Attack (P키)
                if (rAttackTimer <= 0f)
                {
                    rootMotionMover.StartAttack(RootMotionMover.AttackType.R_Attack);
                    rAttackTimer = rAttackCooldown;
                    BattleUIController.Instance?.TriggerCooldown(isLeftAgent, "Attack3", rAttackCooldown);
                }
                else { AddReward(-0.1f); }
                break;
            case 7: // Defend
                if (defendTimer <= 0f)
                {
                    rootMotionMover.SetDefend(true);
                    Invoke(nameof(ResetDefendState), 1.0f); // 1초 뒤 방어 해제
                    defendTimer = defendCooldown;
                    BattleUIController.Instance?.TriggerCooldown(isLeftAgent, "Defend", defendCooldown);
                }
                break;
        }

        if (targetHealth <= 0f)
        {
            SetReward(2.0f);
            string winner = isLeftAgent ? "Left Agent" : "Right Agent";
            BattleUIController.Instance?.ShowWinMessage(winner);
            EndEpisode();
        }
        else if (agentHealth <= 0f)
        {
            SetReward(-1.0f);
            string winner = isLeftAgent ? "Right Agent" : "Left Agent";
            BattleUIController.Instance?.ShowWinMessage(winner);
            EndEpisode();
        }
    }

    private void ResetDefendState()
    {
        rootMotionMover.SetDefend(false);
    }

    public void TakeDamage(float damage)
    {
        agentHealth -= damage;
        BattleUIController.Instance?.UpdateHealth(isLeftAgent, agentHealth, 100f);
        if (agentHealth <= 0f)
        {
            SetReward(-1.0f);
            EndEpisode();
        }
    }

    public void OnSuccessfulAttack(float damageDefault)
    {
        float scaledReward = damageDefault * 0.4f;
        AddReward(scaledReward);
        Debug.Log($"공격 성공! 데미지: {damageDefault}, 보상: {scaledReward}");
    }

    public void OnFailedAttack(float damageDefault)
    {
        float scaledReward = -damageDefault * 0.02f;
        AddReward(scaledReward);
        Debug.Log($"공격 실패! 데미지: {damageDefault}, 페널티: {scaledReward}");
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