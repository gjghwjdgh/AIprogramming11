using UnityEngine;

// 파일 이름: RandomChanceNode.cs
public class RandomChanceNode : Node
{
    private float probability; // 성공 확률 (0.0 ~ 1.0)

    public RandomChanceNode(float probability)
    {
        this.probability = Mathf.Clamp01(probability); // 확률 값을 0과 1 사이로 제한
    }

    public override NodeState Evaluate()
    {
        if (Random.value < probability)
        {
            return NodeState.SUCCESS;
        }
        
        return NodeState.FAILURE;
    }
}