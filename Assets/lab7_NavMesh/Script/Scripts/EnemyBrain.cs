using UnityEngine;
using UnityEngine.AI;

namespace NavLab
{
    // FSM врага: Patrol → Chase → Search → Return → Patrol
    [RequireComponent(typeof(EnemyMover))]
    public class EnemyBrain : MonoBehaviour
    {
        [Header("Патруль")]
        public Transform[] waypoints;
        public float patrolSpeed = 2f;
        public float waitAtWaypoint = 1f;        // пауза на точке

        [Header("Обнаружение игрока")]
        public Transform player;                 // если пусто — найдётся по тегу Player
        public float viewRadius = 6f;
        public float viewAngle = 90f;
        public bool checkWalls = true;           // не видеть сквозь стены (Physics.Linecast)
        public float eyeHeight = 0.5f;           // глаза выше центра капсулы

        [Header("Преследование")]
        public float chaseSpeed = 3.5f;
        public float repathInterval = 0.25f;     // пересчёт пути не чаще, чем раз в N секунд
        public float minTargetShift = 0.3f;      // и только если игрок сдвинулся хотя бы на столько

        [Header("Поиск")]
        public float searchLookTime = 3f;        // сколько секунд осматривается на месте
        public float lookTurnSpeed = 120f;       // градусов в секунду

        [Header("Отладка")]
        public bool showCornerGui = true;        // надписи в левом верхнем углу (одному врагу из толпы)
        public bool showHeadLabel = true;        // состояние над головой

        private EnemyMover mover;
        private EnemyState state = EnemyState.Patrol;
        private int currentWaypoint = 0;
        private Vector3 lastKnownPlayerPos;
        private float repathTimer;
        private float waitTimer;
        private Vector3 lastRepathTarget;

        public EnemyState State => state;

        void Awake()
        {
            mover = GetComponent<EnemyMover>();
            if (player == null)
            {
                var p = GameObject.FindWithTag("Player");
                if (p != null) player = p.transform;
            }
        }

        void Start()
        {
            EnterState(EnemyState.Patrol);
        }

        void Update()
        {
            switch (state)
            {
                case EnemyState.Patrol: TickPatrol(); break;
                case EnemyState.Chase:  TickChase();  break;
                case EnemyState.Search: TickSearch(); break;
                case EnemyState.Return: TickReturn(); break;
            }
        }

        // ---------- Переходы ----------

        void SetState(EnemyState next)
        {
            if (next == state) return;
            Debug.Log($"{name}: {state} → {next}");
            state = next;
            EnterState(next);
        }

        // Что делаем в момент входа в состояние
        void EnterState(EnemyState s)
        {
            waitTimer = 0f;
            switch (s)
            {
                case EnemyState.Patrol:
                    mover.Agent.speed = patrolSpeed;
                    GoToWaypoint(currentWaypoint);
                    break;
                case EnemyState.Chase:
                    mover.Agent.speed = chaseSpeed;
                    repathTimer = 0f;                    // первый пересчёт — сразу
                    lastRepathTarget = Vector3.positiveInfinity;
                    break;
                case EnemyState.Search:
                    mover.Agent.speed = patrolSpeed;
                    mover.MoveTo(lastKnownPlayerPos);
                    break;
                case EnemyState.Return:
                    mover.Agent.speed = patrolSpeed;
                    currentWaypoint = NearestWaypoint();
                    GoToWaypoint(currentWaypoint);
                    break;
            }
        }

        // ---------- Зрение ----------

        bool CanSeePlayer()
        {
            if (player == null) return false;

            Vector3 toPlayer = player.position - transform.position;
            toPlayer.y = 0f;

            // 1) расстояние
            if (toPlayer.magnitude > viewRadius) return false;

            // 2) угол обзора
            Vector3 forward = transform.forward;
            forward.y = 0f;
            if (Vector3.Angle(forward, toPlayer) > viewAngle * 0.5f) return false;

            // 3) нет ли стены между глазами и игроком
            if (checkWalls)
            {
                Vector3 eye = transform.position + Vector3.up * eyeHeight;
                if (Physics.Linecast(eye, player.position, out RaycastHit hit, ~0, QueryTriggerInteraction.Ignore))
                {
                    if (!hit.transform.IsChildOf(player)) return false;   // упёрлись не в игрока
                }
            }
            return true;
        }

        // ---------- Состояния ----------

        void TickPatrol()
        {
            if (CanSeePlayer()) { RememberPlayer(); SetState(EnemyState.Chase); return; }
            if (waypoints == null || waypoints.Length == 0) return;

            if (mover.HasArrived())
            {
                waitTimer += Time.deltaTime;
                if (waitTimer >= waitAtWaypoint)
                {
                    waitTimer = 0f;
                    currentWaypoint = (currentWaypoint + 1) % waypoints.Length;   // по кругу
                    GoToWaypoint(currentWaypoint);
                }
            }
        }

