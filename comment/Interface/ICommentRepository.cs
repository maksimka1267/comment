﻿using comment.Data.Model;

namespace comment.Interface
{
    public interface ICommentRepository
    {
        Task MakeCommentAsync(Comment comment);
        Task DeleteCommentAsync(Guid id);
        Task<Comment> GetCommentByIdAsync(Guid id);
        Task<List<Comment>> GetAllCommentsAsync();
        Task<IQueryable<Comment>> GetAllCommentsByHomePageAsync(Guid homePageId);
        Task UpdateCommentAsync(Comment comment);
    }
}