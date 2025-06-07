// 파일 이름: Inverter.cs

// Decorator 노드의 한 종류로, 자식 노드의 결과를 반대로 뒤집습니다.
public class Inverter : Node
{
    private Node child;

    public Inverter(Node child)
    {
        this.child = child;
    }

    public override NodeState Evaluate()
    {
        switch (child.Evaluate())
        {
            case NodeState.FAILURE:
                // 자식이 실패하면, Inverter는 성공을 반환합니다.
                return NodeState.SUCCESS;
            case NodeState.SUCCESS:
                // 자식이 성공하면, Inverter는 실패를 반환합니다.
                return NodeState.FAILURE;
            case NodeState.RUNNING:
                // 자식이 실행 중이면, 그대로 실행 중 상태를 유지합니다.
                return NodeState.RUNNING;
        }
        // 기본적으로는 실패를 반환합니다.
        return NodeState.FAILURE;
    }
}