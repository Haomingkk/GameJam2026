using UnityEngine;

namespace GameJam26.Enemy
{
    public class RigidbodyMotor2D : IActorMotor2D
    {
        private readonly Rigidbody2D _rb;

        public RigidbodyMotor2D(Rigidbody2D rb)
        {
            _rb = rb;
        }

        public void MoveTowards(Vector2 targetPos, float speed, float dt)
        {

        }

        public Vector2 GetCurrentVelocity()
        {
            return _rb.linearVelocity;
        }

        public void Stop()
        {
            _rb.linearVelocity = Vector2.zero;
        }

        public void DisableMotor()
        {
            throw new System.NotImplementedException();
        }

        public void EnableMotor()
        {
            throw new System.NotImplementedException();
        }

        public bool Reached(float stopDistance)
        {
            throw new System.NotImplementedException();
        }
    }
}