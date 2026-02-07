using UnityEngine;
using GameJam26.FSM;

namespace GameJam26.Enemy
{
    /// <summary>
    /// 怪物A的Idle状态
    /// </summary>
    public class MonsterAIdleState : IState<MonsterAContext>
    {
        public string Name => "Idle";

        public void OnEnter(MonsterAContext context)
        {
            var speed = context.Motor.GetCurrentVelocity();
            if (speed.sqrMagnitude < 1e-4f)
            {
                context.currentDirection = FaceDirection.Down;
            }
            else
            {
                float ax = Mathf.Abs(speed.x);
                float ay = Mathf.Abs(speed.y);
                if (ax >= ay)
                {
                    context.currentDirection = speed.x > 0 ? FaceDirection.Right : FaceDirection.Left;
                }
                else
                {
                    context.currentDirection = speed.y > 0 ? FaceDirection.Up : FaceDirection.Down;
                }
            }

            context.enterIdleTime = context.currentTime;
            context.Motor.Stop();
            context.AnimDriver.EnterIdle(context.CurrentDirection);
        }

        public void Tick(MonsterAContext context, float deltaTime) { }

        public void OnExit(MonsterAContext context) { }
    }
}
