// 파일 이름: BT_Aggressive_Paladin.cs (최종 완성 버전)
using UnityEngine;
using System.Collections.Generic;
using System.Linq; // Concat이나 Distinct 같은 LINQ 메서드를 사용할 경우 필요

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
    public string criticalAttackStateName = "Critical_Attack"; // 오타 수정 (CriticalAttackStateName -> Critical_Attack)
    public string defendStateName = "Defend";
    public string defendEndedStateName = "Defend_Ended"; // 적이 방어를 마치거나 공격 후 돌아오는 일반적인 유휴 상태 (가정)
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

    void Start()
    {
        root = new Selector(new List<Node>
        {
            // --- 최우선 순위 1: 생존 ---
            new Selector(new List<Node>
            {
                new Sequence(new List<Node> { new IsHealthLowNode(transform, 0f), new ActionLoggerNode(this, "사망", new DieNode(transform)) }),
                new Sequence(new List<Node> { new IsEnemyCritAttackDetectedNode(targetAnimator, criticalAttackStateName), new IsCooldownCompleteNode(transform, "Evade"), new ActionLoggerNode(this, "치명타 회피", new EvadeNode(transform, "Backward", 0.5f, this)) }) // EvadeNode 지속시간 임의 지정
            }),

            // --- 우선 순위 2: 기회 포착 (회전베기) ---
            new Sequence(new List<Node>
            {
                // 적의 애니메이터가 '방어 종료 후' 상태임을 감지
                new IsEnemyDefendEndedNode(targetAnimator, defendEndedStateName), // 이곳에 정확한 방어 종료 후 애니메이션 상태 이름 지정
                new IsEnemyInDistanceNode(transform, target, spinAttackRange),
                new IsCooldownCompleteNode(transform, "SpinAttack"),
                // SpinAttackNode도 다른 공격 노드들처럼 코루틴 기반으로 수정해야 합니다. (임시 값)
                new ActionLoggerNode(this, "필살기(스핀 공격)", new SpinAttackNode(transform, PaladinActuator.AttackType.R_Attack, "SpinAttack", 3.0f, 1.5f, this)) 
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
                        // DefendNode에 현재 BT_Aggressive_Paladin 인스턴스(this)를 전달
                        new ActionLoggerNode(this, "방어", new DefendNode(transform, this)) 
                    }),
                    // 3-2. 방어가 쿨타임이면 -> 긴급 회피 시도
                    new Sequence(new List<Node>
                    {
                        new IsCooldownCompleteNode(transform, "Evade"),
                        // EvadeNode도 코루틴 기반으로 수정 필요 (임시 값)
                        new ActionLoggerNode(this, "긴급 회피", new EvadeNode(transform, "Backward", 0.5f, this)) 
                    })
                })
            }),

            // --- 우선 순위 4: 핵심 교전 로직 (거리별 판단) ---
            new Selector(new List<Node>
            {
                // 새로운 로직 1: 모든 공격 쿨타임이 완료된 경우 -> 공격 사거리 안쪽으로 접근
                new Sequence(new List<Node>
                {
                    new IsAllAttacksReadyNode(transform, "BasicAttack", "KickAttack", "SpinAttack"), // 확인할 공격 쿨타임 이름들
                    new IsNotInOptimalCombatRangeNode(transform, target, 0f, basicAttackRange * 0.9f), // basicAttackRange의 90% 안으로 접근 목표
                    new ActionLoggerNode(this, "공격 대기 접근", new MaintainDistanceNode(transform, target, basicAttackRange * 0.8f, 0.1f)) // 사거리보다 조금 더 안쪽으로
                }),

                // 새로운 로직 2: 모든 공격이 쿨타임 중인 경우 -> 거리 벌리기
                new Sequence(new List<Node>
                {
                    new IsAllAttacksOnCooldownNode(transform, "BasicAttack", "KickAttack", "SpinAttack"), // 확인할 공격 쿨타임 이름들
                    new IsEnemyInDistanceNode(transform, target, engageDistance), // engageDistance 안쪽에 있을 경우에만 발동
                    new ActionLoggerNode(this, "쿨타임 중 거리 벌리기", new MoveAwayNode(transform, target))
                }),

                // 4-1. 너무 가까울 때: 걸어서 뒤로 물러나기
                new Sequence(new List<Node>
                {
                    new IsEnemyInDistanceNode(transform, target, tooCloseDistance),
                    // MaintainDistanceNode로 변경: 너무 가까울 때도 일정 거리를 유지하도록 시도
                    new ActionLoggerNode(this, "전술적 후퇴", new MaintainDistanceNode(transform, target, tooCloseDistance + 0.1f, 0.1f)) 
                }),

                // 4-3. 너무 멀 때: 타겟에게 접근
                new Sequence(new List<Node>
                {
                    new IsNotInOptimalCombatRangeNode(transform, target, optimalCombatDistanceMin, optimalCombatDistanceMax),
                    new ActionLoggerNode(this, "타겟에게 접근", new MaintainDistanceNode(transform, target, (optimalCombatDistanceMin + optimalCombatDistanceMax) / 2f, 0.1f))
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
                            // KickAttackNode 생성자 수정
                            new ActionLoggerNode(this, "가드 브레이크(발차기)", new KickAttackNode(transform, PaladinActuator.AttackType.E_Kick, "KickAttack", 15.0f, 0.8f, this)) // 쿨타임과 애니메이션 지속 시간 설정
                        }),
                        // 공격하기에 안전하면 -> 공격
                        new Sequence(new List<Node>
                        {
                            new IsSafeToAttackNode(transform, targetAnimator, lowHealthThreshold),
                            new Selector(new List<Node>
                            {
                                // KickAttackNode 생성자 수정
                                new Sequence(new List<Node> { new IsCooldownCompleteNode(transform, "KickAttack"), new ActionLoggerNode(this, "발차기", new KickAttackNode(transform, PaladinActuator.AttackType.E_Kick, "KickAttack", 15.0f, 0.8f, this)) }),
                                // BasicAttackNode 생성자 수정
                                new Sequence(new List<Node> { new IsCooldownCompleteNode(transform, "BasicAttack"), new ActionLoggerNode(this, "기본 공격", new BasicAttackNode(transform, PaladinActuator.AttackType.Q_Attack, "BasicAttack", 6.0f, 0.5f, this)) }) // 쿨타임과 애니메이션 지속 시간 설정
                            })
                        }),
                        // 할 게 없으면 -> 간 보기 (주석 처리됨)
                        // new Sequence(new List<Node>
                        // {
                        //      new IsCooldownCompleteNode(transform, "Strafe"),
                        //      new ActionLoggerNode(this, "간 보기(스트레이프)", new StrafeNode(transform))
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
            AnimatorStateInfo currentTargetStateInfo = targetAnimator.GetCurrentAnimatorStateInfo(0);
            if (currentTargetStateInfo.IsName(normalAttackStateName)) targetState = "Normal Attack";
            else if (currentTargetStateInfo.IsName(criticalAttackStateName)) targetState = "Critical Attack";
            else if (currentTargetStateInfo.IsName(defendStateName)) targetState = "Defending";
            else if (currentTargetStateInfo.IsName(opponentIdleStateName)) targetState = "Idle";
            else if (currentTargetStateInfo.IsName(defendEndedStateName)) targetState = "Defend Ended";
            status += $"Target Animator State: {targetState}\n";
        }

        // 추가적인 조건 노드 상태 출력 (디버깅용)
        if (cooldownManager != null && targetAnimator != null)
        {
            // 새로운 인스턴스를 계속 생성하는 것은 비효율적이지만, 디버그용으로 사용
            bool isEnemyAttackImminent = new IsEnemyAttackImminentNode(targetAnimator, normalAttackStateName).Evaluate() == NodeState.SUCCESS;
            status += $"IsEnemyAttackImminent: {isEnemyAttackImminent}\n";
            bool isHealthHighEnough = new IsHealthHighEnoughToDefendNode(transform, lowHealthThreshold).Evaluate() == NodeState.SUCCESS;
            status += $"IsHealthHighEnoughToDefend: {isHealthHighEnough}\n";
            bool isDefendCooldownComplete = cooldownManager.IsCooldownFinished("Defend");
            status += $"IsDefendCooldownComplete: {isDefendCooldownComplete}\n";
            
            // IsAllAttacksReadyNode 및 IsAllAttacksOnCooldownNode는 별도의 파일에 정의되어야 합니다.
            // 아래 코드는 해당 파일들이 존재하고 올바르게 컴파일될 때만 작동합니다.
            // If the files are not recognized, you'll get CS0246 errors.
            bool isAllAttacksReady = new IsAllAttacksReadyNode(transform, "BasicAttack", "KickAttack", "SpinAttack").Evaluate() == NodeState.SUCCESS;
            status += $"IsAllAttacksReady: {isAllAttacksReady}\n";
            bool isAllAttacksOnCooldown = new IsAllAttacksOnCooldownNode(transform, "BasicAttack", "KickAttack", "SpinAttack").Evaluate() == NodeState.SUCCESS;
            status += $"IsAllAttacksOnCooldown: {isAllAttacksOnCooldown}\n";
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