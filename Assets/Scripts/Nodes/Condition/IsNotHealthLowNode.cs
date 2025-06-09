using UnityEngine;

// 파일 이름: IsNotHealthLowNode.cs
public class IsNotHealthLowNode : Node
{
    private CharacterStatus status;
    private float healthThreshold;

    public IsNotHealthLowNode(Transform agentTransform, float healthThreshold)
    {
        this.status = agentTransform.GetComponent<CharacterStatus>();
        this.healthThreshold = healthThreshold;
    }

    public override NodeState Evaluate()
    {
        if (status == null) return NodeState.FAILURE;

        // 현재 체력이 기준치보다 '높다면' (즉, 낮지 않다면) 성공
        if (status.currentHealth > healthThreshold)
        {
            return NodeState.SUCCESS;
        }
        
        return NodeState.FAILURE;
    }
}