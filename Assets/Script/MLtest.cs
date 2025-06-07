using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class MLtest : Agent
{
    public GameObject sword;
    public RootMotionMover rootMotionMover;
    public Transform Target;

    public float agentHealth = 100f;
    public float targetHealth = 100f;
    public float forceMultiplier = 10;

    private Rigidbody rBody;
    private Vector3 lastSwordVelocity;
    private Vector3 lastSwordPosition;

    private bool isDefending = false;
    private int stepCount = 0;
    public int maxStepLimit = 1000;

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

    public override void OnEpisodeBegin()
    {
        agentHealth = 100f;
        targetHealth = 100f;
        stepCount = 0;

        transform.localPosition = new Vector3(Random.Range(-220f, -215f), 0.0f, Random.Range(3f, 7f));
        Target.localPosition = new Vector3(Random.Range(-215f, -210f), 0.0f, Random.Range(3f, 7f));

        rBody.linearVelocity = Vector3.zero;
        rBody.angularVelocity = Vector3.zero;

        lastSwordPosition = sword.transform.position;
        lastSwordVelocity = Vector3.zero;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        Vector3 relativePosition = Target.localPosition - transform.localPosition;

        sensor.AddObservation(relativePosition);
        sensor.AddObservation(transform.localPosition);
        sensor.AddObservation(Target.localPosition);

        sensor.AddObservation(rBody.linearVelocity);

        Rigidbody targetBody = Target.GetComponent<Rigidbody>();
        if (targetBody != null)
        {
            sensor.AddObservation(targetBody.linearVelocity);
        }

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
        stepCount++;

        int action = actionBuffers.DiscreteActions[0];
        rootMotionMover.animator.SetFloat("v", 0.0f);

        float distanceToTarget = Vector3.Distance(transform.localPosition, Target.localPosition);
        float distanceReward = 0.01f * (1.0f - Mathf.Clamp01(distanceToTarget / 10f));
        AddReward(distanceReward);

        // 디버그용 행동명 변수
        string actionName = "Idle";

        switch (action)
        {
            case 0:
                AddReward(-0.01f);
                actionName = "Idle";
                break;
            case 1:
                rootMotionMover.animator.SetFloat("v", 1.0f);
                AddReward(0.01f);
                actionName = "Move Forward";
                break;
            case 2:
                rootMotionMover.animator.SetFloat("v", -2.0f);
                AddReward(0.01f);
                actionName = "Move Backward";
                break;
            case 3:
                rootMotionMover.Dodge();
                actionName = "Dodge";
                break;
            case 4:
                rootMotionMover.StartAttack(RootMotionMover.AttackType.Q_Attack);
                AddReward(-0.005f);
                actionName = "Q_Attack";
                break;
            case 5:
                rootMotionMover.StartAttack(RootMotionMover.AttackType.E_Kick);
                AddReward(-0.005f);
                actionName = "E_Kick";
                break;
            case 6:
                rootMotionMover.StartAttack(RootMotionMover.AttackType.R_Attack);
                AddReward(-0.005f);
                actionName = "R_Attack";
                break;
            case 7:
                isDefending = true;
                actionName = "Defend";
                break;
        }

        rootMotionMover.SetDefend(isDefending);

        // 디버그 출력
        Debug.Log($"[Step {stepCount}] Action: {actionName} | AgentHP: {agentHealth} | TargetHP: {targetHealth} | Distance: {distanceToTarget:F2}");

        if (targetHealth <= 0f)
        {
            Debug.Log("[Episode End] 승리");
            SetReward(1.0f + (agentHealth / 100f));
            EndEpisode();
        }
        else if (agentHealth <= 0f)
        {
            Debug.Log("[Episode End] 패배");
            SetReward(-1.0f);
            EndEpisode();
        }
        else if (stepCount >= maxStepLimit)
        {
            Debug.Log("[Episode End] 시간 초과");
            AddReward(-0.5f);
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

    public void OnAttackSuccess(float damage)
    {
        float reward = 0.5f + damage * 0.05f;
        AddReward(reward);
        Debug.Log($"[공격 성공] 데미지: {damage}, 보상: {reward}");
    }

    public void OnEffectiveCounterAttack(float damageDefault)
    {
        AddReward(0.2f + damageDefault * 0.01f);
    }

    public void OnDefendSuccess(float attackAccel)
    {
        float reward = 0.1f + 0.04f * Mathf.Clamp((attackAccel - 1f), 0f, 10f);
        AddReward(reward);
        Debug.Log($"[방어 성공] 공격 가속도: {attackAccel}, 보상: {reward}");
    }

    public void OnDodgeSuccess(float attackAccel)
    {
        float reward = 0.1f + 0.04f * Mathf.Clamp((attackAccel - 1f), 0f, 10f);
        AddReward(reward);
        Debug.Log($"[회피 성공] 공격 가속도: {attackAccel}, 보상: {reward}");
    }
}
