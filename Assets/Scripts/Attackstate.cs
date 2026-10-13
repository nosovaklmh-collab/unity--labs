using UnityEngine;
using static EnemyFSMEnum;

public class AttackState : IEnemyState
{
    private float timer;

    public void Enter(EnemyFSM e)
    {
        Debug.Log("Enter Attack");
        timer = 0f;               // первый удар — сразу
    }

    public void Exit(EnemyFSM e) { Debug.Log("Exit Attack"); }

    public void Update(EnemyFSM e)
    {
        if (e.DistanceToPlayer() > e.attackRange)
        {
            e.ChangeState(e.chaseState);
            return;
        }

        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            Debug.Log("Attack!");
            timer = 1f;           // следующий удар через секунду
        }
    }
}