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
    public float lowHealthThreshold = 30f;
    public float engageDistance = 12f;
    public float preferredEngagementDistanceMin = 1.0f; // 기존 필드
    public float preferredEngagementDistanceMax = 2.5f; // 기존 필드
    public float tooCloseDistance = 0.8f;
    public float strongAttackRange = 3.0f;

    // 상대방(플레이어)의 애니메이션 상태 이름
    [Header("Opponent Animation State Names")]
    public string normalAttackStateName = "공격0";
    public string criticalAttackStateName = "공격1";
    public string defendStateName = "방어";
    public string opponentIdleStateName = "기본자세"; // 기존 idleStateName 필드의 이름을 명확하게 변경 (상대방 것임을 표시)
                                                  // 또는 아래 인터페이스 구현에서 이 필드를 사용하도록 합니다.
    public string[] postAttackLagStateNames = { "기본자세" };
    public string[] wideOpenStateNames = { "Stunned", "KnockedDown" };


    // --- IPaladinParameters 인터페이스 멤버 구현 ---
    public float optimalCombatDistanceMin
    {
        get { return preferredEngagementDistanceMin; }
        // 만약 인터페이스에서 set도 요구한다면:
        // set { preferredEngagementDistanceMin = value; }
    }

    public float optimalCombatDistanceMax
    {
        get { return preferredEngagementDistanceMax; }
        // 만약 인터페이스에서 set도 요구한다면:
        // set { preferredEngagementDistanceMax = value; }
    }

    public string idleStateName // 인터페이스에서 요구하는 이름으로 프로퍼티 생성
    {
        // 이 idleStateName이 '상대방'의 idle 상태를 의미한다면:
        get { return opponentIdleStateName; }
        // 만약 인터페이스에서 set도 요구한다면:
        // set { opponentIdleStateName = value; }

        // 만약 이 idleStateName이 'AI 자신'의 idle 애니메이션 상태 이름을 의미하고,
        // 그것이 다른 값이어야 한다면 별도의 필드를 만들고 연결해야 합니다.
        // 예를 들어, public string myIdleStateName = "Paladin_Idle"; 와 같이 선언하고
        // get { return myIdleStateName; } 로 연결할 수 있습니다.
        // 현재 코드에서는 "Opponent Animation State Names" 섹션에 있으므로 상대방 것으로 간주하고 연결했습니다.
    }
    // --- 인터페이스 멤버 구현 끝 ---


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

            // --- 우선 순위 2: 적의 행동에 대한 즉각적인 반응 (방어/회피) ---
            new Selector(new List<Node>
            {
                // 2-1. 적의 치명적 공격 감지 시 회피
                new Sequence(new List<Node>
                {
                    new IsEnemyCritAttackDetectedNode(targetAnimator, criticalAttackStateName),
                    new IsCooldownCompleteNode(transform, "Evade"),
                    new IsHealthLowNode(transform, criticalHealthThreshold),
                    new EvadeNode(transform, "Backward")
                }),
                // 2-2. 적의 일반 공격 감지 시 방어 시도
                new Sequence(new List<Node>
                {
                    new IsEnemyAttackImminentNode(targetAnimator, normalAttackStateName),
                    new IsCooldownCompleteNode(transform, "Defend"),
                    new IsEnemyInDistanceNode(transform, target, preferredEngagementDistanceMax), // 인터페이스 구현 후 optimalCombatDistanceMax 사용 가능
                    new DefendNode(transform)
                })
            }),

            // --- 우선 순위 3: 적의 특정 상태를 이용한 공격 ---
            new Selector(new List<Node>
            {
                // 3-1. 적이 방금 방어를 풀었을 때(또는 방어 후 빈틈) 강력한 공격 시도
                new Sequence(new List<Node>
                {
                    // 인터페이스 구현 후 optimalCombatDistanceMax 또는 idleStateName 사용 가능 (여기서는 opponentIdleStateName 사용)
                    new IsEnemyJustFinishedDefendingNode(targetAnimator, defendStateName, opponentIdleStateName), // 기존 opponentIdleStateName 사용
                    new IsCooldownCompleteNode(transform, "SpinAttack"),
                    new IsEnemyInDistanceNode(transform, target, strongAttackRange),
                    new SpinAttackNode(transform)
                }),
                // 3-2. 적의 큰 빈틈(넘어짐, 스턴 등)에 강력한 공격
                new Sequence(new List<Node>
                {
                    new IsEnemyWideOpenNode(targetAnimator, wideOpenStateNames),
                    new IsCooldownCompleteNode(transform, "SpinAttack"),
                    new IsEnemyInDistanceNode(transform, target, strongAttackRange),
                    new SpinAttackNode(transform)
                })
            }),

            // --- 우선 순위 4: 주도적인 공격 (자신이 위험하지 않을 때) ---
            new Sequence(new List<Node>
            {
                new IsNotHealthLowNode(transform, lowHealthThreshold),
                new Selector(new List<Node>
                {
                    // 4-1. 적이 아무 행동이 없으면(Idle 상태) 랜덤 공격 시도
                    new Sequence(new List<Node>
                    {
                        // 인터페이스 구현 후 optimalCombatDistanceMin, optimalCombatDistanceMax, idleStateName 사용 가능
                        new IsEnemyIdleNode(targetAnimator, opponentIdleStateName), // 기존 opponentIdleStateName 사용
                        new IsInOptimalCombatRangeNode(transform, target, optimalCombatDistanceMin, optimalCombatDistanceMax), // 인터페이스 프로퍼티 사용
                        new IsSafeToAttackNode(transform, targetAnimator, lowHealthThreshold),
                        new RandomAttackNode(transform, new string[] {"BasicAttack", "KickAttack"})
                    }),

                    // 4-2. 일반적인 최적 거리에서의 기본 공격
                    new Sequence(new List<Node>
                    {
                        new IsInOptimalCombatRangeNode(transform, target, optimalCombatDistanceMin, optimalCombatDistanceMax), // 인터페이스 프로퍼티 사용
                        new IsSafeToAttackNode(transform, targetAnimator, lowHealthThreshold),
                        new IsCooldownCompleteNode(transform, "BasicAttack"),
                        new BasicAttackNode(transform)
                    }),

                    // 4-3. 회피를 이용한 공격적 접근 후 공격 (거리가 멀 때)
                    new Sequence(new List<Node>
                    {
                        new IsNotInOptimalCombatRangeNode(transform, target, optimalCombatDistanceMin, optimalCombatDistanceMax), // 인터페이스 프로퍼티 사용
                        new IsEnemyInDistanceNode(transform, target, engageDistance),
                        new IsCooldownCompleteNode(transform, "Evade"),
                        new EvadeNode(transform, "Forward"),
                        new IsCooldownCompleteNode(transform, "BasicAttack"),
                        new BasicAttackNode(transform)
                    })
                })
            }),

            // --- 우선 순위 5: 위치 선정 (위의 모든 공격/방어 조건이 맞지 않을 때) ---
            new Selector(new List<Node>
            {
                // 5-1. 적이 너무 가까울 때 거리 벌리기
                new Sequence(new List<Node>
                {
                    new IsEnemyInDistanceNode(transform, target, tooCloseDistance),
                    new Selector(new List<Node>
                    {
                        new Sequence(new List<Node> { new IsCooldownCompleteNode(transform, "KickAttack"), new KickAttackNode(transform) }),
                        new MoveAwayNode(transform, target)
                    })
                }),
                // 5-2. 기본 거리 유지 (선호하는 교전 거리로 이동)
                new Sequence(new List<Node>
                {
                    // 인터페이스 구현 후 optimalCombatDistanceMin, optimalCombatDistanceMax 사용 가능
                    new IsNotInOptimalCombatRangeNode(transform, target, optimalCombatDistanceMin, optimalCombatDistanceMax), // 인터페이스 프로퍼티 사용
                    new MaintainDistanceNode(transform, target, (optimalCombatDistanceMin + optimalCombatDistanceMax) / 2f) // 인터페이스 프로퍼티 사용
                })
            }),

            // --- 최후 순위: 대기 ---
            new IdleNode(transform) // 이 IdleNode가 AI 자신의 idle 애니메이션을 사용한다면, 해당 애니메이션 이름이 필요할 수 있습니다.
                                    // 만약 인터페이스의 idleStateName이 AI 자신의 것이라면 여기서 사용될 수 있습니다.
        });
    }

    void Update()
    {
        if (root != null)
        {
            root.Evaluate();
        }
    }
}