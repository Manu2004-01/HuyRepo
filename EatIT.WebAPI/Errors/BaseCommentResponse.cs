
namespace EatIT.WebAPI.Errors
{
    public class BaseCommentResponse
    {
        public BaseCommentResponse(int statusCode)
        {
            this.Statuscodes = statusCode;
            this.Message = DefaultMessageForStatuscodes(statusCode);
        }

        public BaseCommentResponse(int statuscodes, string message)
        {
            this.Statuscodes = statuscodes;
            this.Message = message ?? DefaultMessageForStatuscodes(statuscodes);
        }

        public int Statuscodes { get; set; }
        public string Message { get; set; }

        private string DefaultMessageForStatuscodes(int statuscodes)
        {
            return statuscodes switch
            {
                400 => "Bad Request",
                401 => "Not Athorize",
                404 => "Resource Not Found",
                500 => "Server Error",
                _ => null
            };
        }
    }

}