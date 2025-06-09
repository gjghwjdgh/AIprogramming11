// 파일 이름: IdleNode.cs
using UnityEngine;

public class IdleNode : Node
{
    private PaladinActuator actuator;

    public IdleNode(Transform agentTransform)
    {
        actuator = agentTransform.GetComponent<PaladinActuator>();
    }

    public override NodeState Evaluate()
    {
        if (actuator != null)
        {
            actuator.SetMovement(0);
        }
        return NodeState.SUCCESS;
    }
}