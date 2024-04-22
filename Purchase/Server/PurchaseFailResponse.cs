namespace Purchase.Base.Server
{
    public class PurchaseFailResponse
    {
        public PurchaseFailRequest Request { get; }
        public ResponseResult Result { get; }

        public PurchaseFailResponse(PurchaseFailRequest request, ResponseResult result)
        {
            Request = request;
            Result = result;
        }
    }
}