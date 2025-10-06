using UnityEngine;

namespace _PrismWars._Scripts.Player {
    public class FlipXController {
        SpriteRenderer _spriteRenderer;

        public FlipXController(SpriteRenderer spriteRenderer) {
            _spriteRenderer = spriteRenderer;
        }
        public void FlipX(Vector2 dir) {
            if (dir.x == 1)
                _spriteRenderer.flipX = false;
            else if (dir.x == -1)
                _spriteRenderer.flipX = true;
        }
    }
}