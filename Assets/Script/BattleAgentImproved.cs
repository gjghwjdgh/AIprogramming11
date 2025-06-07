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

    private float currentHealth;
    private bool isAttacking = false;
    private bool isDefending = false;

    // Cooldown Timers
    private float attackCooldown = 2.5f;
    private float defendCooldown = 2.0f;
    private float dodgeCooldown = 4.0f;

    private float attackTimer = 0f;
    private float defendTimer = 0f;
    private float dodgeTimer = 0f;

    private int stepCount = 0;
    private const int maxStepLimit = 1000;

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

        transform.position = new Vector3(Random.Range(-5f, 0f), 0f, Random.Range(-2f, 2f));

        attackTimer = 0f;
        defendTimer = 0f;
        dodgeTimer = 0f;
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
            case 0: // Idle
                animator.SetFloat("v", 0);
                AddReward(-0.005f);
                break;
            case 1: // Move Forward
                Move(Vector3.forward);
                AddReward(0.01f);
                break;
            case 2: // Move Backward
                Move(Vector3.back);
                AddReward(0.005f);
                break;
            case 3: // Attack
                if (attackTimer <= 0f)
                {
                    PerformAttack();
                    attackTimer = attackCooldown;
                }
                else
                {
                    AddReward(-0.3f);
                }
                break;
            case 4: // Defend
                if (defendTimer <= 0f)
                {
                    PerformDefend();
                    defendTimer = defendCooldown;
                }
                break;
            case 5: // Dodge
                if (dodgeTimer <= 0f)
                {
                    PerformDodge();
                    dodgeTimer = dodgeCooldown;
                }
                break;
        }

        float distanceToEnemy = enemy != null ? Vector3.Distance(transform.position, enemy.position) : 0f;
        AddReward(0.01f * (1.0f - Mathf.Clamp01(distanceToEnemy / 10f)));

        if (currentHealth <= 0)
        {
            SetReward(-1f);
            EndEpisode();
        }
        else if (stepCount >= maxStepLimit)
        {
            SetReward(-0.5f);
            EndEpisode();
        }
    }

    private void Move(Vector3 direction)
    {
        Vector3 movement = transform.TransformDirection(direction) * moveSpeed * Time.deltaTime;
        rb.MovePosition(rb.position + movement);
        animator.SetFloat("v", direction.z);
    }

    private void PerformAttack()
    {
        animator.SetTrigger("attackTrigger");
        isAttacking = true;

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
        Invoke(nameof(ResetDefendState), 1.0f);
    }

    private void PerformDodge()
    {
        animator.SetTrigger("dodge");
        rb.MovePosition(rb.position + transform.right * -1.5f);
        AddReward(-0.05f);
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
        else if (Input.GetKey(KeyCode.Q)) da[0] = 3;
        else if (Input.GetKey(KeyCode.LeftShift)) da[0] = 4;
        else if (Input.GetKey(KeyCode.Space)) da[0] = 5;
    }
}
