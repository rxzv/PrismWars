using _PrismWars._Scripts.Core.Infrastructure.Interfaces.Attack;
using _PrismWars._Scripts.Game.Player.Model;
using UnityEngine;

namespace _PrismWars._Scripts.Game.Player.Controllers.Attack {
    public abstract class AttackController : IAttack {
        protected Transform _playerTransform;
        protected PlayerElement _playerElement;
        protected Vector3 _attackPos;

        protected AttackController(Transform playerTransform, PlayerElement playerElement) {
            _playerTransform = playerTransform;
            _playerElement = playerElement;
        }

        public abstract void Attack();

    }
}