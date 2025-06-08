// 파일 이름: ActionLoggerNode.cs (수정 버전)
public class ActionLoggerNode : Node
{
    private BT_Brain brain; // BT_Aggressive_Paladin 대신 BT_Brain 타입을 사용
    private string actionName;
    private Node childNode;

    public ActionLoggerNode(BT_Brain brain, string actionName, Node childNode)
    {
        this.brain = brain;
        this.actionName = actionName;
        this.childNode = childNode;
    }

    public override NodeState Evaluate()
    {
        if (brain != null)
        {
            brain.currentActionName = actionName;
        }
        return childNode.Evaluate();
    }
}