using _PrismWars._Scripts.Components;
using _PrismWars._Scripts.UI.Model;
using UnityEngine;

namespace _PrismWars._Scripts.Player {
    public class AttackMeleeController {
        PlayerElement _playerElement;
        float _attackRange;
        LayerMask _enemyLayer;
        float _damage;
        Vector3 transformRightForPlayer;
        
        public AttackMeleeController(PlayerElement playerElement, float attackRange, LayerMask enemyLayer, float defaultDamage = 1) {
            _playerElement = playerElement;
            _attackRange = attackRange;
            _enemyLayer = enemyLayer;
            _damage = defaultDamage;
        }

        public void MeleeAttack(GameObject go, SpriteRenderer spriteRenderer) {
            if (spriteRenderer.flipX)
                transformRightForPlayer = -go.transform.right;
            else
                transformRightForPlayer = go.transform.right;
            
            Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(
                go.transform.position + transformRightForPlayer * _attackRange,
                _attackRange,
                _enemyLayer
            );

            foreach (Collider2D enemy in hitEnemies) {
                IDamageable enemyHealth = enemy.GetComponent<IDamageable>();
                if (enemyHealth != null && enemyHealth != go.GetComponent<IDamageable>())
                    enemyHealth.TakeDamage(_playerElement, _damage);
            }
        }
        
        public void OnDrawGizmosSelected(Transform transform) {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(
                transform.position + transformRightForPlayer * _attackRange, 
                _attackRange
            );
        }
    }
}