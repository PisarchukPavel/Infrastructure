using System;

namespace Bootstrap.Base
{
    public interface IOperation : IOperationStatus, IOperationStarter
    {
        // NONE
    }
    
    public interface IOperationStatus
    {
        bool Done { get; }
        float Progress { get; }
    }

    public interface IOperationStarter
    {
        void Start();
    }
}