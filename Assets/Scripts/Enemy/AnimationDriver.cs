using UnityEngine;
namespace GameJam26.Enemy
{
    public interface IAnimationDriver
    {
        void EnterIdle();
        void EnterMove(Vector2 dir);
        void EnterAttack();
        void SetMoveDir(Vector2 dir);
    }
    public class AnimationDriver : IAnimationDriver
    {
        private readonly Animator _anim;
        public AnimationDriver(Animator anim)
        {
            _anim = anim;
        }

        private static readonly int Move = Animator.StringToHash("Move");
        private static readonly int MoveX = Animator.StringToHash("MoveX");
        private static readonly int MoveY = Animator.StringToHash("MoveY");
        private static readonly int Attack = Animator.StringToHash("Attack");

        public void EnterIdle()
        {
            _anim.SetBool(Move, false);
        }

        public void EnterMove(Vector2 dir)
        {
            _anim.SetBool(Move, true);
            SetMoveDir(dir);
        }

        public void SetMoveDir(Vector2 dir)
        {
            _anim.SetFloat(MoveX, dir.x);
            _anim.SetFloat(MoveY, dir.y);
        }

        public void EnterAttack()
        {
            _anim.SetTrigger(Attack);
        }
    }
}