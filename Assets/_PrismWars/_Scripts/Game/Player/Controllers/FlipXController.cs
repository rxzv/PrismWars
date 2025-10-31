using Unity.Netcode;
using UnityEngine;

namespace _PrismWars._Scripts.Player {
    public class FlipXController {
        SpriteRenderer _spriteRenderer;

        public FlipXController(SpriteRenderer spriteRenderer) {
            _spriteRenderer = spriteRenderer;
        }
        
        [ClientRpc]
        public void FlipXClientRpc(float dir) {
            if (dir > 0)
                _spriteRenderer.flipX = false;
            else if (dir < 0)
                _spriteRenderer.flipX = true;
        }
    }
}