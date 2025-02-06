using comment.Data;
using comment.Data.Model;
using comment.Interface;
using Microsoft.EntityFrameworkCore;

namespace comment.Repository
{
    public class AttachmentRepository : IAttachmentRepository
    {
        private readonly AppDbContext _context;

        public AttachmentRepository(AppDbContext context)
        {
            _context = context;
        }
        public async Task AddAsync(Attachment attachment)
        {
            _context.Attachments.Add(attachment);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var attachment = await _context.Attachments.FindAsync(id);
            if (attachment != null)
            {
                _context.Attachments.Remove(attachment);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<Attachment>> GetAllAsync()
        {
            return await _context.Attachments.ToListAsync();
        }

        public async Task<IEnumerable<Attachment>> GetByCommentIdAsync(Guid commentId)
        {
            return await _context.Attachments
                .Where(a => a.CommentId == commentId)
                .ToListAsync();
        }

        public async Task<Attachment?> GetByIdAsync(Guid id)
        {
            return await _context.Attachments.FindAsync(id);
        }
    }
}
