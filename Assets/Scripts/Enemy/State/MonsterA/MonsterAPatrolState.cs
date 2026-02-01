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
            context.animationDriver.EnterIdle();
        }

        public void Tick(MonsterAContext context, float deltaTime)
        {
            //Debug.Log("Patrolling at position: " + context.Root.position);
        }

        public void OnExit(MonsterAContext context)
        {
            
        }
    }
}