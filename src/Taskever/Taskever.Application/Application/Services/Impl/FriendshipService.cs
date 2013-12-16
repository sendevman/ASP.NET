using System;
using System.Linq;
using System.Net.Mail;
using System.Text;
using Abp.Domain.Uow;
using Abp.Exceptions;
using Abp.Modules.Core.Application.Services;
using Abp.Modules.Core.Application.Services.Impl;
using Abp.Modules.Core.Domain.Entities;
using Abp.Modules.Core.Domain.Repositories;
using Taskever.Application.Services.Dto.Friendships;
using Taskever.Domain.Entities;
using Taskever.Domain.Enums;
using Taskever.Domain.Policies;
using Taskever.Domain.Repositories;

namespace Taskever.Application.Services.Impl
{
    public class FriendshipService : IFriendshipService
    {
        private readonly IUserRepository _userRepository;
        private readonly IFriendshipRepository _friendshipRepository;
        private readonly IEmailService _emailService;
        private readonly IFriendshipPolicy _friendshipPolicy;

        public FriendshipService(IUserRepository userRepository, IFriendshipRepository friendshipRepository, IFriendshipPolicy friendshipPolicy, IEmailService emailService)
        {
            _userRepository = userRepository;
            _friendshipRepository = friendshipRepository;
            _friendshipPolicy = friendshipPolicy;
            _emailService = emailService;
        }

        public virtual GetFriendshipsOutput GetFriendships(GetFriendshipsInput input)
        {
            //TODO: Check if current user can see friendships of the the requested user!
            var friendships = _friendshipRepository.GetAllWithFriendUser(input.UserId, input.Status, input.CanAssignTask);
            return new GetFriendshipsOutput { Friendships = friendships.MapIList<Friendship, FriendshipDto>() };
        }

        [UnitOfWork]
        public GetFriendshipsByMostActiveOutput GetFriendshipsByMostActive(GetFriendshipsByMostActiveInput input)
        {
            var friendships =
                _friendshipRepository
                    .GetAllWithFriendUser(User.CurrentUserId)
                    .Where(f => f.Status == FriendshipStatus.Accepted)
                    .OrderByDescending(friendship => friendship.LastVisitTime)
                    .Take(input.MaxResultCount)
                    .ToList();

            return new GetFriendshipsByMostActiveOutput { Friendships = friendships.MapIList<Friendship, FriendshipDto>() };
        }

        [UnitOfWork]
        public virtual void ChangeFriendshipProperties(ChangeFriendshipPropertiesInput input)
        {
            var currentUser = _userRepository.Load(User.CurrentUserId);
            var friendShip = _friendshipRepository.Get(input.Id); //TODO: Call GetOrNull and throw a specific exception?

            if (!_friendshipPolicy.CanChangeFriendshipProperties(currentUser, friendShip))
            {
                throw new ApplicationException("Can not change properties of this friendship!");
            }

            //TODO: Implement mappings using Auto mapper!

            if (input.CanAssignTask.HasValue)
            {
                friendShip.CanAssignTask = input.CanAssignTask.Value;
            }

            if (input.FollowActivities.HasValue)
            {
                friendShip.FollowActivities = input.FollowActivities.Value;
            }
        }

        [UnitOfWork]
        public virtual SendFriendshipRequestOutput SendFriendshipRequest(SendFriendshipRequestInput input)
        {
            var friendUser = _userRepository.Query(q => q.FirstOrDefault(user => user.EmailAddress == input.EmailAddress));
            if (friendUser == null)
            {
                throw new AbpUserFriendlyException("Can not find a user with email address: " + input.EmailAddress);
            }

            var currentUser = _userRepository.Load(User.CurrentUserId);

            //Check if they are already friends
            var friendship = _friendshipRepository.GetOrNull(currentUser.Id, friendUser.Id);
            if (friendship != null)
            {
                if (friendship.CanBeAcceptedBy(currentUser))
                {
                    friendship.AcceptBy(currentUser);
                }

                return new SendFriendshipRequestOutput { Status = friendship.Status };
            }

            //Add new friendship request
            friendship = Friendship.CreateAsRequest(currentUser, friendUser);
            _friendshipRepository.Insert(friendship);

            SendRequestEmail(friendship);

            return new SendFriendshipRequestOutput { Status = friendship.Status };
        }

