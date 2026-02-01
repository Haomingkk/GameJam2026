using UnityEngine;
using UnityEngine.AI;

namespace GameJam26
{
    public class Door : MonoBehaviour, IInteractable
    {
        public enum DoorState { HealthyLocked, HealthyOpen, Damaged }
        
        [Header("State")]
        [SerializeField] DoorState currentState = DoorState.HealthyLocked;
        [SerializeField] int maxHP = 5;
        [SerializeField] int currentHP = 5;

        [Header("Nav / Block")]
        [SerializeField] NavMeshObstacle obstacle;
        [SerializeField] Collider2D col2D;

        [Header("Nav Link Settings")]
        [SerializeField] float linkWidth = 0.25f;
        [SerializeField] bool bidirectional = true;
        [SerializeField] float linkOffset = 0.5f;

        NavMeshLinkInstance linkInstance;
        SpriteRenderer spriteRenderer;
        Color originalColor;

        void Awake()
        {
            if (!obstacle) obstacle = GetComponent<NavMeshObstacle>();
            if (!col2D) col2D = GetComponent<Collider2D>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer) originalColor = spriteRenderer.color;
            
            currentHP = maxHP;
            SyncObstacleFromCollider();
            ApplyState();
        }

        void OnDisable()
        {
            RemoveLink();
        }

        public void SetOpen(bool open)
        {
            if (currentState == DoorState.Damaged) return;
            
            currentState = open ? DoorState.HealthyOpen : DoorState.HealthyLocked;
            ApplyState();
        }

        public void TakeDamage(int damage)
        {
            if (currentState == DoorState.Damaged) return;
            if (currentState == DoorState.HealthyOpen) return;
            
            currentHP -= damage;
            Debug.Log($"Door took {damage} damage! HP: {currentHP}/{maxHP}");
            
            if (currentHP <= 0)
            {
                currentHP = 0;
                Break();
            }
            else
            {
                UpdateDamageVisual();
            }
        }

        void Break()
        {
            currentState = DoorState.Damaged;
            Debug.Log("Door is broken!");
            ApplyState();
        }

        public void NotifyAttacked()
        {
            TakeDamage(1);
        }

        void UpdateDamageVisual()
        {
            if (spriteRenderer && currentState == DoorState.HealthyLocked)
            {
                float hpPercent = (float)currentHP / maxHP;
                spriteRenderer.color = Color.Lerp(Color.red, originalColor, hpPercent);
            }
        }

        void ApplyState()
        {
            bool passable = (currentState == DoorState.HealthyOpen || currentState == DoorState.Damaged);

            if (obstacle)
            {
                SyncObstacleFromCollider();
                obstacle.carving = true;
                obstacle.enabled = !passable;
            }
            
            if (col2D)
            {
                col2D.isTrigger = passable;
            }

            if (spriteRenderer)
            {
                switch (currentState)
                {
                    case DoorState.HealthyLocked:
                        spriteRenderer.enabled = true;
                        spriteRenderer.color = originalColor;
                        break;
                        
                    case DoorState.HealthyOpen:
                        spriteRenderer.enabled = true;
                        Color transparent = originalColor;
                        transparent.a = 0.3f;
                        spriteRenderer.color = transparent;
                        break;
                        
                    case DoorState.Damaged:
                        spriteRenderer.enabled = false;
                        break;
                }
            }

            if (passable) EnsureLink();
            else RemoveLink();
        }

        void SyncObstacleFromCollider()
        {
            if (!obstacle || !col2D) return;

            Bounds bounds = col2D.bounds;
            obstacle.shape = NavMeshObstacleShape.Box;
            obstacle.center = transform.InverseTransformPoint(bounds.center);
            obstacle.size = new Vector3(bounds.size.x, bounds.size.y, 1f);
        }

        void EnsureLink()
        {
            if (linkInstance.valid) return;
            if (!col2D) return;

            Bounds bounds = col2D.bounds;
            Vector3 center = bounds.center;
            Vector3 size = bounds.size;

            bool isHorizontal = size.x > size.y;

            Vector3 startPos, endPos;

            if (isHorizontal)
            {
                startPos = new Vector3(center.x - size.x / 2 - linkOffset, center.y, center.z);
                endPos = new Vector3(center.x + size.x / 2 + linkOffset, center.y, center.z);
            }
            else
            {
                startPos = new Vector3(center.x, center.y - size.y / 2 - linkOffset, center.z);
                endPos = new Vector3(center.x, center.y + size.y / 2 + linkOffset, center.z);
            }

            var data = new NavMeshLinkData
            {
                startPosition = startPos,
                endPosition = endPos,
                width = linkWidth,
                bidirectional = bidirectional,
                costModifier = -1,
                area = 0
            };

            linkInstance = NavMesh.AddLink(data);
        }

        void RemoveLink()
        {
            if (linkInstance.valid)
                NavMesh.RemoveLink(linkInstance);

            linkInstance = default;
        }

        public void Interact()
        {
            if (currentState == DoorState.Damaged)
            {
                Debug.Log("Door is broken and cannot be interacted with!");
                return;
            }

            bool shouldOpen = (currentState == DoorState.HealthyLocked);
            SetOpen(shouldOpen);
            Debug.Log($"Door is now {(shouldOpen ? "open" : "closed")}");
        }

    }
}
