using UnityEngine;

// 파일 이름: IsInOptimalCombatRangeNode.cs
public class IsInOptimalCombatRangeNode : Node
{
    private Transform agentTransform;
    private Transform targetTransform;
    private float minRange;
    private float maxRange;

    public IsInOptimalCombatRangeNode(Transform agentTransform, Transform target, float minRange, float maxRange)
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

        // 거리가 최소 범위와 최대 범위 '사이' (양 끝 포함)에 있으면 성공
        if (distance >= minRange && distance <= maxRange)
        {
            return NodeState.SUCCESS;
        }

        return NodeState.FAILURE;
    }
}