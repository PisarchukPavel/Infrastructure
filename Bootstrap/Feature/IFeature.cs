namespace Bootstrap.Feature
{
    public interface IFeature
    {
        bool Enabled { get; }
        string Name { get; }
    }
}