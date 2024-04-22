namespace Purchase.Base.Server
{
    public class PurchasePendingResponse
    {
        public PurchasePendingRequest Request { get; }
        public ResponseResult Result { get; }
        public string Transaction { get; }
        public object Content { get; }

        public PurchasePendingResponse(PurchasePendingRequest request, ResponseResult result, string transaction, object content = null)
        {
            Request = request;
            Result = result;
            Transaction = transaction;
            Content = content;
        }
    }
}