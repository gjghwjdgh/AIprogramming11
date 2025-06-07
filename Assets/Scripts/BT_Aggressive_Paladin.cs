// 파일 이름: BT_Aggressive_Paladin.cs (최종 완성 버전)
using UnityEngine;
using System.Collections.Generic;

public class BT_Aggressive_Paladin : MonoBehaviour, IPaladinParameters
{
    // --- 기존의 모든 변수 선언은 그대로 둡니다 ---
    [HideInInspector]
    public string currentActionName = "None";
    [Header("Debugging")]
    public bool enableDebugLog = true;
    public float debugLogInterval = 0.5f;
    private float debugTimer;
    [Header("AI Target")]
    public Transform target;
    public Animator targetAnimator;
    private Node root;
    [Header("AI Behavior Parameters")]
    public float criticalHealthThreshold = 20f;
    public float lowHealthThreshold = 35f;
    public float engageDistance = 12f;
    public float optimalCombatDistanceMin = 1.0f;
    public float optimalCombatDistanceMax = 1.5f; // Inspector 스크린샷 값 반영
    public float tooCloseDistance = 0.8f;
    [Header("Attack Range Parameters")]
    public float basicAttackRange = 1.5f;     // Inspector 스크린샷 값 반영
    public float kickAttackRange = 1.0f;      // Inspector 스크린샷 값 반영
    public float spinAttackRange = 2.5f;
    [Header("Opponent Animation State Names")]
    public string normalAttackStateName = "Attack_Normal";
    public string criticalAttackStateName = "Attack_Critical";
    public string defendStateName = "Defend";
    public string opponentIdleStateName = "Idle_Battle";
    public string[] postAttackLagStateNames = { "Idle_Battle" };
    public string[] wideOpenStateNames = { "Stunned" };
    // IPaladinParameters 구현부는 그대로 둡니다.
    float IPaladinParameters.optimalCombatDistanceMin { get { return this.optimalCombatDistanceMin; } }
    float IPaladinParameters.optimalCombatDistanceMax { get { return this.optimalCombatDistanceMax; } }
    string IPaladinParameters.idleStateName { get { return this.opponentIdleStateName; } }

    private PaladinActuator actuator;
    private CooldownManager cooldownManager;

    void Awake()
    {
        cooldownManager = GetComponent<CooldownManager>();
        actuator = GetComponent<PaladinActuator>();
    }

    // BT_Aggressive_Paladin.cs의 Start() 함수를 아래 코드로 교체하세요.
    void Start()
    {
        root = new Selector(new List<Node>
        {
            // --- 최우선 순위 1: 생존 ---
            new Selector(new List<Node>
            {
                new Sequence(new List<Node> { new IsHealthLowNode(transform, 0f), new ActionLoggerNode(this, "사망", new DieNode(transform)) }),
                new Sequence(new List<Node> { new IsEnemyCritAttackDetectedNode(targetAnimator, criticalAttackStateName), new IsCooldownCompleteNode(transform, "Evade"), new ActionLoggerNode(this, "치명타 회피", new EvadeNode(transform, "Backward")) })
            }),

            // --- 우선 순위 2: 기회 포착 ---
            new Sequence(new List<Node>
            {
                new IsEnemyWideOpenNode(targetAnimator, wideOpenStateNames),
                new IsEnemyInDistanceNode(transform, target, spinAttackRange),
                new IsCooldownCompleteNode(transform, "SpinAttack"),
                new ActionLoggerNode(this, "필살기(스핀 공격)", new SpinAttackNode(transform))
            }),

            // --- 우선 순위 3: 방어 또는 차선책(회피) ---
            new Sequence(new List<Node>
            {
                new IsEnemyAttackImminentNode(targetAnimator, normalAttackStateName),
                new IsHealthHighEnoughToDefendNode(transform, lowHealthThreshold),
                new Selector(new List<Node> // 방어를 먼저 시도하고, 안되면 회피
                {
                    // 3-1. 방어 시도
                    new Sequence(new List<Node>
                    {
                        new IsCooldownCompleteNode(transform, "Defend"),
                        new ActionLoggerNode(this, "방어", new DefendNode(transform))
                    }),
                    // 3-2. 방어가 쿨타임이면 -> 긴급 회피 시도
                    new Sequence(new List<Node>
                    {
                        new IsCooldownCompleteNode(transform, "Evade"),
                        new ActionLoggerNode(this, "긴급 회피", new EvadeNode(transform, "Backward"))
                    })
                })
            }),

            // --- 우선 순위 4: 핵심 교전 로직 (거리별 판단) ---
            new Selector(new List<Node>
            {
                // 4-1. 너무 가까울 때: 걸어서 뒤로 물러나기
                new Sequence(new List<Node>
                {
                    new IsEnemyInDistanceNode(transform, target, tooCloseDistance),
                    new ActionLoggerNode(this, "전술적 후퇴", new MaintainDistanceNode(transform, target, (optimalCombatDistanceMin + optimalCombatDistanceMax) / 2f, 0.3f))
                }),

                // 4-3. 너무 멀 때: 타겟에게 접근
                new Sequence(new List<Node>
                {
                    new IsNotInOptimalCombatRangeNode(transform, target, optimalCombatDistanceMin, optimalCombatDistanceMax),
                    new ActionLoggerNode(this, "타겟에게 접근", new MaintainDistanceNode(transform, target, (optimalCombatDistanceMin + optimalCombatDistanceMax) / 2f, 0.3f))
                }),
                // 4-2. 최적 교전 거리일 때: 공격 또는 간 보기
                new Sequence(new List<Node>
                {
                    new IsInOptimalCombatRangeNode(transform, target, optimalCombatDistanceMin, optimalCombatDistanceMax),
                    new Selector(new List<Node>
                    {
                        // 상대가 방어 중이면 -> 가드 브레이크(발차기) 시도
                        new Sequence(new List<Node>
                        {
                            new IsEnemyDefendingNode(targetAnimator, defendStateName),
                            new IsEnemyInDistanceNode(transform, target, kickAttackRange),
                            new IsCooldownCompleteNode(transform, "KickAttack"),
                            new ActionLoggerNode(this, "가드 브레이크(발차기)", new KickAttackNode(transform))
                        }),
                        // 공격하기에 안전하면 -> 공격
                        new Sequence(new List<Node>
                        {
                            new IsSafeToAttackNode(transform, targetAnimator, lowHealthThreshold),
                            new Selector(new List<Node>
                            {
                                new Sequence(new List<Node> { new IsCooldownCompleteNode(transform, "KickAttack"), new ActionLoggerNode(this, "발차기", new KickAttackNode(transform)) }),
                                new Sequence(new List<Node> { new IsCooldownCompleteNode(transform, "BasicAttack"), new ActionLoggerNode(this, "기본 공격", new BasicAttackNode(transform)) })
                            })
                        }),
                        // 할 게 없으면 -> 간 보기
                        // new Sequence(new List<Node>
                        // {
                        //     new IsCooldownCompleteNode(transform, "Strafe"),
                        //     new ActionLoggerNode(this, "간 보기(스트레이프)", new StrafeNode(transform))
                        // })
                    })
                }),

            }),

            // --- 최후 순위: 대기 ---
            new ActionLoggerNode(this, "대기", new IdleNode(transform))
        });
    }
    
    void Update()
    {
        // 가장 단순하고 확실한 Update 로직
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