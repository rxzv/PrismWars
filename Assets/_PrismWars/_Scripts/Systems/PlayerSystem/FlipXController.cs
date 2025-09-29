using UnityEngine;

namespace _PrismWars._Scripts.Player {
    public class FlipXController {
        Transform _transform;

        public FlipXController(Transform transform) {
            _transform = transform;
        }
        public void FlipX(Vector2 dir) {
            if (dir.magnitude < 0.1f) return;
            
            if (dir.x >= 0.1f)
                _transform.localRotation = Quaternion.Euler(
                    _transform.localRotation.x, 
                    0, 
                    _transform.localRotation.z);
            else if (dir.x <= -0.1f) 
                _transform.localRotation = Quaternion.Euler(
                    _transform.localRotation.x, 
                    180, 
                    _transform.localRotation.z);
                
        }
    }
}