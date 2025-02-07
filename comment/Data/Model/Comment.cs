using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Net.Mail;

namespace comment.Data.Model
{
    public class Comment
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [RegularExpression(@"^[a-zA-Z0-9]+$", ErrorMessage = "Допустимы только латинские буквы и цифры.")]
        public string UserName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Url]
        public string? HomePage { get; set; }

        [Required]  
        [MaxLength(2000)]
        public string Text { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Guid? ParentId { get; set; }

        public List<Guid>? Attachments { get; set; }
    }
}
