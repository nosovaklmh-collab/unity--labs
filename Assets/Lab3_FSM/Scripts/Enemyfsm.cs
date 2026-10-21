using UnityEngine;
using static EnemyFSMEnum;

// Часть 3: FSM на классах-состояниях (паттерн State)
public class EnemyFSM : MonoBehaviour
{
    [Header("Ссылки")]
    public Transform player;
    public Transform[] waypoints;

    [Header("Параметры")]
    public float visionRange = 8f;
    public float attackRange = 2f;
    public float speed = 2f;

    [HideInInspector] public int currentWaypoint = 0;
    [HideInInspector] public Vector3 lastKnownPosition;

    // Состояния создаём ОДИН раз и переиспользуем (без new в каждом переходе)
    public readonly PatrolState patrolState = new PatrolState();
    public readonly ChaseState chaseState = new ChaseState();
    public readonly AttackState attackState = new AttackState();
    public readonly SearchState searchState = new SearchState();

    private IEnemyState currentState;

    void Start()
    {
        ChangeState(patrolState);
    }

    void Update()
    {
        currentState?.Update(this);
    }

    public void ChangeState(IEnemyState next)
    {
        if (currentState == next) return;
        Debug.Log($"FSM: {currentState?.GetType().Name ?? "null"} -> {next.GetType().Name}");
        currentState?.Exit(this);
        currentState = next;
        currentState.Enter(this);
    }

    public float DistanceToPlayer()
    {
        return Vector3.Distance(transform.position, player.position);
    }

    public void MoveTowards(Vector3 target)
    {
        Vector3 dir = target - transform.position;
        dir.y = 0;
        if (dir.sqrMagnitude < 0.01f) return;
        transform.position += dir.normalized * speed * Time.deltaTime;
        transform.forward = dir.normalized;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = currentState switch
        {
            PatrolState => Color.yellow,
            ChaseState => new Color(1f, 0.5f, 0f),
            AttackState => Color.red,
            SearchState => Color.blue,
            _ => Color.white
        };
        Gizmos.DrawWireSphere(transform.position, 0.7f);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, visionRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}