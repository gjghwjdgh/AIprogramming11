using UnityEngine;
using System.Collections.Generic;

public class BT_Aggressive_Paladin : MonoBehaviour, IPaladinParameters
{
    // AI가 적으로 인식할 대상 (인스펙터에서 할당)
    public Transform target;
    public Animator targetAnimator;

    // 행동 트리의 최상위 루트 노드
    private Node root;

    // AI의 성향 및 판단 기준 값
    [Header("AI Behavior Parameters")]
    public float criticalHealthThreshold = 20f;
    public float lowHealthThreshold = 35f;
    public float engageDistance = 12f;          // 이 거리 안으로 들어오면 교전 고려 시작
    public float optimalCombatDistanceMin = 1.0f; // 선호하는 최소 교전 거리 (공격 시작 가능 거리)
    public float optimalCombatDistanceMax = 3.0f; // 선호하는 최대 교전 거리 (이 이상 벌어지면 접근 시도)
    public float tooCloseDistance = 0.8f;         // 너무 가까워서 뒤로 물러나야 하는 거리

    [Header("Attack Range Parameters")]
    public float basicAttackRange = 2.0f;     // 기본 공격 유효 사거리
    public float kickAttackRange = 1.8f;      // 발차기 공격 유효 사거리
    public float spinAttackRange = 2.5f;      // 회전베기 공격 유효 사거리

    // 상대방(플레이어)의 애니메이션 상태 이름
    [Header("Opponent Animation State Names")]
    public string normalAttackStateName = "공격0";
    public string criticalAttackStateName = "공격1";
    public string defendStateName = "방어";
    public string opponentIdleStateName = "기본자세"; // 인터페이스 호환용
    public string[] postAttackLagStateNames = { "기본자세" };
    public string[] wideOpenStateNames = { "Stunned", "KnockedDown" };


    // --- IPaladinParameters 인터페이스 멤버 구현 ---
    // 이 프로퍼티들은 ShouldFeintNode 등에서 공통적으로 사용될 수 있습니다.
    // 현재 BT_Aggressive_Paladin 내부 로직에서는 위에 선언된 public 변수들을 직접 사용합니다.
    // 만약 인터페이스를 통해 접근해야 하는 노드가 있다면, 이 프로퍼티들이 해당 public 변수를 반환하도록 합니다.
    float IPaladinParameters.optimalCombatDistanceMin { get { return this.optimalCombatDistanceMin; } }
    float IPaladinParameters.optimalCombatDistanceMax { get { return this.optimalCombatDistanceMax; } }
    string IPaladinParameters.idleStateName { get { return this.opponentIdleStateName; } }
    // --- 인터페이스 멤버 구현 끝 ---


    private CooldownManager cooldownManager; // 최적화를 위해 Start에서 한 번만 가져옴

    void Awake()
    {
        cooldownManager = GetComponent<CooldownManager>();
        if (cooldownManager == null)
        {
            Debug.LogError("BT_Aggressive_Paladin: CooldownManager 컴포넌트가 없습니다!");
        }
    }

