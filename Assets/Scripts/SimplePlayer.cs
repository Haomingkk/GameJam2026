using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

namespace GameJam26
{
    public class SimplePlayer : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    
    [Header("Interaction Settings")]
    [SerializeField] private float interactionRange = 2f;
    [SerializeField] private LayerMask interactableLayer;
    
    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb;
    private Vector2 movementInput;
    private List<IInteractable> nearbyInteractables = new List<IInteractable>();
    private IInteractable closestInteractable;
    
    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
    }
    
    private void Update()
    {
        FindClosestInteractable();
    }
    
    private void FixedUpdate()
    {
        MovePlayer();
    }
    
    public void OnMove(InputValue value)
    {
        movementInput = value.Get<Vector2>();
    }
    
    public void OnInteract(InputValue value)
    {
        if (value.isPressed)
        {
            TryInteract();
        }
    }
    
    private void MovePlayer()
    {
        Vector2 movement = movementInput;
        if (movement.magnitude > 1f)
        {
            movement.Normalize();
        }
        
        rb.linearVelocity = movement * moveSpeed;
    }
    
    private void FindClosestInteractable()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, interactionRange, interactableLayer);
        
        nearbyInteractables.Clear();
        closestInteractable = null;
        float closestDistance = float.MaxValue;
        
        foreach (Collider2D collider in colliders)
        {
            IInteractable interactable = collider.GetComponent<IInteractable>();
            if (interactable != null)
            {
                nearbyInteractables.Add(interactable);
                
                float distance = Vector2.Distance(transform.position, collider.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestInteractable = interactable;
                }
            }
        }
    }
    
    private void TryInteract()
    {
        if (closestInteractable != null)
        {
            closestInteractable.Interact();
        }
        else
        {
            Debug.Log("Nothing to interact with nearby.");
        }
    }
    
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }
    }
}
