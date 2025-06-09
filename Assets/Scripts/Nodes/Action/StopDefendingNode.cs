// 파일 이름: StopDefendingNode.cs
using UnityEngine;

public class StopDefendingNode : Node
{
    private PaladinActuator actuator;

    public StopDefendingNode(Transform agentTransform)
    {
        this.actuator = agentTransform.GetComponent<PaladinActuator>();
    }

    public override NodeState Evaluate()
    {
        if (actuator != null)
        {
            actuator.SetDefend(false);
        }
        // 방패를 내리는 행동은 항상 성공으로 처리
        return NodeState.SUCCESS;
    }
}