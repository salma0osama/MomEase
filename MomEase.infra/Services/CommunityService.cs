using Microsoft.Extensions.Logging;
using MomEase.core.DTOS.CommunityDTO;
using MomEase.core.Entities;
using MomEase.core.Enums;
using MomEase.core.Interfaces;
using Microsoft.AspNetCore.Http;

namespace MomEase.infra.Services
{
    public class CommunityService : ICommunityService
    {
        private readonly ICommunityRepository _communityRepository;
        private readonly IUserRepository _userRepository;
        private readonly IFileStorageService _fileStorageService;
        private readonly INotificationService _notificationService;
        private readonly ILogger<CommunityService> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;


        public CommunityService(
            ICommunityRepository communityRepository,
            IUserRepository userRepository,
            IFileStorageService fileStorageService,
            INotificationService notificationService,
            ILogger<CommunityService> logger,
            IHttpContextAccessor httpContextAccessor)
        {
            _communityRepository = communityRepository;
            _userRepository = userRepository;
            _fileStorageService = fileStorageService;
            _notificationService = notificationService;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
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

        public async Task DeletePostAsync(int postId, int userId, bool isAdmin = false)
        {
            try
            {
                var post = await _communityRepository.GetPostByIdAsync(postId);
                if (post == null)
                    throw new KeyNotFoundException("Post not found");

                // الأدمن يقدر يمسح أي بوست
                if (!isAdmin && post.UserId != userId)
                    throw new UnauthorizedAccessException(
                        "You are not authorized to delete this post"); ;

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

                // 🔔 إرسال Notification لصاحب البوست (إلا لو هو نفسه اللي علّق)
                if (post.UserId != userId)
                {
                    _logger.LogInformation(
                        "Sending comment notification to post owner {PostOwnerId}",
                        post.UserId);

                    try
                    {
                        // ✅ اجلب بيانات المعلق
                        var commenter = full?.User;
                        var commenterName = commenter != null
                            ? $"{commenter.FirstName} {commenter.LastName}".Trim()
                            : "Unknown User";

                        // ✅ اجلب لغة صاحب البوست
                        var postOwner = post.User;
                        var lang = postOwner?.PreferredLanguage ?? "en";

                        // ✅ بناء الرسالة مع الاسم واللغة
                        string title, message;
                        var commentPreview = dto.Text.Length > 60
                            ? dto.Text.Substring(0, 60) + "..."
                            : dto.Text;

                        if (lang == "ar")
                        {
                            title = " تعليق جديد على بوستك";
                            message = $"{commenterName} علق: \"{commentPreview}\"";
                        }
                        else
                        {
                            title = " New comment on your post";
                            message = $"{commenterName} commented: \"{commentPreview}\"";
                        }

                        // ✅ إرسال Notification
                        await _notificationService.SendRealtimeNotificationAsync(
                            post.UserId,
                            title,
                            message,
                            "CommunityComment",
                            postId,
                            actionUrl: $"/posts/{postId}"
                        );

                        _logger.LogInformation(
                            "Comment notification sent to user {UserId} about comment from {CommenterName}",
                            post.UserId, commenterName);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to send comment notification");
                        // لا نرمي Exception - Comment تم إضافته بنجاح
                    }
                }

                _logger.LogInformation("Comment added {CommentId} on post {PostId}",
                    added.CommentId, postId);

                return MapToCommentDto(full!, userId, post.UserId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding comment on post {PostId}", postId);
                throw;
            }
        }

        public async Task<List<PostCommentDto>> GetPostCommentsAsync(
        int postId, int currentUserId)
        {
            try
            {
                var post = await _communityRepository.GetPostByIdAsync(postId);
                if (post == null)
                    throw new KeyNotFoundException("Post not found");

                var comments = await _communityRepository.GetPostCommentsAsync(postId);
                return comments.Select(c =>
                    MapToCommentDto(c, currentUserId, post.UserId)).ToList();
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
                var post = await _communityRepository.GetPostByIdAsync(postId);

                return MapToCommentDto(comment, comment.UserId, post!.UserId);
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
                var post = await _communityRepository.GetPostByIdAsync(postId);

                return MapToCommentDto(updated, userId, post!.UserId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating comment {CommentId}", commentId);
                throw;
            }
        }

        public async Task DeleteCommentAsync(
    int commentId, int postId, int userId, bool isAdmin = false)
        {
            try
            {
                var comment = await _communityRepository.GetCommentByIdAsync(commentId);
                if (comment == null || comment.PostId != postId)
                    throw new KeyNotFoundException("Comment not found");

                // ✅ صاحب الكومنت أو صاحب البوست أو الأدمن
                var post = await _communityRepository.GetPostByIdAsync(postId);
                bool isPostOwner = post?.UserId == userId;
                bool isCommentOwner = comment.UserId == userId;

                if (!isCommentOwner && !isPostOwner && !isAdmin)
                    throw new UnauthorizedAccessException(
                        "You are not authorized to delete this comment");

                await _communityRepository.DeleteCommentAsync(comment);
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

                // 🔔 إرسال Notification لصاحب البوست (إلا لو هو نفسه اللي تفاعل)
                if (post.UserId != userId)
                {
                    _logger.LogInformation(
                        "Sending reaction notification to post owner {PostOwnerId}",
                        post.UserId);

                    try
                    {
                        // ✅ اجلب بيانات الشخص اللي عمل Reaction
                        var reactor = await _userRepository.GetByIdAsync(userId);
                        var reactorName = reactor != null
                            ? $"{reactor.FirstName} {reactor.LastName}".Trim()
                            : "Unknown User";

                        // ✅ اجلب لغة صاحب البوست
                        var postOwner = post.User;
                        var lang = postOwner?.PreferredLanguage ?? "en";

                        // ✅ Map Reaction Type to Emoji
                        string reactionEmoji = reactionType switch
                        {
                            ReactionType.LIKE => "👍",
                            ReactionType.LOVE => "❤️",
                            ReactionType.SUPPORT => "🤗",
                            ReactionType.HELPFUL => "💡",
                            _ => "👏"
                        };

                        // ✅ بناء الرسالة مع الاسم واللغة
                        string title, message;

                        if (lang == "ar")
                        {
                            title = $" تفاعل جديد على بوستك";
                            message = $"{reactorName} عمل {GetArabicReactionName(reactionType)} على بوستك {reactionEmoji}";
                        }
                        else
                        {
                            title = " New reaction on your post";
                            message = $"{reactorName} reacted {reactionEmoji} to your post";
                        }

                        // ✅ إرسال Notification
                        await _notificationService.SendRealtimeNotificationAsync(
                            post.UserId,
                            title,
                            message,
                            "CommunityReaction",
                            postId,
                            actionUrl: $"/posts/{postId}"
                        );

                        _logger.LogInformation(
                            "Reaction notification sent to user {UserId} about reaction from {ReactorName}",
                            post.UserId, reactorName);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to send reaction notification");
                        // لا نرمي Exception - Reaction تمت إضافتها بنجاح
                    }
                }

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

                if (post.UserId != userId)
                {
                    try
                    {
                        // ✅ اجلب بيانات الشخص اللي عمل Save
                        var saver = await _userRepository.GetByIdAsync(userId);
                        var saverName = saver != null
                            ? $"{saver.FirstName} {saver.LastName}".Trim()
                            : "Unknown User";

                        // ✅ اجلب لغة صاحب البوست
                        var postOwner = post.User;
                        var lang = postOwner?.PreferredLanguage ?? "en";

                        // ✅ بناء الرسالة مع الاسم واللغة
                        string title, message;

                        if (lang == "ar")
                        {
                            title = " حفظ جديد لبوستك";
                            message = $"{saverName} حفظ بوستك";
                        }
                        else
                        {
                            title = " Your post was saved";
                            message = $"{saverName} saved your post";
                        }

                        // ✅ إرسال Notification
                        await _notificationService.SendRealtimeNotificationAsync(
                            post.UserId,
                            title,
                            message,
                            "CommunityPostSaved",
                            postId,
                            actionUrl: $"/posts/{postId}"
                        );

                        _logger.LogInformation(
                            "Save notification sent to user {UserId} about save from {SaverName}",
                            post.UserId, saverName);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to send save notification");
                        // لا نرمي Exception - Post تم حفظه بنجاح
                    }
                }

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
                    PostId = added.PostId ?? 0,
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

                var post = await _communityRepository.GetPostByIdNoTrackingAsync(report.PostId ?? 0);
                if (dto.Action == "DeletePost" && post == null)
                    throw new KeyNotFoundException("Post not found or already deleted");

                switch (dto.Action)
                {
                    case "DeletePost":
                        // 🔔 إرسال Notification: Admin حذف البوست
                        if (post != null)
                        {
                            // ✅ اجلب لغة صاحب البوست
                            var postOwner = post.User;
                            var lang = postOwner?.PreferredLanguage ?? "en";

                            // حذف الـ Media files
                            if (post.PostMedia != null && post.PostMedia.Any())
                                foreach (var media in post.PostMedia)
                                    await _fileStorageService.DeleteFileAsync(media.MediaUrl);

                            // حذف الـ Post
                            await _communityRepository.DeletePostAsync(post);

                            _logger.LogInformation(
                                "Sending post deletion notification to user {UserId}",
                                post.UserId);

                            try
                            {
                                // ✅ بناء الرسالة حسب اللغة
                                string title, message;

                                if (lang == "ar")
                                {
                                    title = "تم حذف بوستك";
                                    message = $"تم حذف بوستك من قبل الإدارة لمخالفة قوانين المجتمع.\n\nالسبب: {dto.AdminNote ?? "محتوى غير مناسب"}";
                                }
                                else
                                {
                                    title = "Post Removed";
                                    message = $"Your post has been removed by admin due to community guideline violations.\n\nReason: {dto.AdminNote ?? "Inappropriate content"}";
                                }

                                // ✅ إرسال Notification
                                await _notificationService.SendRealtimeNotificationAsync(
                                    post.UserId,
                                    title,
                                    message,
                                    "CommunityPostDeleted",
                                    post.PostId,
                                    actionUrl: $"/my-posts"
                                );

                                _logger.LogInformation(
                                    "Post deletion notification sent to user {UserId}",
                                    post.UserId);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, "Failed to send post deletion notification");
                            }
                        }
                        else
                        {
                            // ✅ البوست اتحذف قبل كده — نكمل بدون error
                            _logger.LogWarning(
                                "Post {PostId} already deleted when reviewing report {ReportId}",
                                report.PostId, reportId);
                        }
                        break;

                    case "Warn":
                        // 🔔 إرسال Notification: Admin بعت تحذير
                        if (post != null)
                        {
                            // ✅ اجلب لغة صاحب البوست
                            var postOwner = post.User;
                            var lang = postOwner?.PreferredLanguage ?? "en";

                            _logger.LogInformation(
                                "Sending warning notification to user {UserId}",
                                post.UserId);

                            try
                            {
                                // ✅ بناء الرسالة حسب اللغة
                                string title, message;

                                if (lang == "ar")
                                {
                                    title = " تحذير من الإدارة";
                                    message = dto.AdminNote ?? "بوستك ينتهك قوانين المجتمع. يرجى مراجعة سياساتنا وتجنب تكرار هذا المحتوى.";
                                }
                                else
                                {
                                    title = " Warning from Admin";
                                    message = dto.AdminNote ?? "Your post violates community guidelines. Please review our policies and avoid similar content.";
                                }

                                // ✅ إرسال Notification
                                await _notificationService.SendRealtimeNotificationAsync(
                                    post.UserId,
                                    title,
                                    message,
                                    "CommunityWarning",
                                    post.PostId,
                                    actionUrl: $"/posts/{post.PostId}"
                                );

                                _logger.LogInformation(
                                    "Warning notification sent to user {UserId}",
                                    post.UserId);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, "Failed to send warning notification");
                            }
                        }
                        break;

                    case "Dismiss":
                        // مفيش notification - البلاغ اترفض
                        _logger.LogInformation(
                            "Report {ReportId} dismissed by admin {AdminId}",
                            reportId, adminId);
                        break;
                }

                // تحديث الـ Report في DB
                await _communityRepository.UpdateReportFields(
                    reportId, adminId, dto.Action, dto.AdminNote?.Trim());

                // ✅ جيبي الـ report محدّث
                var freshReport = await _communityRepository.GetReportByIdAsync(reportId);

                return new PostReportDto
                {
                    ReportId = reportId,
                    PostId = report.PostId ?? 0,
                    ReporterId = report.ReporterId,
                    ReporterName = freshReport?.Reporter != null
                        ? $"{freshReport.Reporter.FirstName} {freshReport.Reporter.LastName}"
                        : "",
                    Reason = report.Reason,
                    ReviewedByName = freshReport?.ReviewedBy != null
                        ? $"{freshReport.ReviewedBy.FirstName} {freshReport.ReviewedBy.LastName}"
                        : "",
                    Action = dto.Action,
                    AdminNote = dto.AdminNote?.Trim(),
                    CreatedAt = report.CreatedAt,
                    ReviewedAt = DateTime.Now,
                    IsReviewed = true
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reviewing report {ReportId}", reportId);
                throw;
            }
        }
        // ===== Comment Replies =====

        public async Task<CommentReplyDto> AddReplyAsync(
            int commentId, int userId, CreateReplyDto dto)
        {
            try
            {
                var comment = await _communityRepository.GetCommentByIdAsync(commentId);
                if (comment == null)
                    throw new KeyNotFoundException("Comment not found");

                if (string.IsNullOrWhiteSpace(dto.Text))
                    throw new ArgumentException("Reply text cannot be empty");

                var reply = new CommentReply
                {
                    CommentId = commentId,
                    UserId = userId,
                    Text = dto.Text.Trim(),
                    CreatedAt = DateTime.Now
                };

                var added = await _communityRepository.AddReplyAsync(reply);
                var full = await _communityRepository.GetReplyByIdAsync(added.ReplyId);

                if (comment.UserId != userId)
                {
                    try
                    {
                        // ✅ اجلب بيانات الشخص اللي عمل Reply
                        var replier = full?.User;
                        var replierName = replier != null
                            ? $"{replier.FirstName} {replier.LastName}".Trim()
                            : "Unknown User";

                        // ✅ اجلب لغة صاحب الكومنت
                        var commentOwner = comment.User;
                        var lang = commentOwner?.PreferredLanguage ?? "en";

                        // ✅ بناء الرسالة مع الاسم واللغة
                        string title, message;
                        var replyPreview = dto.Text.Length > 60
                            ? dto.Text.Substring(0, 60) + "..."
                            : dto.Text;

                        if (lang == "ar")
                        {
                            title = " رد جديد على تعليقك";
                            message = $"{replierName} رد: \"{replyPreview}\"";
                        }
                        else
                        {
                            title = " New reply to your comment";
                            message = $"{replierName} replied: \"{replyPreview}\"";
                        }

                        // ✅ إرسال Notification
                        await _notificationService.SendRealtimeNotificationAsync(
                            comment.UserId,
                            title,
                            message,
                            "CommunityCommentReply",
                            commentId,
                            actionUrl: $"/posts/{comment.PostId}#comment-{commentId}"
                        );

                        _logger.LogInformation(
                            "Reply notification sent to user {UserId} about reply from {ReplierName}",
                            comment.UserId, replierName);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to send reply notification");
                        // لا نرمي Exception - Reply تم إضافتها بنجاح
                    }
                }

                _logger.LogInformation("Reply added {ReplyId} on comment {CommentId}",
                    added.ReplyId, commentId);

                return MapToReplyDto(full!, userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding reply to comment {CommentId}", commentId);
                throw;
            }
        }

        public async Task<List<CommentReplyDto>> GetCommentRepliesAsync(
            int commentId, int currentUserId)
        {
            try
            {
                var comment = await _communityRepository.GetCommentByIdAsync(commentId);
                if (comment == null)
                    throw new KeyNotFoundException("Comment not found");

                var replies = await _communityRepository.GetCommentRepliesAsync(commentId);
                return replies.Select(r => MapToReplyDto(r, currentUserId)).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving replies for comment {CommentId}", commentId);
                throw;
            }
        }

        public async Task<CommentReplyDto> UpdateReplyAsync(
            int replyId, int commentId, int userId, UpdateReplyDto dto)
        {
            try
            {
                var reply = await _communityRepository.GetReplyByIdAsync(replyId);
                if (reply == null || reply.CommentId != commentId)
                    throw new KeyNotFoundException("Reply not found");

                if (reply.UserId != userId)
                    throw new UnauthorizedAccessException(
                        "You are not authorized to update this reply");

                if (string.IsNullOrWhiteSpace(dto.Text))
                    throw new ArgumentException("Reply text cannot be empty");

                reply.Text = dto.Text.Trim();
                reply.UpdatedAt = DateTime.Now;

                var updated = await _communityRepository.UpdateReplyAsync(reply);
                return MapToReplyDto(updated, userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating reply {ReplyId}", replyId);
                throw;
            }
        }

        public async Task DeleteReplyAsync(int replyId, int commentId, int userId)
        {
            try
            {
                var reply = await _communityRepository.GetReplyByIdAsync(replyId);
                if (reply == null || reply.CommentId != commentId)
                    throw new KeyNotFoundException("Reply not found");

                if (reply.UserId != userId)
                    throw new UnauthorizedAccessException(
                        "You are not authorized to delete this reply");

                await _communityRepository.DeleteReplyAsync(reply);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting reply {ReplyId}", replyId);
                throw;
            }
        }

        // ===== Comment Reactions =====

        public async Task<CommentReactionDto> AddCommentReactionAsync(
    int commentId, int userId, AddCommentReactionDto dto)
        {
            try
            {
                var comment = await _communityRepository.GetCommentByIdAsync(commentId);
                if (comment == null)
                    throw new KeyNotFoundException("Comment not found");

                if (!Enum.TryParse<ReactionType>(dto.ReactionType, true, out var reactionType))
                    throw new ArgumentException(
                        "Invalid reaction type. Must be: LIKE, LOVE, SUPPORT, HELPFUL");

                var existing = await _communityRepository
                    .GetUserCommentReactionAsync(commentId, userId);
                if (existing != null)
                    throw new InvalidOperationException(
                        "You already reacted to this comment. Use PUT to update.");

                var reaction = new CommentReaction
                {
                    CommentId = commentId,
                    UserId = userId,
                    ReactionType = reactionType
                };

                var added = await _communityRepository.AddCommentReactionAsync(reaction);

                // 🔔 إرسال Notification لصاحب الكومنت (إلا لو هو نفسه)
                if (comment.UserId != userId)
                {
                    try
                    {
                        // ✅ اجلب بيانات الشخص اللي عمل Reaction
                        var reactor = await _userRepository.GetByIdAsync(userId);
                        var reactorName = reactor != null
                            ? $"{reactor.FirstName} {reactor.LastName}".Trim()
                            : "Unknown User";

                        // ✅ اجلب لغة صاحب الكومنت
                        var commentOwner = comment.User;
                        var lang = commentOwner?.PreferredLanguage ?? "en";

                        // ✅ Map Reaction Type to Emoji and Arabic Name
                        string reactionEmoji = reactionType switch
                        {
                            ReactionType.LIKE => "👍",
                            ReactionType.LOVE => "❤️",
                            ReactionType.SUPPORT => "🤗",
                            ReactionType.HELPFUL => "💡",
                            _ => "👏"
                        };

                        string arabicReactionName = GetArabicReactionName(reactionType);

                        // ✅ بناء الرسالة مع الاسم واللغة
                        string title, message;

                        if (lang == "ar")
                        {
                            title = " تفاعل جديد على تعليقك";
                            message = $"{reactorName} عمل {arabicReactionName} {reactionEmoji} على تعليقك";
                        }
                        else
                        {
                            title = " New reaction on your comment";
                            message = $"{reactorName} reacted {reactionEmoji} to your comment";
                        }

                        // ✅ إرسال Notification
                        await _notificationService.SendRealtimeNotificationAsync(
                            comment.UserId,
                            title,
                            message,
                            "CommunityCommentReaction",
                            commentId,
                            actionUrl: $"/posts/{comment.PostId}#comment-{commentId}"
                        );

                        _logger.LogInformation(
                            "Comment reaction notification sent to user {UserId} about reaction from {ReactorName}",
                            comment.UserId, reactorName);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to send comment reaction notification");
                        // لا نرمي Exception - Reaction تمت إضافتها بنجاح
                    }
                }

                _logger.LogInformation("Reaction added to comment {CommentId} by user {UserId}",
                    commentId, userId);

                return MapToCommentReactionDto(added);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding reaction to comment {CommentId}", commentId);
                throw;
            }
        }

        public async Task<List<CommentReactionDto>> GetCommentReactionsAsync(int commentId)
        {
            try
            {
                var comment = await _communityRepository.GetCommentByIdAsync(commentId);
                if (comment == null)
                    throw new KeyNotFoundException("Comment not found");

                var reactions = await _communityRepository.GetCommentReactionsAsync(commentId);
                return reactions.Select(r => MapToCommentReactionDto(r)).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving reactions for comment {CommentId}", commentId);
                throw;
            }
        }

        public async Task<CommentReactionsCountDto> GetCommentReactionsCountAsync(int commentId)
        {
            try
            {
                var reactions = await _communityRepository.GetCommentReactionsAsync(commentId);
                var byType = reactions
                    .GroupBy(r => r.ReactionType.ToString())
                    .ToDictionary(g => g.Key, g => g.Count());

                return new CommentReactionsCountDto
                {
                    TotalCount = reactions.Count,
                    ByType = byType
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving reactions count for comment {CommentId}", commentId);
                throw;
            }
        }


        public async Task<CommentReactionDto> UpdateCommentReactionAsync(
    int commentId, int userId, UpdateCommentReactionDto dto)
        {
            try
            {
                var reaction = await _communityRepository
                    .GetUserCommentReactionAsync(commentId, userId);
                if (reaction == null)
                    throw new KeyNotFoundException("Reaction not found");

                if (!Enum.TryParse<ReactionType>(dto.ReactionType, true, out var reactionType))
                    throw new ArgumentException(
                        "Invalid reaction type. Must be: LIKE, LOVE, SUPPORT, HELPFUL");

                reaction.ReactionType = reactionType;
                var updated = await _communityRepository.UpdateCommentReactionAsync(reaction);

                // ✅ Reload مع الـ User data
                var fresh = await _communityRepository
                    .GetCommentReactionByIdAsync(updated.ReactionId);

                return MapToCommentReactionDto(fresh!);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating reaction for comment {CommentId}", commentId);
                throw;
            }
        }

        public async Task DeleteCommentReactionAsync(int commentId, int userId)
        {
            try
            {
                var reaction = await _communityRepository
                    .GetUserCommentReactionAsync(commentId, userId);
                if (reaction == null)
                    throw new KeyNotFoundException("Reaction not found");

                await _communityRepository.DeleteCommentReactionAsync(reaction);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting reaction for comment {CommentId}", commentId);
                throw;
            }
        }

        // ===== Private Mappers =====

        private CommentReplyDto MapToReplyDto(CommentReply reply, int currentUserId)
        {
            return new CommentReplyDto
            {
                ReplyId = reply.ReplyId,
                CommentId = reply.CommentId,
                UserId = reply.UserId,
                UserName = reply.User != null
                    ? $"{reply.User.FirstName} {reply.User.LastName}"
                    : "",
                UserPhoto = reply.User?.MotherProfile?.ProfilePictureUrl != null
                    ? GetFullUrl(reply.User.MotherProfile.ProfilePictureUrl)
                    : null,
                Text = reply.Text,
                IsMyReply = reply.UserId == currentUserId,
                CreatedAt = reply.CreatedAt,
                UpdatedAt = reply.UpdatedAt
            };
        }

        private CommentReactionDto MapToCommentReactionDto(CommentReaction reaction)
        {
            return new CommentReactionDto
            {
                ReactionId = reaction.ReactionId,
                CommentId = reaction.CommentId,
                UserId = reaction.UserId,
                UserName = reaction.User != null
                    ? $"{reaction.User.FirstName} {reaction.User.LastName}"
                    : "",
                UserPhoto = reaction.User?.MotherProfile?.ProfilePictureUrl != null
                    ? GetFullUrl(reaction.User.MotherProfile.ProfilePictureUrl)
                    : null,
                ReactionType = reaction.ReactionType.ToString()
            };
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
                Text = post.Text,
                Media = post.PostMedia?.OrderBy(m => m.Order)

    .Select(m => new PostMediaDto
    {
        MediaId = m.MediaId,
        MediaUrl = GetFullUrl(m.MediaUrl),
        MediaType = m.MediaType.ToString(),
        Order = m.Order
    }).ToList() ?? new(),
                CommentsCount = post.PostComments?.Count ?? 0,
                ReactionsCount = post.PostReactions?.Count ?? 0,
                MyReaction = myReaction?.ReactionType.ToString(),
                CreatedAt = post.CreatedAt,
                UpdatedAt = post.UpdatedAt,
                IsSaved = post.SavedPosts?.Any(s => s.UserId == currentUserId) ?? false,
                IsMyPost = post.UserId == currentUserId
            };
        }

        private PostCommentDto MapToCommentDto(
    PostComments comment, int currentUserId, int postOwnerId)
        {
            var myReaction = comment.Reactions?
                .FirstOrDefault(r => r.UserId == currentUserId);

            return new PostCommentDto
            {
                CommentId = comment.CommentId,
                PostId = comment.PostId,
                UserId = comment.UserId,
                UserName = comment.User != null
                    ? $"{comment.User.FirstName} {comment.User.LastName}"
                    : "",
                UserPhoto = comment.User?.MotherProfile?.ProfilePictureUrl != null
                    ? GetFullUrl(comment.User.MotherProfile.ProfilePictureUrl)
                    : null,
                Text = comment.Text,
                IsMyComment = comment.UserId == currentUserId,
                CanDelete = comment.UserId == currentUserId || postOwnerId == currentUserId,
                RepliesCount = comment.Replies?.Count ?? 0,
                ReactionsCount = comment.Reactions?.Count ?? 0,
                MyReaction = myReaction?.ReactionType.ToString(),
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
                UserPhoto = reaction.User?.MotherProfile?.ProfilePictureUrl != null
                    ? GetFullUrl(reaction.User.MotherProfile.ProfilePictureUrl)
                    : null,
                ReactionType = reaction.ReactionType.ToString()
            };
        }

        private PostReportDto MapToReportDto(PostReports report)
        {
            return new PostReportDto
            {
                ReportId = report.ReportId,
                PostId = report.PostId ?? 0,
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
        private string GetFullUrl(string relativeUrl)
        {
            if (string.IsNullOrEmpty(relativeUrl)) return relativeUrl;
            if (relativeUrl.StartsWith("http")) return relativeUrl;

            var request = _httpContextAccessor.HttpContext?.Request;
            if (request == null) return relativeUrl;

            return $"{request.Scheme}://{request.Host}{relativeUrl}";
        }
        private string GetArabicReactionName(ReactionType type)
        {
            return type switch
            {
                ReactionType.LIKE => "لايك",
                ReactionType.LOVE => "حب",
                ReactionType.SUPPORT => "دعم",
                ReactionType.HELPFUL => "مفيد",
                _ => "تفاعل"
            };
        }

        private async Task<Users?> GetUserByIdAsync(int userId)
        {
            return await _userRepository.GetByIdAsync(userId);
        }
    }
}