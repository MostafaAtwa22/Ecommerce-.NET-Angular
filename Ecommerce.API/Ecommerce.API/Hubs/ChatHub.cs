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

namespace Ecommerce.API.Hubs
{
    [Authorize(Roles = "Admin,SuperAdmin")]
    public partial class ChatHub : Hub<IChatClient>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IChatbotService _chatbotService;
        public static readonly ConcurrentDictionary<string, OnlineUserDto> _onlineUsers = new();
        
        public ChatHub(UserManager<ApplicationUser> userManager,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IChatbotService chatbotService)
        {
            _userManager = userManager;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _chatbotService = chatbotService;
        }

        public override async Task OnConnectedAsync()
        {
            var httpConext = Context.GetHttpContext();

            var receiverId = httpConext?.Request.Query["receiverId"].ToString();

            var sender = await _userManager.FindUserByClaimPrinciplesAsync(Context?.User!);
            if (sender is null)
                throw new HubException("Sender is not exists");
            var connectionId = Context?.ConnectionId;

            if (_onlineUsers.ContainsKey(sender.Id))
            {
                _onlineUsers.AddOrUpdate(sender.Id,
                    _ => new OnlineUserDto {ConnectionId = connectionId!},
                    (_, existing) =>  {
                        existing.ConnectionId = connectionId!;
                        return existing;
                        }!
                );
            }
            else
            {
                var user = _mapper.Map<OnlineUserDto>(sender);
                user.ConnectionId = connectionId!;
                _onlineUsers.TryAdd(sender.Id, user);

                var currentUser = _mapper.Map<ProfileResponseDto>(sender);
                await Clients.AllExcept(connectionId!).Notify(currentUser);
            }

            if (!string.IsNullOrEmpty(receiverId))
                await LoadMessages(new MessageSpecParams { SenderId = receiverId });
            
            await Clients.All.OnlineUsers(await GetAllUsers());
        }
        
        public async Task SendMessage(MessageRequestDto messageDto)
        {
            // Validate input
            if (string.IsNullOrWhiteSpace(messageDto.Content))
                throw new HubException("Message content cannot be empty");

            if (string.IsNullOrWhiteSpace(messageDto.ReciverId))
                throw new HubException("Receiver ID is required");

            var sender = await _userManager.FindUserByClaimPrinciplesAsync(Context?.User!);
            if (sender is null)
                throw new HubException("Sender is not exists");

            var receiver = await _userManager.FindByIdAsync(messageDto.ReciverId);
            if (receiver is null)
                throw new HubException($"Receiver with ID '{messageDto.ReciverId}' not found");

            var message = _mapper.Map<Message>(messageDto);
            message.SenderId = sender.Id;

            await _unitOfWork.Repository<Message>()
                .Create(message);
            await _unitOfWork.Complete();
            
            var response = _mapper.Map<MessageResponseDto>(message);
            await Clients.User(messageDto.ReciverId).ReceiveNewMessage(response);
        }

        public async Task NotifyTyping(string receiverUserName)
        {
            if (string.IsNullOrWhiteSpace(receiverUserName))
                throw new HubException("Receiver username is required");

            var sender = await _userManager.FindUserByClaimPrinciplesAsync(Context?.User!);
            if (sender is null)
                throw new HubException("Sender not found");

            var connectionId = _onlineUsers.Values.FirstOrDefault(x => x.UserName == receiverUserName)?.ConnectionId;
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
                throw new HubException("Received user ID cannot be null or empty");
            
            var otherUser = await _userManager.FindByIdAsync(specParams.SenderId);
            if (otherUser is null)
                throw new HubException($"User with ID '{specParams.SenderId}' not found");

            specParams.ReceiverId = currentUser.Id;

            var spec = MessageSpecifications.BuildChatHistorySpec(specParams);
            var countSpec = MessageSpecifications.BuildChatHistoryCountSpec(specParams);

            var totalItems = await _unitOfWork.Repository<Message>().CountAsync(countSpec);
            var messages = await _unitOfWork.Repository<Message>().GetAllWithSpecAsync(spec);

            // Flip to chronological order for the client
            var orderedMessages = messages.OrderBy(x => x.CreatedAt).ToList();

            // Mark unread messages as read
            var unreadSpec = MessageSpecifications.BuildUnreadMessagesSpec(currentUser.Id, specParams.SenderId);
            var unreadMessages = await _unitOfWork.Repository<Message>().GetAllWithSpecAsync(unreadSpec);

            if (unreadMessages.Any())
            {
                foreach (var message in unreadMessages)
                {
                    message.IsRead = true;
                    _unitOfWork.Repository<Message>().Update(message);
                }
                await _unitOfWork.Complete();
            }

            var messageDtos = _mapper.Map<IReadOnlyList<MessageResponseDto>>(orderedMessages);
            
            var pagination = new Pagination<MessageResponseDto>(specParams.PageIndex, specParams.PageSize, totalItems, messageDtos);
            
            await Clients.Caller.ReceiveMessageList(pagination);
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var currentUser = await _userManager.FindUserByClaimPrinciplesAsync(Context?.User!);
            if (currentUser is not null)
            {
                _onlineUsers.TryRemove(currentUser.Id, out _);
                await Clients.All.OnlineUsers(await GetAllUsers());
            }
        }

        private async Task<IEnumerable<OnlineUserDto>> GetAllUsers()
        {
            var currentUser = await _userManager.FindUserByClaimPrinciplesAsync(Context?.User!);
            if (currentUser is null)
                return Enumerable.Empty<OnlineUserDto>();

            var onlineUserIds = _onlineUsers.Keys.ToList();

            // Get all users
            var allUsers = await _userManager.Users.ToListAsync();

            // Get unread message counts for current user from all senders in a single query
            var unreadCounts = await _unitOfWork.Repository<Message>().GetAllQueryable()
                .Where(x => x.ReciverId == currentUser.Id && !x.IsRead)
                .GroupBy(x => x.SenderId)
                .Select(g => new { SenderId = g.Key, Count = g.Count() })
                .ToListAsync();

            var unreadCountDict = unreadCounts.ToDictionary(x => x.SenderId, x => x.Count);

            var users = allUsers.Select(u =>
            {
                var dto = _mapper.Map<OnlineUserDto>(u);
                dto.IsOnline = onlineUserIds.Contains(u.Id);
                dto.UnReadCount = unreadCountDict.ContainsKey(u.Id) ? unreadCountDict[u.Id] : 0;
                return dto;
            }).OrderByDescending(u => u.IsOnline);

            return users;
        }
    }
}