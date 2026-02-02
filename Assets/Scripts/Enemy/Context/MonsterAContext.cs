using UnityEngine;

namespace GameJam26.Enemy
{
    public class MonsterAContext
    {
        // Unity组件
        public Transform Root { get; }
        public IAnimationDriver animationDriver;

        // Runtime对象
        public Transform target;

        // 配置
        public MonsterAConfig Config { get;}

        // 移动能力
        public IActorMotor2D Motor { get; }

        // 门交互
        public Door currentDoor;
        public bool shouldBreakDoor;
        public float doorBreakEndTime;

        // 追逐
        public Vector2 spawnPos;
        public float lastSeePlayerTime;
        public float currentTime;
        public bool hasLineOfSight;             // 当前是否有视线
        public bool considerPlayerAsEnemy;      // 是否将玩家视为敌人

        // 击退
        public bool isKnockback;
        public float knockEndTime;
        public Vector2 knockDirection;
        public float nextBumpTime;
        public float knockbackSpeed;

        // 面具B相关
        public bool isMaskBActive;
        public Transform maskBTarget;


        public MonsterAContext(Transform root, Animator animator, MonsterAConfig config, IActorMotor2D motor)
        {
            Root = root;
            animationDriver = new AnimationDriver(animator);
            Config = config;
            Motor = motor;
        }
    }

}
