namespace LightContainer.Reflection.Direct
{
    public class DirectReflection
    {
        public virtual bool Inject(int id, object target, object[] parameters)
        {
            return false;
        }

        public virtual bool Resolve(int id, out object result, object[] parameters)
        {
            result = default;
            return false;
        }
    }
}