namespace Purchase.Base.Server
{
    public class PurchaseStartResponse
    {
        public PurchaseStartRequest Request { get; }
        public ResponseResult Result { get; }
        public string Transaction { get; }

        public PurchaseStartResponse(PurchaseStartRequest request, ResponseResult result, string transaction)
        {
            Request = request;
            Result = result;
            Transaction = transaction;
        }
    }
}