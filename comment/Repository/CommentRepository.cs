using comment.Data;
using comment.Data.Model;
using comment.Interface;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileSystemGlobbing;

namespace comment.Repository
{
    public class CommentRepository : ICommentRepository
    {
        private readonly AppDbContext _context;
        public CommentRepository(AppDbContext context)
        {
            _context = context;
        }
        public async Task DeleteCommentAsync(Guid id)
        {
            try
            {
                var comment = await _context.Comments.FindAsync(id);

                if (comment != null)
                {
                    _context.Comments.Remove(comment);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    throw new Exception($"Коментаря з Id {id} не знайдено.");
                }
            }
            catch (Exception ex)
            {
                // Логируйте исключение
                throw new Exception($"Помилка при видаленні Коментаря: {ex.Message}", ex);
            }
        }

        public async Task UpdateCommentAsync(Comment comment)
        {
            try
            {
                _context.Entry(comment).State = EntityState.Modified;
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Помилка при редагуваны Коментаря: {ex.Message}", ex);
            }
        }

        public async Task<List<Comment>> GetAllCommentsAsync()
        {
            return await _context.Comments
                .OrderByDescending(c => c.CreatedAt) // Последние комментарии первыми
                .ToListAsync();
        }


        public async Task<IQueryable<Comment>> GetAllCommentsByHomePageAsync(Guid homePageId)
        {
            return await Task.FromResult(
            _context.Comments.
                Where(x => x.ParentId == homePageId).
                OrderByDescending(x => x.CreatedAt)
            );
        }

        public async Task<Comment> GetCommentByIdAsync(Guid id)
        {
            return await _context.Comments.FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task MakeCommentAsync(Comment comment)
        {
            try
            {
                _context.Entry(comment).State = EntityState.Added;
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Помилка при створені Коментаря: {ex.Message}", ex);
            }
        }
    }
}
