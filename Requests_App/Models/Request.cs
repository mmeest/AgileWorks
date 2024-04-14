using System.ComponentModel.DataAnnotations;

namespace Requests_App.Models
{
    public class Request
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Content is required")]
        [StringLength(255, MinimumLength = 1, ErrorMessage = "Content length must be between 1 and 255 characters")]
        public string Content { get; set; }
        [Required]
        public DateTime CreatedAt { get; set; }
        [Required]
        public DateTime Deadline { get; set; }
        [Required]
        public bool Resolved { get; set; }
    }
}


