// 파일 이름: MoveAwayNode.cs
using UnityEngine;

public class MoveAwayNode : Node
{
    private PaladinActuator actuator;

    public MoveAwayNode(Transform agentTransform, Transform targetTransform)
    {
        this.actuator = agentTransform.GetComponent<PaladinActuator>();
    }

    public override NodeState Evaluate()
    {
        actuator.SetMovement(-1.0f);
        return NodeState.SUCCESS;
    }
}