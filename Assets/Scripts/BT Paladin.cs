using UnityEngine;
using System.Collections.Generic;

public class BTPaladin : MonoBehaviour
{

    public Transform target;
    public Animator targetAnimator;

    private Node root; // 우리 행동 트리의 최상위 루트 노드

    void Start()
    {
        // 이 곳에서 BT 초안 문서를 보며 코드로 트리를 만듭니다.
        // transform은 이 스크립트가 붙어있는 'BT' 캐릭터의 Transform입니다.
        root = new Selector(new List<Node>
        {
            // 1. 사망 처리 시퀀스
            new Sequence(new List<Node>
            {
                new IsHealthLowNode(transform, 0f), // 자신의 체력 <= 0
                new DieNode(transform) // 사망 행동
            }),

            // 2. 수비 집중형 에이전트 로직 (예시)
            new Selector(new List<Node>
            {
                // 2-1. 치명적 공격 회피 시퀀스
                new Sequence(new List<Node>
                {
                    new IsEnemyCritAttackDetectedNode(targetAnimator, "Enemy_Critical_Strike"), // 적의 치명적 공격 감지
                    new IsCooldownCompleteNode(transform, "Evade"), // 회피 쿨타임 완료
                    new IsHealthLowNode(transform, 40f), // 자신의 체력 < 40%
                    new EvadeNode(transform) // 회피 행동
                }),

                // 2-2. 정밀 방어 시퀀스
                new Sequence(new List<Node>
                {
                    new IsEnemyAttackImminentNode(targetAnimator, "Enemy_Attack"), // 적의 공격 임박
                    new IsCooldownCompleteNode(transform, "Defend"), // 방어 쿨타임 완료
                    new IsEnemyInDistanceNode(transform, target, 2.0f), // 적과의 거리 '근접'
                    new DefendNode(transform) // 방어하기 행동
                }),

                // ... BT 초안의 나머지 로직들을 위와 같은 방식으로 계속 추가 ...
            })
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