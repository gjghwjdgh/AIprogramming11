// 파일 이름: BT_Defensive_Paladin.cs
using UnityEngine;
using System.Collections.Generic;

public class BT_Defensive_Paladin : BT_Brain
{
    [Header("Debugging")]
    public bool enableDebugLog = true;
    public float debugLogInterval = 0.5f;
    private float debugTimer;

    [Header("AI Target")]
    public Transform target;
    public Animator targetAnimator;

    private Node root;

    // --- AI 성향 파라미터: 수비형에 맞게 기본값 조정 ---
    [Header("AI Behavior Parameters")]
    public float criticalHealthThreshold = 35f; // 더 높은 체력에서부터 치명타를 위협으로 간주
    public float lowHealthThreshold = 50f;      // 체력 50% 이하부터 '낮다'고 판단
    public float engageDistance = 10f;
    public float optimalCombatDistanceMin_Inspector = 1.0f; // 이름 변경
    public float optimalCombatDistanceMax_Inspector = 2.0f; // 이름 변경
    public float tooCloseDistance = 1.5f;       // 더 넓은 개인 공간을 선호

    [Header("Attack Range Parameters")]
    public float basicAttackRange = 2.0f;
    public float kickAttackRange = 1.8f;
    public float spinAttackRange = 2.5f;

    [Header("Opponent Animation State Names")]
    public string normalAttackStateName = "Attack_Normal";
    public string criticalAttackStateName = "Attack_Critical";
    public string defendStateName = "Defend";
    public string opponentIdleStateName = "Idle_Battle";
    public string[] postAttackLagStateNames = { "Idle_Battle" };
    public string[] wideOpenStateNames = { "Stunned" };

    // IPaladinParameters 인터페이스 멤버 구현
    public override float optimalCombatDistanceMin { get { return this.optimalCombatDistanceMin_Inspector; } }
    public override float optimalCombatDistanceMax { get { return this.optimalCombatDistanceMax_Inspector; } }
    public override string idleStateName { get { return this.opponentIdleStateName; } }


    private PaladinActuator actuator;
    private CooldownManager cooldownManager;

    void Awake()
    {
        cooldownManager = GetComponent<CooldownManager>();
        actuator = GetComponent<PaladinActuator>();
    }
    
    // ####################################################################
    // ### 수비형 AI의 새로운 생각의 흐름 (행동 트리) ###
    // ####################################################################
    void Start()
    {
        root = new Selector(new List<Node>
        {
            // --- 최우선 순위 1: 생존 및 방어 ---
            new Selector(new List<Node>
            {
                new Sequence(new List<Node> { new IsHealthLowNode(transform, 0f), new ActionLoggerNode(this, "사망", new DieNode(transform)) }),
                new Sequence(new List<Node> { new IsEnemyCritAttackDetectedNode(targetAnimator, criticalAttackStateName), new IsCooldownCompleteNode(transform, "Evade"), new ActionLoggerNode(this, "치명타 회피", new EvadeNode(transform, "Backward")) }),
                new Sequence(new List<Node> // 일반 공격은 최우선으로 방어
                {
                    new IsEnemyAttackImminentNode(targetAnimator, normalAttackStateName),
                    new IsHealthHighEnoughToDefendNode(transform, lowHealthThreshold),
                    new ActionLoggerNode(this, "방어", new DefendNode(transform))
                })
            }),

            // --- 우선 순위 2: 위치 선정 (안전 거리 확보) ---
            new Selector(new List<Node>
            {
                // 2-1. 너무 가까우면 최우선으로 뒤로 물러나기
                new Sequence(new List<Node>
                {
                    new IsEnemyInDistanceNode(transform, target, tooCloseDistance),
                    new IsCooldownCompleteNode(transform, "Evade"),
                    new ActionLoggerNode(this, "너무 가까움! 후퇴", new EvadeNode(transform, "Backward"))
                }),
                // 2-2. 교전 거리 밖이면 -> 안전한 최적 거리로 이동
                new Sequence(new List<Node>
                {
                    new IsNotInOptimalCombatRangeNode(transform, target, optimalCombatDistanceMin, optimalCombatDistanceMax),
                    new ActionLoggerNode(this, "안전 거리 유지", new MaintainDistanceNode(transform, target, (optimalCombatDistanceMin + optimalCombatDistanceMax) / 2f, 0.2f))
                })
            }),

            // --- 우선 순위 3: 반격 (수비형 AI의 주된 공격 수단) ---
            new Selector(new List<Node>
            {
                // 3-1. 적의 큰 빈틈(스턴 등)에 대한 반격
                new Sequence(new List<Node>
                {
                    new IsEnemyWideOpenNode(targetAnimator, wideOpenStateNames),
                    new IsEnemyInDistanceNode(transform, target, spinAttackRange),
                    new IsCooldownCompleteNode(transform, "SpinAttack"),
                    new ActionLoggerNode(this, "반격(필살기)", new SpinAttackNode(transform))
                }),
                // 3-2. 방어 성공 직후의 반격
                new Sequence(new List<Node>
                {
                    new DidDefendSucceedNode(transform), // '방금 방어에 성공했는가?' 조건 노드 필요
                    new IsEnemyInPostAttackLagNode(targetAnimator, postAttackLagStateNames),
                    new IsEnemyInDistanceNode(transform, target, kickAttackRange),
                    new IsCooldownCompleteNode(transform, "KickAttack"),
                    new ActionLoggerNode(this, "반격(발차기)", new KickAttackNode(transform))
                })
            }),

            // --- 우선 순위 4: 제한적인 선제공격 (매우 안전할 때만) ---
            new Sequence(new List<Node>
            {
                new IsInOptimalCombatRangeNode(transform, target, optimalCombatDistanceMin, optimalCombatDistanceMax),
                new IsSafeToAttackNode(transform, targetAnimator, lowHealthThreshold),
                new IsTargetNotAttackingOrDefendingNode(targetAnimator, normalAttackStateName, criticalAttackStateName, defendStateName),
                new RandomChanceNode(0.2f), // 20%의 낮은 확률로만 선제공격 시도
                new IsCooldownCompleteNode(transform, "BasicAttack"),
                new ActionLoggerNode(this, "견제 공격", new BasicAttackNode(transform))
            }),

            // --- 최후 순위: 대기 (안전 거리에서 간 보기) ---
            new ActionLoggerNode(this, "대기", new IdleNode(transform))
        });
    }

