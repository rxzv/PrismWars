using _PrismWars._Scripts.UI.Model;

namespace _PrismWars._Scripts.Core.Infrastructure.Interfaces.Health {
    public interface IDamageable {
        void TakeDamage(PlayerElement playerElement, float damage, ulong playerId);
    }
}