        [UnitOfWork] //TODO: Need UnitOfWork, I think no!
        public virtual void RemoveFriendship(RemoveFriendshipInput input)
        {
            var currentUser = _userRepository.Load(User.CurrentUserId);
            var friendship = _friendshipRepository.Get(input.Id); //TODO: Call GetOrNull and throw a specific exception?

            if (!_friendshipPolicy.CanRemoveFriendship(currentUser, friendship)) //TODO: Maybe this method can throw exception!
            {
                throw new ApplicationException("Can not remove this friendship!"); //TODO: User friendliy exception
            }

            _friendshipRepository.Delete(friendship);
        }

        [UnitOfWork]
        public virtual void AcceptFriendship(AcceptFriendshipInput input)
        {
            var friendship = _friendshipRepository.Get(input.Id); //TODO: Call GetOrNull and throw a specific exception?
            var currentUser = _userRepository.Load(User.CurrentUserId);

            friendship.AcceptBy(currentUser);
        }

        public virtual void RejectFriendship(RejectFriendshipInput input)
        {
            RemoveFriendship(new RemoveFriendshipInput { Id = input.Id });
        }

        public void CancelFriendshipRequest(CancelFriendshipRequestInput input)
        {
            RemoveFriendship(new RemoveFriendshipInput { Id = input.Id });
        }

        [UnitOfWork]
        public void UpdateLastVisitTime(UpdateLastVisitTimeInput input)
        {
            var friendship = _friendshipRepository.GetOrNull(User.CurrentUserId, input.FriendUserId);
            if (friendship != null)
            {
                friendship.LastVisitTime = DateTime.Now;
            }
        }

        private void SendRequestEmail(Friendship friendship)
        {
            var mail = new MailMessage();
            mail.To.Add(friendship.Friend.EmailAddress);
            mail.IsBodyHtml = true;
            mail.Subject = friendship.User.NameAndSurname + " wants to be your friend on Taskever";
            mail.SubjectEncoding = Encoding.UTF8;

            var mailBuilder = new StringBuilder();
            mailBuilder.Append(
@"<!DOCTYPE html>
<html lang=""en"" xmlns=""http://www.w3.org/1999/xhtml"">
<head>
    <meta charset=""utf-8"" />
    <title>{TEXT_HTML_TITLE}</title>
    <style>
        body {
            font-family: Verdana, Geneva, 'DejaVu Sans', sans-serif;
            font-size: 12px;
        }
    </style>
</head>
<body>
    <h3>{TEXT_HEADER}</h3>
    <p>{TEXT_DESCRIPTION}</p>
    <p>&nbsp;</p>
    <p><a href=""http://www.taskever.com/#friends?activeSection=FriendshipRequests"">{TEXT_CLICK_TO_ANSWER}</a></p>
    <p>&nbsp;</p>
    <p><a href=""http://www.taskever.com"">http://www.taskever.com</a></p>
</body>
</html>");

            mailBuilder.Replace("{TEXT_HTML_TITLE}", "Friendship request on Taskever");
            mailBuilder.Replace("{TEXT_HEADER}", "You have a friendship request on Taskever");
            mailBuilder.Replace("{TEXT_DESCRIPTION}", friendship.User.NameAndSurname + " has sent a friendship request to you.");
            mailBuilder.Replace("{TEXT_CLICK_TO_ANSWER}", "Click here to answer to the request.");

            mail.Body = mailBuilder.ToString();
            mail.BodyEncoding = Encoding.UTF8;

            _emailService.SendEmail(mail);
        }
    }
}