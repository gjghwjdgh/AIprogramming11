// 파일 이름: MaintainDistanceNode.cs
using UnityEngine;

public class MaintainDistanceNode : Node
{
    private Transform agentTransform;
    private Transform targetTransform;
    private PaladinActuator actuator;
    private float idealDistance;
    private float tolerance;

    public MaintainDistanceNode(Transform agentTransform, Transform target, float idealDistance, float tolerance = 0.5f)
    {
        this.agentTransform = agentTransform;
        this.targetTransform = target;
        this.actuator = agentTransform.GetComponent<PaladinActuator>();
        this.idealDistance = idealDistance;
        this.tolerance = tolerance;
    }

    public override NodeState Evaluate()
    {
        if (targetTransform == null)
        {
            actuator.SetMovement(0);
            return NodeState.FAILURE;
        }

        float currentDistance = Vector3.Distance(agentTransform.position, targetTransform.position);
        
        Vector3 directionToTarget = targetTransform.position - agentTransform.position;
        directionToTarget.y = 0;
        actuator.SetRotation(Quaternion.LookRotation(directionToTarget));

        if (currentDistance > idealDistance + tolerance)
        {
            actuator.SetMovement(1.0f);
            return NodeState.RUNNING;
        }
        else if (currentDistance < idealDistance - tolerance)
        {
            actuator.SetMovement(-1.0f);
            return NodeState.RUNNING;
        }
        else
        {
            actuator.SetMovement(0);
            return NodeState.SUCCESS;
        }
    }
}