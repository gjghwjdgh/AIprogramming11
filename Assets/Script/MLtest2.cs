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

    public float agentHealth = 100f;
    public float targetHealth = 100f;


    Rigidbody rBody;

    //public override void Heuristic(in ActionBuffers actionsOut)
    //{
    //    var continuousActions = actionsOut.ContinuousActions;
    //    continuousActions[0] = Input.GetAxis("Horizontal"); // 예: 키보드 좌우 입력
    //    continuousActions[1] = Input.GetAxis("Vertical");   // 예: 키보드 전후 입력
    //}
    public override void Heuristic(in ActionBuffers actionsOut)
    {

        Debug.Log("타겟의 휴리스틱이 계속 호출되고 있니?");
        var discreteActions = actionsOut.DiscreteActions;
        discreteActions[0] = 0;

        if (Input.GetKey(KeyCode.I))
            discreteActions[0] = 1; // Move Forward
        else if (Input.GetKey(KeyCode.J))
            discreteActions[0] = 2; // Move Backward
        else if (Input.GetKey(KeyCode.K))
            discreteActions[0] = 3; // Dodge
        else if (Input.GetKey(KeyCode.U))
            discreteActions[0] = 4; // Q_Attack
        else if (Input.GetKey(KeyCode.O))
            discreteActions[0] = 5; // E_Kick
        else if (Input.GetKey(KeyCode.P))
            discreteActions[0] = 6; // R_Attack
        else if (Input.GetKey(KeyCode.RightShift))
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
    }
    public Transform Target;

    public override void OnEpisodeBegin()
    {

        agentHealth = 100f;
        targetHealth = 100f; 

        // 중력 작용 직전에 정확히 바닥 위로 보정
        Vector3 startPosition = new Vector3(-213.9f, 0.0f, 5.0f);
        this.transform.localPosition = startPosition;

        //this.rBody.linearVelocity = Vector3.zero;
        //this.rBody.angularVelocity = Vector3.zero;

        Target.localPosition = new Vector3(-217.8f, 0.0f, 5.0f);
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(Target.localPosition);
        sensor.AddObservation(this.transform.localPosition);

        //칼의 현재 위치, 속도 계산
        Vector3 currentPos = sword.transform.position;
        Vector3 currentVelocity = (currentPos - lastSwordPosition) / Time.fixedDeltaTime;

        //칼의 가속도 계산: 현재 속도 - 이전 속도  / 시간변화량
        Vector3 swordAcceleration = (currentVelocity - lastSwordVelocity) / Time.fixedDeltaTime;



        //관찰값으로 추가
        sensor.AddObservation(swordAcceleration.x);
        sensor.AddObservation(swordAcceleration.z);

        //다음 프레임 계산을 위해 저장
        lastSwordPosition = currentPos;
        lastSwordVelocity = currentVelocity;

        //다른 관찰값도 여기 추가하면 됨.


    }


    // ... 기존 변수 및 메서드 ...

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
            float scaledReward = baseReward * (attackAccel / 5f);
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



    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        int discreteAction = actionBuffers.DiscreteActions[0];

        // 기본적으로 항상 방어자세 해제
        rootMotionMover.SetDefend(false);

        // RootMotionMover의 animator의 v값 초기화
        rootMotionMover.animator.SetFloat("v", 0.0f); // Idle 기본값

        switch (discreteAction)
        {
            case 0: // Idle
                AddReward(-0.01f); // 가만히 있으면 작은 패널티
                break;
            case 1: // Move Forward
                rootMotionMover.animator.SetFloat("v", 1.0f); // 전진 애니메이션 트리거
                AddReward(0.01f);
                break;
            case 2: // Move Backward
                rootMotionMover.animator.SetFloat("v", -1.0f); // 후진 애니메이션 트리거
                AddReward(0.01f);
                break;
            case 3: // Dodge
                if (!rootMotionMover.isDodgging)  // dodge 중이 아닐 때만 실행
                    rootMotionMover.Dodge();
                break; 

            case 4: // Q_Attack
                rootMotionMover.StartAttack(RootMotionMover.AttackType.Q_Attack);
                break;
            case 5: // E_Kick
                rootMotionMover.StartAttack(RootMotionMover.AttackType.E_Kick);
                break;
            case 6: // R_Attack
                rootMotionMover.StartAttack(RootMotionMover.AttackType.R_Attack);
                break;
            case 7: // Defend
                rootMotionMover.SetDefend(true);
                break;
        }

        // Agent나 Target이 죽으면 에피소드 종료
        if (targetHealth <= 0f)
        {
            float healthRatio = agentHealth / 100f;
            SetReward(1.0f + healthRatio);
            EndEpisode();
        }
        else if (agentHealth <= 0f)
        {
            SetReward(-1.0f);
            EndEpisode();
        }
    }


    //public override void OnActionReceived(ActionBuffers actionBuffers)
    //{
    //    int discreteAction = actionBuffers.DiscreteActions[0];

    //    // 기본적으로 v=0으로 초기화
    //    rootMotionMover.animator.SetFloat("v", 0.0f);

    //    switch (discreteAction)
    //    {
    //        case 0: // Idle
    //            AddReward(-0.01f); // 가만히 있으면 작은 패널티
    //            break;
    //        case 1: // Move Forward
    //            rBody.AddForce(transform.forward * forceMultiplier);
    //            rootMotionMover.animator.SetFloat("v", 1.0f);
    //            AddReward(0.01f); // 이동하면 소량 보상
    //            break;
    //        case 2: // Move Backward
    //            rBody.AddForce(-transform.forward * forceMultiplier);
    //            rootMotionMover.animator.SetFloat("v", -1.0f);
    //            AddReward(0.01f);
    //            break;
    //        case 3: // Dodge
    //            rootMotionMover.Dodge();
    //            break;
    //        case 4: // Q_Attack
    //            rootMotionMover.StartAttack(RootMotionMover.AttackType.Q_Attack);
    //            break;
    //        case 5: // E_Kick
    //            rootMotionMover.StartAttack(RootMotionMover.AttackType.E_Kick);
    //            break;
    //        case 6: // R_Attack
    //            rootMotionMover.StartAttack(RootMotionMover.AttackType.R_Attack);
    //            break;
    //        case 7: // Defend
    //            rootMotionMover.SetDefend(true);
    //            break;
    //    }

    //    // Agent나 Target이 죽으면 에피소드 종료
    //    if (targetHealth <= 0f)
    //    {
    //        float healthRatio = agentHealth / 100f;
    //        SetReward(1.0f + healthRatio);
    //        EndEpisode();
    //    }
    //    else if (agentHealth <= 0f)
    //    {
    //        SetReward(-1.0f);
    //        EndEpisode();
    //    }
    //}



    public void TakeDamage(float damage)
    {
        agentHealth -= damage;
        if (agentHealth <= 0f)
        {
            SetReward(-1.0f);
            EndEpisode();
        }
    }



    public void OnEffectiveCounterAttack(float damageDefault)
    {
        AddReward(0.2f + damageDefault * 0.01f);
    }






}