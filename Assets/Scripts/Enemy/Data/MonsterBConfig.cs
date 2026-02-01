using UnityEngine;

namespace GameJam26.Enemy
{
    [CreateAssetMenu(fileName = "MonsterBConfig", menuName = "Enemy/MonsterB Config")]
    public class MonsterBConfig : ScriptableObject
    {
        [Header("Patrol")]
        public float patrolSpeed = 3.0f;
        public float patrolIdleDuration = 5.0f;    // 巡逻移动等待时间
        public float patrolRadius = 5.0f;          // 巡逻半径
        public float patrolMoveStopDistance = 0.2f;// 走到点多近算到达
        [Header("Chase")]
        public float chaseSpeed = 5.0f;
        public float senseDistance = 8.0f;         // 视野距离
        public float stopDistance = 0.1f;          // 追逐时与目标的最小距离
        public float lostTargetTimeout = 2.0f;     // 失去目标后，放弃追逐的时间
        public float doorBreakDuration = 3.0f;     // 破门所需时间
        [Header("Knockback")]
        public float knockbackDistance = 5.0f;     // 击退距离
        public float knockbackDuration = 0.5f;     // 击退持续时间
        public float bumpCooldown = 0.4f;          // 碰撞后冷却时间 
    }
}
