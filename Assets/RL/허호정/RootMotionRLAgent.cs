using UnityEngine;
using System.Collections;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class RootMotionRLAgent : Agent // Ŭ���� �̸��� RootMotionRLAgent ������ ����
{
    private Animator animator;
    public TrailRenderer swordTrail;

    public enum AttackType { None = 0, Q_Attack = 1, E_Kick = 2, R_Attack = 3 } // None �߰�
    private AttackType currentAttackTypeInternal; // ���� ���� ������

    public float qAttackTrailDuration = 0.4f;
    public float rAttackTrailDuration = 0.6f;
    public float qAttackTrailDelay = 0.05f;
    public float rAttackTrailDelay = 0.1f;

    private bool isAttacking = false;

    public SoundManager soundManager;
    public AudioClip qAttackSfx;
    public AudioClip rAttackSfx;
    public AudioClip eKickSfx;
    public AudioClip wKeySfx;
    public AudioClip sKeySfx;

    // RL ������Ʈ ���� ���� ����
    [Header("RL Settings")]
    public Transform target; // �н� ��ǥ (��: ������ ���)
    public float arenaSizeX = 10f; // ����� X�� ���� ũ�� (��: 10�̸� -10 ~ 10)
    public float arenaSizeZ = 10f; // ����� Z�� ���� ũ��
    public float moveSpeedMultiplier = 1.0f; // �ִϸ����� v���� ������ �� (��Ʈ��� �ӵ� ������)

    private Vector3 startPosition;
    private Quaternion startRotation;

    // --- Agent �⺻ �޼��� ---

    public override void Initialize()
    {
        animator = GetComponent<Animator>();
        animator.applyRootMotion = true; // ��Ʈ ��� ���

        if (swordTrail != null)
            swordTrail.enabled = false;

        if (soundManager == null)
        {
            // �����Ϳ��� ����� ���� SoundManager�� �Ҵ��ϵ��� ����
            // Debug.LogError("SoundManager�� ������� �ʾҽ��ϴ�!", this);
        }

        startPosition = transform.localPosition;
        startRotation = transform.localRotation;

        // isAttacking�� ���Ǽҵ� ���� �� Ȯ���� false��
        isAttacking = false;
        currentAttackTypeInternal = AttackType.None;
    }

    public override void OnEpisodeBegin()
    {
        // ������Ʈ ��ġ �� ȸ�� ����
        transform.localPosition = startPosition + new Vector3(Random.Range(-arenaSizeX / 2, arenaSizeX / 2), 0, Random.Range(-arenaSizeZ / 2, arenaSizeZ / 2));
        transform.localRotation = Quaternion.Euler(0, Random.Range(0, 360), 0);


        // Ÿ���� �ִٸ� Ÿ�� ��ġ�� ���� (����� �� ���� ��ġ)
        if (target != null)
        {
            target.localPosition = new Vector3(Random.Range(-arenaSizeX * 0.8f, arenaSizeX * 0.8f),
                                             target.localPosition.y, // Ÿ���� Y���� �����ϰų� ������ ���
                                             Random.Range(-arenaSizeZ * 0.8f, arenaSizeZ * 0.8f));
        }

        // ���� ���� �ʱ�ȭ
        isAttacking = false;
        currentAttackTypeInternal = AttackType.None;
        animator.SetFloat("v", 0f);
        animator.SetBool("isDefending", false);
        // �ִϸ����� Ʈ���� ���� (�ʿ��ϴٸ�)
        // animator.ResetTrigger("attackTrigger");

        if (swordTrail != null)
            swordTrail.enabled = false;

        if (soundManager != null)
            soundManager.StopWalkingSound();
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // ������Ʈ �ڽ��� ����
        sensor.AddObservation(transform.localPosition.normalized); // ����ȭ�� ��ġ (3 floats)
        sensor.AddObservation(transform.forward); // ����ȭ�� ���� ���� (3 floats) - ȸ�� ��� ��� ����
        sensor.AddObservation(isAttacking); // ���� ���� ������ (1 float: 0 or 1)
        sensor.AddObservation(animator.GetBool("isDefending")); // ���� ��� ������ (1 float: 0 or 1)
        sensor.AddObservation(animator.GetFloat("v")); // ���� �̵� �� (1 float)

        // Ÿ�ٰ��� ���� (Ÿ���� �����Ǿ� ���� ���)
        if (target != null)
        {
            Vector3 dirToTarget = (target.position - transform.position).normalized;
            sensor.AddObservation(dirToTarget); // Ÿ�� ���� (3 floats)
            sensor.AddObservation(Vector3.Distance(transform.position, target.position)); // Ÿ�ٰ��� �Ÿ� (1 float)
        }
        else // Ÿ���� ���� ��� 0���� ä��
        {
            sensor.AddObservation(Vector3.zero); // 3 floats
            sensor.AddObservation(0f); // 1 float
        }
        // �� ���� ��: 3 + 3 + 1 + 1 + 1 + 3 + 1 = 13 floats (Ÿ�� ���� ��)
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        // �ൿ �迭 ���� (Behavior Parameters���� ������ ���)
        // Discrete Actions:
        // Branch 0: �̵� (0: ����, 1: ������, 2: �ڷ�)
        // Branch 1: ���� (0: ����, 1: Q, 2: E, 3: R)
        // Branch 2: ��� (0: ����, 1: ���)

        int moveAction = actions.DiscreteActions[0];
        int attackAction = actions.DiscreteActions[1]; // AttackType enum�� �� ��ġ (0: None, 1:Q, 2:E, 3:R)
        int defenseAction = actions.DiscreteActions[2];

        // 1. �̵� ó��
        float v = 0.0f;
        if (!isAttacking) // ���� ���� �ƴ� ���� �̵�/��� ����
        {
            if (moveAction == 1) v = 1.0f * moveSpeedMultiplier;
            else if (moveAction == 2) v = -1.0f * moveSpeedMultiplier;
        }
        animator.SetFloat("v", v);

        // 2. ��� ó��
        if (!isAttacking)
        {
            animator.SetBool("isDefending", defenseAction == 1);
        }

        // 3. ���� ó��
        if (!isAttacking && attackAction != (int)AttackType.None)
        {
            AttackType selectedAttack = (AttackType)attackAction;
            StartAttackInternal(selectedAttack);
        }

        // --- ���� ���� ---
        // ����ִ� �Ϳ� ���� ���� �г�Ƽ (�ð� ��� �г�Ƽ)
        AddReward(-0.0005f);

        // (����) Ÿ�ٰ��� �Ÿ��� ���� ���� (��������� ����)
        if (target != null)
        {
            float distanceToTarget = Vector3.Distance(transform.position, target.position);
            // AddReward((1.0f / (1.0f + distanceToTarget)) * 0.001f); // �Ÿ��� �������� ū ���� (�̼� ���� �ʿ�)

            // (����) Ÿ���� �������� �� ū ����
            // �� �κ��� ���� ���� ���� �ý��۰� �����ؾ� �մϴ�.
            // ���⼭�� ������ ���� �ִϸ��̼� �߿� Ÿ���� Ư�� ���� ���� ������ ����
            if (isAttacking && distanceToTarget < 2.0f) // Q, E ���� ���� �뷫 2m ����
            {
                // � �����̳Ŀ� ���� ���� ���� ����
                switch (currentAttackTypeInternal)
                {
                    case AttackType.Q_Attack:
                        AddReward(0.5f); // Q���� ����
                        Debug.Log("Q Attack Hit Target! Reward: 0.5");
                        break;
                    case AttackType.E_Kick:
                        AddReward(0.3f); // E���� ����
                        Debug.Log("E Kick Hit Target! Reward: 0.3");
                        break;
                    case AttackType.R_Attack: // R������ ������ �� �аų� ȿ���� ���� �� ����
                        if (distanceToTarget < 3.0f) AddReward(1.0f); // R���� ����
                        Debug.Log("R Attack Hit Target! Reward: 1.0");
                        break;
                }
                // ���������� ���������Ƿ� ���Ǽҵ� ���� �Ǵ� Ÿ�� ����
                // EndEpisode();
                // �Ǵ� Ÿ�� ��ġ�� �缳���ؼ� ��� �н�
                // target.localPosition = new Vector3(Random.Range(-arenaSizeX * 0.8f, arenaSizeX * 0.8f), target.localPosition.y, Random.Range(-arenaSizeZ * 0.8f, arenaSizeZ * 0.8f));
            }
        }

        // (����) �ʹ� ���� �������� ������ ���� �г�Ƽ
        // if (!isAttacking && GetCumulativeReward() < -0.5f) // Ư�� �ð� ���� ���� ���ϸ�
        // {
        //    AddReward(-0.01f);
        // }


        // --- ���� ó�� (������) ---
        HandleSounds(v, moveAction);
    }


    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActionsOut = actionsOut.DiscreteActions;
        discreteActionsOut.Clear();

        // �̵� (W, S Ű)
        if (Input.GetKey(KeyCode.W)) discreteActionsOut[0] = 1;
        else if (Input.GetKey(KeyCode.S)) discreteActionsOut[0] = 2;
        else discreteActionsOut[0] = 0;

        // ���� (Q, E, R Ű)
        if (Input.GetKeyDown(KeyCode.Q)) discreteActionsOut[1] = (int)AttackType.Q_Attack;
        else if (Input.GetKeyDown(KeyCode.E)) discreteActionsOut[1] = (int)AttackType.E_Kick;
        else if (Input.GetKeyDown(KeyCode.R)) discreteActionsOut[1] = (int)AttackType.R_Attack;
        else discreteActionsOut[1] = (int)AttackType.None;

        // ��� (LeftShift Ű)
        if (Input.GetKey(KeyCode.LeftShift)) discreteActionsOut[2] = 1;
        else discreteActionsOut[2] = 0;
    }

    // (StartAttack�� StartAttackInternal�� �����Ͽ� RL�� Action�� ����)

    void StartAttackInternal(AttackType attackType)
    {
        if (isAttacking) return; // �̹� ���� ���̸� ���� ����

        isAttacking = true;
        currentAttackTypeInternal = attackType; // ���� ���� Ÿ�� ���

        if (soundManager != null) soundManager.StopWalkingSound();

        animator.SetTrigger("attackTrigger");
        animator.SetInteger("attackIndex", (int)attackType - 1); // enum None ������ -1 (���� Q=0, E=1, R=2)

        if (soundManager != null)
        {
            switch (attackType)
            {
                case AttackType.Q_Attack:
                    if (qAttackSfx != null) soundManager.PlaySoundEffect(qAttackSfx);
                    if (swordTrail != null) StartCoroutine(PlayTrail(qAttackTrailDuration, qAttackTrailDelay));
                    break;
                case AttackType.E_Kick:
                    if (eKickSfx != null) soundManager.PlaySoundEffect(eKickSfx);
                    break;
                case AttackType.R_Attack:
                    if (rAttackSfx != null) soundManager.PlaySoundEffect(rAttackSfx);
                    if (swordTrail != null) StartCoroutine(PlayTrail(rAttackTrailDuration, rAttackTrailDelay));
                    break;
            }
        }
        StartCoroutine(ResetAttackStateAfterAnimation(GetAnimationLength(attackType)));
    }

    IEnumerator PlayTrail(float duration, float delay)
    {
        if (swordTrail == null) yield break;
        if (delay > 0) yield return new WaitForSeconds(delay);
        swordTrail.enabled = true;
        swordTrail.Clear();
        yield return new WaitForSeconds(duration);
        swordTrail.enabled = false;
    }

    IEnumerator ResetAttackStateAfterAnimation(float animationLength)
    {
        yield return new WaitForSeconds(animationLength);
        isAttacking = false;
        currentAttackTypeInternal = AttackType.None; // ���� �������Ƿ� None����

        // R ���� �� Y ��ġ ���� (������)
        if (animator.GetCurrentAnimatorStateInfo(0).IsName("R_Attack_Animation_State_Name")) // ���� �ִϸ��̼� ���� �̸����� Ȯ��
        {
            Vector3 fixedPos = transform.position;
            fixedPos.y = startPosition.y; // �ʱ� Y ������ ���� �Ǵ� 0.0f
            transform.position = fixedPos;
        }
    }

    float GetAnimationLength(AttackType attackType)
    {
        // �� ������ ���� �ִϸ��̼� Ŭ�� ���̿� ���� ��Ȯ�� �����ؾ� �մϴ�.
        switch (attackType)
        {
            case AttackType.Q_Attack: return qAttackTrailDelay + qAttackTrailDuration + 0.1f; // �ִϸ��̼� ��ü ����
            case AttackType.E_Kick: return 0.7f; // E_Kick �ִϸ��̼� ��ü ����
            case AttackType.R_Attack: return rAttackTrailDelay + rAttackTrailDuration + 0.1f; // �ִϸ��̼� ��ü ����
            default: return 0.5f;
        }
    }

    void OnAnimatorMove()
    {
        if (Time.deltaTime == 0) return; // �Ͻ����� �� ��Ȳ ����

        // R ���� �ִϸ��̼��� Y�� �̵��� ����ϰ�, �������� Y���� ���� (��Ʈ��� Ư�� ���� ����)
        // currentAttackTypeInternal�� ����Ͽ� ���� ���� ���� ���ݿ� ���� �ٸ��� ó�� ����
        if (isAttacking && currentAttackTypeInternal == AttackType.R_Attack)
        {
            transform.position += animator.deltaPosition;
        }
        else
        {
            Vector3 deltaPos = animator.deltaPosition;
            deltaPos.y = 0.0f; // �Ϲ� �̵��̳� �ٸ� ���� �� Y�� ����
            transform.position += deltaPos;
        }
        transform.rotation *= animator.deltaRotation;
    }

    // �ִϸ��̼� �̺�Ʈ���� isAttacking = false ȣ�� (������, �ڷ�ƾ�� �ߺ� ���ɼ�)
    public void AnimationFinished_SetAttackingFalse()
    {
        isAttacking = false;
        currentAttackTypeInternal = AttackType.None;
    }

    // �� �浹 ���� (����� ���� "Wall" �±׸� �ٿ��� ��)
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Wall"))
        {
            AddReward(-0.1f); // ���� �ε����� �г�Ƽ
            // Debug.Log("�� �浹! �г�Ƽ: -0.1");
            // EndEpisode(); // ���� �ε����� ���Ǽҵ� ���� (������)
        }
    }

    private void HandleSounds(float vValue, int moveActionValue)
    {
        if (soundManager == null) return;

        bool shouldPlayWalkingSound = !isAttacking && moveActionValue == 1 && vValue > 0;
        if (shouldPlayWalkingSound && wKeySfx != null)
        {
            soundManager.PlayWalkingSound(wKeySfx);
        }
        else
        {
            soundManager.StopWalkingSound();
        }

        // SŰ(�ڷΰ���) ���� (�ܹ߼�)
        // ML-Agents������ GetKeyDown�� ���� ȿ���� ������ ���� ���� �� �ʿ�
        // ���⼭�� �ܼ�ȭ�� ���� '�ڷΰ��� �׼��� ���õǾ���, isAttacking�� �ƴ� ��' �����θ� ǥ��
        if (!isAttacking && moveActionValue == 2 && sKeySfx != null /* && previousMoveAction != 2 (���� �ʿ�) */)
        {
            // soundManager.PlaySoundEffect(sKeySfx); // ���� ����� �� �����Ƿ� SoundManager���� �ߺ� ���� �ʿ�
        }
    }

    // BT용 코드들입니다~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    // BT용 코드들입니다~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    // BT용 코드들입니다~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

    public void Dodge(string direction)
    {
        // 애니메이터에 "Dodge_Forward", "Dodge_Backward" 같은 방향별 트리거가 있다고 가정
        // 또는 direction 값을 기반으로 h, v 파라미터를 설정하여 하나의 Blend Tree로 제어할 수도 있습니다.
        // 예시: animator.SetFloat("h", ...), animator.SetFloat("v", ...)
        animator.SetTrigger("Dodge_" + direction); // 예: "Dodge_Backward" 트리거 발동
    }

    public void FeintStep(string direction)
    {
        // FeintStep도 Dodge와 유사하게 방향별 트리거 또는 Blend Tree로 제어
        animator.SetTrigger("FeintStep_" + direction);
    }

    // DieNode를 위한 함수 (선택적이지만 일관성을 위해 추천)
    public void Die()
    {
        animator.SetTrigger("Die");
    }

    // 이동 제어 함수
    // vertical: 1.0f (전진), -1.0f (후진), 0 (정지)
    public void SetMovement(float vertical)
    {
        // MLtest.cs에서 사용하던 v 파라미터를 그대로 활용
        animator.SetFloat("v", vertical);
    }

    // 회전 제어 함수
    public void SetRotation(Quaternion rotation)
    {
        // 캐릭터의 회전을 즉시 또는 부드럽게(Slerp) 설정
        transform.rotation = rotation; 
    }

    // BT용 코드들입니다~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

}