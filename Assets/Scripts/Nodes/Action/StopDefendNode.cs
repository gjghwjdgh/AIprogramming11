using UnityEngine;

public class StopDefendNode : Node
{
    private PaladinActuator actuator;

    public StopDefendNode(Transform agentTransform)
    {
        this.actuator = agentTransform.GetComponent<PaladinActuator>();
    }

    public override NodeState Evaluate()
    {
        if (actuator != null)
        {
            actuator.StopDefense();
        }
        return NodeState.SUCCESS;
    }
}