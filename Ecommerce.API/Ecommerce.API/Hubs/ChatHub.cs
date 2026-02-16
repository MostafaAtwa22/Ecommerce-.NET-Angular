using System.Collections.Concurrent;
using AutoMapper;
using Ecommerce.API.Dtos.Requests;
using Ecommerce.API.Dtos.Responses;
using Ecommerce.API.Extensions;
using Ecommerce.Core.Entities.Chat;
using Ecommerce.Core.Entities.Identity;
using Ecommerce.Core.Interfaces;
using Ecommerce.API.Dtos;
using Microsoft.AspNetCore.Authorization;
using Ecommerce.Core.Spec;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Ecommerce.Core.Params;
using Ecommerce.Core.Constants;

namespace Ecommerce.API.Hubs
{
    [Authorize(Roles = "Admin,SuperAdmin")]
    public partial class ChatHub : Hub<IChatClient>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IChatbotService _chatbotService;
        private readonly IWebHostEnvironment _environment;
        public static readonly ConcurrentDictionary<string, OnlineUserDto> _onlineUsers = new();

        public ChatHub(UserManager<ApplicationUser> userManager,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IChatbotService chatbotService,
            IWebHostEnvironment environment)
        {
            _userManager = userManager;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _chatbotService = chatbotService;
            _environment = environment;
        }

        public override async Task OnConnectedAsync()
        {
            var httpContext = Context.GetHttpContext();
            var otherUserId = httpContext?.Request.Query["otherUserId"].ToString();

            var sender = await _userManager.FindUserByClaimPrinciplesAsync(Context?.User!);
            if (sender is null)
                throw new HubException("Sender does not exist");

            var connectionId = Context?.ConnectionId;

            if (_onlineUsers.ContainsKey(sender.Id))
            {
                _onlineUsers.AddOrUpdate(sender.Id,
                    _ => new OnlineUserDto { ConnectionId = connectionId! },
                    (_, existing) =>
                    {
                        existing.ConnectionId = connectionId!;
                        return existing;
                    }
                );
            }
            else
            {
                var user = _mapper.Map<OnlineUserDto>(sender);
                user.ConnectionId = connectionId!;
                user.IsOnline = true;
                _onlineUsers.TryAdd(sender.Id, user);

                var currentUser = _mapper.Map<ProfileResponseDto>(sender);
                await Clients.AllExcept(connectionId!).Notify(currentUser);
                
                // Broadcast online status to others (without unread counts)
                var statusUpdate = _mapper.Map<OnlineUserDto>(sender);
                statusUpdate.IsOnline = true;
                await Clients.AllExcept(connectionId!).UpdateUserStatus(statusUpdate);
            }

            if (!string.IsNullOrEmpty(otherUserId))
            {
                if (_onlineUsers.TryGetValue(sender.Id, out var onlineUser))
                {
                    onlineUser.CurrentChatUserId = otherUserId;
                }
                await LoadMessages(new MessageSpecParams { SenderId = otherUserId });
            }

            // Mark all messages TO this user as received now that they are connected
            var unreceivedSpec = MessageSpecifications.BuildUnreceivedMessagesSpec(sender.Id);
            var unreceivedMessages = await _unitOfWork.Repository<Message>().GetAllWithSpecAsync(unreceivedSpec);
            if (unreceivedMessages.Any())
            {
                var sendersToNotify = unreceivedMessages.Select(m => m.SenderId).Distinct();
                foreach (var msg in unreceivedMessages)
                {
                    msg.IsReceived = true;
                    _unitOfWork.Repository<Message>().Update(msg);
                }
                await _unitOfWork.Complete();

                foreach (var senderId in sendersToNotify)
                {
                    await Clients.User(senderId).MarkMessagesAsReceived(sender.Id);
                }
            }

            // Send initial user list only to the caller
            await Clients.Caller.OnlineUsers(await GetUsersForUser(sender.Id));
        }

