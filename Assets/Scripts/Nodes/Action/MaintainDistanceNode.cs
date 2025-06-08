// 파일 이름: MaintainDistanceNode.cs
using UnityEngine;

public class MaintainDistanceNode : Node
{
    private Transform agentTransform;
    private Transform targetTransform;
    private PaladinActuator actuator;
    private float idealDistance;
    private float tolerance;

    public MaintainDistanceNode(Transform agent, Transform target, float idealDist, float tol)
    {
        this.agentTransform = agent;
        this.targetTransform = target;
        this.actuator = agent.GetComponent<PaladinActuator>();
        this.idealDistance = idealDist;
        this.tolerance = tol;
    }

    public override NodeState Evaluate()
    {
        if (targetTransform == null || actuator == null) return NodeState.FAILURE;

        float distance = Vector3.Distance(agentTransform.position, targetTransform.position);

        if (Mathf.Abs(distance - idealDistance) <= tolerance)
        {
            actuator.SetMovement(0);
            return NodeState.SUCCESS;
        }
        else
        {
            actuator.SetMovement(0);
            actuator.SetRotation(Quaternion.LookRotation(targetTransform.position - agentTransform.position));
            actuator.SetMovement(distance > idealDistance ? 0.75f : -0.75f);
            return NodeState.SUCCESS;
        }
    }
}