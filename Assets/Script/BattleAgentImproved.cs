using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using System.Collections;

public class BattleAgentImproved : Agent
{
    private Animator animator;
    private Rigidbody rb;

    public Transform enemy;
    public float moveSpeed = 2f;
    public float maxHealth = 100f;

    [Header("Agent Side")]
    public bool isLeftAgent;

    private float currentHealth;

    // 1. 모든 행동을 제어할 단일 잠금 변수
    private bool isActionLocked = false;

    // 2. 각 스킬의 독립적인 쿨다운 변수들
    [Header("Cooldowns")]
    private float attack0_Cooldown = 2.5f;
    private float attack1_Cooldown = 3.0f;
    private float attack2_Cooldown = 4.0f;
    private float defendCooldown = 2.0f;
    private float dodgeCooldown = 4.0f;

    private float attack0_Timer = 0f;
    private float attack1_Timer = 0f;
    private float attack2_Timer = 0f;
    private float defendTimer = 0f;
    private float dodgeTimer = 0f;

    // 3. 각 행동의 지속 시간 (Invoke에서 사용)
    [Header("Action Durations")]
    private float attack0_Duration = 1.0f;
    private float attack1_Duration = 1.2f;
    private float attack2_Duration = 1.5f;
    private float defend_Duration = 1.0f;
    private float dodge_Duration = 0.8f;


    private int stepCount = 0;
    private const int maxStepLimit = 100000;

    private enum AttackType { Attack0, Attack1, Attack2 }

