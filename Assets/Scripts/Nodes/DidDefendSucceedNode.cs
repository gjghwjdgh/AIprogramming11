using UnityEngine;

public class DidDefendSucceedNode : Node
{
    private CharacterStatus status;

    public DidDefendSucceedNode(Transform agentTransform)
    {
        status = agentTransform.GetComponent<CharacterStatus>();
    }

    public override NodeState Evaluate()
    {
        if (status.didJustDefend)
        {
            // 한 번만 체크하고 즉시 상태를 되돌려서, 반격이 한 번만 나가도록 함
            status.didJustDefend = false;
            return NodeState.SUCCESS;
        }
        
        return NodeState.FAILURE;
    }
}