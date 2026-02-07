using UnityEngine;
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
            context.animationDriver.EnterIdle(Vector2.zero);
        }
        public void Tick(MonsterBContext context, float deltaTime) { }
        public void OnExit(MonsterBContext context) { }
    }
}