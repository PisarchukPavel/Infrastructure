namespace Purchase.Base
{
    public enum eServicesStatus
    {
        WaitInitialize = 0,
        ProcessingInitialize = 1,
        FailedInitialize = 2,
        
        Ready = 3,
        Purchase = 4,
        Additional = 5
    }
}