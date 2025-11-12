using System;
using _PrismWars._Scripts.Core.Infrastructure.Interfaces.Services;
using _PrismWars._Scripts.Core.Patterns.Factory.PoolFactory;

namespace _PrismWars._Scripts.Game.Services.Server.PoolServices {
    public class ProjectileServerService : IService, IInitializable, IDisposable {
        private readonly GenericPoolServerService _poolService;

        public ProjectileServerService() {
            var factory = ServiceLocator.Singleton.Get<ProjectileFactory>();
            _poolService = new GenericPoolServerService(factory);
        }

        public void Initialize() => _poolService.Initialize();
        public void Dispose() => _poolService.Dispose();
    }
}