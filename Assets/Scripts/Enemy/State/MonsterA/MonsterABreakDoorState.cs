using GameJam26.FSM;
using UnityEngine;

namespace GameJam26.Enemy
{
    public class MonsterABreakDoorState : IState<MonsterAContext>
    {
        public string Name => "BreakDoor";

        public void OnEnter(MonsterAContext context)
        {
            context.AnimDriver.EnterAttack();
            context.Motor.Stop();
            context.doorBreakEndTime = context.currentTime + context.Config.doorBreakDuration;
        }

        public void Tick(MonsterAContext context, float deltaTime)
        {
            if (context.currentTime >= context.doorBreakEndTime)
            {
                // 避免卡死在这一步
                context.shouldBreakDoor = false;
                
                if (context.currentDoor == null)
                {
                    Debug.LogWarning("MonsterABreakDoorState: currentDoor is already null on break end.");
                    return;
                }

                context.currentDoor.SetOpen(true);
                context.currentDoor = null;
            }
        }

        public void OnExit(MonsterAContext context)
        {

        }
    }

}
