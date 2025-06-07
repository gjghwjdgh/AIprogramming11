using UnityEngine;
using System.Collections.Generic;

public class BT_Defensive_Paladin : MonoBehaviour, IPaladinParameters
{
    public Transform target;
    public Animator targetAnimator;

    private Node root;

    [Header("AI Behavior Parameters")]
    public float criticalHealthThreshold = 35f;
    public float lowHealthThreshold = 55f; // 수비형은 낮은 체력 기준을 더 높게 설정하여 신중함 증가
    public float engageDistance = 9f;
    public float tooCloseDistance = 1.8f; // 너무 가깝다고 판단하는 거리도 약간 더 넓게
    public float strongAttackCounterRange = 2.8f; // 반격용 강공격 사거리
    public float feintStepDistance = 0.8f; // 간 보기 스텝은 짧게
    public float afterAttackRepositionChance = 0.7f; // 공격 후 위치 변경 확률 (수비형은 더 높은 확률로 안전 확보)

    [Header("Attack Range Parameters")]
    public float basicAttackRange = 2.2f;
    public float kickAttackRange = 2.0f;
    public float spinAttackRange = 3.0f; // 반격 또는 매우 확실한 기회에만 사용

    [Header("Opponent Animation State Names")]
    public string normalAttackStateName = "공격0";
    public string criticalAttackStateName = "공격1";
    public string defendStateName = "방어";

    // IPaladinParameters 인터페이스 멤버 구현 (공격형과 동일하게 적용)
    [field: SerializeField]
    public float optimalCombatDistanceMin { get; private set; } = 2.8f; // 선호 최소 교전 거리 (더 멈)
    [field: SerializeField]
    public float optimalCombatDistanceMax { get; private set; } = 5.0f; // 선호 최대 교전 거리 (더 멈)
    [field: SerializeField]
    public string idleStateName { get; private set; } = "기본자세"; // 상대방의 idle 상태 이름

    public string[] postAttackLagStateNames = { "기본자세" };
    public string[] wideOpenStateNames = { "Stunned", "KnockedDown" };

    private float currentPreferredMinDistance; // 동적 거리 조절용
    private float currentPreferredMaxDistance;

    private CooldownManager cooldownManager;

    void Awake()
    {
        cooldownManager = GetComponent<CooldownManager>();
        if (cooldownManager == null)
        {
            Debug.LogError("BT_Defensive_Paladin: CooldownManager 컴포넌트가 없습니다!");
        }
    }

