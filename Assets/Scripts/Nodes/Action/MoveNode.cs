// 파일 이름: MoveNode.cs
using UnityEngine;

public class MoveNode : Node
{
    private Transform agentTransform;
    private Transform targetTransform;
    private PaladinActuator actuator;

    public MoveNode(Transform agentTransform, Transform targetTransform)
    {
        this.agentTransform = agentTransform;
        this.targetTransform = targetTransform;
        this.actuator = agentTransform.GetComponent<PaladinActuator>();
    }

    public override NodeState Evaluate()
    {
        if (targetTransform == null)
        {
            actuator.SetMovement(0);

            return NodeState.FAILURE;
        }

        Vector3 directionToTarget = targetTransform.position - agentTransform.position;
        directionToTarget.y = 0;
        actuator.SetRotation(Quaternion.LookRotation(directionToTarget));
        
        actuator.SetMovement(1.0f);
        
        return NodeState.RUNNING;
    }
}