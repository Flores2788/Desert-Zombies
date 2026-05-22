using UnityEngine;
using UnityEngine.AI;

public class ZombieChasingState : StateMachineBehaviour
{
    private NavMeshAgent agent;
    private Transform player;

    public float chaseSpeed = 6f;

    public float stopChasingDistance = 21f;
    public float attackingDistance = 2.5f;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

        if (playerObject != null)
        {
            player = playerObject.transform;
        }

        agent = animator.GetComponent<NavMeshAgent>();

        if (agent != null)
        {
            agent.speed = chaseSpeed;

            // Try to place the agent on the nearest NavMesh point if it is slightly off.
            if (agent.isActiveAndEnabled && !agent.isOnNavMesh)
            {
                if (NavMesh.SamplePosition(animator.transform.position, out NavMeshHit hit, 2f, NavMesh.AllAreas))
                {
                    agent.Warp(hit.position);
                }
            }
        }
    }

    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (player == null || agent == null)
            return;

        if (!agent.isActiveAndEnabled || !agent.isOnNavMesh)
            return;

        agent.SetDestination(player.position);

        Vector3 lookDirection = player.position - animator.transform.position;
        lookDirection.y = 0f;

        if (lookDirection.sqrMagnitude > 0.01f)
        {
            animator.transform.rotation = Quaternion.LookRotation(lookDirection);
        }

        float distanceFromPlayer = Vector3.Distance(player.position, animator.transform.position);

        if (distanceFromPlayer > stopChasingDistance)
        {
            animator.SetBool("isChasing", false);
        }

        if (distanceFromPlayer < attackingDistance)
        {
            animator.SetBool("isAttacking", true);
        }
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (agent == null)
            return;

        if (!agent.isActiveAndEnabled || !agent.isOnNavMesh)
            return;

        agent.SetDestination(animator.transform.position);
    }
}
