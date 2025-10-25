namespace aspnetcoreapi.Common
{
    public class ApiResponse
    {
        public string Title { get; set; } = default!;
        public int Status { get; set; }
        public string Message { get; set; } = default!;
        public object? Data { get; set; } 
    }
}