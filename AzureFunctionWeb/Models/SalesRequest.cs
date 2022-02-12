namespace AzureFunctionWeb.Models
{
    public class SalesRequest
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Status { get; set; }
        //public IFormFile File { get; set; }
    }
}
