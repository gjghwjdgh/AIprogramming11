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
        // 에피소드 초기화
        currentHealth = maxHealth;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        isAttacking = false;
        transform.position = new Vector3(-214.78f, 0f, 105.03f);

        attackTimer = 0f;
        defendTimer = 0f;
        dodgeTimer = 0f;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // --- [변경점 2] 절대 위치 대신 상대적, 정규화된 데이터 관측 ---

        // 내 상태 (체력, 쿨타임) - 정규화
        sensor.AddObservation(currentHealth / maxHealth);
        sensor.AddObservation(attackTimer / attackCooldown);
        sensor.AddObservation(defendTimer / defendCooldown);
        sensor.AddObservation(dodgeTimer / dodgeCooldown);

        // 적과의 관계 (상대 위치, 거리)
        if (enemy != null)
        {
            // 나를 기준으로 한 적의 상대적 위치 (AI에게 더 유용한 정보)
            sensor.AddObservation(transform.InverseTransformPoint(enemy.position));
            // 적과의 거리
            sensor.AddObservation(Vector3.Distance(transform.position, enemy.position) / 10f); // 거리도 정규화 (최대 예상 거리로 나눔)
        }
        else // 적이 없으면 0으로 채움
        {
            sensor.AddObservation(Vector3.zero);
            sensor.AddObservation(0f);
        }
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        // --- [변경점 3] 행동 전, 적을 바라보도록 회전 ---
        if (enemy != null && !isAttacking)
        {
            Vector3 directionToEnemy = (enemy.position - transform.position).normalized;
            directionToEnemy.y = 0;
            Quaternion lookRotation = Quaternion.LookRotation(directionToEnemy);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
        }

        int action = actions.DiscreteActions[0]; // 0~5

        if (isAttacking) return;

        // 타이머 감소
        attackTimer -= Time.deltaTime;
        defendTimer -= Time.deltaTime;
        dodgeTimer -= Time.deltaTime;

        switch (action)
        {
            case 0: // Idle
                animator.SetFloat("v", 0);
                break;
            case 1: // Move Forward
                Move(Vector3.forward);
                break;
            case 2: // Move Backward
                Move(Vector3.back);
                break;
            case 3: // Attack
                if (attackTimer <= 0f)
                {
                    PerformAttack();
                    attackTimer = attackCooldown;
                }
                else
                {
                    AddReward(-0.3f); // 쿨 중 공격 시 패널티
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

        // 패배 조건 체크
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
            // 상대방 에이전트의 TakeDamage 호출
            BattleAgent otherAgent = enemy.GetComponent<BattleAgent>();
            if (otherAgent != null)
            {
                otherAgent.TakeDamage(10f);
                AddReward(+1f);
            }
        }
        else
        {
            // 헛스윙 패널티
            AddReward(-0.1f);
        }

        Invoke(nameof(ResetAttackState), 1.0f);
    }

    void PerformDefend()
    {
        animator.SetBool("isDefending", true);
        AddReward(+0.05f); // 방어 자세 유지에 대한 작은 보상 (지나치게 높으면 방어만 함)
        Invoke(nameof(ResetDefendState), 1.0f);
    }

    void PerformDodge()
    {
        animator.SetTrigger("dodge");
        rb.MovePosition(rb.position + transform.right * -1.5f); // 오른쪽 키 기준 오른쪽으로 회피하도록 수정
        AddReward(-0.1f);
    }

    public void TakeDamage(float amount)
    {
        // 방어 중일 때
        if (animator.GetBool("isDefending"))
        {
            AddReward(+0.5f); // 방어 성공 시 보상
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