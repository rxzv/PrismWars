using _PrismWars._Scripts.Components;
using UnityEngine;

namespace _PrismWars._Scripts.Player {
    public class AttackMeleeController {
        float _attackRange;
        LayerMask _enemyLayer;
        float _damage;
        
        public AttackMeleeController(float attackRange, LayerMask enemyLayer, float defaultDamage = 1) {
            _attackRange = attackRange;
            _enemyLayer = enemyLayer;
            _damage = defaultDamage;
        }

        public void MeleeAttack(GameObject go) {
            Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(
                go.transform.position + go.transform.right * _attackRange,
                _attackRange,
                _enemyLayer
            );

            foreach (Collider2D enemy in hitEnemies) {
                IDamageable enemyHealth = enemy.GetComponent<IDamageable>();
                if (enemyHealth != null && enemyHealth != go.GetComponent<IDamageable>())
                    enemyHealth.TakeDamage(_damage);
            }
        }
        
        public void OnDrawGizmosSelected(Transform transform) {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(
                transform.position + transform.right * _attackRange, 
                _attackRange
            );
        }
    }
}