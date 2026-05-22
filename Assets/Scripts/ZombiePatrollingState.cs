using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public class ZombiePatrollingState : StateMachineBehaviour
{
    private float timer;

    public float patrollingTime = 10f;
    public float detectionArea = 100f;
    public float patrolSpeed = 2f;

    private Transform player;
    private NavMeshAgent agent;

    private List<Transform> waypointsList = new List<Transform>();

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        timer = 0f;

        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

        if (playerObject != null)
        {
            player = playerObject.transform;
        }
        else
        {
            Debug.LogWarning("ZombiePatrollingState: No object with Player tag found.");
        }

        agent = animator.GetComponent<NavMeshAgent>();

        if (agent == null)
        {
            Debug.LogWarning("ZombiePatrollingState: No NavMeshAgent found on zombie.");
            return;
        }

        agent.speed = patrolSpeed;

        if (agent.isActiveAndEnabled && !agent.isOnNavMesh)
        {
            if (NavMesh.SamplePosition(animator.transform.position, out NavMeshHit hit, 2f, NavMesh.AllAreas))
            {
                agent.Warp(hit.position);
            }
            else
            {
                Debug.LogWarning("ZombiePatrollingState: Zombie is not on a NavMesh.");
                return;
            }
        }

        waypointsList.Clear();

        GameObject waypointCluster = GameObject.FindGameObjectWithTag("Waypoints");

        if (waypointCluster != null)
        {
            foreach (Transform t in waypointCluster.transform)
            {
                waypointsList.Add(t);
            }
        }
        else
        {
            Debug.LogWarning("ZombiePatrollingState: No object with Waypoints tag found.");
        }

        SetRandomWaypoint();
    }

    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (agent == null)
            return;

        if (!agent.isActiveAndEnabled || !agent.isOnNavMesh)
            return;

        if (waypointsList.Count > 0)
        {
            if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
            {
                SetRandomWaypoint();
            }
        }

        timer += Time.deltaTime;

        if (timer > patrollingTime)
        {
            animator.SetBool("isPatrolling", false);
        }

        if (player != null)
        {
            float distanceFromPlayer = Vector3.Distance(player.position, animator.transform.position);

            if (distanceFromPlayer < detectionArea)
            {
                animator.SetBool("isChasing", true);
            }
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

    private void SetRandomWaypoint()
    {
        if (agent == null)
            return;

        if (!agent.isActiveAndEnabled || !agent.isOnNavMesh)
            return;

        if (waypointsList.Count == 0)
            return;

        Vector3 nextPosition = waypointsList[Random.Range(0, waypointsList.Count)].position;
        agent.SetDestination(nextPosition);
    }
}