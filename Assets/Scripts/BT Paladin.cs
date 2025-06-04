using UnityEngine;
using System.Collections.Generic;

public class BT_Paladin : MonoBehaviour
{
    public Transform target;
    public Animator targetAnimator;

    [Header("AI Behavior Parameters")]
    public float criticalHealthThreshold = 25f;
    public float lowHealthThreshold = 40f;
    public float engageDistance = 10f;
    public float optimalCombatDistanceMin = 2.0f;
    public float optimalCombatDistanceMax = 4.0f;
    public float tooCloseDistance = 1.0f;

    // ▼▼▼ 상대방(플레이어)의 애니메이션 상태 이름을 여기에 정의합니다 (인스펙터에서 수정 가능하게 public으로) ▼▼▼
    [Header("Opponent Animation State Names")]
    public string normalAttackStateName = "공격0"; // 플레이어의 일반 공격 애니메이션 상태 이름
    public string criticalAttackStateName = "공격1"; // 플레이어의 치명타/강공격 애니메이션 상태 이름 (예시)
    // 플레이어의 공격 후딜레이나 빈틈 상태 이름들을 배열로 정의
    public string[] postAttackLagStateNames = { "기본자세" }; // 공격 직후 '기본자세'로 돌아오는 것을 짧은 빈틈으로 가정
    public string[] wideOpenStateNames = { "Stunned", "KnockedDown" }; // 플레이어가 넘어지거나 스턴 상태일 때 (플레이어 애니메이터에 이런 상태가 있어야 함)


    private Node root;

    void Start()
    {
        root = new Selector(new List<Node>
        {
            // --- 최우선 순위: 생존 ---
            new Sequence(new List<Node>
            {
                new IsHealthLowNode(transform, 0f),
                new DieNode(transform)
            }),

            new Selector(new List<Node>
            {
                new Sequence(new List<Node>
                {
                    // 플레이어의 'criticalAttackStateName' 상태를 치명타로 감지
                    new IsEnemyCritAttackDetectedNode(targetAnimator, criticalAttackStateName),
                    new IsCooldownCompleteNode(transform, "Evade"),
                    new IsHealthLowNode(transform, criticalHealthThreshold),
                    new EvadeNode(transform, "Backward")
                }),
                new Sequence(new List<Node>
                {
                    // 플레이어의 'normalAttackStateName' 상태를 일반 공격으로 감지
                    new IsEnemyAttackImminentNode(targetAnimator, normalAttackStateName),
                    new IsCooldownCompleteNode(transform, "Defend"),
                    new IsEnemyInDistanceNode(transform, target, optimalCombatDistanceMin + 0.5f),
                    new DefendNode(transform)
                })
            }),

            // --- 우선 순위: 기회 포착 및 반격 ---
            new Selector(new List<Node>
            {
                new Sequence(new List<Node>
                {
                    new DidDefendSucceedNode(transform),
                    // 플레이어가 'postAttackLagStateNames' 중 하나의 상태일 때를 후딜레이로 감지
                    new IsEnemyInPostAttackLagNode(targetAnimator, postAttackLagStateNames),
                    new IsEnemyInDistanceNode(transform, target, optimalCombatDistanceMin + 1.0f),
                    new Selector(new List<Node>
                    {
                        new Sequence(new List<Node> { new IsCooldownCompleteNode(transform, "KickAttack"), new KickAttackNode(transform) }),
                        new Sequence(new List<Node> { new IsCooldownCompleteNode(transform, "BasicAttack"), new BasicAttackNode(transform) })
                    })
                }),
                new Sequence(new List<Node>
                {
                    // 플레이어가 'wideOpenStateNames' 중 하나의 상태일 때를 큰 빈틈으로 감지
                    new IsEnemyWideOpenNode(targetAnimator, wideOpenStateNames),
                    new IsCooldownCompleteNode(transform, "SpinAttack"),
                    new IsEnemyInDistanceNode(transform, target, optimalCombatDistanceMax),
                    new SpinAttackNode(transform)
                })
            }),

            // --- 우선 순위: 주도적인 전투 운영 및 위치 선정 ---
            new Selector(new List<Node>
            {
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
                new Sequence(new List<Node>
                {
                    // 플레이어가 'postAttackLagStateNames' 중 하나를 작은 빈틈으로 간주
                    new IsEnemyShowingSmallOpeningNode(targetAnimator, postAttackLagStateNames),
                    new IsCooldownCompleteNode(transform, "BasicAttack"),
                    new IsSafeToAttackNode(transform, targetAnimator, lowHealthThreshold),
                    new IsEnemyInDistanceNode(transform, target, optimalCombatDistanceMax),
                    new BasicAttackNode(transform)
                }),
                new Sequence(new List<Node>
                {
                    new IsNotInOptimalCombatRangeNode(transform, target, optimalCombatDistanceMin, optimalCombatDistanceMax),
                    new MaintainDistanceNode(transform, target, (optimalCombatDistanceMin + optimalCombatDistanceMax) / 2)
                })
            }),
            new IdleNode(transform)
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