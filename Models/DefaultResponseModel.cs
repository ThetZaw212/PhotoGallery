namespace PhotoGallery.Models
{
    public class DefaultResponseModel
    {
        public required bool Success { get; set; }
        public required int Code { get; set; }
        public DefaultResponseMessageModel? Message { get; set; }
        public dynamic? Data { get; set; }
    }

    public record struct DefaultResponseMessageModel(string EN, string MM);
}
