using UnityEngine;

public class IsEnemyInDistanceNode : Node
{
    private Transform agentTransform;
    private Transform targetTransform;
    private float distanceThreshold; // 이 거리보다 가까우면 성공

    public IsEnemyInDistanceNode(Transform agentTransform, Transform targetTransform, float distanceThreshold)
    {
        this.agentTransform = agentTransform;
        this.targetTransform = targetTransform;
        this.distanceThreshold = distanceThreshold;
    }

    public override NodeState Evaluate()
    {
        if (targetTransform == null) return NodeState.FAILURE;

        // 에이전트와 타겟 사이의 실제 거리를 계산합니다.
        float distance = Vector3.Distance(agentTransform.position, targetTransform.position);

        // 실제 거리가 우리가 설정한 기준치보다 가깝거나 같으면 성공을 반환합니다.
        if (distance <= distanceThreshold)
        {
            return NodeState.SUCCESS;
        }

        return NodeState.FAILURE;
    }
}