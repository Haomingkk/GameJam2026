using UnityEngine;
using GameJam26.Enemy;
using GameJam26.FSM;

public class MonsterBKnockbackState : IState<MonsterBContext>
{
    public string Name => "Knockback";
    public void OnEnter(MonsterBContext context)
    {
        context.Motor.Stop();
        context.Motor.DisableMotor();
    }
    public void Tick(MonsterBContext context, float deltaTime)
    {
        context.Root.position += (Vector3)(context.knockDirection * context.knockbackSpeed * deltaTime);

        if (context.currentTime >= context.knockEndTime)
        {
            context.isKnockback = false;
        }
    }
    public void OnExit(MonsterBContext context)
    {
        context.Motor.EnableMotor();
        context.Motor.Stop();
    }
}
