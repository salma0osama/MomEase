using Microsoft.Extensions.Logging;
using MomEase.core.DTOS.CommunityDTO;
using MomEase.core.Entities;
using MomEase.core.Enums;
using MomEase.core.Interfaces;

namespace MomEase.infra.Services
{
    public class CommunityService : ICommunityService
    {
        private readonly ICommunityRepository _communityRepository;
        private readonly IFileStorageService _fileStorageService;
        private readonly ILogger<CommunityService> _logger;

        public CommunityService(
            ICommunityRepository communityRepository,
            IFileStorageService fileStorageService,
            ILogger<CommunityService> logger)
        {
            _communityRepository = communityRepository;
            _fileStorageService = fileStorageService;
            _logger = logger;
        }

        // ===== Posts =====

        public async Task<PostsPagedDto> GetAllPostsAsync(
            int pageNumber, int pageSize, int currentUserId)
        {
            try
            {
                if (pageNumber < 1) pageNumber = 1;
                if (pageSize < 1 || pageSize > 50) pageSize = 10;

                var (posts, totalCount) = await _communityRepository
                    .GetAllPostsAsync(pageNumber, pageSize);

                return new PostsPagedDto
                {
                    Posts = posts.Select(p => MapToPostDto(p, currentUserId)).ToList(),
                    TotalCount = totalCount,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    HasNextPage = pageNumber * pageSize < totalCount,
                    HasPreviousPage = pageNumber > 1
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all posts");
                throw;
            }
        }

        public async Task<CommunityPostDto> GetPostByIdAsync(int postId, int currentUserId)
        {
            try
            {
                var post = await _communityRepository.GetPostByIdAsync(postId);
                if (post == null)
                    throw new KeyNotFoundException("Post not found");

                return MapToPostDto(post, currentUserId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving post {PostId}", postId);
                throw;
            }
        }

        public async Task<CommunityPostDto> CreatePostAsync(int userId, CreatePostDto dto)
        {
            try
            {
                // Validate: لازم يكون فيه text أو media
                if (string.IsNullOrWhiteSpace(dto.Text) &&
                    (dto.MediaFiles == null || !dto.MediaFiles.Any()))
                    throw new ArgumentException("Post must have text or media");

                // Validate media files
                if (dto.MediaFiles != null && dto.MediaFiles.Any())
                {
                    if (dto.MediaFiles.Count > 10)
                        throw new ArgumentException("Maximum 10 media files allowed");

                    foreach (var file in dto.MediaFiles)
                    {
                        ValidateMediaFile(file);
                    }
                }

                // Create post
                var post = new CommunityPosts
                {
                    UserId = userId,
                    Text = dto.Text?.Trim(),
                    CreatedAt = DateTime.Now
                };

                var createdPost = await _communityRepository.AddPostAsync(post);

                // Upload media files
                if (dto.MediaFiles != null && dto.MediaFiles.Any())
                {
                    var mediaList = new List<PostMedia>();
                    int order = 0;

                    foreach (var file in dto.MediaFiles)
                    {
                        var mediaUrl = await _fileStorageService.UploadFileAsync(file, "community");
                        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
                        var mediaType = IsVideoExtension(extension) ? MediaType.Video : MediaType.Photo;

                        mediaList.Add(new PostMedia
                        {
                            PostId = createdPost.PostId,
                            MediaUrl = mediaUrl,
                            MediaType = mediaType,
                            Order = order++
                        });
                    }

                    await _communityRepository.AddPostMediaAsync(mediaList);
                }

                // Reload post with all data
                var fullPost = await _communityRepository.GetPostByIdAsync(createdPost.PostId);

                _logger.LogInformation("Post created {PostId} by user {UserId}",
                    createdPost.PostId, userId);

                return MapToPostDto(fullPost!, userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating post for user {UserId}", userId);
                throw;
            }
        }

        public async Task<CommunityPostDto> UpdatePostAsync(
    int postId, int userId, UpdatePostDto dto)
        {
            try
            {
                // ✅ تحقق إن فيه تعديل فعلي أولاً
                if (string.IsNullOrWhiteSpace(dto.Text) &&
                    (dto.MediaIdsToDelete == null || !dto.MediaIdsToDelete.Any()) &&
                    (dto.NewMediaFiles == null || !dto.NewMediaFiles.Any()))
                    throw new ArgumentException("No changes provided");

                var post = await _communityRepository.GetPostByIdAsync(postId);
                if (post == null)
                    throw new KeyNotFoundException("Post not found");

                if (post.UserId != userId)
                    throw new UnauthorizedAccessException(
                        "You are not authorized to update this post");

                // تعديل الـ Text لو موجود
                if (!string.IsNullOrWhiteSpace(dto.Text))
                {
                    post.Text = dto.Text.Trim();
                    post.UpdatedAt = DateTime.Now;
                }

                await _communityRepository.UpdatePostAsync(post);

                // حذف صور معينة
                if (dto.MediaIdsToDelete != null && dto.MediaIdsToDelete.Any())
                {
                    foreach (var mediaId in dto.MediaIdsToDelete)
                    {
                        var media = await _communityRepository.GetPostMediaByIdAsync(mediaId);

                        if (media == null)
                            throw new KeyNotFoundException(
                                $"Media with ID {mediaId} not found");

                        if (media.PostId != postId)
                            throw new UnauthorizedAccessException(
                                $"Media with ID {mediaId} does not belong to this post");

                        await _fileStorageService.DeleteFileAsync(media.MediaUrl);
                        await _communityRepository.DeleteSingleMediaAsync(media);
                    }
                }

                // إضافة صور جديدة
                if (dto.NewMediaFiles != null && dto.NewMediaFiles.Any())
                {
                    var remainingMedia = post.PostMedia?.Count ?? 0;
                    if (remainingMedia + dto.NewMediaFiles.Count > 10)
                        throw new ArgumentException("Total media cannot exceed 10 files");

                    foreach (var file in dto.NewMediaFiles)
                        ValidateMediaFile(file);

                    var mediaList = new List<PostMedia>();
                    int order = post.PostMedia?.Count ?? 0;

                    foreach (var file in dto.NewMediaFiles)
                    {
                        var mediaUrl = await _fileStorageService
                            .UploadFileAsync(file, "community");
                        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
                        var mediaType = IsVideoExtension(extension)
                            ? MediaType.Video
                            : MediaType.Photo;

                        mediaList.Add(new PostMedia
                        {
                            PostId = postId,
                            MediaUrl = mediaUrl,
                            MediaType = mediaType,
                            Order = order++
                        });
                    }

                    await _communityRepository.AddPostMediaAsync(mediaList);
                }

                var updatedPost = await _communityRepository.GetPostByIdAsync(postId);

                _logger.LogInformation("Post updated {PostId}", postId);

                return MapToPostDto(updatedPost!, userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating post {PostId}", postId);
                throw;
            }
        }

        public async Task DeletePostAsync(int postId, int userId)
        {
            try
            {
                var post = await _communityRepository.GetPostByIdAsync(postId);
                if (post == null)
                    throw new KeyNotFoundException("Post not found");

                if (post.UserId != userId)
                    throw new UnauthorizedAccessException(
                        "You are not authorized to delete this post");

                // Delete media files from storage
                if (post.PostMedia != null && post.PostMedia.Any())
                {
                    foreach (var media in post.PostMedia)
                    {
                        await _fileStorageService.DeleteFileAsync(media.MediaUrl);
                    }
                }

                await _communityRepository.DeletePostAsync(post);

                _logger.LogInformation("Post deleted {PostId}", postId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting post {PostId}", postId);
                throw;
            }
        }

        public async Task<PostsPagedDto> GetPostsByUserIdAsync(
            int targetUserId, int pageNumber, int pageSize, int currentUserId)
        {
            try
            {
                if (pageNumber < 1) pageNumber = 1;
                if (pageSize < 1 || pageSize > 50) pageSize = 10;

                var posts = await _communityRepository.GetPostsByUserIdAsync(targetUserId);
                var totalCount = posts.Count;

                var pagedPosts = posts
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                return new PostsPagedDto
                {
                    Posts = pagedPosts.Select(p => MapToPostDto(p, currentUserId)).ToList(),
                    TotalCount = totalCount,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    HasNextPage = pageNumber * pageSize < totalCount,
                    HasPreviousPage = pageNumber > 1
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving posts for user {UserId}", targetUserId);
                throw;
            }
        }

        public async Task<PostsPagedDto> GetMyPostsAsync(
            int userId, int pageNumber, int pageSize)
        {
            return await GetPostsByUserIdAsync(userId, pageNumber, pageSize, userId);
        }

        // ===== Comments =====

        public async Task<PostCommentDto> AddCommentAsync(
            int postId, int userId, CreateCommentDto dto)
        {
            try
            {
                var post = await _communityRepository.GetPostByIdAsync(postId);
                if (post == null)
                    throw new KeyNotFoundException("Post not found");

                if (string.IsNullOrWhiteSpace(dto.Text))
                    throw new ArgumentException("Comment text cannot be empty");

                var comment = new PostComments
                {
                    PostId = postId,
                    UserId = userId,
                    Text = dto.Text.Trim(),
                    CreatedAt = DateTime.Now
                };

                var added = await _communityRepository.AddCommentAsync(comment);

                // Reload with user data
                var full = await _communityRepository.GetCommentByIdAsync(added.CommentId);

                _logger.LogInformation("Comment added {CommentId} on post {PostId}",
                    added.CommentId, postId);

                return MapToCommentDto(full!);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding comment on post {PostId}", postId);
                throw;
            }
        }

        public async Task<List<PostCommentDto>> GetPostCommentsAsync(int postId)
        {
            try
            {
                var post = await _communityRepository.GetPostByIdAsync(postId);
                if (post == null)
                    throw new KeyNotFoundException("Post not found");

                var comments = await _communityRepository.GetPostCommentsAsync(postId);
                return comments.Select(c => MapToCommentDto(c)).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving comments for post {PostId}", postId);
                throw;
            }
        }

        public async Task<PostCommentDto> GetCommentByIdAsync(int commentId, int postId)
        {
            try
            {
                var comment = await _communityRepository.GetCommentByIdAsync(commentId);
                if (comment == null || comment.PostId != postId)
                    throw new KeyNotFoundException("Comment not found");

                return MapToCommentDto(comment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving comment {CommentId}", commentId);
                throw;
            }
        }

        public async Task<PostCommentDto> UpdateCommentAsync(
            int commentId, int postId, int userId, UpdateCommentDto dto)
        {
            try
            {
                var comment = await _communityRepository.GetCommentByIdAsync(commentId);
                if (comment == null || comment.PostId != postId)
                    throw new KeyNotFoundException("Comment not found");

                if (comment.UserId != userId)
                    throw new UnauthorizedAccessException(
                        "You are not authorized to update this comment");

                if (string.IsNullOrWhiteSpace(dto.Text))
                    throw new ArgumentException("Comment text cannot be empty");

                comment.Text = dto.Text.Trim();
                comment.UpdatedAt = DateTime.Now;

                var updated = await _communityRepository.UpdateCommentAsync(comment);

                _logger.LogInformation("Comment updated {CommentId}", commentId);

                return MapToCommentDto(updated);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating comment {CommentId}", commentId);
                throw;
            }
        }

        public async Task DeleteCommentAsync(int commentId, int postId, int userId)
        {
            try
            {
                var comment = await _communityRepository.GetCommentByIdAsync(commentId);
                if (comment == null || comment.PostId != postId)
                    throw new KeyNotFoundException("Comment not found");

                if (comment.UserId != userId)
                    throw new UnauthorizedAccessException(
                        "You are not authorized to delete this comment");

                await _communityRepository.DeleteCommentAsync(comment);

                _logger.LogInformation("Comment deleted {CommentId}", commentId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting comment {CommentId}", commentId);
                throw;
            }
        }

        // ===== Reactions =====

        public async Task<PostReactionDto> AddReactionAsync(
            int postId, int userId, AddReactionDto dto)
        {
            try
            {
                var post = await _communityRepository.GetPostByIdAsync(postId);
                if (post == null)
                    throw new KeyNotFoundException("Post not found");

                if (!Enum.TryParse<ReactionType>(dto.ReactionType, true, out var reactionType))
                    throw new ArgumentException(
                        "Invalid reaction type. Must be: LIKE, LOVE, SUPPORT, HELPFUL");

                // لو عنده reaction قديمة امسحها
                var existing = await _communityRepository.GetUserReactionAsync(postId, userId);
                if (existing != null)
                    throw new InvalidOperationException(
                        "You already reacted to this post. Use PUT to update.");

                var reaction = new PostReactions
                {
                    PostId = postId,
                    UserId = userId,
                    ReactionType = reactionType
                };

                var added = await _communityRepository.AddReactionAsync(reaction);

                _logger.LogInformation("Reaction added on post {PostId} by user {UserId}",
                    postId, userId);

                return MapToReactionDto(added);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding reaction on post {PostId}", postId);
                throw;
            }
        }

        public async Task<List<PostReactionDto>> GetPostReactionsAsync(int postId)
        {
            try
            {
                var post = await _communityRepository.GetPostByIdAsync(postId);
                if (post == null)
                    throw new KeyNotFoundException("Post not found");

                var reactions = await _communityRepository.GetPostReactionsAsync(postId);
                return reactions.Select(r => MapToReactionDto(r)).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving reactions for post {PostId}", postId);
                throw;
            }
        }

        public async Task<ReactionsCountDto> GetReactionsCountAsync(int postId)
        {
            try
            {
                var post = await _communityRepository.GetPostByIdAsync(postId);
                if (post == null)
                    throw new KeyNotFoundException("Post not found");

                var reactions = await _communityRepository.GetPostReactionsAsync(postId);

                var byType = reactions
                    .GroupBy(r => r.ReactionType.ToString())
                    .ToDictionary(g => g.Key, g => g.Count());

                return new ReactionsCountDto
                {
                    TotalCount = reactions.Count,
                    ByType = byType
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving reactions count for post {PostId}", postId);
                throw;
            }
        }

        public async Task DeleteReactionAsync(int postId, int userId)
        {
            try
            {
                var reaction = await _communityRepository.GetUserReactionAsync(postId, userId);
                if (reaction == null)
                    throw new KeyNotFoundException("Reaction not found");

                await _communityRepository.DeleteReactionAsync(reaction);

                _logger.LogInformation("Reaction deleted on post {PostId} by user {UserId}",
                    postId, userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting reaction on post {PostId}", postId);
                throw;
            }
        }

        public async Task<PostReactionDto> UpdateReactionAsync(
            int postId, int userId, UpdateReactionDto dto)
        {
            try
            {
                var reaction = await _communityRepository.GetUserReactionAsync(postId, userId);
                if (reaction == null)
                    throw new KeyNotFoundException("Reaction not found");

                if (!Enum.TryParse<ReactionType>(dto.ReactionType, true, out var reactionType))
                    throw new ArgumentException(
                        "Invalid reaction type. Must be: LIKE, LOVE, SUPPORT, HELPFUL");

                reaction.ReactionType = reactionType;
                var updated = await _communityRepository.UpdateReactionAsync(reaction);

                _logger.LogInformation("Reaction updated on post {PostId} by user {UserId}",
                    postId, userId);

                return MapToReactionDto(updated);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating reaction on post {PostId}", postId);
                throw;
            }
        }

        // ===== Private Helpers =====

        private void ValidateMediaFile(Microsoft.AspNetCore.Http.IFormFile file)
        {
            if (file.Length == 0)
                throw new ArgumentException("File cannot be empty");

            if (file.Length > 50 * 1024 * 1024)
                throw new ArgumentException("File size must be less than 50MB");

            var allowedExtensions = new[]
            {
                ".jpg", ".jpeg", ".png", ".gif", ".webp",
                ".mp4", ".mov", ".avi", ".mkv"
            };

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(extension))
                throw new ArgumentException(
                    "Unsupported file type. Allowed: JPG, PNG, GIF, WEBP, MP4, MOV, AVI, MKV");
        }

        private bool IsVideoExtension(string extension)
        {
            return new[] { ".mp4", ".mov", ".avi", ".mkv" }.Contains(extension);
        }

        private CommunityPostDto MapToPostDto(CommunityPosts post, int currentUserId)
        {
            var myReaction = post.PostReactions?
                .FirstOrDefault(r => r.UserId == currentUserId);

            return new CommunityPostDto
            {
                PostId = post.PostId,
                UserId = post.UserId,
                UserName = post.User != null
                    ? $"{post.User.FirstName} {post.User.LastName}"
                    : "",
                UserPhoto = post.User?.MotherProfile?.ProfilePictureUrl,
                //UserPhoto = null,
                Text = post.Text,
                Media = post.PostMedia?.OrderBy(m => m.Order)
                    .Select(m => new PostMediaDto
                    {
                        MediaId = m.MediaId,
                        MediaUrl = m.MediaUrl,
                        MediaType = m.MediaType.ToString(),
                        Order = m.Order
                    }).ToList() ?? new(),
                CommentsCount = post.PostComments?.Count ?? 0,
                ReactionsCount = post.PostReactions?.Count ?? 0,
                MyReaction = myReaction?.ReactionType.ToString(),
                CreatedAt = post.CreatedAt,
                UpdatedAt = post.UpdatedAt
            };
        }

        private PostCommentDto MapToCommentDto(PostComments comment)
        {
            return new PostCommentDto
            {
                CommentId = comment.CommentId,
                PostId = comment.PostId,
                UserId = comment.UserId,
                UserName = comment.User != null
                    ? $"{comment.User.FirstName} {comment.User.LastName}"
                    : "",
                UserPhoto = comment.User?.MotherProfile?.ProfilePictureUrl,
                //UserPhoto = null,
                Text = comment.Text,
                CreatedAt = comment.CreatedAt,
                UpdatedAt = comment.UpdatedAt
            };
        }

        private PostReactionDto MapToReactionDto(PostReactions reaction)
        {
            return new PostReactionDto
            {
                ReactionId = reaction.ReactionId,
                PostId = reaction.PostId,
                UserId = reaction.UserId,
                UserName = reaction.User != null
                    ? $"{reaction.User.FirstName} {reaction.User.LastName}"
                    : "",
                ReactionType = reaction.ReactionType.ToString()
            };
        }
        // ===== Saved Posts =====

        public async Task<SavedPostDto> SavePostAsync(int postId, int userId)
        {
            try
            {
                var post = await _communityRepository.GetPostByIdAsync(postId);
                if (post == null)
                    throw new KeyNotFoundException("Post not found");

                var existing = await _communityRepository.GetSavedPostAsync(postId, userId);
                if (existing != null)
                    throw new InvalidOperationException("Post already saved");

                var savedPost = new SavedPosts
                {
                    PostId = postId,
                    UserId = userId,
                    SavedAt = DateTime.Now
                };

                var result = await _communityRepository.SavePostAsync(savedPost);

                _logger.LogInformation("Post {PostId} saved by user {UserId}", postId, userId);

                return new SavedPostDto
                {
                    SavedPostId = result.SavedPostId,
                    PostId = result.PostId,
                    SavedAt = result.SavedAt,
                    Post = MapToPostDto(post, userId)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving post {PostId}", postId);
                throw;
            }
        }

        public async Task RemoveFromSavedAsync(int postId, int userId)
        {
            try
            {
                var savedPost = await _communityRepository.GetSavedPostAsync(postId, userId);
                if (savedPost == null)
                    throw new KeyNotFoundException("Saved post not found");

                await _communityRepository.RemoveSavedPostAsync(savedPost);

                _logger.LogInformation("Post {PostId} removed from saved by user {UserId}",
                    postId, userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing saved post {PostId}", postId);
                throw;
            }
        }

        public async Task<List<SavedPostDto>> GetSavedPostsAsync(int userId)
        {
            try
            {
                var savedPosts = await _communityRepository.GetUserSavedPostsAsync(userId);

                return savedPosts.Select(s => new SavedPostDto
                {
                    SavedPostId = s.SavedPostId,
                    PostId = s.PostId,
                    SavedAt = s.SavedAt,
                    Post = MapToPostDto(s.Post, userId)
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving saved posts for user {UserId}", userId);
                throw;
            }
        }

        // ===== Reports =====

        public async Task<PostReportDto> ReportPostAsync(int postId, int userId, CreateReportDto dto)
        {
            try
            {
                var post = await _communityRepository.GetPostByIdAsync(postId);
                if (post == null)
                    throw new KeyNotFoundException("Post not found");

                if (string.IsNullOrWhiteSpace(dto.Reason))
                    throw new ArgumentException("Report reason cannot be empty");

                if (dto.Reason.Length > 500)
                    throw new ArgumentException("Reason cannot exceed 500 characters");

                // تحقق إن المستخدم مش بيبلغ عن بوسته
                if (post.UserId == userId)
                    throw new InvalidOperationException("You cannot report your own post");

                // تحقق إن المستخدم مش بعت report قبل كده
                var existing = await _communityRepository.GetReportAsync(postId, userId);
                if (existing != null)
                    throw new InvalidOperationException("You already reported this post");

                var report = new PostReports
                {
                    PostId = postId,
                    ReporterId = userId,
                    Reason = dto.Reason.Trim(),
                    CreatedAt = DateTime.Now
                };

                var added = await _communityRepository.AddReportAsync(report);

                _logger.LogInformation("Post {PostId} reported by user {UserId}", postId, userId);

                return new PostReportDto
                {
                    ReportId = added.ReportId,
                    PostId = added.PostId,
                    ReporterId = added.ReporterId,
                    ReporterName = $"{post.User?.FirstName} {post.User?.LastName}",
                    Reason = added.Reason,
                    CreatedAt = added.CreatedAt,
                    IsReviewed = false
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reporting post {PostId}", postId);
                throw;
            }
        }
        public async Task<List<PostReportDto>> GetAllReportsAsync()
        {
            try
            {
                var reports = await _communityRepository.GetAllReportsAsync();
                return reports.Select(r => MapToReportDto(r)).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all reports");
                throw;
            }
        }

        public async Task<PostReportDto> GetReportByIdAsync(int reportId)
        {
            try
            {
                var report = await _communityRepository.GetReportByIdAsync(reportId);
                if (report == null)
                    throw new KeyNotFoundException("Report not found");

                return MapToReportDto(report);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving report {ReportId}", reportId);
                throw;
            }
        }

        public async Task<List<PostReportDto>> GetPendingReportsAsync()
        {
            try
            {
                var reports = await _communityRepository.GetPendingReportsAsync();
                return reports.Select(r => MapToReportDto(r)).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving pending reports");
                throw;
            }
        }

        public async Task<List<PostReportDto>> GetReviewedReportsAsync()
        {
            try
            {
                var reports = await _communityRepository.GetReviewedReportsAsync();
                return reports.Select(r => MapToReportDto(r)).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving reviewed reports");
                throw;
            }
        }

        public async Task<PostReportDto> ReviewReportAsync(
        int reportId, int adminId, ReviewReportDto dto)
        {
            try
            {
                var report = await _communityRepository.GetReportByIdAsync(reportId);
                if (report == null)
                    throw new KeyNotFoundException("Report not found");

                if (report.ReviewedById != null)
                    throw new InvalidOperationException("Report already reviewed");

                // Validate action
                var validActions = new[] { "Dismiss", "DeletePost", "Warn" };
                if (!validActions.Contains(dto.Action))
                    throw new ArgumentException(
                        "Invalid action. Must be: Dismiss, DeletePost, Warn");

                switch (dto.Action)
                {
                    case "DeletePost":
                        var post = await _communityRepository.GetPostByIdAsync(report.PostId);
                        if (post != null)
                        {
                            if (post.PostMedia != null && post.PostMedia.Any())
                                foreach (var media in post.PostMedia)
                                    await _fileStorageService.DeleteFileAsync(media.MediaUrl);

                            await _communityRepository.DeletePostAsync(post);
                            // TODO: Send notification to post owner
                        }
                        break;

                    case "Warn":
                        // TODO: Send warning notification to post owner
                        break;

                    case "Dismiss":
                        break;
                }

                report.ReviewedById = adminId;
                report.ReviewedAt = DateTime.Now;
                report.Action = dto.Action;
                report.AdminNote = dto.AdminNote?.Trim();
                var updated = await _communityRepository.UpdateReportAsync(report);

                // أضيفي السطر ده
                var freshReport = await _communityRepository.GetReportByIdAsync(updated.ReportId);
                _logger.LogInformation(
                    "Report {ReportId} reviewed by admin {AdminId} with action {Action}",
                    reportId, adminId, dto.Action);

                return MapToReportDto(freshReport!);

                
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reviewing report {ReportId}", reportId);
                throw;
            }
        }

        private PostReportDto MapToReportDto(PostReports report)
        {
            return new PostReportDto
            {
                ReportId = report.ReportId,
                PostId = report.PostId,
                ReporterId = report.ReporterId,
                ReporterName = report.Reporter != null
                    ? $"{report.Reporter.FirstName} {report.Reporter.LastName}"
                    : "",
                Reason = report.Reason,
                ReviewedByName = report.ReviewedBy != null
                    ? $"{report.ReviewedBy.FirstName} {report.ReviewedBy.LastName}"
                    : null,
                Action = report.Action,        
                AdminNote = report.AdminNote,  
                CreatedAt = report.CreatedAt,
                ReviewedAt = report.ReviewedAt,
                IsReviewed = report.ReviewedById != null
            };
        }
    }
}