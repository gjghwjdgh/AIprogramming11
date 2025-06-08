// EvadeNode.cs (Modify this file)
using UnityEngine;
using System.Collections; // For coroutines

public class EvadeNode : Node
{
    private Transform agentTransform;
    private PaladinActuator actuator;
    private string direction;
    private float evadeDuration; // New parameter for duration
    private BT_Aggressive_Paladin btPaladin; // New parameter for BT_Aggressive_Paladin instance

    private bool isEvadeActive = false; // To track if evade is ongoing
    private Coroutine evadeCoroutine = null;

    // Constructor now takes 4 arguments
    public EvadeNode(Transform agent, string dir, float duration, BT_Aggressive_Paladin btPaladinInstance)
    {
        this.agentTransform = agent;
        this.actuator = agent.GetComponent<PaladinActuator>();
        this.direction = dir;
        this.evadeDuration = duration;
        this.btPaladin = btPaladinInstance;
    }

    public override NodeState Evaluate()
    {
        if (actuator == null || btPaladin == null) return NodeState.FAILURE;

        // Prevent multiple actions at once
        if (actuator.IsActionInProgress && !isEvadeActive)
        {
            return NodeState.FAILURE;
        }

        // If evade is already active, keep running
        if (isEvadeActive && evadeCoroutine != null)
        {
            return NodeState.RUNNING;
        }

        // Start new evade action
        if (!isEvadeActive && evadeCoroutine == null)
        {
            // You might need a cooldown check for Evade here if it's not handled by the BT_Aggressive_Paladin
            // if (!cooldownManager.IsCooldownFinished("Evade")) return NodeState.FAILURE;

            actuator.Dodge(direction); // This triggers the animation
            actuator.OnActionStart();

            // cooldownManager.StartCooldown("Evade", evadeCooldownDuration); // If evade has its own cooldown

            Debug.Log($"<color=blue>EvadeNode: Initiating Evade ({direction}) for {evadeDuration}s.</color>");

            isEvadeActive = true;
            evadeCoroutine = btPaladin.StartCoroutine(EvadeCoroutineInternal(evadeDuration));
            return NodeState.RUNNING;
        }

        // Evade completed
        if (!isEvadeActive && evadeCoroutine == null)
        {
            Debug.Log("<color=green>EvadeNode: Evade completed. Returning SUCCESS.</color>");
            return NodeState.SUCCESS;
        }

        return NodeState.FAILURE; // Unexpected state
    }

    private IEnumerator EvadeCoroutineInternal(float duration)
    {
        yield return new WaitForSeconds(duration);

        actuator.OnActionEnd(); // Reset IsActionInProgress
        Debug.Log("<color=yellow>EvadeCoroutine: Evade duration ended.</color>");
        isEvadeActive = false;
        evadeCoroutine = null;
    }
}