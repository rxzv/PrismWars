using System;
using _PrismWars._Scripts.Player;
using Unity.Netcode;
using UnityEngine;

namespace _PrismWars._Scripts.Core.Infrastructure.Network.Components.BombCart {
    public class BombCart : NetworkBehaviour{
        
        NetworkVariable<BombCartState> _state = new();

        void OnTriggerEnter2D(Collider2D other) {
        }
        void OnTriggerExit2D(Collider2D other) {
            
        }

        public enum BombCartState { OfRest, OfMotion, InZone }
    }
}