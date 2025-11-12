using R3;
using Unity.Netcode;

namespace _PrismWars._Scripts.Core.Infrastructure.Interfaces {
    public interface IPoolEventSource {
        Subject<NetworkObjectReference> OnGetPoolObject { get; }
        Subject<NetworkObjectReference> OnReleasePoolObject { get; }
        Subject<NetworkObjectReference> OnDestroyPoolObject { get; }

    }
}