namespace Bootstrap.Loading
{
    public interface ILoader<in T>
    {
        void Load(T data);
    }
}