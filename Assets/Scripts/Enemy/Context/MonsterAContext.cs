using UnityEngine;

namespace GameJam26.Enemy
{
    public enum FaceDirection
    {
        Down = 0,
        Left = 1,
        Up = 2,
        Right = 3
    }

    public class MonsterAContext
    {
        // Unity组件
        public Transform Root { get; }
        public IAnimationDriver AnimDriver { get; }

        // Runtime对象
        public Transform target;

        // 配置
        public MonsterAConfig Config { get; }

        // 移动能力
        public IActorMotor2D Motor { get; }

        // 侦查范围显示
        public Transform rangeVisual;

        // 门交互
        public Door currentDoor;
        public bool shouldBreakDoor;
        public float doorBreakEndTime;

        // 方向
        public static readonly Vector2[] DirVec =
        {
            Vector2.down,
            Vector2.left,
            Vector2.up,
            Vector2.right,
        };
        public Vector2 CurrentDirection => DirVec[(int)currentDirection];



        // 巡逻
        public FaceDirection currentDirection;
        public float lastChangeDirectionTime;

        // 追逐
        public Vector2 spawnPos;
        public float lastSeePlayerTime;
        public float enterIdleTime;
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
            AnimDriver = new AnimationDriver(animator);
            Config = config;
            Motor = motor;
        }
    }

}
