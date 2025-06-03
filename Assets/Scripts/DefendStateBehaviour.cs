using UnityEngine;

public class DefendStateBehaviour : StateMachineBehaviour
{
    private Shield shield;

    // 방어(Defend) 애니메이션 상태에 '진입'할 때 호출됨
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // 캐릭터에 붙어있는 Shield 스크립트를 찾음
        if (shield == null)
        {
            shield = animator.GetComponentInChildren<Shield>();
        }

        // 방패의 Collider를 활성화
        if (shield != null && shield.shieldCollider != null)
        {
            shield.shieldCollider.enabled = true;
        }
    }

    // 방어(Defend) 애니메이션 상태에서 '빠져나갈' 때 호출됨
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (shield == null)
        {
            shield = animator.GetComponentInChildren<Shield>();
        }

        // 방패의 Collider를 비활성화
        if (shield != null && shield.shieldCollider != null)
        {
            shield.shieldCollider.enabled = false;
        }
    }
}