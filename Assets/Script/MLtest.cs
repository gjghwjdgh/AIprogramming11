using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;

using Unity.MLAgents.Sensors;

using System.IO;




public class MLtest : Agent
{

    public GameObject sword; // Inspector에서 드래그&드롭으로 할당


    public RootMotionMover rootMotionMover;

    public float agentHealth = 100f;
    public float targetHealth = 100f;




   // public float lastAttackDamage = 0f;


    Rigidbody rBody;

    //public override void Heuristic(in ActionBuffers actionsOut)
    //{
    //    var continuousActions = actionsOut.ContinuousActions;
    //    continuousActions[0] = Input.GetAxis("Horizontal"); // 예: 키보드 좌우 입력
    //    continuousActions[1] = Input.GetAxis("Vertical");   // 예: 키보드 전후 입력
    //}
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

    //업데이트 함수- 에피소드 강제 종료
    float episodeTimer = 0f;
    float maxEpisodeTime = 50f;

    void Update()
    {
        episodeTimer += Time.deltaTime;
        if (episodeTimer > maxEpisodeTime)
        {
            Debug.LogWarning("에피소드가 너무 오래 걸려 강제 종료!");
            EndEpisode();
            episodeTimer = 0f;
        }
    }


    public override void OnEpisodeBegin()
    {

        agentHealth = 100f;
        targetHealth = 100f;
        episodeTimer = 0f;

        //// UI 업데이트
        TestUIController.Instance.SetLeftHealth(agentHealth, 100f);
        TestUIController.Instance.SetRightHealth(targetHealth, 100f);

        // 중력 작용 직전에 정확히 바닥 위로 보정
        Vector3 startPosition = new Vector3(-216.0f, 0.0f, -0.1f); 
        this.transform.localPosition = startPosition;

        //this.rBody.linearVelocity = Vector3.zero;
        //this.rBody.angularVelocity = Vector3.zero;

        Target.localPosition = new Vector3(-213.0f, 0.0f, -0.1f);
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(Target.localPosition);
        sensor.AddObservation(this.transform.localPosition);

        // 상대와의 거리 (옵션)
        Vector3 relativePosition = Target.localPosition - transform.localPosition;
        sensor.AddObservation(relativePosition);


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



    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        int discreteAction = actionBuffers.DiscreteActions[0];

        float distanceToTarget = Vector3.Distance(transform.localPosition, Target.localPosition);


        if (distanceToTarget < 4.0f)
        {

            AddReward(0.03f); // 가까우면 보상
        }

        //// 너무 멀어지면 패널티
        if (distanceToTarget > 5.0f)
        {
            AddReward(-0.05f); // 페널티도 고려
        }


        // 상대의 RootMotionMover 가져오기
        RootMotionMover opponentRootMotion = null;
        if (Target != null)
        {
            opponentRootMotion = Target.GetComponent<RootMotionMover>();
        }
        // 상대의 isAttacking 여부 확인
        bool opponentIsAttacking = (opponentRootMotion != null && opponentRootMotion.isAttacking);

        // RootMotionMover의 animator의 v값 초기화
        rootMotionMover.animator.SetFloat("v", 0.0f); // Idle 기본값



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
                
                    AddReward(0.05f);
                
                    break;
            case 4:
                rootMotionMover.StartAttack(RootMotionMover.AttackType.Q_Attack);

                break;
            case 5:
                rootMotionMover.StartAttack(RootMotionMover.AttackType.E_Kick);
                break;
            case 6:
                rootMotionMover.StartAttack(RootMotionMover.AttackType.R_Attack);
                break;
            case 7:
                bool isDefending = Input.GetKey(KeyCode.LeftShift); // 입력 체크!
                rootMotionMover.SetDefend(isDefending); // 입력 상태 그대로 방어 상태에 반영
                Debug.Log("방어 상태: " + isDefending);
                
                    AddReward(0.05f);
                
                
                break;
        }
        // Defend 상태 최종 반영 (한 프레임만 true로 끝나지 않도록!)
        //rootMotionMover.SetDefend(isDefending);

        // Agent나 Target이 죽으면 에피소드 종료

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
        if (agentHealth <= 0f)
        {
            SetReward(-1.0f);
            EndEpisode();
        }
    }

    public void OnSuccessfulAttack(float damageDefault)
    {
        float scaledReward = damageDefault * 0.2f; // 예: 데미지 비율 0.01로 조정
        AddReward(scaledReward);
        Debug.Log($"공격 성공! 데미지: {damageDefault}, 보상: {scaledReward}");
    }


    public void OnEffectiveCounterAttack(float damageDefault)
    {
        AddReward(0.2f + damageDefault * 0.01f);
    }

    // Agent.cs 등에서 직접 추가
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Boundary"))
        {
            Debug.Log("Boundary 충돌: 음의 보상 주기");
            AddReward(-0.07f);  // 보상은 필요에 따라 조정
        }
    }







}