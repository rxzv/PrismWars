using _PrismWars._Scripts.Game.Player.Model;
using UnityEngine;

namespace _PrismWars._Scripts.Core.Infrastructure.Interfaces {
    public interface IPoolObject {

        public void SetPosition(Vector2 startPos, Vector2 direction);
        public void SetType(PlayerElement element, ulong playerId);

    }
}