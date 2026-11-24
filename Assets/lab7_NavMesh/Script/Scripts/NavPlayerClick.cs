using UnityEngine;
using UnityEngine.AI;

namespace NavLab
{
    // Домашняя часть 1: игрок тоже NavMeshAgent и ходит по клику мышью.
    // Приоритет avoidance: меньше число — «важнее». У игрока 10, у врагов 50 —
    // враги уступают дорогу игроку, а он их «расталкивает».
    [RequireComponent(typeof(NavMeshAgent))]
    public class NavPlayerClick : MonoBehaviour
    {
        public int avoidancePriority = 10;

        private NavMeshAgent agent;

        void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
            agent.avoidancePriority = avoidancePriority;
        }

        void Update()
        {
            if (!Input.GetMouseButtonDown(0) || Camera.main == null) return;

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 200f, ~0, QueryTriggerInteraction.Ignore))
            {
                if (NavMesh.SamplePosition(hit.point, out var navHit, 1f, NavMesh.AllAreas))
                    agent.SetDestination(navHit.position);
            }
        }
    }
}
