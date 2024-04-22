namespace Bootstrap.Loading
{
    public abstract class Loader<T> : LoaderBase, ILoader<T>
    {
        void ILoader<T>.Load(T data)
        {
            Load(data);
        }

        protected abstract void Load(T data);
    }
}