using MediatR;
using System;

namespace comment.Events
{
    public class CommentAddedEvent : INotification
    {
        public Guid CommentId { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public DateTime CreatedAt { get; set; }

        public CommentAddedEvent(Guid commentId, string userName, string email, DateTime createdAt)
        {
            CommentId = commentId;
            UserName = userName;
            Email = email;
            CreatedAt = createdAt;
        }
    }
}
