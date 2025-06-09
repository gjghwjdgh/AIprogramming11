using UnityEngine;

// 파일 이름: IsHealthHighEnoughToDefendNode.cs
public class IsHealthHighEnoughToDefendNode : Node
{
    private CharacterStatus status;
    private float healthThreshold; // 이 체력 이상이어야 방어 고려

    public IsHealthHighEnoughToDefendNode(Transform agentTransform, float healthThreshold)
    {
        this.status = agentTransform.GetComponent<CharacterStatus>();
        this.healthThreshold = healthThreshold;
    }

    public override NodeState Evaluate()
    {
        if (status == null)
        {
            Debug.LogError("IsHealthHighEnoughToDefendNode: CharacterStatus 컴포넌트를 찾을 수 없습니다.");
            return NodeState.FAILURE;
        }

        if (status.currentHealth >= healthThreshold)
        {
            return NodeState.SUCCESS; // 체력이 기준치 이상이면 성공
        }
        
        return NodeState.FAILURE;
    }
}