using GameJam26.Enemy;
using GameJam26.FSM;
using UnityEngine;

namespace GameJam26.Enemy
{
    public class MonsterBChaseState : IState<MonsterBContext>
    {
        public string Name => "Chase";
        public void OnEnter(MonsterBContext context)
        {
            Debug.Log("MonsterB Entering Chase State");
            Vector3 targetPos = context.target.position;

            context.Motor.MoveTowards(targetPos, context.Config.chaseSpeed, 0f);
            Vector2 currentDir = context.Motor.GetCurrentVelocity();
            currentDir.y = 0;
            currentDir.x = currentDir.x > 0 ? 1 : -1;
            context.animationDriver.EnterMove(currentDir);
        }
        public void Tick(MonsterBContext context, float deltaTime)
        {
            if (context.target == null)
            {
                Debug.LogWarning("MonsterBChaseState Tick called but target is null");
                return;
            }
            Vector3 targetPos = context.target.position;

            context.Motor.MoveTowards(targetPos, context.Config.chaseSpeed, deltaTime);
            Vector2 currentDir = context.Motor.GetCurrentVelocity();
            currentDir.y = 0;
            currentDir.x = currentDir.x > 0 ? 1 : -1;
            context.animationDriver.SetMoveDir(currentDir);
        }
        public void OnExit(MonsterBContext context)
        {
            context.target = null;
            context.Motor.Stop();
            Debug.Log("MonsterB Exiting Chase State");
        }
    }

}
