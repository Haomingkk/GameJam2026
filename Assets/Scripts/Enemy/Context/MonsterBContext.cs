using UnityEngine;

namespace GameJam26.Enemy
{
    public class MonsterBContext
    {
        // Unity组件
        public Transform Root { get; }
        public IAnimationDriver animationDriver;

        // Runtime对象
        public Transform target;

        // 配置
        public MonsterBConfig Config { get; }

        // 移动能力
        public IActorMotor2D Motor { get; }

        // 状态时间
        public float currentTime;

        // 门交互
        public Door currentDoor;
        public bool shouldBreakDoor;
        public float doorBreakEndTime;

        // 追逐
        public Vector2 spawnPos;
        public float lastSeePlayerTime;
        public bool hasLineOfSight;             // 当前是否有视线
        public bool considerPlayerAsEnemy;      // 是否将玩家视为敌人

        // 击退
        public bool isKnockback;
        public float knockEndTime;
        public Vector2 knockDirection;
        public float nextBumpTime;
        public float knockbackSpeed;

        // 巡逻
        public float patrolIdleEndTime;
        public Vector2 patrolTargetPos;

        public MonsterBContext(Transform root, Animator animator, MonsterBConfig config, IActorMotor2D motor)
        {
            Root = root;
            animationDriver = new AnimationDriver(animator);
            Config = config;
            Motor = motor;
        }
    }
}