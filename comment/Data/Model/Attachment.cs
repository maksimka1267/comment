using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.Graph.Models;


namespace comment.Data.Model
{
    public class Attachment
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid CommentId { get; set; }

        [Required]
        public byte[] FileData { get; set; }

        [Required]
        public AttachmentType FileType { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}

