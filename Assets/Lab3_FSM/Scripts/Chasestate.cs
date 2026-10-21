using UnityEngine;
using static EnemyFSMEnum;

public class ChaseState : IEnemyState
{
    public void Enter(EnemyFSM e) { Debug.Log("Enter Chase"); }
    public void Exit(EnemyFSM e) { Debug.Log("Exit Chase"); }

    public void Update(EnemyFSM e)
    {
        e.MoveTowards(e.player.position);
        e.lastKnownPosition = e.player.position;
        float dist = e.DistanceToPlayer();

        if (dist < e.attackRange)
            e.ChangeState(e.attackState);
        else if (dist > e.visionRange * 1.5f)
            e.ChangeState(e.searchState);
    }
}