        public async Task SendMessage(MessageRequestDto messageDto)
        {
            if (string.IsNullOrWhiteSpace(messageDto.Content) && string.IsNullOrWhiteSpace(messageDto.AttachmentUrl))
                throw new HubException("Message content or attachment is required");

            if (string.IsNullOrWhiteSpace(messageDto.ReciverId))
                throw new HubException("Receiver ID is required");

            var sender = await _userManager.FindUserByClaimPrinciplesAsync(Context?.User!);
            if (sender is null)
                throw new HubException("Sender does not exist");

            var receiver = await _userManager.FindByIdAsync(messageDto.ReciverId);
            if (receiver is null)
                throw new HubException($"Receiver with ID '{messageDto.ReciverId}' not found");

            var message = _mapper.Map<Message>(messageDto);
            message.SenderId = sender.Id;
            message.CreatedAt = DateTimeOffset.UtcNow;
            message.IsRead = false;
            
            var receiverIsOnline = _onlineUsers.TryGetValue(receiver.Id, out var receiverOnlineInfo);
            message.IsReceived = receiverIsOnline;

            // If receiver is looking at this chat, mark as read immediately
            if (receiverIsOnline && receiverOnlineInfo!.CurrentChatUserId == sender.Id)
            {
                message.IsRead = true;
            }

            message.AttachmentUrl = messageDto.AttachmentUrl;
            message.AttachmentName = messageDto.AttachmentName;
            message.AttachmentType = messageDto.AttachmentType;

            await _unitOfWork.Repository<Message>().Create(message);
            await _unitOfWork.Complete();

            var response = _mapper.Map<MessageResponseDto>(message);

            // Send to receiver
            await Clients.User(messageDto.ReciverId).ReceiveNewMessage(response);

            // Send back to sender
            await Clients.Caller.ReceiveNewMessage(response);

            // Update sidebar for receiver (new unread message and last message)
            var senderForReceiverSidebar = await GetUserForSidebar(sender.Id, receiver.Id);
            await Clients.User(receiver.Id).UpdateUserSidebar(senderForReceiverSidebar);

            // Update sidebar for sender (last message)
            var receiverForSenderSidebar = await GetUserForSidebar(receiver.Id, sender.Id);
            await Clients.Caller.UpdateUserSidebar(receiverForSenderSidebar);
        }

        public async Task EditMessage(int messageId, string newContent)
        {
            if (string.IsNullOrWhiteSpace(newContent))
                throw new HubException("Message content cannot be empty");

            var sender = await _userManager.FindUserByClaimPrinciplesAsync(Context?.User!);
            if (sender is null) throw new HubException("User not found");

            var message = await _unitOfWork.Repository<Message>().GetByIdAsync(messageId);
            if (message is null) throw new HubException("Message not found");

            if (message.SenderId != sender.Id)
                throw new HubException("You are not authorized to edit this message");

            message.Content = newContent;
            message.IsEdited = true;

            _unitOfWork.Repository<Message>().Update(message);
            await _unitOfWork.Complete();

            var response = _mapper.Map<MessageResponseDto>(message);

            await Clients.User(message.ReciverId).MessageEdited(response);
            await Clients.Caller.MessageEdited(response);
        }

        public async Task DeleteMessage(int messageId)
        {
            var sender = await _userManager.FindUserByClaimPrinciplesAsync(Context?.User!);
            if (sender is null) throw new HubException("User not found");

            var message = await _unitOfWork.Repository<Message>().GetByIdAsync(messageId);
            if (message is null) throw new HubException("Message not found");

            if (message.SenderId != sender.Id)
                throw new HubException("You are not authorized to delete this message");

            _unitOfWork.Repository<Message>().Delete(message);
            await _unitOfWork.Complete();

            // We might want to send the ID or the updated DTO. Sending ID is usually enough for deletion/hiding.
            await Clients.User(message.ReciverId).MessageDeleted(messageId);
            await Clients.Caller.MessageDeleted(messageId);
        }

        public async Task MarkMessageAsRead(int messageId)
        {
            var currentUser = await _userManager.FindUserByClaimPrinciplesAsync(Context?.User!);
            if (currentUser is null) throw new HubException("User not found");

            var message = await _unitOfWork.Repository<Message>().GetByIdAsync(messageId);
            if (message is null) return;

            if (message.ReciverId != currentUser.Id)
                return;

            if (!message.IsRead)
            {
                message.IsRead = true;
                _unitOfWork.Repository<Message>().Update(message);
                await _unitOfWork.Complete();

                // Notify sender
                await Clients.User(message.SenderId).ReceiveMessageRead(messageId);

                // Update receiver's own sidebar to decrement unread count
                var senderForSidebar = await GetUserForSidebar(message.SenderId, currentUser.Id);
                await Clients.Caller.UpdateUserSidebar(senderForSidebar);
            }
        }

