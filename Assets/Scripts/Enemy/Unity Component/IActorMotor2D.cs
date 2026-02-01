using UnityEngine;

namespace GameJam26.Enemy
{
    public interface IActorMotor2D
    {
        void MoveTowards(Vector2 targetPos, float speed, float dt);
        Vector2 GetCurrentVelocity();
        void Stop();

        void DisableMotor();

        void EnableMotor();
    }
}