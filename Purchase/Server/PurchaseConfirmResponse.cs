namespace Purchase.Base.Server
{
    public class PurchaseConfirmResponse
    {
        public PurchaseConfirmRequest Request { get; }
        public ResponseResult Result { get; }
        public object Content { get; }
        
        public PurchaseConfirmResponse(PurchaseConfirmRequest request, ResponseResult result, object content = null)
        {
            Request = request;
            Result = result;
            Content = content;
        }
    }
}