using UnityEngine;
using GameJam26.FSM;

namespace GameJam26.Enemy
{
    /// <summary>
    /// 怪物A的有限状态机构建器
    /// </summary>
    public static class MonsterAFSMBuilder
    {
        public static StateMachine<MonsterAContext> Build(MonsterAContext context)
        {
            var fsm = new StateMachine<MonsterAContext>();

            var patrolState = new MonsterAPatrolState();
            var chaseState = new MonsterAChaseState();
            var breakDoorState = new MonsterABreakDoorState();
            var knockbackState = new MonsterAKnockbackState();
            var returnToSpawnPosState = new MonsterAReturnToSpawnPosState();

            // 任意状态 -> 击退状态
            fsm.AddAnyTransition(knockbackState, new PredTransition<MonsterAContext>(
                context => context.isKnockback,
                "Get Knockback"
                ));

            // 巡逻状态 -> 追逐状态
            fsm.AddTransition(patrolState, chaseState, new PredTransition<MonsterAContext>(
                context => context.target != null && context.considerPlayerAsEnemy,
                "Find Target"
                ));

            // 追逐状态 -> 回到起始点
            fsm.AddTransition(chaseState, returnToSpawnPosState, new PredTransition<MonsterAContext>(
                context => !context.considerPlayerAsEnemy || (context.target == null && (context.currentTime - context.lastSeePlayerTime) >= context.Config.lostTargetTimeout),
                "Lose Target"
                ));

            // 回到起始点 -> 巡逻状态
            fsm.AddTransition(returnToSpawnPosState, patrolState, new PredTransition<MonsterAContext>(
                context => context.Motor.Reached(context.Config.patrolMoveStopDistance),
                "Reached Spawn Position"
                ));

            // 回到起始点 -> 追逐状态
            fsm.AddTransition(returnToSpawnPosState, chaseState, new PredTransition<MonsterAContext>(
                context => context.target != null && context.considerPlayerAsEnemy,
                "Find Target While Returning"
                ));

            // 追逐状态 -> 破门状态
            fsm.AddTransition(chaseState, breakDoorState, new PredTransition<MonsterAContext>(
                context => context.currentDoor != null && context.shouldBreakDoor,
                "Door Block Path to Player"
                ));

            // 破门状态 -> 追逐状态
            fsm.AddTransition(breakDoorState, chaseState, new PredTransition<MonsterAContext>(
                context => context.currentDoor == null && !context.shouldBreakDoor && context.considerPlayerAsEnemy && (context.hasLineOfSight || (context.currentTime - context.lastSeePlayerTime) <= context.Config.lostTargetTimeout),
                "Finish Breaking Door and Resume Chase"
                ));

            // 破门状态 -> 巡逻状态
            fsm.AddTransition(breakDoorState, patrolState, new PredTransition<MonsterAContext>(
                context => context.currentDoor == null && !context.shouldBreakDoor && (!context.considerPlayerAsEnemy || (!context.hasLineOfSight && (context.currentTime - context.lastSeePlayerTime) >= context.Config.lostTargetTimeout)),
                "Finish Breaking Door and Lost Target"
                ));

            // 击退状态 -> 追逐状态
            fsm.AddTransition(knockbackState, chaseState, new PredTransition<MonsterAContext>(
                context => !context.isKnockback && context.considerPlayerAsEnemy && (context.hasLineOfSight || (context.currentTime - context.lastSeePlayerTime) <= context.Config.lostTargetTimeout),
                "Knockback Ended and Resume Chase"
                ));

            // 击退状态 -> 巡逻状态
            fsm.AddTransition(knockbackState, patrolState, new PredTransition<MonsterAContext>(
                context => !context.isKnockback && (!context.considerPlayerAsEnemy || (!context.hasLineOfSight && (context.currentTime - context.lastSeePlayerTime) >= context.Config.lostTargetTimeout)),
                "Knockback Ended and Lost Target"
                ));

            fsm.SetInitialState(patrolState, context);
            return fsm;
        }
    }
}
