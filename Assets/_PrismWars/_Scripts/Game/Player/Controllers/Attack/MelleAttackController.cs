using _PrismWars._Scripts.Core.Infrastructure.Interfaces.Health;
using _PrismWars._Scripts.Game.Player.Controllers.Attack;
using _PrismWars._Scripts.UI.Model;
using UnityEngine;

namespace _PrismWars._Scripts.Player {
    public class AttackMeleeController : AttackController {
        float _attackRange;
        LayerMask _enemyLayer;
        float _damage;
        ulong _playerId;
        SpriteRenderer _spriteRenderer;
        
        public AttackMeleeController(PlayerElement playerElement, 
            float attackRange, LayerMask enemyLayer, ulong playerId, Transform playerTransform, float defaultDamage = 1)
        : base(playerTransform, playerElement) {
            _attackRange = attackRange;
            _enemyLayer = enemyLayer;
            _damage = defaultDamage;
            _playerId = playerId;
            _spriteRenderer = playerTransform.GetComponent<SpriteRenderer>();
        }

        public override void Attack() {
            _attackPos = _spriteRenderer.flipX ? -_playerTransform.right : _playerTransform.right;

            Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(
                _playerTransform.position + _attackPos * _attackRange,
                _attackRange,
                _enemyLayer
            );

            foreach (Collider2D enemy in hitEnemies) {
                IDamageable enemyHealth = enemy.GetComponent<IDamageable>();
                if (enemyHealth != null && enemyHealth != _playerTransform.GetComponent<IDamageable>())
                    enemyHealth.TakeDamage(_playerElement, _damage, _playerId);
            }
        }
        public void OnDrawGizmosSelected(Transform transform) {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(
                transform.position + _attackPos * _attackRange, 
                _attackRange
            );
        }
    }
}