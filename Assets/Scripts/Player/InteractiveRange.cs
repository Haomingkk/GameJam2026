using GameJam26;
using System.Collections.Generic;
using UnityEngine;

namespace GameJam2026.GamePlay
{
    public class InteractiveRange : MonoBehaviour
    {
        private readonly List<IInteractable> _nearby = new();
        private IInteractable _currentInteractable;
        private bool _calculatingNearInteractable = true;
        private void OnTriggerEnter2D(Collider2D other)
        {
            var interactable = other.GetComponentInParent<IInteractable>();
            if (interactable == null) return;

            if (!_nearby.Contains(interactable))
                _nearby.Add(interactable);

            _currentInteractable = _GetClosestInteractable();
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            var interactable = other.GetComponentInParent<IInteractable>();
            if (interactable == null) return;

            _nearby.Remove(interactable);

            _currentInteractable = _GetClosestInteractable();
        }
        private IInteractable _GetClosestInteractable()
        {
            if (_calculatingNearInteractable == false) return null;
            Vector2 p = transform.position;

            float bestDistSq = float.PositiveInfinity;
            IInteractable best = null;


            for (int i = _nearby.Count - 1; i >= 0; i--)
            {
                var it = _nearby[i];
                if (it == null)
                {
                    _nearby.RemoveAt(i);
                    continue;
                }

                var comp = it as Component;
                if (comp == null) continue;

                float d = ((Vector2)comp.transform.position - p).sqrMagnitude;
                if (d < bestDistSq)
                {
                    bestDistSq = d;
                    best = it;
                }
            }

            return best;
        }
    }
}
