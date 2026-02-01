using GameJam26.FSM;

namespace GameJam26.Enemy
{
    public class MonsterBIdleState : IState<MonsterBContext>
    {
        public string Name => "Idle";
        public void OnEnter(MonsterBContext context)
        {
            context.Motor.Stop();
            context.patrolIdleEndTime = context.currentTime + context.Config.patrolIdleDuration;
            context.animationDriver.EnterIdle();
        }
        public void Tick(MonsterBContext context, float deltaTime) { }
        public void OnExit(MonsterBContext context) { }
    }
}