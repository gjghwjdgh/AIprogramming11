// 파일 이름: IsNotInOptimalCombatRangeNode.cs (디버깅 버전)
using UnityEngine;

public class IsNotInOptimalCombatRangeNode : Node
{
    private Transform agentTransform;
    private Transform targetTransform;
    private float minRange;
    private float maxRange;

    public IsNotInOptimalCombatRangeNode(Transform agentTransform, Transform target, float minRange, float maxRange)
    {
        this.agentTransform = agentTransform;
        this.targetTransform = target;
        this.minRange = minRange;
        this.maxRange = maxRange;
    }

    public override NodeState Evaluate()
    {
        if (targetTransform == null) return NodeState.FAILURE;

        float distance = Vector3.Distance(agentTransform.position, targetTransform.position);

        // 거리가 최적 범위를 벗어났는지 확인
        if (distance < minRange || distance > maxRange)
        {
            return NodeState.SUCCESS;
        }
        else
        {
            return NodeState.FAILURE;
        }
    }
}