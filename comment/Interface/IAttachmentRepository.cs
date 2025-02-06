using comment.Data.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace comment.Interface
{
    public interface IAttachmentRepository
    {
        Task<IEnumerable<Attachment>> GetAllAsync();
        Task<Attachment?> GetByIdAsync(Guid id);
        Task<IEnumerable<Attachment>> GetByCommentIdAsync(Guid commentId);
        Task AddAsync(Attachment attachment);
        Task DeleteAsync(Guid id);
    }
}
