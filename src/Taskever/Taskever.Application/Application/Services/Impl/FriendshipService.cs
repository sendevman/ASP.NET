using System;
using System.Linq;
using Abp.Domain.Uow;
using Abp.Exceptions;
using Abp.Modules.Core.Application.Services.Impl;
using Abp.Modules.Core.Data.Repositories;
using Abp.Modules.Core.Domain.Entities;
using Taskever.Application.Services.Dto.Friendships;
using Taskever.Data.Repositories;
using Taskever.Domain.Entities;
using Taskever.Domain.Enums;
using Taskever.Domain.Policies;

namespace Taskever.Application.Services.Impl
{
    public class FriendshipService : IFriendshipService
    {
        private readonly IUserRepository _userRepository;
        private readonly IFriendshipRepository _friendshipRepository;

        private readonly IFriendshipPolicy _friendshipPolicy;

        public FriendshipService(IUserRepository userRepository, IFriendshipRepository friendshipRepository, IFriendshipPolicy friendshipPolicy)
        {
            _userRepository = userRepository;
            _friendshipRepository = friendshipRepository;
            _friendshipPolicy = friendshipPolicy;
        }

        public GetFriendshipsOutput GetFriendships(GetFriendshipsInput input)
        {
            //TODO: Check if current user can see friendships of the the requested user!
            var friendships = _friendshipRepository.GetAllWithFriendUser(input.UserId, input.Status, input.CanAssignTask);
            return new GetFriendshipsOutput { Friendships = friendships.MapIList<Friendship, FriendshipDto>() };
        }

        [UnitOfWork]
        public ChangeFriendshipPropertiesOutput ChangeFriendshipProperties(ChangeFriendshipPropertiesInput input)
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

            if (input.FallowActivities.HasValue)
            {
                friendShip.FallowActivities = input.FallowActivities.Value;
            }

            _friendshipRepository.Update(friendShip);

            return new ChangeFriendshipPropertiesOutput();
        }

        public SendFriendshipRequestOutput SendFriendshipRequest(SendFriendshipRequestInput input)
        {
            var friendUser = _userRepository.Query(q => q.FirstOrDefault(user => user.EmailAddress == input.EmailAddress));
            if (friendUser == null)
            {
                throw new AbpUserFriendlyException("Can not find a user with email address: " + input.EmailAddress);
            }

            var currentUser = _userRepository.Load(User.CurrentUserId);

            _friendshipRepository.Insert(
                new Friendship
                    {
                        User = currentUser,
                        Status = FriendshipStatus.WaitingApprovalFromFriend,
                        Friend = friendUser
                    });

            _friendshipRepository.Insert(
                new Friendship
                    {
                        User = friendUser,
                        Status = FriendshipStatus.WaitingApprovalFromUser,
                        Friend = currentUser
                    });

            return new SendFriendshipRequestOutput();
        }

        [UnitOfWork]
        public RemoveFriendshipOutput RemoveFriendship(RemoveFriendshipInput input)
        {
            var friendShipOfUser = _friendshipRepository.Get(input.Id); //TODO: Call GetOrNull and throw a specific exception?
            if (friendShipOfUser.User.Id != User.CurrentUserId) //TODO: Do in a policy?
            {
                throw new ApplicationException("Can not remove this friendship!"); //TODO: User friendliy exception
            }

            _friendshipRepository.Delete(friendShipOfUser.Id);

            var friendshipOfFriend = _friendshipRepository.GetOrNull(friendShipOfUser.Friend.Id, friendShipOfUser.User.Id);
            if (friendshipOfFriend != null)
            {
                _friendshipRepository.Delete(friendshipOfFriend);
            }

            return new RemoveFriendshipOutput();
        }

        [UnitOfWork]
        public AcceptFriendshipOutput AcceptFriendship(AcceptFriendshipInput input)
        {
            var currentUser = _userRepository.Load(User.CurrentUserId);

            var friendship = _friendshipRepository.Get(input.Id); //TODO: Call GetOrNull and throw a specific exception?
            friendship.Accept(currentUser);

            var friendshipOfFriend = _friendshipRepository.GetOrNull(friendship.Friend.Id, friendship.User.Id);
            if (friendshipOfFriend != null)
            {
                friendshipOfFriend.Accept(currentUser);
            }

            return new AcceptFriendshipOutput();
        }
    }
}