namespace Bootstrap.Helper
{
    public class GroupProperty<T> : IGroupProperty<T>
    {
        public T Value => _value;
        
        T IGroupPropertyGetter<T>.Value => _value;
        
        private T _value = default;

        public GroupProperty()
        {
            _value = default;
        }

        public GroupProperty(T value)
        {
            _value = value;
        }
        
        void IGroupPropertySetter<T>.Set(T value)
        {
            _value = value;
        }
    }
    
    public interface IGroupProperty<T> : IGroupPropertyGetter<T>, IGroupPropertySetter<T>
    {
        // NONE
    }
    
    public interface IGroupPropertyGetter<out T>
    {
        T Value { get; }
    }
    
    public interface IGroupPropertySetter<in T>
    {
        void Set(T value);
    }
}