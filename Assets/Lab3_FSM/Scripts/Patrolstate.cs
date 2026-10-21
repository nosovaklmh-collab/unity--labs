using UnityEngine;
using static EnemyFSMEnum;

public class PatrolState : IEnemyState
{
    public void Enter(EnemyFSM e) { Debug.Log("Enter Patrol"); }
    public void Exit(EnemyFSM e) { Debug.Log("Exit Patrol"); }

    public void Update(EnemyFSM e)
    {
        Transform target = e.waypoints[e.currentWaypoint];
        e.MoveTowards(target.position);

        Vector3 a = e.transform.position; a.y = 0;
        Vector3 b = target.position; b.y = 0;
        if (Vector3.Distance(a, b) < 0.3f)
            e.currentWaypoint = (e.currentWaypoint + 1) % e.waypoints.Length;

        if (e.DistanceToPlayer() < e.visionRange)
            e.ChangeState(e.chaseState);
    }
}