using GameJam26.Enemy;
using GameJam26.FSM;
using UnityEngine;
using UnityEngine.AI;

namespace GameJam26.Enemy
{
    public class MonsterBPatrolState : IState<MonsterBContext>
    {
        public string Name => "Patrol";
        public void OnEnter(MonsterBContext context)
        {
            Debug.Log("MonsterB Entering Patrol State");

            context.target = null;

            context.animationDriver.EnterMove(Vector2.zero);

            context.patrolTargetPos = PickRandomNavPointNear(context, context.spawnPos, context.Config.patrolRadius);

            context.Motor.MoveTowards(context.patrolTargetPos, context.Config.patrolSpeed, 0f);
        }
        public void Tick(MonsterBContext context, float deltaTime)
        {
            Vector2 currentDir = context.Motor.GetCurrentVelocity();
            if (currentDir.sqrMagnitude > 0.01f)
            {
                currentDir.y = 0;
                currentDir.x = currentDir.x > 0 ? 1 : -1;
                context.animationDriver.SetMoveDir(currentDir);
            }
            
        }
        public void OnExit(MonsterBContext context)
        {
            Debug.Log("MonsterB Exiting Patrol State");
        }

        private static Vector2 PickRandomNavPointNear(MonsterBContext context, Vector2 center, float radius)
        {
            for (int i = 0; i < 8; i++)
            {
                Vector2 candidate = center + Random.insideUnitCircle * radius;

                // 你项目如果是 XY 平面导航：Vector3(x, y, z固定)
                var pos3 = new Vector3(candidate.x, candidate.y, context.Root.position.z);

                if (NavMesh.SamplePosition(pos3, out var hit, 1.0f, NavMesh.AllAreas))
                    return new Vector2(hit.position.x, hit.position.y);
            }

            // 兜底：回到中心点附近
            return center;
        }
    }


}

