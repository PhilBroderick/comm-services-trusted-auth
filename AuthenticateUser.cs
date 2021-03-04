using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Azure.Communication;
using Azure.Communication.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace CommServices.TrustedAuth
{
    public static class AuthenticateUser
    {
        private static readonly List<User> AuthenticatedUsers = new List<User>();
        
        [FunctionName("AuthenticateUser")]
        public static async Task<IActionResult> RunAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)]
            HttpRequest req, ILogger log)
        {
            var userId = req.Query["userId"];
            User user;

            var connectionString = Environment.GetEnvironmentVariable("COMM_SERVICES_CONNECTION_STRING");
            var client = new CommunicationIdentityClient(connectionString);
            
            if (string.IsNullOrWhiteSpace(userId) || AuthenticatedUsers.All(a => a.Id != userId))
            {
                var identityWithTokenResponse =
                    await client.CreateUserWithTokenAsync(new[] {CommunicationTokenScope.Chat});

                user = new User(identityWithTokenResponse.Value.user.Id,
                    identityWithTokenResponse.Value.token.ExpiresOn, identityWithTokenResponse.Value.token.Token);
                AuthenticatedUsers.Add(user);
            }
            else
            {
                var existingUser = AuthenticatedUsers.Single(a => a.Id == userId);
                if (existingUser.ExpiresOn > DateTimeOffset.Now)
                {
                    user = existingUser;
                }
                else
                {
                    var identityToRefresh = new CommunicationUserIdentifier(existingUser.Id);
                    var updatedToken =
                        await client.IssueTokenAsync(identityToRefresh, new[] {CommunicationTokenScope.Chat });
                    existingUser.AccessToken = updatedToken.Value.Token;
                    existingUser.ExpiresOn = updatedToken.Value.ExpiresOn;
                    user = existingUser;
                }
            }

            return new OkObjectResult(user);
        }
    }

    public class User
    {
        public User(string id, DateTimeOffset expiresOn, string accessToken)
        {
            Id = id;
            ExpiresOn = expiresOn;
            AccessToken = accessToken;
        }
        public string Id { get; }
        public DateTimeOffset ExpiresOn { get; set; }
        public string AccessToken { get; set; }
    }
}