using UnityEngine;
using System.Collections.Generic;


public class BT_Defensive_Paladin : MonoBehaviour, IPaladinParameters
{
    public Transform target;
    public Animator targetAnimator;

    private Node root;

    [Header("AI Behavior Parameters")]
    public float criticalHealthThreshold = 20f;
    public float lowHealthThreshold = 35f;
    public float engageDistance = 12f;

    // IPaladinParameters 인터페이스 멤버 구현 (기존 필드를 프로퍼티로 변경)
    [field: SerializeField] // Unity 인스펙터에서 값을 할당할 수 있도록 함
    public float optimalCombatDistanceMin { get; private set; } = 1.0f;

    [field: SerializeField] // Unity 인스펙터에서 값을 할당할 수 있도록 함
    public float optimalCombatDistanceMax { get; private set; } = 3.0f;

    public float tooCloseDistance = 0.5f;
    public float strongAttackRange = 2.5f;
    public float feintStepDistance = 1.0f;
    public float afterAttackRepositionChance = 0.6f;

    [Header("Opponent Animation State Names")]
    public string normalAttackStateName = "공격0";
    public string criticalAttackStateName = "공격1";
    public string defendStateName = "방어";

    // IPaladinParameters 인터페이스 멤버 구현 (기존 필드를 프로퍼티로 변경)
    [field: SerializeField] // Unity 인스펙터에서 값을 할당할 수 있도록 함
    public string idleStateName { get; private set; } = "기본자세"; // 이 값은 상대방의 idle 상태 이름으로 가정

    public string[] postAttackLagStateNames = { "기본자세" };
    public string[] wideOpenStateNames = { "Stunned", "KnockedDown" };

    // 내부 상태 변수 (거리 동적 변경용)
    private float currentPreferredMaxDistance;

    private CooldownManager cooldownManager; // Awake에서 할당

    void Awake() // Start보다 먼저 호출
    {
        cooldownManager = GetComponent<CooldownManager>();
    }

