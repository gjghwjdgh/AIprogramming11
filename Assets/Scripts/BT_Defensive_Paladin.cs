// 파일 이름: BT_Defensive_Paladin.cs
using UnityEngine;
using System.Collections.Generic;

public class BT_Defensive_Paladin : BT_Brain 
{
    // --- 변수 선언부 ---
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
    public float criticalHealthThreshold = 35f;
    public float lowHealthThreshold = 50f;
    public float engageDistance = 10f;
    public float optimalCombatDistanceMin_Inspector = 2.5f; // 더 먼 거리를 선호
    public float optimalCombatDistanceMax_Inspector = 4.0f;
    public float tooCloseDistance = 1.5f;

    [Header("Attack Range Parameters")]
    public float basicAttackRange = 2.0f;
    public float kickAttackRange = 1.8f;
    public float spinAttackRange = 2.5f;

    [Header("Opponent Animation State Names")]
    public string normalAttackStateName = "Attack_Normal"; // 실제 상대방 애니메이터에 맞게 수정 필요
    public string criticalAttackStateName = "Attack_Critical";
    public string defendStateName = "Defend";
    public string opponentIdleStateName = "Idle_Battle";
    public string[] postAttackLagStateNames = { "Idle_Battle" };
    public string[] wideOpenStateNames = { "Stunned" };

    // --- 부모 클래스(BT_Brain)의 추상 프로퍼티 구현 ---
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
        // 최상위 노드: 여러 행동 중 가장 우선순위 높은 것을 선택
        root = new Selector(new List<Node>
        {
            // --- 최우선 순위 1: 생존 본능 (죽음, 치명타 회피) ---
            new Selector(new List<Node>
            {
                new Sequence(new List<Node> { new IsHealthLowNode(transform, 0f), new ActionLoggerNode(this, "사망", new DieNode(transform)) }),
                // 적의 치명타 공격이 감지되고, 회피 쿨타임이 아닐 때 -> 뒤로 회피      
                new Sequence(new List<Node>
                {
                    new IsEnemyCritAttackDetectedNode(targetAnimator, criticalAttackStateName),
                    new ActionLoggerNode(this, "치명타 회피", new EvadeNode(transform, "Backward")) // EvadeNode 사용
                })
            }),
            
            // --- 우선 순위 2: 일반 공격에 대한 2초 방어 ---
            new Sequence(new List<Node>
            {
                new IsEnemyAttackImminentNode(targetAnimator, normalAttackStateName),
                new IsHealthHighEnoughToDefendNode(transform, lowHealthThreshold), // 방어에 충분한 체력인지 확인
                new IsCooldownCompleteNode(transform, "Defend"),
                new ActionLoggerNode(this, "2초 방어", new TimedDefendNode(transform, 1.0f))
            }),

            // --- 행동 잠금 확인: AI가 공격/회피 등 다른 행동 중일 때는 아래 로직을 실행하지 않음 ---
            new Sequence(new List<Node>
            {
                new Inverter(new IsActionInProgressNode(actuator)),

                // --- 우선 순위 3: 핵심 교전 로직 (방어 중이 아닐 때만 실행) ---
                new Selector(new List<Node>
                {
                    // 3-1. 반격 (수비형 AI의 핵심 공격 수단)
                    new Selector(new List<Node>
                    {
                        new Sequence(new List<Node> { new IsEnemyWideOpenNode(targetAnimator, wideOpenStateNames), new IsEnemyInDistanceNode(transform, target, spinAttackRange), new IsCooldownCompleteNode(transform, "SpinAttack"), new ActionLoggerNode(this, "반격(필살기)", new SpinAttackNode(transform)) }),
                        new Sequence(new List<Node> { new DidDefendSucceedNode(transform), new IsEnemyInPostAttackLagNode(targetAnimator, postAttackLagStateNames), new IsEnemyInDistanceNode(transform, target, kickAttackRange), new IsCooldownCompleteNode(transform, "KickAttack"), new ActionLoggerNode(this, "반격(발차기)", new KickAttackNode(transform)) })
                    }),
                    
                    // 3-2. 위치 선정
                    new Selector(new List<Node>
                    {
                         new Sequence(new List<Node> { new IsEnemyInDistanceNode(transform, target, tooCloseDistance), new IsCooldownCompleteNode(transform, "Reposition"), new ActionLoggerNode(this, "전술적 후퇴", new MoveAwayNode(transform, target)) }),
                         new Sequence(new List<Node> { new IsNotInOptimalCombatRangeNode(transform, target, optimalCombatDistanceMin, optimalCombatDistanceMax), new ActionLoggerNode(this, "안전 거리 유지", new MaintainDistanceNode(transform, target, (optimalCombatDistanceMin + optimalCombatDistanceMax) / 2f, 0.2f)) })
                    }),

                    // 3-3. 제한적인 선제공격 (매우 안전하고 확률적으로만)
                    new Sequence(new List<Node>
                    {
                        new IsInOptimalCombatRangeNode(transform, target, optimalCombatDistanceMin, optimalCombatDistanceMax),
                        new IsSafeToAttackNode(transform, targetAnimator, lowHealthThreshold),
                        new RandomChanceNode(0.1f), // 10%의 낮은 확률로만 선제공격 시도
                        new IsCooldownCompleteNode(transform, "BasicAttack"),
                        new ActionLoggerNode(this, "견제 공격", new BasicAttackNode(transform))
                    }),

                    // 3-4. 최후의 수단: 대기
                    new ActionLoggerNode(this, "대기", new IdleNode(transform))
                })
            })
        });
    }

    // Update()와 PrintDebugStatus() 함수는 공격형 AI와 동일하게 유지
    void Update()
    {
        if (root != null)
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