    public override void Initialize()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
        currentHealth = maxHealth;
    }

    public override void OnEpisodeBegin()
    {
        currentHealth = maxHealth;
        stepCount = 0;
        isActionLocked = false; // 에피소드 시작 시 잠금 해제

        // Invoke 호출이 남아있을 수 있으므로 취소
        CancelInvoke("ReleaseActionLock");

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        animator.SetFloat("v", 0);
        animator.SetBool("isDefending", false);

        // isLeftAgent 값에 따라 초기 위치를 다르게 설정
        if (isLeftAgent)
        {
            transform.position = new Vector3(Random.Range(-3f, -2f), 0f, Random.Range(-2f, 2f));
            transform.rotation = Quaternion.Euler(0, 90, 0);
        }
        else
        {
            transform.position = new Vector3(Random.Range(2f, 3f), 0f, Random.Range(-2f, 2f));
            transform.rotation = Quaternion.Euler(0, -90, 0);
        }


        // 모든 개별 타이머 초기화
        attack0_Timer = 0f;
        attack1_Timer = 0f;
        attack2_Timer = 0f;
        defendTimer = 0f;
        dodgeTimer = 0f;

        BattleUIController.Instance?.UpdateHealth(isLeftAgent, currentHealth, maxHealth);
        BattleUIController.Instance?.HideWinMessage();
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // 총 관측값: 11개
        sensor.AddObservation(currentHealth / maxHealth); // 1
        sensor.AddObservation(isActionLocked);           // 2
        sensor.AddObservation(attack0_Timer / attack0_Cooldown); // 3
        sensor.AddObservation(attack1_Timer / attack1_Cooldown); // 4
        sensor.AddObservation(attack2_Timer / attack2_Cooldown); // 5
        sensor.AddObservation(defendTimer / defendCooldown); // 6
        sensor.AddObservation(dodgeTimer / dodgeCooldown);   // 7

        if (enemy != null)
        {
            Vector3 relativePos = transform.InverseTransformPoint(enemy.position);
            sensor.AddObservation(relativePos.normalized); // 8, 9, 10
            sensor.AddObservation(Vector3.Distance(transform.position, enemy.position) / 10f); // 11
        }
        else
        {
            sensor.AddObservation(Vector3.zero);
            sensor.AddObservation(0f);
        }
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        stepCount++;

        if (enemy != null && !isActionLocked)
        {
            Vector3 directionToEnemy = (enemy.position - transform.position).normalized;
            directionToEnemy.y = 0;
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(directionToEnemy), Time.fixedDeltaTime * 5f);
        }

        // 쿨다운 타이머들은 항상 감소
        if (attack0_Timer > 0) attack0_Timer -= Time.fixedDeltaTime;
        if (attack1_Timer > 0) attack1_Timer -= Time.fixedDeltaTime;
        if (attack2_Timer > 0) attack2_Timer -= Time.fixedDeltaTime;
        if (defendTimer > 0) defendTimer -= Time.fixedDeltaTime;
        if (dodgeTimer > 0) dodgeTimer -= Time.fixedDeltaTime;

        // 행동 잠금 상태에서는 새로운 행동을 실행하지 않음
        if (isActionLocked) return;

        int action = actions.DiscreteActions[0];

        switch (action)
        {
            case 0: // 정지
                animator.SetFloat("v", 0);
                AddReward(-0.005f);
                break;
            case 1: // 전진
                Move(Vector3.forward);
                //AddReward(0.001f);
                break;
            case 2: // 후진
                Move(Vector3.back);
                //AddReward(0.001f);
                break;
            case 3: // 공격 0
                if (attack0_Timer <= 0f)
                {
                    PerformAttack(AttackType.Attack0);
                }
                break;
            case 4: // 공격 1
                if (attack1_Timer <= 0f)
                {
                    PerformAttack(AttackType.Attack1);
                }
                break;
            case 5: // 공격 2
                if (attack2_Timer <= 0f)
                {
                    PerformAttack(AttackType.Attack2);
                }
                break;
            case 6: // 방어
                if (defendTimer <= 0f)
                {
                    PerformDefend();
                }
                break;
            case 7: // 회피
                if (dodgeTimer <= 0f)
                {
                    PerformDodge();
                }
                break;
        }

        // 적과의 거리에 따른 보상
        if (enemy != null)
        {
            float distanceToEnemy = Vector3.Distance(transform.position, enemy.position);
            // 가까울수록 보상 증가 (거리가 0일때 0.01, 10일때 0)
            AddReward(0.001f * (1.0f - Mathf.Clamp01(distanceToEnemy / 10f)));
        }


        if (currentHealth <= 0f)
        {
            SetReward(-1f);
            EndEpisode();
            NotifyWinner(false); // 내가 졌음
        }
        else if (stepCount >= maxStepLimit)
        {
            SetReward(-0.5f);
            EndEpisode();
        }
        else if (enemy != null)
        {
            BattleAgentImproved enemyAgent = enemy.GetComponent<BattleAgentImproved>();
            if (enemyAgent != null && enemyAgent.currentHealth <= 0f)
            {
                SetReward(+1f);
                EndEpisode();
                NotifyWinner(true); // 내가 이겼음
            }
        }
    }

    private void Move(Vector3 direction)
    {
        if (isActionLocked) return;
        Vector3 movement = transform.TransformDirection(direction) * moveSpeed * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + movement);
        animator.SetFloat("v", direction.z);
    }

    private void PerformAttack(AttackType type)
    {
        isActionLocked = true; // 행동 잠금
        animator.SetFloat("v", 0); // 공격 중에는 이동 애니메이션 정지

        int index = (int)type;
        animator.SetInteger("attackIndex", index);
        animator.SetTrigger("attackTrigger");

        float duration = 0f;

        switch (type)
        {
            case AttackType.Attack0:
                attack0_Timer = attack0_Cooldown;
                duration = attack0_Duration;
                BattleUIController.Instance?.TriggerCooldown(isLeftAgent, "Attack1", attack0_Cooldown);
                break;
            case AttackType.Attack1:
                attack1_Timer = attack1_Cooldown;
                duration = attack1_Duration;
                BattleUIController.Instance?.TriggerCooldown(isLeftAgent, "Attack2", attack1_Cooldown);
                break;
            case AttackType.Attack2:
                attack2_Timer = attack2_Cooldown;
                duration = attack2_Duration;
                BattleUIController.Instance?.TriggerCooldown(isLeftAgent, "Attack3", attack2_Cooldown);
                break;
        }

        // 행동 지속 시간 이후에 잠금 해제 예약
        Invoke("ReleaseActionLock", duration);

        if (enemy != null && Vector3.Distance(transform.position, enemy.position) < 2.0f)
        {
            BattleAgentImproved other = enemy.GetComponent<BattleAgentImproved>();
            if (other != null && !other.IsDefending()) // 상대가 방어 중이 아닐 때
            {
                other.TakeDamage(15f, true); // 피격 성공
                AddReward(+0.2f);
            }
            else // 상대가 방어 중일 때
            {
                AddReward(-0.05f); // 공격이 막힘
            }
        }
        else // 공격이 빗나감
        {
            AddReward(-0.1f);
        }
    }

    private void PerformDefend()
    {
        isActionLocked = true;
        animator.SetFloat("v", 0);
        defendTimer = defendCooldown;

        animator.SetBool("isDefending", true);
        BattleUIController.Instance?.TriggerCooldown(isLeftAgent, "Defend", defendCooldown);

        // 방어 지속 시간 이후에 잠금 해제 예약
        Invoke("ReleaseActionLock", defend_Duration);
    }

    private void PerformDodge()
    {
        isActionLocked = true;
        animator.SetFloat("v", 0);
        dodgeTimer = dodgeCooldown;

        animator.SetTrigger("dodgeTrigger");

        // 회피는 순간적인 움직임으로 처리
        rb.AddForce(transform.right * -10f, ForceMode.VelocityChange);

        BattleUIController.Instance?.TriggerCooldown(isLeftAgent, "Dodge", dodgeCooldown);

        // 회피 모션 시간 이후에 잠금 해제 예약
        Invoke("ReleaseActionLock", dodge_Duration);
        AddReward(0.05f); // 회피 행동 자체에 작은 보상
    }

    public void TakeDamage(float amount, bool isDirectHit)
    {
        if (animator.GetBool("isDefending"))
        {
            AddReward(+0.3f); // 방어 성공 보상
            // 상대 에이전트에게도 알려주기 위해, 상대방이 직접 호출
            return;
        }

        currentHealth -= amount;
        if (isDirectHit) AddReward(-0.2f); // 직접적인 피격에 대한 패널티

        BattleUIController.Instance?.UpdateHealth(isLeftAgent, currentHealth, maxHealth);

        if (currentHealth <= 0f)
        {
            // EndEpisode()는 OnActionReceived에서 처리하므로 여기서는 보상만 설정
            SetReward(-1f);
        }
    }

    // 방어 상태인지 외부에서 확인할 수 있는 메서드
    public bool IsDefending()
    {
        return animator.GetBool("isDefending");
    }


    public void ReleaseActionLock()
    {
        isActionLocked = false;
        if (animator.GetBool("isDefending"))
        {
            animator.SetBool("isDefending", false);
        }
    }

    private void NotifyWinner(bool selfWon)
    {
        if (BattleUIController.Instance != null)
        {
            string winner;
            if (selfWon)
            {
                winner = isLeftAgent ? "Left Agent" : "Right Agent";
            }
            else
            {
                winner = isLeftAgent ? "Right Agent" : "Left Agent";
            }
            BattleUIController.Instance.ShowWinMessage(winner);
        }
    }


    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var da = actionsOut.DiscreteActions;
        da[0] = 0;

        if (Input.GetKey(KeyCode.W)) da[0] = 1;
        else if (Input.GetKey(KeyCode.S)) da[0] = 2;
        else if (Input.GetKey(KeyCode.Alpha1)) da[0] = 3; // Q
        else if (Input.GetKey(KeyCode.Alpha2)) da[0] = 4; // E
        else if (Input.GetKey(KeyCode.Alpha3)) da[0] = 5; // R
        else if (Input.GetKey(KeyCode.LeftShift)) da[0] = 6;
        else if (Input.GetKey(KeyCode.Space)) da[0] = 7;
    }
}