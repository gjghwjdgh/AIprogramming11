// 파일 이름: ActionLoggerNode.cs
using UnityEngine;

public class ActionLoggerNode : Node
{
    private BT_Aggressive_Paladin brain;
    private string actionName;
    private Node childNode;

    public ActionLoggerNode(BT_Aggressive_Paladin brain, string actionName, Node childNode)
    {
        this.brain = brain;
        this.actionName = actionName;
        this.childNode = childNode;
    }

    public override NodeState Evaluate()
    {
        // 이 노드가 실행될 때, AI의 뇌(brain)에 자신의 행동 이름을 기록합니다.
        if (brain != null)
        {
            brain.currentActionName = actionName;
        }

        // 그 다음, 자식 노드를 실행하고 그 결과를 그대로 반환합니다.
        return childNode.Evaluate();
    }
}