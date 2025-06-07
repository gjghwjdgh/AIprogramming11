using UnityEngine;
using System.Collections.Generic;

public class BT_Paladin : MonoBehaviour
{
    // AI가 적으로 인식할 대상 (인스펙터에서 할당)
    public Transform target;
    public Animator targetAnimator;

    // 행동 트리의 최상위 루트 노드
    private Node root;

    // AI의 성향 및 판단 기준 값 (인스펙터에서 조절 가능하도록 public으로 선언)
    [Header("AI Behavior Parameters")]
    public float criticalHealthThreshold = 25f; // 치명적 상황으로 판단하는 체력 (예: 25%)
    public float lowHealthThreshold = 40f;      // 체력이 낮다고 판단하는 기준 (예: 40%)
    public float engageDistance = 10f;          // 적과 교전을 시작하려는 최대 거리 (현재 코드에서는 직접 사용되지 않으나, 향후 확장 가능)
    public float optimalCombatDistanceMin = 2.0f; // 선호하는 최소 교전 거리
    public float optimalCombatDistanceMax = 4.0f; // 선호하는 최대 교전 거리
    public float tooCloseDistance = 1.0f;       // 너무 가깝다고 판단하는 거리

    // 상대방(플레이어)의 애니메이션 상태 이름을 여기에 정의 (인스펙터에서 수정 가능)
    [Header("Opponent Animation State Names")]
    public string normalAttackStateName = "공격0"; // 플레이어의 일반 공격 애니메이션 상태 이름
    public string criticalAttackStateName = "공격1"; // 플레이어의 치명타/강공격 애니메이션 상태 이름 (예시)
    // 플레이어의 공격 후딜레이나 빈틈 상태 이름들을 배열로 정의
    public string[] postAttackLagStateNames = { "기본자세" }; // 공격 직후 '기본자세'로 돌아오는 것을 짧은 빈틈으로 가정
    public string[] wideOpenStateNames = { "Stunned", "KnockedDown" }; // 플레이어가 넘어지거나 스턴 상태일 때 (플레이어 애니메이터에 이런 상태가 있어야 함)

    void Start()
    {
        // 행동 트리의 루트를 Selector로 설정하여, 여러 행동 중 하나를 선택하게 함
        root = new Selector(new List<Node>
        {
            // --- 최우선 순위: 생존 ---
            new Sequence(new List<Node> // 1. 사망 처리 (가장 먼저 체크)
            {
                new IsHealthLowNode(transform, 0f), // 자신의 체력이 0 이하면
                new DieNode(transform)              // 사망 행동
            }),

            new Selector(new List<Node> // 2. 치명적인 위협 대응
            {
                // 2-1. 치명적 공격 회피 (체력이 매우 낮을 때 더 적극적으로 회피)
                new Sequence(new List<Node>
                {
                    new IsEnemyCritAttackDetectedNode(targetAnimator, criticalAttackStateName),
                    new IsCooldownCompleteNode(transform, "Evade"),
                    new IsHealthLowNode(transform, criticalHealthThreshold),
                    new EvadeNode(transform, "Backward") // 예시: 뒤로 회피
                }),
                // 2-2. 일반 공격에 대한 정밀 방어
                new Sequence(new List<Node>
                {
                    new IsEnemyAttackImminentNode(targetAnimator, normalAttackStateName),
                    new IsCooldownCompleteNode(transform, "Defend"),
                    new IsEnemyInDistanceNode(transform, target, optimalCombatDistanceMin + 0.5f), // 방어 유효 거리
                    new DefendNode(transform)
                })
            }),

            // --- 우선 순위: 기회 포착 및 반격 ---
            new Selector(new List<Node>
            {
                // 3-1. 방어 성공 직후 즉각적인 반격
                new Sequence(new List<Node>
                {
                    new DidDefendSucceedNode(transform),
                    new IsEnemyInPostAttackLagNode(targetAnimator, postAttackLagStateNames),
                    new IsEnemyInDistanceNode(transform, target, optimalCombatDistanceMin + 1.0f), // 반격 유효 거리
                    new Selector(new List<Node> // 상황에 맞는 반격 수단 선택
                    {
                        new Sequence(new List<Node> { new IsCooldownCompleteNode(transform, "KickAttack"), new KickAttackNode(transform) }),
                        new Sequence(new List<Node> { new IsCooldownCompleteNode(transform, "BasicAttack"), new BasicAttackNode(transform) })
                    })
                }),
                // 3-2. 적의 큰 빈틈을 노린 강력한 공격
                new Sequence(new List<Node>
                {
                    new IsEnemyWideOpenNode(targetAnimator, wideOpenStateNames),
                    new IsCooldownCompleteNode(transform, "SpinAttack"),
                    new IsEnemyInDistanceNode(transform, target, optimalCombatDistanceMax), // 회전베기 유효 사거리
                    new SpinAttackNode(transform)
                })
            }),

            // --- 우선 순위: 주도적인 전투 운영 및 위치 선정 ---
            new Selector(new List<Node>
            {
                // 4-1. 적이 너무 가까울 때 거리 벌리기 (생존 및 공간 확보)
                new Sequence(new List<Node>
                {
                    new IsEnemyInDistanceNode(transform, target, tooCloseDistance),
                    new Selector(new List<Node> // 공간 확보 수단 선택
                    {
                        new Sequence(new List<Node> { new IsCooldownCompleteNode(transform, "KickAttack"), new KickAttackNode(transform) }),
                        new Sequence(new List<Node> { new IsCooldownCompleteNode(transform, "Evade"), new EvadeNode(transform, "Backward") }),
                        new MoveAwayNode(transform, target) // 뒤로 물러나기
                    })
                }),

                // 4-2. 최적 거리에서의 주도적 교전 시도 (이전에 "안전하고 유리한 상황에서 짧은 견제 공격" 부분에 통합)
                new Sequence(new List<Node>
                {
                    new IsInOptimalCombatRangeNode(transform, target, optimalCombatDistanceMin, optimalCombatDistanceMax), // 현재 최적 교전 거리 내
                    new IsSafeToAttackNode(transform, targetAnimator, lowHealthThreshold), // 공격해도 안전한 상황
                    new IsCooldownCompleteNode(transform, "BasicAttack"),                 // 기본 공격 쿨타임 완료
                    // (추가) 적이 명확한 공격/방어 자세가 아닐 때 등의 조건 추가 가능
                    new BasicAttackNode(transform)                                         // 기본 공격 시도
                }),

                // 4-3. 신중한 거리 유지 및 위치 선점 (공격/방어할 상황이 아니고, 최적 거리도 아닐 때)
                new Sequence(new List<Node>
                {
                    new IsNotInOptimalCombatRangeNode(transform, target, optimalCombatDistanceMin, optimalCombatDistanceMax), // 최적 교전 범위를 벗어났을 때
                    new MaintainDistanceNode(transform, target, (optimalCombatDistanceMin + optimalCombatDistanceMax) / 2f, 0.1f) // 중간값으로 거리 유지 시도
                })
            }),

            // --- 최후 순위: 기본 대기 상태 ---
            // 위 모든 조건에 해당하지 않으면, 적을 주시하며 기본 대기 자세를 취함
            new IdleNode(transform)
        });
    }

    void Update()
    {
        // 매 프레임마다 트리의 루트부터 평가를 시작합니다.
        if (root != null)
        {
            root.Evaluate();
        }
    }
}