// Часть 3: общий интерфейс для всех состояний
public interface IEnemyState
{
    void Enter(EnemyFSM enemy);   // вызывается один раз при входе
    void Update(EnemyFSM enemy);  // вызывается каждый кадр
    void Exit(EnemyFSM enemy);    // вызывается один раз при выходе
}