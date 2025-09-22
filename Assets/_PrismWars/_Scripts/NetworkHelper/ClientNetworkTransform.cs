using Unity.Netcode.Components;
using UnityEngine;

namespace PrismWars.NetworkHelper {
    public enum AuthorityMods {
        Server,
        Client
    }
    
    [DisallowMultipleComponent]
    public class ClientNetworkTransform : NetworkTransform {
        public AuthorityMods authorityMode = AuthorityMods.Client;

        protected override bool OnIsServerAuthoritative() => authorityMode == AuthorityMods.Server;
    }
}