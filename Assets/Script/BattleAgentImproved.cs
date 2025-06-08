using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

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
    private bool isAttacking = false;
    private bool isDefending = false;

    private float attackCooldown = 2.5f;
    private float defendCooldown = 2.0f;
    private float dodgeCooldown = 4.0f;

    private float attackTimer = 0f;
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

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        isAttacking = false;
        isDefending = false;

        animator.SetFloat("v", 0);
        animator.SetBool("isDefending", false);

        transform.position = new Vector3(Random.Range(-210f, -205f), 0f, Random.Range(-2f, 2f));

        attackTimer = 0f;
        defendTimer = 0f;
        dodgeTimer = 0f;

        BattleUIController.Instance?.UpdateHealth(isLeftAgent, currentHealth, maxHealth);
        BattleUIController.Instance?.HideWinMessage();
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(currentHealth / maxHealth);
        sensor.AddObservation(attackTimer / attackCooldown);
        sensor.AddObservation(defendTimer / defendCooldown);
        sensor.AddObservation(dodgeTimer / dodgeCooldown);

        if (enemy != null)
        {
            Vector3 relativePos = transform.InverseTransformPoint(enemy.position);
            sensor.AddObservation(relativePos);
            sensor.AddObservation(Vector3.Distance(transform.position, enemy.position) / 10f);
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

        if (enemy != null && !isAttacking)
        {
            Vector3 directionToEnemy = (enemy.position - transform.position).normalized;
            directionToEnemy.y = 0;
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(directionToEnemy), Time.deltaTime * 5f);
        }

        int action = actions.DiscreteActions[0];

        if (isAttacking) return;

        attackTimer -= Time.deltaTime;
        defendTimer -= Time.deltaTime;
        dodgeTimer -= Time.deltaTime;

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
                if (attackTimer <= 0f)
                {
                    PerformAttack(AttackType.Attack0);
                    attackTimer = attackCooldown;
                }
                else
                {
                    AddReward(-0.3f);
                }
                break;
            case 4: // 공격 1
                if (attackTimer <= 0f)
                {
                    PerformAttack(AttackType.Attack1);
                    attackTimer = attackCooldown;
                }
                else
                {
                    AddReward(-0.3f);
                }
                break;
            case 5: // 공격 2
                if (attackTimer <= 0f)
                {
                    PerformAttack(AttackType.Attack2);
                    attackTimer = attackCooldown;
                }
                else
                {
                    AddReward(-0.3f);
                }
                break;
            case 6:
                if (defendTimer <= 0f)
                {
                    PerformDefend();
                    defendTimer = defendCooldown;
                }
                break;
            case 7:
                if (dodgeTimer <= 0f)
                {
                    PerformDodge();
                    dodgeTimer = dodgeCooldown;
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
        Vector3 movement = transform.TransformDirection(direction) * moveSpeed * Time.deltaTime;
        rb.MovePosition(rb.position + movement);
        animator.SetFloat("v", direction.z);
    }

    private void PerformAttack(AttackType type)
    {
        isAttacking = true;

        int index = (int)type;
        animator.SetInteger("attackIndex", index);
        animator.SetTrigger("attackTrigger");

        switch (type)
        {
            case AttackType.Attack0:
                BattleUIController.Instance?.TriggerCooldown(isLeftAgent, "Attack1", attackCooldown);
                break;
            case AttackType.Attack1:
                BattleUIController.Instance?.TriggerCooldown(isLeftAgent, "Attack2", attackCooldown);
                break;
            case AttackType.Attack2:
                BattleUIController.Instance?.TriggerCooldown(isLeftAgent, "Attack3", attackCooldown);
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

        Invoke(nameof(ResetAttackState), 1.0f);
    }

    private void PerformDefend()
    {
        isDefending = true;
        animator.SetBool("isDefending", true);
        AddReward(+0.1f);

        BattleUIController.Instance?.TriggerCooldown(isLeftAgent, "Defend", defendCooldown);
        Invoke(nameof(ResetDefendState), 1.0f);
    }

    private void PerformDodge()
    {
        Debug.Log("회피 모션 시작됨!");
        animator.SetTrigger("dodgeTrigger");
        rb.MovePosition(rb.position + transform.right * -1.5f);
        AddReward(-0.05f);

        BattleUIController.Instance?.TriggerCooldown(isLeftAgent, "Dodge", dodgeCooldown);
    }

    public void TakeDamage(float amount)
    {
        if (isDefending)
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

    private void ResetAttackState() => isAttacking = false;

    private void ResetDefendState()
    {
        isDefending = false;
        animator.SetBool("isDefending", false);
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var da = actionsOut.DiscreteActions;
        da[0] = 0;

        if (Input.GetKey(KeyCode.W)) da[0] = 1;
        else if (Input.GetKey(KeyCode.S)) da[0] = 2;
        else if (Input.GetKey(KeyCode.Q)) da[0] = 3; // 공격0
        else if (Input.GetKey(KeyCode.E)) da[0] = 4; // 공격1
        else if (Input.GetKey(KeyCode.R)) da[0] = 5; // 공격2
        else if (Input.GetKey(KeyCode.LeftShift)) da[0] = 6; // 방어
        else if (Input.GetKey(KeyCode.Space)) da[0] = 7; // 회피
    }
}
