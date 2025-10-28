using _PrismWars._Scripts.UI.Model;

namespace _PrismWars._Scripts.Components {
    public interface IDamageable {
        void TakeDamage(PlayerElement playerElement, float damage);
    }
}