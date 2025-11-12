namespace _PrismWars._Scripts.Core.Infrastructure.Interfaces.Services {
    public interface IInitializable<T1, T2, T3> {
        void Initialize(T1 data1, T2 data2, T3 data3);
    }
    public interface IInitializable<T1, T2> {
        void Initialize(T1 data1, T2 data2);
    }
    public interface IInitializable<T> {
        void Initialize(T data);
    }
    public interface IInitializable {
        void Initialize();
    }
}