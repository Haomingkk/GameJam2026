using UnityEngine;
using UnityEngine.AI;
using GameJam2026.GamePlay;
using GameJam26.FSM;
using GameJam26.Core;

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
        [SerializeField]
        private Transform _rangeVisual;     // 侦查范围显示


        private MonsterAContext _context;
        private StateMachine<MonsterAContext> _fsm;



        private void Awake()
        {
            var agent = GetComponent<NavMeshAgent>();
            var anim = GetComponent<Animator>();
            if (_chaseTarget == null)
            {
                var player = FindFirstObjectByType<PlayerController>();
                if (player != null)
                {
                    _chaseTarget = player.transform;
                }
                else
                {
                    Debug.LogError("MonsterABrain: Chase target is not assigned and no PlayerController found in the scene.");
                }
            }
            _context = new MonsterAContext(transform, anim, config, new NavAgentMotor2D(agent, transform))
            {
                spawnPos = transform.position,
                nextBumpTime = Time.time,
                knockbackSpeed = config.knockbackDistance / config.knockbackDuration,
                rangeVisual = _rangeVisual,
            };

            _fsm = MonsterAFSMBuilder.Build(_context);
        }

        private void OnEnable()
        {
            EventHandler.NotifyActiveMaskB += _OnNotifyActiveMaskB;
            EventHandler.NotifyDeactiveMaskB += _OnNotifyDeactiveMaskB;
        }
        private void OnDisable()
        {
            EventHandler.NotifyActiveMaskB -= _OnNotifyActiveMaskB;
            EventHandler.NotifyDeactiveMaskB -= _OnNotifyDeactiveMaskB;
        }

        private void _OnNotifyActiveMaskB(Transform player)
        {
            float sqrDistance = (player.position - transform.position).sqrMagnitude;
            if (sqrDistance < _context.Config.maskBTriggerMonsterADistance * _context.Config.maskBTriggerMonsterADistance)
            {
                _context.isMaskBActive = true;
                _context.maskBTarget = player;
            }
        }

        private void _OnNotifyDeactiveMaskB(Transform player)
        {
            _context.isMaskBActive = false;
            _context.maskBTarget = null;
        }

        private void Update()
        {
            _context.currentTime = Time.time;

            bool see = _SensePlayer();

            _context.hasLineOfSight = see;
            _context.considerPlayerAsEnemy = PlayerController.instance.GetCurrentMaskState() != MaskState.MaskA;

            if (see && _context.considerPlayerAsEnemy)
            {
                _context.target = _chaseTarget;
                _context.lastSeePlayerTime = Time.time;
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
                if (!_context.considerPlayerAsEnemy)
                {
                    return;
                }
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
            EventHandler.CallInstantiateMonsterFace(_context.Config.monsterFace);
        }

        private bool _SensePlayer()
        {
            Debug.Log("Current State: " + _fsm.Current.Name);
            if (_fsm.Current.Name == "Chase" || _fsm.Current.Name == "Special Chase")
            {
                int maskLayer = ~(LayerMask.GetMask("Monster"));
                RaycastHit2D hit = Physics2D.Raycast(_context.Root.position, (_chaseTarget.position - _context.Root.position).normalized, _context.Config.senseDistance, maskLayer);

                if (hit.collider != null && (hit.transform.CompareTag(_chaseTarget.tag)))
                {
                    //Debug.DrawRay(_context.Root.position, (_chaseTarget.position - _context.Root.position).normalized * _context.Config.senseDistance, Color.red);
                    return true;
                }
            }
            else if (_fsm.Current.Name == "Patrol" || _fsm.Current.Name == "Idle" || _fsm.Current.Name == "ReturnToSpawnPos")
            {
                //Debug.Log("Patrol/Idle/ReturnToSpawnPos state sensing player...");
                
                Vector2 playerDirection = (_chaseTarget.position - _context.Root.position).normalized;
                Vector2 monsterForward = MonsterAContext.DirVec[(int)_context.currentDirection];
                Debug.Log("monsterForward: " + monsterForward);
                Debug.DrawRay(_context.Root.position, monsterForward * _context.Config.senseDistance, Color.blue);
                //Debug.DrawRay(_context.Root.position, playerDirection * _context.Config.senseDistance, Color.blue);
                bool lessThan45Deg = Vector2.Dot(monsterForward, playerDirection) >= Consts.Cos45;
                if (_fsm.Current.Name == "ReturnToSpawnPos")
                {
                    Debug.Log("ReturnToSpawnPos state sensing player...");
                    Debug.Log("Dot Product: " + Vector2.Dot(monsterForward, playerDirection) + ", lessThan45Deg: " + lessThan45Deg);
                }
                // 如果和当前面朝方向夹角小于45度，则进行视线检测
                if (lessThan45Deg)
                {
                    int maskLayer = ~(LayerMask.GetMask("Monster"));
                    RaycastHit2D hit = Physics2D.Raycast(_context.Root.position, (_chaseTarget.position - _context.Root.position).normalized, _context.Config.senseDistance, maskLayer);
                    Debug.DrawRay(_context.Root.position, (_chaseTarget.position - _context.Root.position).normalized * _context.Config.senseDistance, Color.red);
                    if (hit.collider != null && (hit.transform.CompareTag(_chaseTarget.tag)))
                    {
                        if (_fsm.Current.Name == "ReturnToSpawnPos")
                        {
                            Debug.Log("ReturnToSpawnPos state detected player!");
                        }
                        return true;
                    }
                }
            }

            return false;
        }
    }
}

