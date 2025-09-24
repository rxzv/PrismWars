using UnityEngine;

namespace _PrismWars._Scripts.Player {
    public class JumpingController {
        Rigidbody2D _rigidbody;
        float _jumpForce;

        public JumpingController(Rigidbody2D rigidbody, float jumpForce) {
            _rigidbody = rigidbody;
            _jumpForce = jumpForce;
        }

        public void Jump() {
            RaycastHit2D hit =  Physics2D.Raycast(_rigidbody.transform.position, Vector2.down);
            if (hit.collider.IsTouchingLayers(LayerMask.GetMask("Ground"))) 
                _rigidbody.AddForce(Vector2.up * _jumpForce, ForceMode2D.Impulse);
        }
    }
}