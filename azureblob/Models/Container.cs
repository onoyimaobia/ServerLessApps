using System.ComponentModel.DataAnnotations;

namespace azureblob.Models
{
    /// <summary>
    /// container class
    /// </summary>
    public class Container
    {
        [Required]
        public string Name { get; set; }
    }
}