    void Start()
    {
        root = new Selector(new List<Node>
        {
            // --- 최우선 순위 1: 생존 ---
            new Sequence(new List<Node> // 사망 처리
            {
                new IsHealthLowNode(transform, 0f),
                new DieNode(transform)
            }),

            // --- 우선 순위 2: 즉각적인 위협 대응 (회피/방어) ---
            new Selector(new List<Node>
            {
                // 2-1. 치명적 공격 회피
                new Sequence(new List<Node>
                {
                    new IsEnemyCritAttackDetectedNode(targetAnimator, criticalAttackStateName),
                    new IsCooldownCompleteNode(transform, "Evade"),
                    new IsHealthLowNode(transform, criticalHealthThreshold + 10f), // 공격형도 치명타는 체력이 좀 있어도 피하려고 함
                    new EvadeNode(transform, "Backward")
                }),
                // 2-2. 일반 공격에 대한 방어 (공격형은 방어 빈도 낮음, 체력 조건 추가)
                new Sequence(new List<Node>
                {
                    new IsEnemyAttackImminentNode(targetAnimator, normalAttackStateName),
                    new IsHealthHighEnoughToDefendNode(transform, lowHealthThreshold), // 체력이 너무 낮으면 방어보다 회피 고려
                    new IsCooldownCompleteNode(transform, "Defend"),
                    new IsEnemyInDistanceNode(transform, target, optimalCombatDistanceMax), // 방어 가능한 최대 거리
                    new DefendNode(transform)
                })
            }),

            // --- 우선 순위 3: 공격 또는 거리 조절 (상황에 따라) ---
            new Selector(new List<Node>
            {
                // 3-1. (최우선 이동) 최적 교전 범위를 벗어났다면 먼저 거리부터 조절!
                new Sequence(new List<Node>
                {
                    new IsNotInOptimalCombatRangeNode(transform, target, optimalCombatDistanceMin, optimalCombatDistanceMax),
                    new IsEnemyInDistanceNode(transform, target, engageDistance + 2f), // 너무 멀리 가진 않았는지 확인 (무한 추적 방지)
                    new MaintainDistanceNode(transform, target, (optimalCombatDistanceMin + optimalCombatDistanceMax) / 2f, 0.5f)
                }),

                // 3-2. 적의 큰 빈틈에 강력한 공격 (사거리 체크 포함)
                new Sequence(new List<Node>
                {
                    new IsEnemyInDistanceNode(transform, target, spinAttackRange), // 회전베기 사거리 확인
                    new IsEnemyWideOpenNode(targetAnimator, wideOpenStateNames),
                    new IsCooldownCompleteNode(transform, "SpinAttack"),
                    new SpinAttackNode(transform),
                    new RandomChanceNode(0.3f), // 30% 확률로 공격 후 빠지기
                    new EvadeNode(transform, "Backward")
                }),

                // 3-3. 최적 거리에서의 주도적 기본 공격 (사거리 체크 포함)
                new Sequence(new List<Node>
                {
                    new IsEnemyInDistanceNode(transform, target, basicAttackRange), // 기본 공격 사거리 확인
                    // IsInOptimalCombatRangeNode는 위에서 MaintainDistance로 커버되므로, 여기서는 실제 공격 사거리만 중요
                    new IsSafeToAttackNode(transform, targetAnimator, lowHealthThreshold),
                    new IsCooldownCompleteNode(transform, "BasicAttack"),
                    new BasicAttackNode(transform),
                    new RandomChanceNode(0.5f), // 50% 확률로 공격 후 빠지기
                    new EvadeNode(transform, "Backward")
                }),

                // 3-4. 발차기로 공격 시작 또는 압박 (사거리 체크 포함)
                new Sequence(new List<Node>
                {
                    new IsEnemyInDistanceNode(transform, target, kickAttackRange), // 발차기 사거리 확인
                    new IsSafeToAttackNode(transform, targetAnimator, lowHealthThreshold), // 발차기도 안전할 때
                    new IsCooldownCompleteNode(transform, "KickAttack"),
                    new KickAttackNode(transform)
                }),
                
                // (추가) 3-5. "간 보기" 후 공격 연계 (최적 거리 안에서만)
                new Sequence(new List<Node>
                {
                    new IsInOptimalCombatRangeNode(transform, target, optimalCombatDistanceMin, optimalCombatDistanceMax),
                    new ShouldFeintNode(transform, target, "Aggressive"), 
                    new IsCooldownCompleteNode(transform, "FeintStep"),
                    new FeintStepNode(transform, "ForwardShort", 1.0f), 
                    new IsEnemyInDistanceNode(transform, target, basicAttackRange), // 간 보기 후 공격 사거리 확인
                    new IsCooldownCompleteNode(transform, "BasicAttack"),
                    new BasicAttackNode(transform)
                }),
                
                // (기존) 3-6. 회피를 이용한 공격적 접근 후 공격 (거리가 멀고, 위의 MaintainDistance로도 해결 안될 때)
                new Sequence(new List<Node>
                {
                    // 이 로직은 MaintainDistanceNode와 역할이 겹칠 수 있어 우선순위나 조건을 더 명확히 해야 함
                    // 예를 들어, 적이 특정 행동을 할 때만 접근하는 식으로
                    new IsNotInOptimalCombatRangeNode(transform, target, optimalCombatDistanceMax, engageDistance), // 최적보다 멀지만 교전은 할만할 때
                    new IsTargetNotAttackingOrDefendingNode(targetAnimator, normalAttackStateName, criticalAttackStateName, defendStateName), // (새로운 조건) 상대가 공격/방어 중이 아닐 때만
                    new IsCooldownCompleteNode(transform, "Evade"),
                    new EvadeNode(transform, "Forward"),
                    new IsEnemyInDistanceNode(transform, target, basicAttackRange),
                    new IsCooldownCompleteNode(transform, "BasicAttack"),
                    new BasicAttackNode(transform)
                })
            }),

            // --- 우선 순위 4: 방어 성공 후 반격 (이전보다 우선순위 약간 낮춤, 공격 시도 후 고려) ---
            new Sequence(new List<Node> 
            {
                new DidDefendSucceedNode(transform),
                new IsEnemyInPostAttackLagNode(targetAnimator, postAttackLagStateNames),
                new IsEnemyInDistanceNode(transform, target, basicAttackRange + 0.5f), // 반격은 조금 더 가까이서
                new Selector(new List<Node>
                {
                    new Sequence(new List<Node> { new IsCooldownCompleteNode(transform, "BasicAttack"), new BasicAttackNode(transform) }),
                    new Sequence(new List<Node> { new IsCooldownCompleteNode(transform, "KickAttack"), new KickAttackNode(transform) })
                })
            }),
            
            // --- 우선 순위 5: 너무 가까울 때 거리 벌리기 ---
            new Sequence(new List<Node> 
            {
                new IsEnemyInDistanceNode(transform, target, tooCloseDistance),
                new Selector(new List<Node>
                {
                    new Sequence(new List<Node> { new IsCooldownCompleteNode(transform, "KickAttack"), new KickAttackNode(transform) }),
                    new Sequence(new List<Node> { new IsCooldownCompleteNode(transform, "Evade"), new EvadeNode(transform, "Backward") }),
                    new MoveAwayNode(transform, target)
                })
            }),

            // --- 최후 순위: 기본 대기 상태 ---
            new IdleNode(transform)
        });
    }

    void Update()
    {
        if (root != null)
        {
            root.Evaluate();
        }
        // 동적 거리 조절 로직은 필요하다면 이전 답변처럼 추가 가능
    }
}