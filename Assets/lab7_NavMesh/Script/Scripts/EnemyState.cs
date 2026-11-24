// ЛР «Патрульный враг на NavMesh» — состояния врага.
// Свой namespace NavLab нужен потому, что в проекте уже есть EnemyState из Lab3_FSM
// (там Patrol, Chase, Attack). Без namespace Unity выдаст ошибку «тип уже определён».
namespace NavLab
{
    public enum EnemyState { Patrol, Chase, Search, Return }
}
