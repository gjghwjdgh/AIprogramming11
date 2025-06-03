using UnityEngine;
using System.Collections.Generic;

public class BT_Paladin : MonoBehaviour
{
    // AI가 적으로 인식할 대상 (인스펙터에서 할당)
    public Transform target;
    public Animator targetAnimator;

    // 행동 트리의 최상위 루트 노드
    private Node root;

    void Start()
    {
        // 이 곳에서 BT 초안 문서를 보며 코드로 트리를 만듭니다.
        root = new Selector(new List<Node>
        {
            // 최우선 순위 1: 사망 처리
            new Sequence(new List<Node>
            {
                new IsHealthLowNode(transform, 0f), // 자신의 체력 <= 0 이면
                new DieNode(transform)              // 사망 행동
            }),

            // 우선 순위 2: 생존 및 방어 관련 행동 (Selector)
            new Selector(new List<Node>
            {
                // 2-1. 치명적 공격 회피 (가장 먼저 체크)
                new Sequence(new List<Node>
                {
                    new IsEnemyCritAttackDetectedNode(targetAnimator, "Enemy_Critical_Strike"), // 적의 치명타 감지
                    new IsCooldownCompleteNode(transform, "Evade"),                              // 회피 쿨타임 완료
                    new IsHealthLowNode(transform, 40f),                                         // 자신의 체력 < 40%
                    new EvadeNode(transform)                                                     // 회피
                }),

                // 2-2. 일반 공격 방어
                new Sequence(new List<Node>
                {
                    new IsEnemyAttackImminentNode(targetAnimator, "Enemy_Attack"), // 적의 일반 공격 감지
                    new IsCooldownCompleteNode(transform, "Defend"),             // 방어 쿨타임 완료
                    new IsEnemyInDistanceNode(transform, target, 2.5f),            // 적이 근접 거리에 있음
                    new DefendNode(transform)                                    // 방어
                })
            }),

            // 우선 순위 3: 반격 및 기회 창출 (Selector)
            new Selector(new List<Node>
            {
                // 3-1. 방어 성공 직후 반격
                new Sequence(new List<Node>
                {
                    new DidDefendSucceedNode(transform),            // (새로운 조건) 방금 방어에 성공했다면
                    new IsEnemyInPostAttackLagNode(targetAnimator), // (새로운 조건) 적이 공격 후딜 상태라면
                    new Selector(new List<Node> // 반격 수단 선택
                    {
                        // 빠른 반격이 필요하면 일반 공격
                        new BasicAttackNode(transform),
                        // 경직을 유도하고 싶으면 발차기
                        new KickAttackNode(transform)
                    })
                })
            }),

            // 우선 순위 4: 거리 조절 및 신중한 공격 (Selector)
            new Selector(new List<Node>
            {
                // 4-1. 적이 너무 가까울 때 거리 벌리기
                new Sequence(new List<Node>
                {
                    new IsEnemyInDistanceNode(transform, target, 1.0f), // 적이 1m 이내로 너무 가까우면
                    new Selector(new List<Node> // 공간 확보 수단 선택
                    {
                        new KickAttackNode(transform), // 발차기로 밀어내거나
                        new MoveAwayNode(transform, target)  // (새로운 행동) 뒤로 물러서기
                    })
                }),

                // 4-2. 적의 큰 빈틈에 결정타 넣기
                new Sequence(new List<Node>
                {
                    new IsEnemyWideOpenNode(targetAnimator),            // (새로운 조건) 적이 큰 빈틈을 보이면
                    new IsCooldownCompleteNode(transform, "SpinAttack"),// 회전베기 쿨타임 완료
                    new SpinAttackNode(transform)                       // 회전베기 공격
                }),
                
                // 4-3. 안전할 때 짧은 단발 공격
                new Sequence(new List<Node>
                {
                    new IsEnemyShowingSmallOpeningNode(targetAnimator), // (새로운 조건) 적이 짧은 빈틈을 보이면
                    new IsCooldownCompleteNode(transform, "BasicAttack"),// 일반공격 쿨타임 완료
                    new BasicAttackNode(transform)                      // 일반공격
                })
            }),

            // 최후 순위 5: 기본 행동 (적에게 접근 또는 대기)
            new MoveNode(transform, target) // 위 모든 조건에 해당하지 않으면 일단 적에게 접근
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