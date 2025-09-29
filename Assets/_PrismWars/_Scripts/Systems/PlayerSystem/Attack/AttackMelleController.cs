using UnityEngine;

namespace _PrismWars._Scripts.Player {
    public class AttackMeleeController {
        float _attackRange;
        LayerMask _enemyLayer;
        float _damage;
        Transform _transform;
        
        public AttackMeleeController(float attackRange, LayerMask enemyLayer, float defaultDamage = 1) {
            _attackRange = attackRange;
            _enemyLayer = enemyLayer;
            _damage = defaultDamage;
        }

        public void MeleeAttack(GameObject go) {
            _transform = go.transform;
            Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(
                go.transform.position + go.transform.forward,
                _attackRange,
                _enemyLayer
            );

            foreach (Collider2D enemy in hitEnemies) {
                HealthComponent enemyHealth = enemy.GetComponent<HealthComponent>();
                if (enemyHealth != null && enemyHealth != go.GetComponent<HealthComponent>())
                    enemyHealth.TakeDamage(_damage);
            }
        }
        
        public void OnDrawGizmosSelected() {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(
                _transform.position + _transform.forward, 
                _attackRange
            );
        }
    }
}