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

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActions = actionsOut.DiscreteActions;
        discreteActions[0] = 0;

        if (Input.GetKey(KeyCode.I)) discreteActions[0] = 1;
        else if (Input.GetKey(KeyCode.K)) discreteActions[0] = 2;
        else if (Input.GetKey(KeyCode.J)) discreteActions[0] = 3;
        else if (Input.GetKey(KeyCode.U)) discreteActions[0] = 4;
        else if (Input.GetKey(KeyCode.O)) discreteActions[0] = 5;
        else if (Input.GetKey(KeyCode.P)) discreteActions[0] = 6;
        else if (Input.GetKey(KeyCode.RightShift)) discreteActions[0] = 7;
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

    void Update()
    {
        episodeTimer += Time.deltaTime;
        if (episodeTimer > maxEpisodeTime)
        {
            Debug.LogWarning("에피소드가 너무 오래 걸려 강제 종료!");
            EndEpisode();
            episodeTimer = 0f;
        }

        // 잠금 해제용 시간 갱신
        UpdateLocks();
    }

    public override void OnEpisodeBegin()
    {
        agentHealth = 100f;
        targetHealth = 100f;
        episodeTimer = 0f;

        TestUIController.Instance.SetLeftHealth(agentHealth, 100f);
        TestUIController.Instance.SetRightHealth(targetHealth, 100f);

        Vector3 startPosition = new Vector3(-213.0f, 0.0f, -0.1f);
        this.transform.localPosition = startPosition;

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

    // 🔒 잠금 관련 변수
    private float dodgeLockTime = 0f;
    private float qAttackLockTime = 0f;
    private float eKickLockTime = 0f;
    private float rAttackLockTime = 0f;
    private float defendLockTime = 0f;

    private float lockDuration = 1.0f;

    private void UpdateLocks()
    {
        dodgeLockTime -= Time.deltaTime;
        qAttackLockTime -= Time.deltaTime;
        eKickLockTime -= Time.deltaTime;
        rAttackLockTime -= Time.deltaTime;
        defendLockTime -= Time.deltaTime;
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
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
                if (dodgeLockTime <= 0f)
                {
                    rootMotionMover.Dodge();
                    TestUIController.Instance.rightDodge.TriggerCooldown();
                    dodgeLockTime = lockDuration;
                }
                break;
            case 4: // Q_Attack
                if (qAttackLockTime <= 0f)
                {
                    rootMotionMover.StartAttack(RootMotionMover.AttackType.Q_Attack);
                    TestUIController.Instance.rightAttack.TriggerCooldown();
                    qAttackLockTime = lockDuration;

                    if (distanceToTarget < 4.0f)
                        AddReward(opponentIsAttacking ? 0.05f : -0.05f);
                }
                break;
            case 5: // E_Kick
                if (eKickLockTime <= 0f)
                {
                    rootMotionMover.StartAttack(RootMotionMover.AttackType.E_Kick);
                    TestUIController.Instance.rightAttack.TriggerCooldown();
                    eKickLockTime = lockDuration;

                    if (distanceToTarget < 4.0f)
                        AddReward(opponentIsAttacking ? 0.05f : -0.05f);
                }
                break;
            case 6: // R_Attack
                if (rAttackLockTime <= 0f)
                {
                    rootMotionMover.StartAttack(RootMotionMover.AttackType.R_Attack);
                    TestUIController.Instance.rightAttack.TriggerCooldown();
                    rAttackLockTime = lockDuration;

                    if (distanceToTarget < 4.0f)
                        AddReward(opponentIsAttacking ? 0.05f : -0.05f);
                }
                break;
            case 7: // Defend
                if (defendLockTime <= 0f)
                {
                    bool isDefending = Input.GetKey(KeyCode.RightShift);
                    rootMotionMover.SetDefend(isDefending);
                    TestUIController.Instance.rightDefend.TriggerCooldown();
                    Debug.Log("방어 상태: " + isDefending);
                    defendLockTime = lockDuration;
                }
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
