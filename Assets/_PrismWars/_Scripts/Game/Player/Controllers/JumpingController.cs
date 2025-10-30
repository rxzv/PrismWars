using UnityEngine;

namespace _PrismWars._Scripts.Player {
    public class JumpingController {
        Rigidbody2D _rigidbody;
        float _jumpForce;
        readonly string _groundLayerName;

        public JumpingController(Rigidbody2D rigidbody, float jumpForce, string groundLayerName = "Ground") {
            _rigidbody = rigidbody;
            _jumpForce = jumpForce;
            _groundLayerName = groundLayerName;
        }

        public void Jump() {
            if (GroundCheck())
                _rigidbody.AddForce(Vector2.up * _jumpForce, ForceMode2D.Impulse);
        }
        bool GroundCheck() {
             RaycastHit2D hit = Physics2D.Raycast(_rigidbody.transform.position, Vector2.down);
             return hit.collider.IsTouchingLayers(LayerMask.GetMask(_groundLayerName));
        }

        public void OnDrawGizmosSelected() {
            if (GroundCheck())
                Gizmos.color = Color.green;
            else
                Gizmos.color = Color.red;
            Gizmos.DrawRay(_rigidbody.transform.position, Vector2.down);
            
        }
    }
}