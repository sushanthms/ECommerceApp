namespace ECommerceBackend.DTOs
{
    public class BannerDto
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string ButtonText { get; set; }
        public string Link { get; set; }
        public IFormFile? Image { get; set; }
    }
}