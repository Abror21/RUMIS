using System;

namespace Izm.Rumis.Domain.Exceptions
{
    public class InvalidUserProfileException : Exception
    {
        public Guid UserId { get; private set; }
        public Guid UserIdInProfile { get; private set; }

        public InvalidUserProfileException(Guid userId, Guid userProfileId)
            : this(userId, userProfileId, null)
        {
        }

        public InvalidUserProfileException(Guid userId, Guid userProfileId, string message)
            : base(message)
        {
            UserId = userId;
            UserIdInProfile = userProfileId;
        }
    }
}
