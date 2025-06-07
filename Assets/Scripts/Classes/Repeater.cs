// 파일 이름: Repeater.cs
using System.Collections.Generic;

// 자식 노드의 실행이 끝나면, 결과를 무시하고 계속해서 자식 노드를 다시 실행시키는 데코레이터 노드입니다.
public class Repeater : Node
{
    private Node child;

    public Repeater(Node child)
    {
        this.child = child;
    }

    public override NodeState Evaluate()
    {
        // 자식의 결과가 성공이든 실패든 상관하지 않습니다.
        child.Evaluate();
        
        // 이 노드 자체는 항상 '진행 중' 상태를 반환하여,
        // 행동 트리가 이 노드에서 멈추고 계속 자식을 반복 실행하도록 만듭니다.
        return NodeState.RUNNING;
    }
}