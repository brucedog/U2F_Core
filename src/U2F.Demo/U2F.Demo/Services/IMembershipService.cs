using System.Collections.Generic;
using System.Threading.Tasks;
using U2F.Demo.Models;

namespace U2F.Demo.Services
{
    public interface IMembershipService
    {
        /// <summary>
        /// Checks to see if the provided username is registered
        /// </summary>
        /// <param name="username">user name to check.</param>
        /// <returns>true if user is registered</returns>
        Task<bool> IsUserRegistered(string username);

        /// <summary>
        /// Checks to see if the username and password are correct
        /// </summary>
        /// <param name="username">username</param>
        /// <param name="password">hashed password</param>
        /// <returns>true if the provided username and password is valid</returns>
        Task<bool> IsValidUserNameAndPassword(string username, string password);

        /// <summary>
        /// Generates a list of challenges for each device the user has registered
        /// </summary>
        /// <param name="username">username to generate challenges for.</param>
        /// <returns>list of challenges for each device the user has registered.</returns>
        Task<List<ServerChallenge>> GenerateDeviceChallenges(string username);

        Task<ServerRegisterResponse> GenerateServerChallenge(string username);

        Task<bool> AuthenticateUser(string userName, string deviceResponse);

        Task<bool> SaveNewUser(string username, string password, string emailAddress);

        Task<bool> CompleteRegistration(string userName, string deviceResponse);

        /// <summary>
        /// Signs user out
        /// </summary>
        Task SignOut();

        /// <summary>
        /// Returns a complete user object via its userName
        /// </summary>
        Task<User> FindUserByUsername(string username);
    }
}