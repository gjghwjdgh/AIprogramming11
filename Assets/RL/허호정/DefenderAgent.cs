using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class DefenderAgent : Agent
{
    private Animator animator;
    private Rigidbody rb;

    public Transform enemy;
    public float moveSpeed = 2f;

    public float maxHealth = 100f;
    private float currentHealth;

    private float defendCooldown = 2.5f;
    private float dodgeCooldown = 5f;
    private float attackCooldown = 2.5f;

    private float defendTimer;
    private float dodgeTimer;
    private float attackTimer;

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
        transform.localPosition = new Vector3(Random.Range(-3f, 3f), 0, Random.Range(-3f, 3f));
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        defendTimer = dodgeTimer = attackTimer = 0f;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.localPosition);
        sensor.AddObservation(currentHealth / maxHealth);
        sensor.AddObservation(defendTimer / defendCooldown);
        sensor.AddObservation(dodgeTimer / dodgeCooldown);
        sensor.AddObservation(attackTimer / attackCooldown);

        if (enemy != null)
            sensor.AddObservation(enemy.localPosition);
        else
            sensor.AddObservation(Vector3.zero);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        int action = actions.DiscreteActions[0];

        if (isAttacking) return;

        defendTimer -= Time.deltaTime;
        dodgeTimer -= Time.deltaTime;
        attackTimer -= Time.deltaTime;

        switch (action)
        {
            case 0: animator.SetFloat("v", 0); break;
            case 1: Move(Vector3.forward); break;
            case 2: Move(Vector3.back); break;
            case 3:
                if (defendTimer <= 0f) { PerformDefend(); defendTimer = defendCooldown; }
                break;
            case 4:
                if (dodgeTimer <= 0f) { PerformDodge(); dodgeTimer = dodgeCooldown; }
                break;
            case 5:
                if (attackTimer <= 0f) { PerformAttack(); attackTimer = attackCooldown; }
                else { AddReward(-0.3f); }
                break;
        }

        if (currentHealth <= 0)
        {
            SetReward(-5f);
            EndEpisode();
        }
    }

    void Move(Vector3 dir)
    {
        Vector3 move = transform.TransformDirection(dir) * moveSpeed * Time.deltaTime;
        rb.MovePosition(rb.position + move);
        animator.SetFloat("v", dir.z);
    }

    void PerformDefend()
    {
        animator.SetBool("isDefending", true);
        AddReward(+0.5f); // 방어 시도 보상
        Invoke(nameof(ResetDefend), 1f);
    }

    void PerformDodge()
    {
        animator.SetTrigger("dodge");
        rb.MovePosition(rb.position + transform.right * 1.5f);
        AddReward(+0.5f); // 회피 시도 보상
    }

    void PerformAttack()
    {
        animator.SetTrigger("attackTrigger");
        isAttacking = true;

        if (enemy != null && Vector3.Distance(transform.position, enemy.position) < 1.5f)
        {
            DefenderAgent other = enemy.GetComponent<DefenderAgent>();
            if (other != null)
            {
                other.TakeDamage(10f);
                AddReward(+1f); // 반격 성공
            }
        }

        Invoke(nameof(ResetAttack), 1f);
    }

    public void TakeDamage(float damage)
    {
        if (animator.GetBool("isDefending"))
        {
            AddReward(+1.0f); // 방어 성공
            return;
        }

        currentHealth -= damage;
        AddReward(-0.5f);
    }

    void ResetDefend() => animator.SetBool("isDefending", false);
    void ResetAttack() => isAttacking = false;

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var da = actionsOut.DiscreteActions;
        da[0] = 0;
        if (Input.GetKey(KeyCode.W)) da[0] = 1;
        else if (Input.GetKey(KeyCode.S)) da[0] = 2;
        else if (Input.GetKey(KeyCode.LeftShift)) da[0] = 3;
        else if (Input.GetKey(KeyCode.Space)) da[0] = 4;
        else if (Input.GetKey(KeyCode.Q)) da[0] = 5;
    }
}