    void Start()
    {
        currentPreferredMinDistance = optimalCombatDistanceMin;
        currentPreferredMaxDistance = optimalCombatDistanceMax;

        root = new Selector(new List<Node>
        {
            // --- 최우선 순위 1: 즉각적인 생존 위협 대응 ---
            new Sequence(new List<Node> { new IsHealthLowNode(transform, 0f), new DieNode(transform) }),

            new Selector(new List<Node> // 치명적 공격 최우선 회피
            {
                new Sequence(new List<Node>
                {
                    new IsEnemyCritAttackDetectedNode(targetAnimator, criticalAttackStateName),
                    new IsCooldownCompleteNode(transform, "Evade"),
                    new EvadeNode(transform, "Backward") // 치명타는 무조건 피하려고 시도
                }),
            }),

            // --- 우선 순위 2: 일반적인 방어 및 안전 확보 ---
            new Selector(new List<Node>
            {
                new Sequence(new List<Node> // 일반 공격 방어
                {
                    new IsEnemyAttackImminentNode(targetAnimator, normalAttackStateName),
                    new IsCooldownCompleteNode(transform, "Defend"),
                    new IsEnemyInDistanceNode(transform, target, currentPreferredMaxDistance), // 현재 선호하는 최대 거리 안에서 방어
                    new DefendNode(transform)
                }),
                new Sequence(new List<Node> // 체력이 낮을 때 적극적으로 거리 벌리기
                {
                    new IsHealthLowNode(transform, lowHealthThreshold),
                    new IsEnemyInDistanceNode(transform, target, currentPreferredMinDistance + 1.0f), // 선호 최소 거리보다 가까우면
                    new IsCooldownCompleteNode(transform, "Evade"),
                    new EvadeNode(transform, "Backward")
                }),
                new Sequence(new List<Node> // 너무 가까우면 공간 확보
                {
                    new IsEnemyInDistanceNode(transform, target, tooCloseDistance),
                    new Selector(new List<Node>
                    {
                        new Sequence(new List<Node> { new IsCooldownCompleteNode(transform, "KickAttack"), new KickAttackNode(transform) }),
                        new Sequence(new List<Node> { new IsCooldownCompleteNode(transform, "Evade"), new EvadeNode(transform, "Backward") }),
                        new MoveAwayNode(transform, target) // 뒤로 물러나기
                    })
                })
            }),

            // --- 우선 순위 3: 안전이 확보된 후의 확실한 반격 및 제한적 공격 ---
            new Selector(new List<Node>
            {
                new Sequence(new List<Node> // 방금 방어를 풀었거나 방어 후 빈틈을 보이는 적에게 반격
                {
                    new IsEnemyJustFinishedDefendingNode(targetAnimator, defendStateName, idleStateName),
                    new IsCooldownCompleteNode(transform, "BasicAttack"), // 반격은 빠른 기본 공격 위주
                    new IsEnemyInDistanceNode(transform, target, basicAttackRange),
                    new BasicAttackNode(transform),
                    new RandomChanceNode(afterAttackRepositionChance), // 반격 후 안전하게 빠지기
                    new EvadeNode(transform, "Backward")
                }),
                new Sequence(new List<Node> // 방어 성공 직후의 짧은 반격
                {
                    new DidDefendSucceedNode(transform),
                    new IsEnemyInPostAttackLagNode(targetAnimator, postAttackLagStateNames),
                    new IsEnemyInDistanceNode(transform, target, kickAttackRange), // 발차기로 밀어내는 반격
                    new IsCooldownCompleteNode(transform, "KickAttack"),
                    new KickAttackNode(transform)
                }),
                new Sequence(new List<Node> // 적의 명확하고 큰 빈틈에만 신중한 강공격
                {
                    new IsEnemyWideOpenNode(targetAnimator, wideOpenStateNames),
                    new IsCooldownCompleteNode(transform, "SpinAttack"),
                    new IsEnemyInDistanceNode(transform, target, spinAttackRange),
                    new SpinAttackNode(transform)
                }),
                // (수비형은 잦은 견제 공격 자제)
            }),

            // --- 우선 순위 4: 신중한 "간 보기" 및 위치 선점 (공격/방어할 상황이 아닐 때) ---
            new Selector(new List<Node>
            {
                new Sequence(new List<Node> // 간 보기 (수비적인 간 보기 조건 필요)
                {
                    new IsInOptimalCombatRangeNode(transform, target, currentPreferredMinDistance, currentPreferredMaxDistance), // 적절한 거리에서만
                    new ShouldFeintNode(transform, target, "Defensive"), // 수비적인 간 보기 조건
                    new IsCooldownCompleteNode(transform, "FeintStep"),
                    new FeintStepNode(transform, Random.value > 0.5f ? "LeftStep" : "RightStep"), // 마지막 인자 삭제
                    new IdleNode(transform) // 간 본 후 바로 공격하지 않고 다시 상황 판단
                }),
                new Sequence(new List<Node> // 기본 거리 유지
                {
                    new IsNotInOptimalCombatRangeNode(transform, target, currentPreferredMinDistance, currentPreferredMaxDistance),
                    new MaintainDistanceNode(transform, target, (currentPreferredMinDistance + currentPreferredMaxDistance) / 2f, 0.8f) // 허용 오차를 조금 더 넉넉하게
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
            UpdatePreferredCombatDistance();
            root.Evaluate();
        }
    }

    void UpdatePreferredCombatDistance()
    {
        CharacterStatus myStatus = GetComponent<CharacterStatus>();
        if (myStatus != null && myStatus.currentHealth < lowHealthThreshold)
        {
            // 체력이 낮으면 더 안전한 (먼) 거리를 선호
            currentPreferredMinDistance = optimalCombatDistanceMin + 0.5f;
            currentPreferredMaxDistance = optimalCombatDistanceMax + 1.0f;
        }
        else if (targetAnimator != null &&
                 (targetAnimator.GetCurrentAnimatorStateInfo(0).IsName(normalAttackStateName) ||
                  targetAnimator.GetCurrentAnimatorStateInfo(0).IsName(criticalAttackStateName)))
        {
            // 상대가 공격 중이면 약간 더 뒤로 물러나 방어/회피 공간 확보
            currentPreferredMinDistance = optimalCombatDistanceMin + 0.2f;
            currentPreferredMaxDistance = optimalCombatDistanceMax + 0.5f;
        }
        else
        {
            // 평소에는 기본 설정된 교전 거리
            currentPreferredMinDistance = optimalCombatDistanceMin;
            currentPreferredMaxDistance = optimalCombatDistanceMax;
        }
    }
}