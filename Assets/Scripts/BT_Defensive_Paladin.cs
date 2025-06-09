// 파일 이름: BT_Defensive_Paladin.cs (확률적 회전베기 적용 버전)
using UnityEngine;
using System.Collections.Generic;

public class BT_Defensive_Paladin : BT_Brain 
{
    // --- 변수 선언부는 그대로 유지 ---
    [Header("Debugging")]
    public bool enableDebugLog = true;
    public float debugLogInterval = 0.5f;
    private float debugTimer;

    [Header("AI Target")]
    public Transform target;
    public Animator targetAnimator;

    private Node root;

    [Header("AI Behavior Parameters")]
    public float criticalHealthThreshold = 35f;
    public float lowHealthThreshold = 50f;
    public float engageDistance = 10f;
    public float optimalCombatDistanceMin_Inspector = 2.5f;
    public float optimalCombatDistanceMax_Inspector = 4.0f;
    public float tooCloseDistance = 1.5f;

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

    void Start()
    {
        root = new Selector(new List<Node>
        {
            // --- 최우선 순위 1: 생존 및 방어 ---
            new Selector(new List<Node>
            {
                new Sequence(new List<Node> { new IsHealthLowNode(transform, 0f), new ActionLoggerNode(this, "사망", new DieNode(transform)) }),
                new Sequence(new List<Node> { new IsEnemyCritAttackDetectedNode(targetAnimator, criticalAttackStateName), new ActionLoggerNode(this, "치명타 회피", new EvadeNode(transform, "Backward")) }),
                new Sequence(new List<Node>
                {
                    new IsEnemyAttackImminentNode(targetAnimator, normalAttackStateName),
                    new IsHealthHighEnoughToDefendNode(transform, lowHealthThreshold),
                    new Selector(new List<Node>
                    {
                        new Sequence(new List<Node> { new IsCooldownCompleteNode(transform, "Defend"), new ActionLoggerNode(this, "방어", new TimedDefendNode(transform, 1.5f)) }),
                        new Sequence(new List<Node> { new IsCooldownCompleteNode(transform, "Evade"), new ActionLoggerNode(this, "긴급 회피", new EvadeNode(transform, "Backward")) })
                    })
                })
            }),

            // --- 우선 순위 2: 위치 선정 (안전 거리 확보) ---
            new Selector(new List<Node>
            {
                new Sequence(new List<Node> { new IsEnemyInDistanceNode(transform, target, tooCloseDistance), new IsCooldownCompleteNode(transform, "Evade"), new ActionLoggerNode(this, "너무 가까움! 후퇴", new EvadeNode(transform, "Backward")) }),
                new Sequence(new List<Node> { new IsNotInOptimalCombatRangeNode(transform, target, optimalCombatDistanceMin, optimalCombatDistanceMax), new ActionLoggerNode(this, "안전 거리 유지", new MaintainDistanceNode(transform, target, (optimalCombatDistanceMin + optimalCombatDistanceMax) / 2f, 0.2f)) })
            }),
            
            // --- 우선 순위 3: 제한적인 공격 (반격 및 기습) ---
            new Selector(new List<Node>
            {

                // ▼▼▼ 이 부분이 핵심 수정사항입니다 ▼▼▼
                // 3-2. 매우 안전할 때, 20% 확률로 기습적인 회전베기
                new Sequence(new List<Node>
                {
                    new IsInOptimalCombatRangeNode(transform, target, optimalCombatDistanceMin, optimalCombatDistanceMax),
                    new IsSafeToAttackNode(transform, targetAnimator, lowHealthThreshold),
                    new IsTargetNotAttackingOrDefendingNode(targetAnimator, normalAttackStateName, criticalAttackStateName, defendStateName),
                    new RandomChanceNode(0.05f), // 5% 확률
                    new IsCooldownCompleteNode(transform, "SpinAttack"),
                    new ActionLoggerNode(this, "기습(회전베기)", new SpinAttackNode(transform))
                }),
                // ▲▲▲ 여기까지가 수정된 로직입니다 ▲▲▲
                // 3-3. 제한적인 선제공격 (매우 안전하고 확률적으로만)
                new Sequence(new List<Node>
                {
                    new IsInOptimalCombatRangeNode(transform, target, optimalCombatDistanceMin, optimalCombatDistanceMax),
                    new IsSafeToAttackNode(transform, targetAnimator, lowHealthThreshold),
                    new RandomChanceNode(0.1f), // 10%의 낮은 확률로만 선제공격 시도
                    new IsCooldownCompleteNode(transform, "BasicAttack"),
                    new ActionLoggerNode(this, "견제 공격", new BasicAttackNode(transform)),

                    // ▼▼▼ 이 줄을 추가! ▼▼▼
                    new ActionLoggerNode(this, "공격 후 후퇴", new EvadeNode(transform, "Backward"))
                }),
            }),

            // --- 최후 순위: 대기 ---
            new ActionLoggerNode(this, "대기", new IdleNode(transform))
        });
    }

    // Update()와 PrintDebugStatus() 함수는 그대로 유지
    void Update()
    {
        if (root != null && !actuator.IsActionInProgress)
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