    void Start()
    {
        currentPreferredMaxDistance = optimalCombatDistanceMax; // 프로퍼티 사용

        root = new Selector(new List<Node>
        {
            // --- 최우선 순위 1: 생존 ---
            new Sequence(new List<Node> { new IsHealthLowNode(transform, 0f), new DieNode(transform) }),

            // --- 우선 순위 2: 위협 대응 ---
            new Selector(new List<Node>
            {
                new Sequence(new List<Node> // 치명타 회피
                {
                    new IsEnemyCritAttackDetectedNode(targetAnimator, criticalAttackStateName),
                    new IsCooldownCompleteNode(transform, "Evade"),
                    new IsHealthLowNode(transform, criticalHealthThreshold + 10f),
                    new EvadeNode(transform, "Backward")
                }),
                new Sequence(new List<Node> // 일반 공격 방어
                {
                    new IsEnemyAttackImminentNode(targetAnimator, normalAttackStateName),
                    new IsHealthHighEnoughToDefendNode(transform, lowHealthThreshold),
                    new IsCooldownCompleteNode(transform, "Defend"),
                    new IsEnemyInDistanceNode(transform, target, optimalCombatDistanceMin + 1.0f), // 프로퍼티 사용
                    new DefendNode(transform)
                })
            }),

            // --- 우선 순위 3: 적극적인 공격 기회 창출 및 실행 ---
            new Selector(new List<Node>
            {
                // 3-1. 적의 큰 빈틈에 강력한 공격
                new Sequence(new List<Node>
                {
                    new IsEnemyWideOpenNode(targetAnimator, wideOpenStateNames),
                    new IsCooldownCompleteNode(transform, "SpinAttack"),
                    new IsEnemyInDistanceNode(transform, target, strongAttackRange),
                    new SpinAttackNode(transform),
                    new RandomChanceNode(afterAttackRepositionChance),
                    new EvadeNode(transform, Random.value > 0.5f ? "Left" : "Right")
                }),

                // 3-2. "간 보기" 후 공격 연계
                new Sequence(new List<Node>
                {
                    new ShouldFeintNode(transform, target, "Aggressive"),
                    new IsCooldownCompleteNode(transform, "FeintStep"),
                    new FeintStepNode(transform, "ForwardShort", feintStepDistance),
                    new IsEnemyInDistanceNode(transform, target, optimalCombatDistanceMax), // 프로퍼티 사용
                    new Selector(new List<Node>
                    {
                        new Sequence(new List<Node> { new IsCooldownCompleteNode(transform, "KickAttack"), new KickAttackNode(transform) }),
                        new Sequence(new List<Node> { new IsCooldownCompleteNode(transform, "BasicAttack"), new BasicAttackNode(transform) })
                    })
                }),

                // 3-3. 최적 거리에서의 주도적 기본 공격
                new Sequence(new List<Node>
                {
                    new IsInOptimalCombatRangeNode(transform, target, optimalCombatDistanceMin, currentPreferredMaxDistance), // optimalCombatDistanceMin 프로퍼티 사용
                    new IsSafeToAttackNode(transform, targetAnimator, lowHealthThreshold),
                    new IsCooldownCompleteNode(transform, "BasicAttack"),
                    new BasicAttackNode(transform),
                    new RandomChanceNode(afterAttackRepositionChance),
                    new EvadeNode(transform, "Backward")
                }),

                // 3-4. 발차기로 공격 시작 또는 압박
                new Sequence(new List<Node>
                {
                    new IsInOptimalCombatRangeNode(transform, target, optimalCombatDistanceMin, currentPreferredMaxDistance), // optimalCombatDistanceMin 프로퍼티 사용
                    new IsCooldownCompleteNode(transform, "KickAttack"),
                    new KickAttackNode(transform),
                    new RandomChanceNode(afterAttackRepositionChance * 0.5f),
                    new EvadeNode(transform, "Left")
                }),

                // 3-5. 회피를 이용한 공격적 접근 후 공격
                new Sequence(new List<Node>
                {
                    new IsNotInOptimalCombatRangeNode(transform, target, optimalCombatDistanceMin, currentPreferredMaxDistance), // optimalCombatDistanceMin 프로퍼티 사용
                    new IsEnemyInDistanceNode(transform, target, engageDistance),
                    new IsCooldownCompleteNode(transform, "Evade"),
                    new EvadeNode(transform, "Forward"),
                    new IsCooldownCompleteNode(transform, "BasicAttack"),
                    new BasicAttackNode(transform)
                })
            }),

            // --- 우선 순위 4: 동적인 거리 조절 및 위치 선정 ---
            new Selector(new List<Node>
            {
                new Sequence(new List<Node> // 너무 가까우면 거리 벌리기
                {
                    new IsEnemyInDistanceNode(transform, target, tooCloseDistance),
                    new Selector(new List<Node>
                    {
                        new Sequence(new List<Node> { new IsCooldownCompleteNode(transform, "KickAttack"), new KickAttackNode(transform) }),
                        new MoveAwayNode(transform, target)
                    })
                }),
                new Sequence(new List<Node> // 기본 거리 유지
                {
                    new IsNotInOptimalCombatRangeNode(transform, target, optimalCombatDistanceMin, currentPreferredMaxDistance), // optimalCombatDistanceMin 프로퍼티 사용
                    new MaintainDistanceNode(transform, target, (optimalCombatDistanceMin + currentPreferredMaxDistance) / 2f, 0.5f) // optimalCombatDistanceMin 프로퍼티 사용
                })
            }),

            // --- 최후 순위: 대기 ---
            new IdleNode(transform) // 이 IdleNode가 AI 자신의 idle 상태를 사용한다면,
                                  // IPaladinParameters.idleStateName이 AI 자신의 것을 의미하는지 확인 필요.
                                  // 현재는 Opponent Animation State Names의 idleStateName을 인터페이스에 연결했으므로,
                                  // 만약 이 노드가 AI 자신의 idle 애니메이션을 재생해야 한다면, 별도의 설정이 필요할 수 있습니다.
        });
    }

    void Update()
    {
        if (root != null)
        {
            UpdatePreferredCombatDistance();
            root.Evaluate();
        }
    }

    void UpdatePreferredCombatDistance()
    {
        CharacterStatus myStatus = GetComponent<CharacterStatus>();
        if (myStatus != null && myStatus.currentHealth < lowHealthThreshold) // myStatus null 체크 추가
        {
            currentPreferredMaxDistance = optimalCombatDistanceMax + 1.0f; // 프로퍼티 사용
        }
        else if (cooldownManager != null && cooldownManager.IsCooldownFinished("SpinAttack"))
        {
            currentPreferredMaxDistance = optimalCombatDistanceMax; // 프로퍼티 사용
        }
        else
        {
            currentPreferredMaxDistance = optimalCombatDistanceMax; // 프로퍼티 사용
        }
    }
}