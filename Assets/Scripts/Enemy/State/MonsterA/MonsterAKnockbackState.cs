using UnityEngine;

using GameJam26.FSM;

namespace GameJam26.Enemy
{
    public class MonsterAKnockbackState : IState<MonsterAContext>
    {
        public string Name => "Knockback";

        public void OnEnter(MonsterAContext context)
        {
            context.Motor.Stop();
            context.Motor.DisableMotor();
        }

        public void Tick(MonsterAContext context, float deltaTime)
        {
            context.Root.position += (Vector3)(context.knockDirection * context.knockbackSpeed * deltaTime);

            if (context.currentTime >= context.knockEndTime)
            {
                context.isKnockback = false;
            }
        }

        public void OnExit(MonsterAContext context)
        {
            context.Motor.EnableMotor();
            context.Motor.Stop();
        }
    }
}