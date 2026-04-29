namespace FileService.Api.Models
{
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public T Data { get; set; }
    }
    
    public class FileResponse
    {
        public string Id { get; set; }
        public string FileName { get; set; }
        public string OriginalFileName { get; set; }
        public string Path { get; set; }
        public string Url { get; set; }
        public long Size { get; set; }
        public string ContentType { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}