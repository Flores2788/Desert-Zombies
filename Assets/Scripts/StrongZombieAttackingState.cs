using UnityEngine;
using UnityEngine.AI;

public class StrongZombieAttackingState : StateMachineBehaviour
{
    private Transform player;
    private NavMeshAgent agent;

    [Header("Tracking Settings")]
    [Tooltip("How far the player must run to escape the attack.")]
    public float stopAttackingDistance = 3.0f; // Slightly larger for the stronger/taller enemy

    [Tooltip("How fast the enemy turns to face you during punches. Higher = faster tracking.")]
    public float rotationSpeed = 6f; // Heavier, slightly slower rotation for a stronger enemy

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // Find player and agent components
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }

        agent = animator.GetComponent<NavMeshAgent>();

        if (agent != null)
        {
            // Stop the agent physically from sliding or moving forward during its swing
            agent.isStopped = true;

            // Turn off the agent's auto-rotation so it doesn't fight this script
            agent.updateRotation = false;
        }
        // Play the attack sound as they start their swing!
        Zombie zombie = animator.GetComponent<Zombie>();
        if (zombie != null)
        {
            zombie.PlayAttackSound();
        }
    }

    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (player == null || agent == null) return;

        // Smoothly rotate the body to track the moving player
        LookAtPlayer(animator);

        // Transition back to chase state if player escapes out of reach
        float distanceFromPlayer = Vector3.Distance(player.position, animator.transform.position);

        if (distanceFromPlayer > stopAttackingDistance)
        {
            animator.SetBool("isAttacking", false);
        }
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (agent != null && agent.enabled)
        {
            // Hand rotation and movement control back over to the NavMeshAgent
            agent.isStopped = false;
            agent.updateRotation = true;
        }
    }

    private void LookAtPlayer(Animator animator)
    {
        // Calculate the horizontal direction vector to the player (ignoring height so the zombie doesn't lean up/down)
        Vector3 direction = player.position - animator.transform.position;
        direction.y = 0;

        if (direction.sqrMagnitude > 0.01f)
        {
            // Define the target look-at rotation
            Quaternion targetRotation = Quaternion.LookRotation(direction);

            // Smoothly Slerp towards that target rotation based on the rotationSpeed
            animator.transform.rotation = Quaternion.Slerp(animator.transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
        }
    }
}