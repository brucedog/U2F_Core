using System;
using System.Linq;

namespace U2F.Core.Models
{
    public class StartedRegistration : BaseModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StartedRegistration"/> class.
        /// </summary>
        /// <param name="challenge">The challenge.</param>
        /// <param name="appId">The application identifier.</param>
        public StartedRegistration(string challenge, string appId)
        {
            if(string.IsNullOrWhiteSpace(challenge) || string.IsNullOrWhiteSpace(appId))
                throw new ArgumentException("Invalid argument(s) were being passed.");

            Version = Crypto.U2F.U2FVersion;
            Challenge = challenge;
            AppId = appId;
        }

        /// <summary>
        /// Gets or sets the version.
        /// Version of the protocol that the to-be-registered U2F token must speak. For
        /// the version of the protocol described herein, must be "U2F_V2"
        /// </summary>
        /// <value>
        /// The version.
        /// </value>
        public string Version { get; private set; }

        /// <summary>
        /// Gets the challenge.
        /// The websafe-base64-encoded challenge.
        /// </summary>
        /// <value>
        /// The challenge.
        /// </value>
        public string Challenge { get; private set; }

        /// <summary>
        /// Gets the application identifier.
        /// The application id that the RP would like to assert. The U2F token will
        /// enforce that the key handle provided above is associated with this
        /// application id. The browser enforces that the calling origin belongs to the
        /// application identified by the application id.
        /// </summary>
        /// <value>
        /// The application identifier.
        /// </value>
        public string AppId { get; private set; }

        public override int GetHashCode()
        {
            int hash = Version.Sum(c => c + 31);
            hash += Challenge.Sum(c => c + 31);
            hash += AppId.Sum(c => c + 31);

            return hash;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is StartedRegistration))
                return false;
            if (this == obj)
                return true;
            if (GetType() != obj.GetType())
                return false;
            StartedRegistration other = (StartedRegistration)obj;
            if (AppId == null)
            {
                if (other.AppId != null)
                    return false;
            }
            else if (!AppId.Equals(other.AppId))
                return false;
            if (Challenge == null)
            {
                if (other.Challenge != null)
                    return false;
            }
            else if (!Challenge.Equals(other.Challenge))
                return false;
            if (Version == null)
            {
                if (other.Version != null)
                    return false;
            }
            else if (!Version.Equals(other.Version))
                return false;
            return true;
        }
    }
}