using GameJam26.Enemy;
using GameJam26.FSM;
using UnityEngine;

namespace GameJam26.Enemy
{
    public class MonsterBBreakDoorState : IState<MonsterBContext>
    {
        public string Name => "BreakDoor";
        public void OnEnter(MonsterBContext context)
        {
            context.animationDriver.EnterAttack();
            context.Motor.Stop();
            context.doorBreakEndTime = context.currentTime + context.Config.doorBreakDuration;
        }
        public void Tick(MonsterBContext context, float deltaTime)
        {
            if (context.currentTime >= context.doorBreakEndTime)
            {
                // 避免卡死在这一步
                context.shouldBreakDoor = false;

                if (context.currentDoor == null)
                {
                    Debug.LogWarning("MonsterBBreakDoorState: currentDoor is already null on break end.");
                    return;
                }

                context.currentDoor.SetOpen(true);
                context.currentDoor = null;
            }
        }
        public void OnExit(MonsterBContext context)
        {

        }
    }
}