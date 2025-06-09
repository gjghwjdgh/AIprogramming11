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

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        animator.SetFloat("v", 0);
        animator.SetBool("isDefending", false);

        transform.position = new Vector3(Random.Range(-210f, -205f), 0f, Random.Range(-2f, 2f));

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
        sensor.AddObservation(isActionLocked);            // 2
        sensor.AddObservation(attack0_Timer / attack0_Cooldown); // 3
        sensor.AddObservation(attack1_Timer / attack1_Cooldown); // 4
        sensor.AddObservation(attack2_Timer / attack2_Cooldown); // 5
        sensor.AddObservation(defendTimer / defendCooldown); // 6
        sensor.AddObservation(dodgeTimer / dodgeCooldown);   // 7

        if (enemy != null)
        {
            Vector3 relativePos = transform.InverseTransformPoint(enemy.position);
            sensor.AddObservation(relativePos); // 8, 9, 10
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
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(directionToEnemy), Time.deltaTime * 5f);
        }

        // 쿨다운 타이머들은 항상 감소
        if (attack0_Timer > 0) attack0_Timer -= Time.deltaTime;
        if (attack1_Timer > 0) attack1_Timer -= Time.deltaTime;
        if (attack2_Timer > 0) attack2_Timer -= Time.deltaTime;
        if (defendTimer > 0) defendTimer -= Time.deltaTime;
        if (dodgeTimer > 0) dodgeTimer -= Time.deltaTime;

        // 행동 잠금 상태에서는 아무것도 실행하지 않음
        if (isActionLocked) return;

        int action = actions.DiscreteActions[0];

        switch (action)
        {
            case 0:
                animator.SetFloat("v", 0);
                AddReward(-0.005f);
                break;
            case 1:
                Move(Vector3.forward);
                AddReward(0.01f);
                break;
            case 2:
                Move(Vector3.back);
                AddReward(0.005f);
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

        float distanceToEnemy = enemy != null ? Vector3.Distance(transform.position, enemy.position) : 0f;
        AddReward(0.01f * (1.0f - Mathf.Clamp01(distanceToEnemy / 10f)));

        if (currentHealth <= 0f)
        {
            Debug.Log($"{gameObject.name} 패배 (체력 0)");
            SetReward(-1f);
            EndEpisode();

            if (BattleUIController.Instance != null)
            {
                string winner = isLeftAgent ? "Right Agent" : "Left Agent";
                BattleUIController.Instance.ShowWinMessage(winner);
            }
        }
        else if (stepCount >= maxStepLimit)
        {
            Debug.Log($"{gameObject.name} 패배 (스텝 제한)");
            SetReward(-0.5f);
            EndEpisode();
        }
        else if (enemy != null)
        {
            BattleAgentImproved enemyAgent = enemy.GetComponent<BattleAgentImproved>();
            if (enemyAgent != null && enemyAgent.currentHealth <= 0f)
            {
                Debug.Log($"{gameObject.name} 승리 (적 처치)");
                SetReward(+1f);
                EndEpisode();

                if (BattleUIController.Instance != null)
                {
                    string winner = isLeftAgent ? "Left Agent" : "Right Agent";
                    BattleUIController.Instance.ShowWinMessage(winner);
                }
            }
        }
    }

    private void Move(Vector3 direction)
    {
        if (isActionLocked) return;
        Vector3 movement = transform.TransformDirection(direction) * moveSpeed * Time.deltaTime;
        rb.MovePosition(rb.position + movement);
        animator.SetFloat("v", direction.z);
    }

    private void PerformAttack(AttackType type)
    {
        isActionLocked = true; // 행동 잠금

        int index = (int)type;
        animator.SetInteger("attackIndex", index);
        animator.SetTrigger("attackTrigger");

        // 타입에 따라 각자 다른 쿨다운 타이머를 리셋하고 UI에 전달
        switch (type)
        {
            case AttackType.Attack0:
                attack0_Timer = attack0_Cooldown;
                BattleUIController.Instance?.TriggerCooldown(isLeftAgent, "Attack1", attack0_Cooldown);
                break;
            case AttackType.Attack1:
                attack1_Timer = attack1_Cooldown;
                BattleUIController.Instance?.TriggerCooldown(isLeftAgent, "Attack2", attack1_Cooldown);
                break;
            case AttackType.Attack2:
                attack2_Timer = attack2_Cooldown;
                BattleUIController.Instance?.TriggerCooldown(isLeftAgent, "Attack3", attack2_Cooldown);
                break;
        }

        if (enemy != null && Vector3.Distance(transform.position, enemy.position) < 1.5f)
        {
            BattleAgentImproved other = enemy.GetComponent<BattleAgentImproved>();
            if (other != null)
            {
                other.TakeDamage(15f);
                AddReward(+1f);
            }
        }
        else
        {
            AddReward(-0.1f);
        }
    }

    private void PerformDefend()
    {
        isActionLocked = true;
        defendTimer = defendCooldown;

        animator.SetBool("isDefending", true);
        BattleUIController.Instance?.TriggerCooldown(isLeftAgent, "Defend", defendCooldown);
    }

    private void PerformDodge()
    {
        isActionLocked = true;
        dodgeTimer = dodgeCooldown;

        animator.SetTrigger("dodgeTrigger");
        rb.MovePosition(rb.position + transform.right * -1.5f);
        BattleUIController.Instance?.TriggerCooldown(isLeftAgent, "Dodge", dodgeCooldown);
    }

    public void TakeDamage(float amount)
    {
        if (animator.GetBool("isDefending"))
        {
            AddReward(+0.5f);
            return;
        }

        currentHealth -= amount;
        AddReward(-0.5f);

        BattleUIController.Instance?.UpdateHealth(isLeftAgent, currentHealth, maxHealth);

        if (currentHealth <= 0f)
        {
            Debug.Log($"{gameObject.name} 패배 (피격으로 체력 0)");
            SetReward(-1f);
            EndEpisode();

            if (BattleUIController.Instance != null)
            {
                string winner = isLeftAgent ? "Right Agent" : "Left Agent";
                BattleUIController.Instance.ShowWinMessage(winner);
            }
        }
    }

    public void ReleaseActionLock()
    {
        isActionLocked = false;
        if (animator.GetBool("isDefending"))
        {
            animator.SetBool("isDefending", false);
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var da = actionsOut.DiscreteActions;
        da[0] = 0;

        if (Input.GetKey(KeyCode.W)) da[0] = 1;
        else if (Input.GetKey(KeyCode.S)) da[0] = 2;
        else if (Input.GetKey(KeyCode.Q)) da[0] = 3;
        else if (Input.GetKey(KeyCode.E)) da[0] = 4;
        else if (Input.GetKey(KeyCode.R)) da[0] = 5;
        else if (Input.GetKey(KeyCode.LeftShift)) da[0] = 6;
        else if (Input.GetKey(KeyCode.Space)) da[0] = 7;
    }
}