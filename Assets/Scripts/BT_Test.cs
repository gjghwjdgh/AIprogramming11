// 파일 이름: BT_Test.cs (로깅 기능이 제거된 테스트 버전)
using UnityEngine;
using System.Collections.Generic;

public class BT_Test : MonoBehaviour
{
    public Transform target;
    private Node root;

    void Start()
    {
        // ActionLoggerNode를 제거하고 순수한 행동 순서만 남깁니다.
        root = new Sequence(new List<Node>
        {
            // 1. 타겟에게 1.5m 거리까지 접근
            new MaintainDistanceNode(transform, target, 1.5f, 0.1f),
            
            // 2. 도착 후 2초간 대기
            new WaitNode(2.0f),

            // 3. 타겟에게서 5.0m 거리까지 후퇴
            new MaintainDistanceNode(transform, target, 5.0f, 0.1f),

            // 4. 후퇴 후 2초간 대기
            new WaitNode(2.0f)
        });
    }

    void Update()
    {
        if (root != null)
        {
            // 액션 잠금이 없으므로, BT_Test 스크립트 자체는 액추에이터 상태를 확인할 필요가 없습니다.
            // 각 노드가 RUNNING 상태를 올바르게 반환하며 스스로를 제어합니다.
            root.Evaluate();
        }
    }
}