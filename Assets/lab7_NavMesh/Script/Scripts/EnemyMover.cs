using System.Diagnostics;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.AI;

namespace NavLab
{
    // Минимальный «двигатель» врага: идёт к точке и умеет сказать, дошёл ли.
    // Путь считается сразу (agent.CalculatePath), чтобы можно было замерить его время
    // в Profiler (маркер «EnemyMover.Repath») и показать на экране.
    [RequireComponent(typeof(NavMeshAgent))]
    public class EnemyMover : MonoBehaviour
    {
        static readonly ProfilerMarker RepathMarker = new ProfilerMarker("EnemyMover.Repath");

        [Header("Мини-задание 2 (тест без FSM)")]
        [Tooltip("Если задано — враг сам идёт к этому объекту. Для теста выключите EnemyBrain.")]
        public Transform debugTarget;
        public float debugRepathInterval = 0.25f;

        public NavMeshAgent Agent { get; private set; }
        public int RepathCount { get; private set; }      // сколько раз пересчитали путь
        public float LastRepathMs { get; private set; }   // время последнего пересчёта
        public float AvgRepathMs => RepathCount > 0 ? totalMs / RepathCount : 0f;

        private NavMeshPath path;
        private float totalMs;
        private bool hasDestination;
        private float debugTimer;

        void Awake()
        {
            Agent = GetComponent<NavMeshAgent>();
            path = new NavMeshPath();
        }

        void Update()
        {
            if (debugTarget == null) return;
            debugTimer -= Time.deltaTime;
            if (debugTimer <= 0f)
            {
                debugTimer = debugRepathInterval;
                MoveTo(debugTarget.position);
            }
        }

        // Идти к точке. Возвращает false, если точка вне NavMesh или пути нет.
        public bool MoveTo(Vector3 point)
        {
            // «Притягиваем» точку к ближайшему месту на NavMesh (в радиусе 2 м)
            if (!NavMesh.SamplePosition(point, out var hit, 2f, NavMesh.AllAreas))
                return false;

            var sw = Stopwatch.StartNew();
            bool ok;
            using (RepathMarker.Auto())
            {
                ok = Agent.CalculatePath(hit.position, path);   // учитывает стоимость областей (Swamp)
            }
            sw.Stop();

            LastRepathMs = (float)sw.Elapsed.TotalMilliseconds;
            totalMs += LastRepathMs;
            RepathCount++;

            if (!ok || path.status == NavMeshPathStatus.PathInvalid) return false;

            Agent.SetPath(path);
            hasDestination = true;
            return true;
        }

        public bool HasArrived(float threshold = 0.5f)
        {
            // Ловушка из методички: пока путь считается, remainingDistance «врёт»
            if (!hasDestination || Agent.pathPending) return false;

            if (Agent.hasPath)
                return Agent.remainingDistance <= threshold;

            // Путь уже сброшен (агент остановился) — проверяем расстояние до цели по полу
            Vector3 d = Agent.destination - transform.position;
            d.y = 0f;
            return d.magnitude <= threshold + Agent.stoppingDistance + 0.1f;
        }

        public void Stop()
        {
            Agent.ResetPath();
            hasDestination = false;
        }
    }
}
