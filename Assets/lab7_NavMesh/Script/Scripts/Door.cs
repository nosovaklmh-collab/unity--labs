using UnityEngine;

namespace NavLab
{
    // Дверь — динамическое препятствие. Пробел (или таймер) — открыть/закрыть.
    // На этот же объект нужен NavMeshObstacle с включённым Carve.
    public class Door : MonoBehaviour
    {
        public float openHeight = 3f;
        public float speed = 2f;
        public KeyCode toggleKey = KeyCode.Space;
        [Tooltip("0 — только по клавише; больше 0 — сама открывается/закрывается каждые N секунд")]
        public float autoToggleInterval = 0f;

        private Vector3 closedPos;
        private Vector3 openPos;
        private bool isOpen;
        private float timer;

        void Start()
        {
            closedPos = transform.position;
            openPos = closedPos + Vector3.up * openHeight;
        }

        void Update()
        {
            if (Input.GetKeyDown(toggleKey)) Toggle();

            if (autoToggleInterval > 0f)
            {
                timer += Time.deltaTime;
                if (timer >= autoToggleInterval) { timer = 0f; Toggle(); }
            }

            Vector3 target = isOpen ? openPos : closedPos;
            transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);
        }

        void Toggle()
        {
            isOpen = !isOpen;
            Debug.Log($"Дверь: {(isOpen ? "открыта" : "закрыта")}");
        }
    }
}
