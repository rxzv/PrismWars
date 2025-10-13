public interface IInitializable<T1, T2, T3> {
    void Initialize(T1 data1, T2 data2, T3 data3);
}
public interface IInitializable<T1, T2> {
    void Initialize(T1 data1, T2 data2);
}
public interface IInitializable<T> {
    void SetType(T data);
}
public interface IInitializable {
    void Initialize();
}