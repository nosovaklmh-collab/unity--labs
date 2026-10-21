using UnityEngine;

// Часть 1 и 2: FSM на enum + switch
public class EnemyFSMEnum : MonoBehaviour
{
    public enum EnemyState { Patrol, Chase, Attack }

    [Header("Ссылки")]
    public Transform player;          // перетащить Player
    public Transform[] waypoints;     // перетащить PointA и PointB

    [Header("Параметры")]
    public float visionRange = 8f;    // радиус, в котором враг видит игрока
    public float attackRange = 2f;    // радиус атаки
    public float speed = 2f;          // скорость движения

    private EnemyState state = EnemyState.Patrol; // текущее состояние
    private int currentWaypoint = 0;              // индекс точки маршрута
    private float attackTimer = 0f;               // таймер атаки

    void Update()
    {
        // В каждом кадре выполняем код только текущего состояния
        switch (state)
        {
            case EnemyState.Patrol: DoPatrol(); break;
            case EnemyState.Chase: DoChase(); break;
            case EnemyState.Attack: DoAttack(); break;
        }
    }

    // Единственная точка смены состояния
    void ChangeState(EnemyState next)
    {
        if (state == next) return;
        Debug.Log($"FSM: {state} -> {next}");
        state = next;
        attackTimer = 0f;
    }

    void DoPatrol()
    {
        Transform target = waypoints[currentWaypoint];
        MoveTowards(target.position);

        // Дошли до точки — берём следующую (по кругу)
        if (FlatDistance(transform.position, target.position) < 0.3f)
            currentWaypoint = (currentWaypoint + 1) % waypoints.Length;

        // Увидели игрока — преследуем
        if (Vector3.Distance(transform.position, player.position) < visionRange)
            ChangeState(EnemyState.Chase);
    }

    void DoChase()
    {
        MoveTowards(player.position);
        float dist = Vector3.Distance(transform.position, player.position);

        if (dist < attackRange)
            ChangeState(EnemyState.Attack);
        else if (dist > visionRange * 1.5f)   // потеряли игрока
            ChangeState(EnemyState.Patrol);
    }

    void DoAttack()
    {
        float dist = Vector3.Distance(transform.position, player.position);
        if (dist > attackRange)
        {
            ChangeState(EnemyState.Chase);
            return;
        }

        // "Бьём" раз в секунду
        attackTimer -= Time.deltaTime;
        if (attackTimer <= 0f)
        {
            Debug.Log("Attack!");
            attackTimer = 1f;
        }
    }

    void MoveTowards(Vector3 target)
    {
        Vector3 dir = target - transform.position;
        dir.y = 0;                                // не летаем вверх/вниз
        if (dir.sqrMagnitude < 0.01f) return;
        transform.position += dir.normalized * speed * Time.deltaTime;
        transform.forward = dir.normalized;       // поворачиваемся лицом
    }

    float FlatDistance(Vector3 a, Vector3 b)
    {
        a.y = 0; b.y = 0;
        return Vector3.Distance(a, b);
    }

    // Часть 2: цвет по состоянию + радиусы
    void OnDrawGizmos()
    {
        Gizmos.color = state switch
        {
            EnemyState.Patrol => Color.yellow,
            EnemyState.Chase => new Color(1f, 0.5f, 0f),
            EnemyState.Attack => Color.red,
            _ => Color.white
        };
        Gizmos.DrawWireSphere(transform.position, 0.7f);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, visionRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}