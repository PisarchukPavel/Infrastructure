namespace Bootstrap.Loading
{
    public interface IEntry<in T>
    {
        void Enter(T data);
    }
}