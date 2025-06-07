public class IsCurrentlyDefendingNode : Node {
    private PaladinActuator actuator;
    public IsCurrentlyDefendingNode(PaladinActuator actuator) { this.actuator = actuator; }
    public override NodeState Evaluate() {
        return actuator.IsCurrentlyDefending ? NodeState.SUCCESS : NodeState.FAILURE;
    }
}