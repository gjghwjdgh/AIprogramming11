using UnityEngine;

// 파일 이름: IsNotInOptimalCombatRangeNode.cs
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

        // 거리가 최소 범위보다 작거나, 최대 범위보다 크면 (즉, 최적 범위를 벗어났으면) 성공
        if (distance < minRange || distance > maxRange)
        {
            return NodeState.SUCCESS;
        }

        // 최적 범위 안에 있으면 실패
        return NodeState.FAILURE;
    }
}