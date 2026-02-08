using UnityEngine;
using GameJam26.FSM;

namespace GameJam26.Enemy
{
    /// <summary>
    /// 怪物B的有限状态机构建器
    /// </summary>
    public class MonsterBFSMBuilder
    {
        public static StateMachine<MonsterBContext> Build(MonsterBContext context)
        {
            var fsm = new StateMachine<MonsterBContext>();

            var patrolState = new MonsterBPatrolState();
            var chaseState = new MonsterBChaseState();
            var breakDoorState = new MonsterBBreakDoorState();
            var knockbackState = new MonsterBKnockbackState();
            var idleState = new MonsterBIdleState();

            // 任意状态 -> 击退状态
            fsm.AddAnyTransition(knockbackState, new PredTransition<MonsterBContext>(
                context => context.isKnockback,
                "Get Knockback"
                ));

            // 待机状态 -> 巡逻状态
            fsm.AddTransition(idleState, patrolState, new PredTransition<MonsterBContext>(
                context => context.currentTime >= context.patrolIdleEndTime,
                "End Idle and Start Patrolling"
                ));

            // 巡逻状态 -> 待机状态
            fsm.AddTransition(patrolState, idleState, new PredTransition<MonsterBContext>(
                context => context.Motor.Reached(context.Config.patrolMoveStopDistance),
                "End Patrolling and Start Idle"
                ));

            // 待机状态 -> 追逐状态
            fsm.AddTransition(idleState, chaseState, new PredTransition<MonsterBContext>(
                context => context.target != null && context.considerPlayerAsEnemy,
                "Find Target From Idle"
                ));

            // 巡逻状态 -> 追逐状态
            fsm.AddTransition(patrolState, chaseState, new PredTransition<MonsterBContext>(
                context => context.target != null && context.considerPlayerAsEnemy,
                "Find Target From Patrol"
                ));

            // 追逐状态 -> 待机状态
            fsm.AddTransition(chaseState, idleState, new PredTransition<MonsterBContext>(
                context => !context.considerPlayerAsEnemy || (context.target == null && (context.currentTime - context.lastSeePlayerTime) >= context.Config.lostTargetTimeout),
                "Lose Target"
                ));

            // 追逐状态 -> 破门状态
            fsm.AddTransition(chaseState, breakDoorState, new PredTransition<MonsterBContext>(
                context => context.currentDoor != null && context.shouldBreakDoor,
                "Door Block Path to Player"
                ));

            // 破门状态 -> 追逐状态
            fsm.AddTransition(breakDoorState, chaseState, new PredTransition<MonsterBContext>(
                context => context.currentDoor == null && !context.shouldBreakDoor && context.considerPlayerAsEnemy && (context.hasLineOfSight || (context.currentTime - context.lastSeePlayerTime) <= context.Config.lostTargetTimeout),
                "Finish Breaking Door and Resume Chase"
                ));

            // 破门状态 -> 巡逻状态
            fsm.AddTransition(breakDoorState, patrolState, new PredTransition<MonsterBContext>(
                context => context.currentDoor == null && !context.shouldBreakDoor && (!context.considerPlayerAsEnemy || (!context.hasLineOfSight && (context.currentTime - context.lastSeePlayerTime) >= context.Config.lostTargetTimeout)),
                "Finish Breaking Door and Lost Target"
                ));

            // 击退状态 -> 追逐状态
            fsm.AddTransition(knockbackState, chaseState, new PredTransition<MonsterBContext>(
                context => !context.isKnockback && context.considerPlayerAsEnemy && (context.hasLineOfSight || (context.currentTime - context.lastSeePlayerTime) <= context.Config.lostTargetTimeout),
                "Knockback Ended and Resume Chase"
                ));

            // 击退状态 -> 巡逻状态
            fsm.AddTransition(knockbackState, patrolState, new PredTransition<MonsterBContext>(
                context => !context.isKnockback && (!context.considerPlayerAsEnemy || (!context.hasLineOfSight && (context.currentTime - context.lastSeePlayerTime) >= context.Config.lostTargetTimeout)),
                "Knockback Ended and Lost Target"
                ));


            fsm.SetInitialState(patrolState, context);
            return fsm;
        }
    }
}