    void Update()
    {
        if (root != null && actuator != null && !actuator.IsActionInProgress)
        {
            root.Evaluate();
        }
        
        if (enableDebugLog && target != null)
        {
            debugTimer += Time.deltaTime;
            if (debugTimer >= debugLogInterval)
            {
                PrintDebugStatus();
                debugTimer = 0f;
            }
        }
    }

    void PrintDebugStatus()
    {
        string status = $"--- AI DEBUG STATUS ({gameObject.name}) ---\n";
        float distance = Vector3.Distance(transform.position, target.position);
        status += $"Distance to Target: {distance:F2} m\n";

        if (targetAnimator != null)
        {
            string targetState = "Unknown";
            if (targetAnimator.GetCurrentAnimatorStateInfo(0).IsName(normalAttackStateName)) targetState = "Normal Attack";
            else if (targetAnimator.GetCurrentAnimatorStateInfo(0).IsName(criticalAttackStateName)) targetState = "Critical Attack";
            else if (targetAnimator.GetCurrentAnimatorStateInfo(0).IsName(defendStateName)) targetState = "Defending";
            else if (targetAnimator.GetCurrentAnimatorStateInfo(0).IsName(opponentIdleStateName)) targetState = "Idle";
            status += $"Target Animator State: {targetState}\n";
        }

        if (cooldownManager != null)
        {
            status += "-- Cooldowns --\n";
            status += $"Basic Attack: {(cooldownManager.IsCooldownFinished("BasicAttack") ? "Ready" : "On Cooldown")}\n";
            status += $"Kick Attack: {(cooldownManager.IsCooldownFinished("KickAttack") ? "Ready" : "On Cooldown")}\n";
            status += $"Spin Attack: {(cooldownManager.IsCooldownFinished("SpinAttack") ? "Ready" : "On Cooldown")}\n";
            status += $"Evade: {(cooldownManager.IsCooldownFinished("Evade") ? "Ready" : "On Cooldown")}\n";
            status += $"Defend: {(cooldownManager.IsCooldownFinished("Defend") ? "Ready" : "On Cooldown")}\n";
            status += $"Strafe: {(cooldownManager.IsCooldownFinished("Strafe") ? "Ready" : "On Cooldown")}\n";
        }
        
        status += "------------------------------------------------";
        Debug.Log(status);
    }
}