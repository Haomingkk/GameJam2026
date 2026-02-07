using UnityEngine;
using GameJam26.FSM;

namespace GameJam26.Enemy
{
    public class MonsterAReturnToSpawnPosState : IState<MonsterAContext>
    {
        public string Name => "ReturnToSpawnPos";

        public void OnEnter(MonsterAContext context)
        {
            context.Motor.Stop();
            context.AnimDriver.EnterMove(Vector2.zero);
            context.Motor.MoveTowards(context.spawnPos, context.Config.patrolSpeed, 0f);
        }

        public void Tick(MonsterAContext context, float deltaTime)
        {
            Vector2 currentDir = context.Motor.GetCurrentVelocity();
            // ÊÓÒ°·¶Î§ÏÔÊ¾
            float degreeFromRight = Mathf.Atan2(currentDir.y, currentDir.x) * Mathf.Rad2Deg;
            float z = degreeFromRight - 90f;
            context.rangeVisual.rotation = Quaternion.Euler(0f, 0f, z);

            if (currentDir.sqrMagnitude > 0.01f)
            {
                currentDir.y = 0;
                currentDir.x = currentDir.x > 0 ? 1 : -1;
                context.AnimDriver.SetMoveDir(currentDir);
            }
        }

        public void OnExit(MonsterAContext context) { }
    }

}

