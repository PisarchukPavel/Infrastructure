namespace Purchase.Base.Server
{
    public class ResponseResult
    {
        public bool Success { get; }
        public int Code { get; }
        public string Message { get; }
        
        public ResponseResult(bool success, int code, string message)
        {
            Success = success;
            Code = code;
            Message = message ?? string.Empty;
        }
    }
}