        void TickChase()
        {
            if (!CanSeePlayer()) { SetState(EnemyState.Search); return; }

            RememberPlayer();

            // Пересчёт не каждый кадр, а по таймеру — и только если игрок заметно сдвинулся
            repathTimer -= Time.deltaTime;
            if (repathTimer <= 0f)
            {
                repathTimer = repathInterval;
                if ((player.position - lastRepathTarget).sqrMagnitude > minTargetShift * minTargetShift)
                {
                    mover.MoveTo(player.position);
                    lastRepathTarget = player.position;
                }
            }
        }

        void TickSearch()
        {
            if (CanSeePlayer()) { RememberPlayer(); SetState(EnemyState.Chase); return; }
            if (!mover.HasArrived()) return;

            // Дошёл до последнего места, где видел игрока, — осматривается
            transform.Rotate(0f, lookTurnSpeed * Time.deltaTime, 0f);
            waitTimer += Time.deltaTime;
            if (waitTimer >= searchLookTime) SetState(EnemyState.Return);
        }

        void TickReturn()
        {
            if (CanSeePlayer()) { RememberPlayer(); SetState(EnemyState.Chase); return; }
            if (mover.HasArrived()) SetState(EnemyState.Patrol);
        }

        // ---------- Помощники ----------

        void RememberPlayer() => lastKnownPlayerPos = player.position;

        void GoToWaypoint(int i)
        {
            if (waypoints == null || waypoints.Length == 0 || waypoints[i] == null) return;
            mover.MoveTo(waypoints[i].position);
        }

        // Ближайшая точка патруля — перебором, а не «к нулевой»
        int NearestWaypoint()
        {
            int best = 0;
            float bestDist = float.MaxValue;
            for (int i = 0; i < waypoints.Length; i++)
            {
                if (waypoints[i] == null) continue;
                float d = (waypoints[i].position - transform.position).sqrMagnitude;
                if (d < bestDist) { bestDist = d; best = i; }
            }
            return best;
        }

        // ---------- Отладка ----------

        void OnGUI()
        {
            if (showCornerGui)
            {
                GUI.Label(new Rect(10, 10, 400, 20), $"State: {state}");
                GUI.Label(new Rect(10, 30, 400, 20), $"Repaths: {mover.RepathCount}");
                GUI.Label(new Rect(10, 50, 400, 20), $"Repath: last {mover.LastRepathMs:F3} ms, avg {mover.AvgRepathMs:F3} ms");
            }

            if (showHeadLabel && Camera.main != null)
            {
                Vector3 sp = Camera.main.WorldToScreenPoint(transform.position + Vector3.up * 1.4f);
                if (sp.z > 0f)
                    GUI.Label(new Rect(sp.x - 40, Screen.height - sp.y - 10, 120, 20), state.ToString());
            }
        }

        void OnDrawGizmos()
        {
            // Путь агента — линией, цветом текущего состояния
            var agent = GetComponent<NavMeshAgent>();
            if (agent != null && agent.hasPath)
            {
                Gizmos.color = StateColor();
                var corners = agent.path.corners;
                for (int i = 0; i < corners.Length - 1; i++)
                    Gizmos.DrawLine(corners[i] + Vector3.up * 0.1f, corners[i + 1] + Vector3.up * 0.1f);
            }
        }

        void OnDrawGizmosSelected()
        {
            // Радиус и угол обзора
            Gizmos.color = new Color(1f, 1f, 0f, 0.6f);
            Gizmos.DrawWireSphere(transform.position, viewRadius);
            Vector3 left = Quaternion.Euler(0f, -viewAngle * 0.5f, 0f) * transform.forward;
            Vector3 right = Quaternion.Euler(0f, viewAngle * 0.5f, 0f) * transform.forward;
            Gizmos.DrawLine(transform.position, transform.position + left * viewRadius);
            Gizmos.DrawLine(transform.position, transform.position + right * viewRadius);

            if (Application.isPlaying && state == EnemyState.Search)
            {
                Gizmos.color = Color.magenta;
                Gizmos.DrawWireSphere(lastKnownPlayerPos, 0.3f);
            }
        }

        Color StateColor()
        {
            switch (state)
            {
                case EnemyState.Chase:  return Color.red;
                case EnemyState.Search: return new Color(1f, 0.6f, 0f);
                case EnemyState.Return: return Color.cyan;
                default:                return Color.green;
            }
        }
    }
}
