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
            var specialChaseState = new MonsterASpecialChaseState();
            var idleState = new MonsterAIdleState();

            // 任意状态 -> 击退状态
            fsm.AddAnyTransition(knockbackState, new PredTransition<MonsterAContext>(
                context => context.isKnockback,
                "Get Knockback"
                ));

            // 巡逻状态 -> 收到Player佩戴面具的通知，进入特殊追逐状态
            fsm.AddTransition(patrolState, specialChaseState, new PredTransition<MonsterAContext>(
                context => context.maskBTarget != null && context.isMaskBActive,
                "Player Wears Mask B While Patrolling"
                ));

            // 回到起始点 -> 特殊追逐状态
            fsm.AddTransition(returnToSpawnPosState, specialChaseState, new PredTransition<MonsterAContext>(
                context => context.maskBTarget != null && context.isMaskBActive,
                "Player Wears Mask B While Returning"
                ));

            // 特殊追逐状态 -> 回到起始点
            fsm.AddTransition(specialChaseState, returnToSpawnPosState, new PredTransition<MonsterAContext>(
                context => !context.isMaskBActive || context.maskBTarget == null,
                "Player Removes Mask B While Special Chasing"
                ));

            // 特殊追逐状态 -> 追逐状态
            fsm.AddTransition(specialChaseState, chaseState, new PredTransition<MonsterAContext>(
                context => context.hasLineOfSight && context.considerPlayerAsEnemy && context.target != null,
                "See Player While Special Chasing"
                ));

            // 巡逻状态 -> 追逐状态
            fsm.AddTransition(patrolState, chaseState, new PredTransition<MonsterAContext>(
                context => context.target != null && context.considerPlayerAsEnemy,
                "Find Target"
                ));

            // 追逐状态 -> 原地Idle状态(只持续短暂几秒)
            fsm.AddTransition(chaseState, idleState, new PredTransition<MonsterAContext>(
                context => !context.considerPlayerAsEnemy || ((context.currentTime - context.lastSeePlayerTime) >= context.Config.lostTargetTimeout),
                "Lose Target"
                ));

            // 原地Idle状态 -> 回到起始点
            fsm.AddTransition(idleState, returnToSpawnPosState, new PredTransition<MonsterAContext>(
                context => context.currentTime - context.enterIdleTime >= context.Config.idleAfterLostTargetTimeout,
                "Finish Idle and Return to Spawn Position"
                ));

            // 原地Idle状态 -> 追逐状态
            fsm.AddTransition(idleState, chaseState, new PredTransition<MonsterAContext>(
                context => context.target != null && context.considerPlayerAsEnemy,
                "Find Target While Idling"
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
