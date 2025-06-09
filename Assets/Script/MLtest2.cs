using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class MLtest2 : Agent
{
    public GameObject sword;
    public RootMotionMover rootMotionMover;

    public float agentHealth = 100f;
    public float targetHealth = 100f;

    Rigidbody rBody;

    // --- [추가] 행동 잠금을 위한 전역 변수 ---
    private bool isActionLocked = false; // 에이전트의 모든 행동이 잠겼는지 확인
    private float actionLockTimer = 0f;  // 남은 잠금 시간

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActions = actionsOut.DiscreteActions;
        discreteActions[0] = 0;

        if (Input.GetKey(KeyCode.I)) discreteActions[0] = 1;      // Forward
        else if (Input.GetKey(KeyCode.K)) discreteActions[0] = 2; // Backward
        else if (Input.GetKey(KeyCode.J)) discreteActions[0] = 3; // Dodge (원래는 L키였으나 J로 가정)
        else if (Input.GetKey(KeyCode.U)) discreteActions[0] = 4; // Q_Attack
        else if (Input.GetKey(KeyCode.O)) discreteActions[0] = 5; // E_Kick
        else if (Input.GetKey(KeyCode.P)) discreteActions[0] = 6; // R_Attack
        else if (Input.GetKey(KeyCode.RightShift)) discreteActions[0] = 7; // Defend
    }

    Vector3 lastSwordVelocity;
    Vector3 lastSwordPosition;
    Vector3 swordAcceleration;

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

    public Transform Target;

    float episodeTimer = 0f;
    float maxEpisodeTime = 40f;

    // --- [수정] Update 메서드에 전역 쿨다운 타이머 로직 추가 ---
    void Update()
    {
        episodeTimer += Time.deltaTime;
        if (episodeTimer > maxEpisodeTime)
        {
            Debug.LogWarning("에피소드가 너무 오래 걸려 강제 종료!");
            EndEpisode();
        }

        // 행동 잠금 타이머가 활성화 되어있으면 시간을 감소시킴
        if (isActionLocked)
        {
            actionLockTimer -= Time.deltaTime;
            if (actionLockTimer <= 0)
            {
                isActionLocked = false; // 시간이 다 되면 잠금 해제
            }
        }
    }

    public override void OnEpisodeBegin()
    {
        agentHealth = 100f;
        targetHealth = 100f;
        episodeTimer = 0f;

        // 에피소드 시작 시 행동 잠금 상태 초기화
        isActionLocked = false;
        actionLockTimer = 0f;

        // UI 체력 설정
        TestUIController.Instance.SetRightHealth(agentHealth, 100f); // MLtest2는 오른쪽 UI를 사용한다고 가정
        TestUIController.Instance.SetLeftHealth(targetHealth, 100f);

        // 위치 설정
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

    public float forceMultiplier = 10f;

    // --- [삭제] 개별 잠금 변수 및 관련 로직은 삭제됨 ---

    // --- [추가] 행동을 잠그는 헬퍼 메서드 ---
    private void LockAction(float duration)
    {
        isActionLocked = true;
        actionLockTimer = duration;
    }

    // --- [수정] OnActionReceived에 전역 잠금 로직 적용 ---
    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        // 행동이 잠겨있으면, 이후 로직을 실행하지 않고 즉시 종료
        if (isActionLocked)
        {
            return;
        }

        int discreteAction = actionBuffers.DiscreteActions[0];
        float distanceToTarget = Vector3.Distance(transform.localPosition, Target.localPosition);

        if (distanceToTarget < 4.0f && discreteAction == 1) AddReward(0.03f);
        if (distanceToTarget > 5.0f) AddReward(-0.05f);

        RootMotionMover opponentRootMotion = null;
        if (Target != null) opponentRootMotion = Target.GetComponent<RootMotionMover>();
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
            case 3: // Dodge
                rootMotionMover.Dodge();
                TestUIController.Instance.rightDodge.TriggerCooldown();
                LockAction(1.5f); // 1.5초간 모든 행동 잠금
                break;
            case 4: // Q_Attack
                rootMotionMover.StartAttack(RootMotionMover.AttackType.Q_Attack);
                TestUIController.Instance.rightAttack.TriggerCooldown();
                LockAction(2.0f); // 2초간 모든 행동 잠금

                if (distanceToTarget < 4.0f)
                    AddReward(opponentIsAttacking ? 0.05f : -0.05f);
                break;
            case 5: // E_Kick
                rootMotionMover.StartAttack(RootMotionMover.AttackType.E_Kick);
                TestUIController.Instance.rightAttack.TriggerCooldown();
                LockAction(2.0f); // 2초간 모든 행동 잠금

                if (distanceToTarget < 4.0f)
                    AddReward(opponentIsAttacking ? 0.05f : -0.05f);
                break;
            case 6: // R_Attack
                rootMotionMover.StartAttack(RootMotionMover.AttackType.R_Attack);
                TestUIController.Instance.rightAttack.TriggerCooldown();
                LockAction(2.5f); // 2.5초간 모든 행동 잠금

                if (distanceToTarget < 4.0f)
                    AddReward(opponentIsAttacking ? 0.05f : -0.05f);
                break;
            case 7: // Defend
                // Defend는 키를 누르고 있는 동안 지속되어야 하므로, Lock을 걸지 않는 것이 일반적입니다.
                // 만약 방어 시작 시 쿨타임을 원한다면 여기에 LockAction()을 추가할 수 있습니다.
                bool isDefending = Input.GetKey(KeyCode.RightShift);
                rootMotionMover.SetDefend(isDefending);
                TestUIController.Instance.rightDefend.TriggerCooldown();
                Debug.Log("방어 상태: " + isDefending);
                break;
        }

        if (targetHealth <= 0f)
        {
            SetReward(2.0f);
            EndEpisode();
        }
        else if (agentHealth <= 0f)
        {
            SetReward(-1.0f);
            EndEpisode();
        }
    }

    public void TakeDamage(float damage)
    {
        agentHealth -= damage;
        TestUIController.Instance.SetRightHealth(agentHealth, 100f); // UI 업데이트
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
        Debug.Log($"공격 실패! 데미지: {damageDefault}, 보상: {scaledReward}");
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