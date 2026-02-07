using UnityEngine;
using GameJam26.FSM;

namespace GameJam26.Enemy
{
    public class MonsterASpecialChaseState : IState<MonsterAContext>
    {
        public string Name => "Special Chase";

        public void OnEnter(MonsterAContext context)
        {
            if (context.maskBTarget == null)
            {
                Debug.Log("MonsterASpecialChaseState OnEnter called but maskBTarget is null");
                return;
            }
            context.Motor.MoveTowards(context.maskBTarget.position, context.Config.chaseSpeed, 0f);
            Vector2 currentDir = context.Motor.GetCurrentVelocity();
            currentDir.y = 0;
            currentDir.x = currentDir.x > 0 ? 1 : -1;
            context.AnimDriver.EnterMove(currentDir);
        }

        public void Tick(MonsterAContext context, float deltaTime)
        {
            if (context.maskBTarget == null)
            {
                Debug.Log("MonsterASpecialChaseState Tick called but maskBTarget is null");
                return;
            }
            Vector3 targetPos = context.maskBTarget.position;
            //Debug.Log("Chasing target at position: " + targetPos);
            context.Motor.MoveTowards(targetPos, context.Config.chaseSpeed, deltaTime);
            Vector2 currentDir = context.Motor.GetCurrentVelocity();
            currentDir.y = 0;
            currentDir.x = currentDir.x > 0 ? 1 : -1;
            context.AnimDriver.SetMoveDir(currentDir);
        }

        public void OnExit(MonsterAContext context)
        {
            context.maskBTarget = null;
            context.Motor.Stop();
        }

        
    }

}
