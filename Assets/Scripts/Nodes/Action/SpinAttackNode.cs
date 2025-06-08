// SpinAttackNode.cs (Modify this file)
using UnityEngine;
using System.Collections;

public class SpinAttackNode : Node
{
    private PaladinActuator actuator;
    private CooldownManager cooldownManager;
    private BT_Aggressive_Paladin btPaladin;

    private PaladinActuator.AttackType attackType;
    private string skillName;
    private float cooldownDuration;
    private float attackAnimationDuration;

    private bool isAttackActive = false;
    private Coroutine attackCoroutine = null;

    // Constructor now takes 6 arguments
    public SpinAttackNode(Transform agentTransform, PaladinActuator.AttackType type, string skillName, float cooldownDuration, float animDuration, BT_Aggressive_Paladin btPaladinInstance)
    {
        actuator = agentTransform.GetComponent<PaladinActuator>();
        cooldownManager = agentTransform.GetComponent<CooldownManager>();
        btPaladin = btPaladinInstance;

        this.attackType = type;
        this.skillName = skillName;
        this.cooldownDuration = cooldownDuration;
        this.attackAnimationDuration = animDuration;
    }

    public override NodeState Evaluate()
    {
        if (actuator == null || cooldownManager == null || btPaladin == null) return NodeState.FAILURE;

        if (actuator.IsActionInProgress && !isAttackActive)
        {
            return NodeState.FAILURE;
        }

        if (isAttackActive && attackCoroutine != null)
        {
            return NodeState.RUNNING;
        }

        if (!cooldownManager.IsCooldownFinished(skillName))
        {
            return NodeState.FAILURE;
        }

        if (!isAttackActive && attackCoroutine == null)
        {
            actuator.StartAttack(attackType);
            actuator.OnActionStart();
            cooldownManager.StartCooldown(skillName, cooldownDuration);

            Debug.Log($"<color=purple>SpinAttackNode: Initiating {skillName} attack.</color>");

            isAttackActive = true;
            attackCoroutine = btPaladin.StartCoroutine(AttackCoroutineInternal(attackAnimationDuration));
            return NodeState.RUNNING;
        }

        if (!isAttackActive && attackCoroutine == null)
        {
            Debug.Log($"<color=green>SpinAttackNode: {skillName} - Attack completed. Returning SUCCESS.</color>");
            return NodeState.SUCCESS;
        }

        Debug.LogWarning($"SpinAttackNode: {skillName} - Unexpected state. Returning FAILURE.");
        return NodeState.FAILURE;
    }

    private IEnumerator AttackCoroutineInternal(float duration)
    {
        yield return new WaitForSeconds(duration);
        actuator.OnActionEnd();
        Debug.Log($"<color=yellow>SpinAttackNode: {skillName} - Attack duration ended. Setting to inactive.</color>");
        isAttackActive = false;
        attackCoroutine = null;
    }
}