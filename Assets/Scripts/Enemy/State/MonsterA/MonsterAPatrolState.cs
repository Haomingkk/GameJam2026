using GameJam26.FSM;
using UnityEngine;

namespace GameJam26.Enemy
{
    /// <summary>
    /// 怪物A的巡逻状态
    /// </summary>
    public class MonsterAPatrolState : IState<MonsterAContext>
    {
        public string Name => "Patrol";

        public void OnEnter(MonsterAContext context)
        {
            Debug.Log("MonsterA Entering Patrol State");
            context.lastChangeDirectionTime = context.currentTime;
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
            float degreeFromRight = Mathf.Atan2(context.CurrentDirection.y, context.CurrentDirection.x) * Mathf.Rad2Deg;
            float z = degreeFromRight - 90f;
            context.rangeVisual.rotation = Quaternion.Euler(0f, 0f, z);
            context.Motor.Stop();
            context.AnimDriver.EnterIdle(context.CurrentDirection);
        }

        public void Tick(MonsterAContext context, float deltaTime)
        {
            var timeSinceLastChange = context.currentTime - context.lastChangeDirectionTime;
            if (timeSinceLastChange >= context.Config.patrolChangeDirectionInterval)
            {
                // 改变方向
                context.lastChangeDirectionTime = context.currentTime;
                context.currentDirection = (FaceDirection)(((int)context.currentDirection + 1) % context.Config.directionCount);
                context.AnimDriver.EnterIdle(context.CurrentDirection);

                // 更新视觉范围方向
                float degreeFromRight = Mathf.Atan2(context.CurrentDirection.y, context.CurrentDirection.x) * Mathf.Rad2Deg;
                float z = degreeFromRight - 90f;
                context.rangeVisual.rotation = Quaternion.Euler(0f, 0f, z);
            }
        }

        public void OnExit(MonsterAContext context)
        {
            
        }
    }
}