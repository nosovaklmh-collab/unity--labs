using System.Collections;
using UnityEngine;
using UnityEngine.Events;

// ¬ариант 2: аптечка. ¬осстанавливает HP через PlayerHealth.Heal(),
// после чего игрок вызывает событие OnHealed Ч UI обновитс€ сам.
// јптечка исчезает и через несколько секунд по€вл€етс€ снова.
public class Medkit : MonoBehaviour
{
    [SerializeField] private int healAmount = 30;
    [SerializeField] private float respawnTime = 5f;
    [SerializeField] private float rotateSpeed = 90f;   // крутитс€, чтобы было заметно

    // —обытие подбора Ч в инспекторе к нему подключаетс€ звук AudioSource.Play()
    public UnityEvent OnPickedUp;

    private Renderer[] renderers;
    private Collider col;

    void Awake()
    {
        renderers = GetComponentsInChildren<Renderer>();
        col = GetComponent<Collider>();
    }

    void Update()
    {
        transform.Rotate(0f, rotateSpeed * Time.deltaTime, 0f, Space.World);
    }

    void OnTriggerEnter(Collider other)
    {
        PlayerHealth player = other.GetComponent<PlayerHealth>();
        if (player != null && player.Heal(healAmount))   // при полном HP аптечка не тратитс€
        {
            OnPickedUp?.Invoke();
            StartCoroutine(Respawn());
        }
    }

    IEnumerator Respawn()
    {
        SetVisible(false);
        yield return new WaitForSeconds(respawnTime);
        SetVisible(true);
    }

    void SetVisible(bool visible)
    {
        foreach (var r in renderers) r.enabled = visible;
        col.enabled = visible;
    }
}