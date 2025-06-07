using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class BattleAgent : Agent
{
    private Animator animator;
    private Rigidbody rb;

    public Transform enemy;
    public float moveSpeed = 2f;

    public float maxHealth = 100f;
    private float currentHealth;

    private float attackCooldown = 2.5f;
    private float defendCooldown = 2.5f;
    private float dodgeCooldown = 5f;

    private float attackTimer;
    private float defendTimer;
    private float dodgeTimer;

    private bool isAttacking;

    public override void Initialize()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
        currentHealth = maxHealth;
    }

    public override void OnEpisodeBegin()
    {
        currentHealth = maxHealth;

#pragma warning disable 0618
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
#pragma warning restore 0618

        isAttacking = false;
        animator.SetFloat("v", 0);
        animator.SetBool("isDefending", false);
        transform.position = new Vector3(-214.78f, 0f, 105.03f);

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
            sensor.AddObservation(transform.InverseTransformPoint(enemy.position));
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
        if (enemy != null && !isAttacking)
        {
            Vector3 directionToEnemy = (enemy.position - transform.position).normalized;
            directionToEnemy.y = 0;
            Quaternion lookRotation = Quaternion.LookRotation(directionToEnemy);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
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
                break;
            case 1:
                Move(Vector3.forward);
                break;
            case 2:
                Move(Vector3.back);
                break;
            case 3:
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
            case 4:
                if (defendTimer <= 0f)
                {
                    PerformDefend();
                    defendTimer = defendCooldown;
                }
                break;
            case 5:
                if (dodgeTimer <= 0f)
                {
                    PerformDodge();
                    dodgeTimer = dodgeCooldown;
                }
                break;
        }

        if (currentHealth <= 0)
        {
            SetReward(-5f);
            EndEpisode();
        }
    }

    void Move(Vector3 direction)
    {
        Vector3 move = transform.TransformDirection(direction) * moveSpeed * Time.deltaTime;
        rb.MovePosition(rb.position + move);
        animator.SetFloat("v", direction.z);
    }

    void PerformAttack()
    {
        animator.SetTrigger("attackTrigger");
        isAttacking = true;

        if (enemy != null && Vector3.Distance(transform.position, enemy.position) < 1.5f)
        {
            BattleAgent otherAgent = enemy.GetComponent<BattleAgent>();
            if (otherAgent != null)
            {
                otherAgent.TakeDamage(10f);
                AddReward(+1f);
            }
        }
        else
        {
            AddReward(-0.1f);
        }

        Invoke(nameof(ResetAttackState), 1.0f);
    }

    void PerformDefend()
    {
        animator.SetBool("isDefending", true);
        AddReward(+0.05f);
        Invoke(nameof(ResetDefendState), 1.0f);
    }

    void PerformDodge()
    {
        animator.SetTrigger("dodge");
        rb.MovePosition(rb.position + transform.right * -1.5f);
        AddReward(-0.1f);
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
    }

    void ResetAttackState() => isAttacking = false;
    void ResetDefendState() => animator.SetBool("isDefending", false);

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
