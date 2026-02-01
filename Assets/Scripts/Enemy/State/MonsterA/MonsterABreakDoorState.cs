using GameJam26.FSM;
using UnityEngine;

namespace GameJam26.Enemy
{
    public class MonsterABreakDoorState : IState<MonsterAContext>
    {
        public string Name => "BreakDoor";

        public void OnEnter(MonsterAContext context)
        {
            context.animationDriver.EnterAttack();
            context.Motor.Stop();
        }

        public void Tick(MonsterAContext context, float deltaTime)
        {

        }

        public void OnExit(MonsterAContext context)
        {

        }
    }

}
