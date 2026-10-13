using UnityEngine;

public class SearchState : IEnemyState
{
    private float timer;

    public void Enter(EnemyFSM e)
    {
        Debug.Log("Enter Search: иду туда, где последний раз видел игрока");
        timer = 3f;
    }

    public void Exit(EnemyFSM e) { Debug.Log("Exit Search"); }

    public void Update(EnemyFSM e)
    {
        if (e.DistanceToPlayer() < e.visionRange)
        {
            e.ChangeState(e.chaseState);
            return;
        }

        Vector3 a = e.transform.position; a.y = 0;
        Vector3 b = e.lastKnownPosition; b.y = 0;

        if (Vector3.Distance(a, b) > 0.3f)
        {
            e.MoveTowards(e.lastKnownPosition);
        }
        else
        {
            timer -= Time.deltaTime;
            if (timer <= 0f)
            {
                Debug.Log("Ќикого не нашЄл, возвращаюсь к патрулю");
                e.ChangeState(e.patrolState);
            }
        }
    }
}