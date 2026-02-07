using GameJam2026.GamePlay;
using GameJam26.FSM;
using UnityEngine;

namespace GameJam26.Enemy
{
    /// <summary>
    /// 怪物A的Chase状态
    /// </summary>
    public class MonsterAChaseState : IState<MonsterAContext>
    {
        public string Name => "Chase";
        public void OnEnter(MonsterAContext context)
        {
            if (context.target == null)
            {
                Debug.Log("MonsterAChaseState OnEnter called but target is null");
                return;
            }
            Vector3 targetPos = context.target.position;


            context.Motor.MoveTowards(targetPos, context.Config.chaseSpeed, 0f);
            Vector2 currentDir = context.Motor.GetCurrentVelocity();
            currentDir.y = 0;
            currentDir.x = currentDir.x > 0 ? 1 : -1;
            context.AnimDriver.EnterMove(currentDir);
        }

        public void Tick(MonsterAContext context, float deltaTime)
        {
            if (context.target == null)
            {
                Debug.Log("MonsterAChaseState Tick called but target is null");
                return;
            }
            Vector3 targetPos = context.target.position;
            //Debug.Log("Chasing target at position: " + targetPos);
            context.Motor.MoveTowards(targetPos, context.Config.chaseSpeed, deltaTime);
            Vector2 currentDir = context.Motor.GetCurrentVelocity().normalized;

            // 视野范围显示
            float degreeFromRight = Mathf.Atan2(currentDir.y, currentDir.x) * Mathf.Rad2Deg;
            float z = degreeFromRight - 90f;
            context.rangeVisual.rotation = Quaternion.Euler(0f, 0f, z);

            currentDir.y = 0;
            currentDir.x = currentDir.x > 0 ? 1 : -1;
            context.AnimDriver.SetMoveDir(currentDir);
        }

        public void OnExit(MonsterAContext context)
        {
            context.target = null;
            context.Motor.Stop();
        }
    }
}