        public async Task MarkAllMessagesAsRead(string otherUserId)
        {
            var currentUser = await _userManager.FindUserByClaimPrinciplesAsync(Context?.User!);
            if (currentUser is null) throw new HubException("User not found");

            var unreadSpec = MessageSpecifications.BuildUnreadMessagesSpec(currentUser.Id, otherUserId);
            var unreadMessages = await _unitOfWork.Repository<Message>().GetAllWithSpecAsync(unreadSpec);

            if (unreadMessages.Any())
            {
                foreach (var message in unreadMessages)
                {
                    message.IsRead = true;
                    _unitOfWork.Repository<Message>().Update(message);
                }
                await _unitOfWork.Complete();

                // Notify sender (the other user) that their messages were read
                await Clients.User(otherUserId).MarkMessagesAsRead(currentUser.Id);

                // Update receiver's own sidebar
                var otherUserForSidebar = await GetUserForSidebar(otherUserId, currentUser.Id);
                await Clients.Caller.UpdateUserSidebar(otherUserForSidebar);
            }
        }

        public async Task NotifyTyping(string receiverUserName)
        {
            if (string.IsNullOrWhiteSpace(receiverUserName))
                throw new HubException("Receiver username is required");

            var sender = await _userManager.FindUserByClaimPrinciplesAsync(Context?.User!);
            if (sender is null)
                throw new HubException("Sender not found");

            var connectionId = _onlineUsers.Values
                .FirstOrDefault(x => x.UserName == receiverUserName)?.ConnectionId;

            if (connectionId is null)
                return;

            await Clients.Client(connectionId).NotifyTypingToUser(sender.UserName!);
        }

        public async Task LoadMessages(MessageSpecParams specParams)
        {
            var currentUser = await _userManager.FindUserByClaimPrinciplesAsync(Context?.User!);
            if (currentUser is null)
                throw new HubException("Current user not found");

            if (string.IsNullOrEmpty(specParams.SenderId))
                throw new HubException("Other user ID cannot be null or empty");

            var otherUser = await _userManager.FindByIdAsync(specParams.SenderId);
            if (otherUser is null)
                throw new HubException($"User with ID '{specParams.SenderId}' not found");

            // Set the receiver as current user
            specParams.ReceiverId = currentUser.Id;

            if (_onlineUsers.TryGetValue(currentUser.Id, out var onlineUser))
            {
                onlineUser.CurrentChatUserId = specParams.SenderId;
            }

            var spec = MessageSpecifications.BuildChatHistorySpec(specParams);
            var countSpec = MessageSpecifications.BuildChatHistoryCountSpec(specParams);

            var totalItems = await _unitOfWork.Repository<Message>().CountAsync(countSpec);
            var messages = await _unitOfWork.Repository<Message>().GetAllWithSpecAsync(spec);

            var orderedMessages = messages.OrderBy(x => x.CreatedAt).ToList();

            var unreadSpec = MessageSpecifications.BuildUnreadMessagesSpec(currentUser.Id, specParams.SenderId);
            var unreadMessages = await _unitOfWork.Repository<Message>().GetAllWithSpecAsync(unreadSpec);

            if (unreadMessages.Any())
            {
                foreach (var message in unreadMessages)
                {
                    message.IsRead = true;
                    message.IsReceived = true;
                    _unitOfWork.Repository<Message>().Update(message);
                }
                await _unitOfWork.Complete();

                // Update caller's sidebar (clear unread counts)
                await Clients.Caller.OnlineUsers(await GetUsersForUser(currentUser.Id));

                // Notify the other user (sender) that their messages were read
                await Clients.User(specParams.SenderId).MarkMessagesAsRead(currentUser.Id);
            }

            var messageDtos = _mapper.Map<IReadOnlyList<MessageResponseDto>>(orderedMessages);

            var pagination = new Pagination<MessageResponseDto>(
                specParams.PageIndex,
                specParams.PageSize,
                totalItems,
                messageDtos
            );

            await Clients.Caller.ReceiveMessageList(pagination);
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var currentUser = await _userManager.FindUserByClaimPrinciplesAsync(Context?.User!);
            if (currentUser is not null)
            {
                _onlineUsers.TryRemove(currentUser.Id, out _);
                
                // Broadcast offline status to others
                var statusUpdate = _mapper.Map<OnlineUserDto>(currentUser);
                statusUpdate.IsOnline = false;
                await Clients.AllExcept(Context?.ConnectionId!).UpdateUserStatus(statusUpdate);
            }

            await base.OnDisconnectedAsync(exception);
        }

