using UnityEngine;
using UnityEngine.AI;

namespace GameJam26.Enemy
{
    public class NavAgentMotor2D : IActorMotor2D
    {
        private readonly NavMeshAgent _agent;
        private readonly Transform _self;

        public NavAgentMotor2D(NavMeshAgent agent, Transform self)
        {
            _agent = agent;
            _self = self;
            _agent.updateUpAxis = false;
            _agent.updateRotation = false;
        }

        public void MoveTowards(Vector2 targetPos, float speed, float dt)
        {
            _agent.isStopped = false;
            _agent.speed = speed;
            var destination = new Vector3(targetPos.x, targetPos.y, _self.position.z);
            _agent.SetDestination(destination);
        }

        public void Stop()
        {
            _agent.isStopped = true;
            _agent.ResetPath();
        }

        public Vector2 GetCurrentVelocity()
        {
            return new Vector2(_agent.velocity.x, _agent.velocity.y);
        }

        public void DisableMotor()
        {
            _agent.enabled = false;
        }

        public void EnableMotor()
        {
            _agent.enabled = true;
        }

    }

}
