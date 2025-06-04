using UnityEngine;

public class IsHealthLowNode : Node
{
    private Transform agentTransform;
    private CharacterStatus status; // 체력 정보를 가진 스크립트
    private float healthThreshold;

    public IsHealthLowNode(Transform agentTransform, float healthThreshold)
    {
        this.agentTransform = agentTransform;
        this.status = agentTransform.GetComponent<CharacterStatus>();
        this.healthThreshold = healthThreshold;
    }

    public override NodeState Evaluate()
    {
        // 체력이 기준치 이하면 Success, 아니면 Failure 반환
        return status.currentHealth <= healthThreshold ? NodeState.SUCCESS : NodeState.FAILURE;
    }
}