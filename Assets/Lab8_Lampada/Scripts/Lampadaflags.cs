using System;
using System.Collections.Generic;
using UnityEngine;

// «Лампада» — флаги игрока: метки «есть или нет» (как в лекции про диалоги и квесты).
// Например: "key_exit" — у девочки есть ключ от выхода.
// Вешается на Player.
public class LampadaFlags : MonoBehaviour
{
    [Tooltip("Только для просмотра: какие флаги сейчас есть")]
    public List<string> current = new List<string>();

    public event Action<string> OnFlagAdded;   // другие скрипты могут подписаться

    private readonly HashSet<string> flags = new HashSet<string>();

    public void Add(string flag)
    {
        if (string.IsNullOrEmpty(flag)) return;
        if (flags.Add(flag))
        {
            current.Add(flag);
            Debug.Log("Лампада: получен флаг " + flag);
            OnFlagAdded?.Invoke(flag);
        }
    }

    // Пустой флаг = условия нет, значит «можно»
    public bool Has(string flag) => string.IsNullOrEmpty(flag) || flags.Contains(flag);
}