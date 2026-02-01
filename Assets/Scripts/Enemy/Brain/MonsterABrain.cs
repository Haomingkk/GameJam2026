using UnityEngine;
using UnityEngine.AI;
using GameJam2026.GamePlay;
using GameJam26.FSM;

namespace GameJam26.Enemy
{
    [RequireComponent(typeof(NavMeshAgent))]
    [RequireComponent(typeof(Collider2D))]
    [RequireComponent(typeof(Animator))]
    public class MonsterABrain : MonoBehaviour
    {
        [SerializeField]
        private MonsterAConfig config;
        [SerializeField]
        private Transform _chaseTarget;     // 追逐目标(玩家)


        private MonsterAContext _context;
        private StateMachine<MonsterAContext> _fsm;



        private void Awake()
        {
            var agent = GetComponent<NavMeshAgent>();
            var anim = GetComponent<Animator>();
            _context = new MonsterAContext(transform, anim, config, new NavAgentMotor2D(agent, transform))
            {
                spawnPos = transform.position,
                nextBumpTime = Time.time,
                knockbackSpeed = config.knockbackDistance / config.knockbackDuration,
            };

            _fsm = MonsterAFSMBuilder.Build(_context);
        }

        private void Update()
        {
            _context.currentTime = Time.time;

            bool see = _SensePlayer();

            _context.hasLineOfSight = see;
            _context.considerPlayerAsEnemy = PlayerController.instance.GetCurrentMaskState() != MaskState.MaskA;

            if (see)
            {
                _context.target = _chaseTarget;
                _context.lastSeePlayerTime = Time.time;
            }
            else
            {
                _context.target = null;
            }

            _fsm.Tick(_context, Time.deltaTime);
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.CompareTag("Door"))
            {
                if (_fsm.Current != null && _fsm.Current.Name == "Chase")
                {
                    var door = collision.GetComponent<Door>();
                    if (door != null)
                    {
                        _context.currentDoor = door;
                        _context.shouldBreakDoor = true;
                    }
                }
            }
            else if (collision.CompareTag("Player"))
            {
                if (Time.time < _context.nextBumpTime)
                {
                    return;
                }
                var player = collision.GetComponent<PlayerController>();
                if (player == null)
                {
                    return;
                }
                _context.nextBumpTime = Time.time + _context.Config.bumpCooldown;
                Vector2 dirToPlayer = (player.transform.position - transform.position).normalized;
                // 调用玩家的knockback方法
                player.OnPlayerDamaged(dirToPlayer);
                // Boss knockback
                _StartKnockback(-dirToPlayer);
            }
        }

        private void _StartKnockback(Vector2 directionAwayFromPlayer)
        {
            _context.isKnockback = true;
            _context.knockEndTime = Time.time + _context.Config.knockbackDuration;
            _context.knockDirection = directionAwayFromPlayer;
        }

        private bool _SensePlayer()
        {
            RaycastHit2D hit = Physics2D.Raycast(_context.Root.position, (_chaseTarget.position - _context.Root.position).normalized, _context.Config.senseDistance);
            if (hit.collider != null && (hit.transform == _chaseTarget || hit.transform.IsChildOf(_chaseTarget)))
            {
                return true;
            }
            return false;
        }
    }
}

