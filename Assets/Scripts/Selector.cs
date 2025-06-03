using System.Collections.Generic;

public class Selector : Node
{
    protected List<Node> children = new List<Node>();

    public Selector(List<Node> children)
    {
        this.children = children;
    }

    public override NodeState Evaluate()
    {
        foreach (Node node in children)
        {
            switch (node.Evaluate())
            {
                case NodeState.FAILURE:
                    // 실패하면 다음 자식으로 넘어감
                    continue;
                case NodeState.SUCCESS:
                    // 자식 중 하나라도 성공하면 즉시 성공 반환
                    return NodeState.SUCCESS;
                case NodeState.RUNNING:
                    // 자식이 아직 실행 중이면 나도 실행 중
                    return NodeState.RUNNING;
                default:
                    continue;
            }
        }
        // 모든 자식이 실패했을 때만 실패 반환
        return NodeState.FAILURE;
    }
}