        private async Task<IEnumerable<OnlineUserDto>> GetUsersForUser(string currentUserId)
        {
            var onlineUserIds = _onlineUsers.Keys.ToList();

            var admins = await _userManager.GetUsersInRoleAsync(Role.Admin.ToString());
            var superAdmins = await _userManager.GetUsersInRoleAsync(Role.SuperAdmin.ToString());

            var allUsers = admins.Concat(superAdmins)
                .DistinctBy(x => x.Id)
                .ToList();

            var unreadCounts = await _unitOfWork.Repository<Message>().GetAllQueryable()
                .Where(x => x.ReciverId == currentUserId && !x.IsRead)
                .GroupBy(x => x.SenderId)
                .Select(g => new { SenderId = g.Key, Count = g.Count() })
                .ToListAsync();

            var unreadCountDict = unreadCounts.ToDictionary(x => x.SenderId, x => x.Count);

            var lastMessages = await _unitOfWork.Repository<Message>().GetAllQueryable()
                .Where(x => x.SenderId == currentUserId || x.ReciverId == currentUserId)
                .GroupBy(x => x.SenderId == currentUserId ? x.ReciverId : x.SenderId)
                .Select(g => new
                {
                    UserId = g.Key,
                    Message = g.OrderByDescending(m => m.CreatedAt).FirstOrDefault()
                })
                .ToListAsync();

            var lastMessageDict = lastMessages.ToDictionary(x => x.UserId, x => x.Message);

            var users = allUsers.Select(u =>
            {
                var dto = _mapper.Map<OnlineUserDto>(u);
                dto.IsOnline = onlineUserIds.Contains(u.Id);
                dto.UnReadCount = unreadCountDict.ContainsKey(u.Id) ? unreadCountDict[u.Id] : 0;

                if (lastMessageDict.TryGetValue(u.Id, out var lastMsg) && lastMsg != null)
                {
                    dto.LastMessage = !string.IsNullOrEmpty(lastMsg.Content)
                        ? lastMsg.Content
                        : (lastMsg.AttachmentUrl != null ? "Sent an attachment 📎" : "");
                    dto.LastMessageTime = lastMsg.CreatedAt;
                }

                return dto;
            })
            .Where(u => u.Id != currentUserId) // Don't include self
            .OrderByDescending(u => u.IsOnline)
            .ThenByDescending(u => u.LastMessageTime);

            return users;
        }

        private async Task<OnlineUserDto> GetUserForSidebar(string targetUserId, string forUserId)
        {
            var targetUser = await _userManager.FindByIdAsync(targetUserId);
            if (targetUser == null) return null!;

            var dto = _mapper.Map<OnlineUserDto>(targetUser);
            dto.IsOnline = _onlineUsers.ContainsKey(targetUserId);

            dto.UnReadCount = await _unitOfWork.Repository<Message>().GetAllQueryable()
                .CountAsync(x => x.ReciverId == forUserId && x.SenderId == targetUserId && !x.IsRead);

            var lastMsg = await _unitOfWork.Repository<Message>().GetAllQueryable()
                .Where(x => (x.SenderId == forUserId && x.ReciverId == targetUserId) ||
                            (x.SenderId == targetUserId && x.ReciverId == forUserId))
                .OrderByDescending(x => x.CreatedAt)
                .FirstOrDefaultAsync();

            if (lastMsg != null)
            {
                dto.LastMessage = !string.IsNullOrEmpty(lastMsg.Content)
                    ? lastMsg.Content
                    : (lastMsg.AttachmentUrl != null ? "Sent an attachment 📎" : "");
                dto.LastMessageTime = lastMsg.CreatedAt;
            }

            return dto;
        }
    }
}