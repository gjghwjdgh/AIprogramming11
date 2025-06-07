// 파일 이름: IsActionInProgressNode.cs
using UnityEngine;

public class IsActionInProgressNode : Node
{
    private PaladinActuator actuator;

    public IsActionInProgressNode(PaladinActuator actuator)
    {
        this.actuator = actuator;
    }

    public override NodeState Evaluate()
    {
        if (actuator != null && actuator.IsActionInProgress)
        {
            // 액션이 진행 중이면 '성공'을 반환
            return NodeState.SUCCESS;
        }
        
        // 액션 중이 아니면 '실패'를 반환
        return NodeState.FAILURE;
    }
}