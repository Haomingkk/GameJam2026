using UnityEngine;

namespace GameJam26.Enemy
{
    [CreateAssetMenu(fileName = "MonsterAConfig", menuName = "Enemy/MonsterA Config")]
    public class MonsterAConfig : ScriptableObject
    {
        [Header("Patrol")]
        public float patrolSpeed = 0f;
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
        [Header("Face Sprite")]
        public Sprite monsterFace;                  // 怪物面部图像
    }

}
