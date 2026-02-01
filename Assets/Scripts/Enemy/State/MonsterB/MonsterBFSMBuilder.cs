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

            return fsm;
        }
    }
}