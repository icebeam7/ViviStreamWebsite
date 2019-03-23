using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ViviStreamWebsite.Helpers
{
    public class ForceOfflineGoogleAuthorizationCodeFlow : GoogleAuthorizationCodeFlow
    {
        public ForceOfflineGoogleAuthorizationCodeFlow(
            GoogleAuthorizationCodeFlow.Initializer initializer) : base(initializer) { }

        public override AuthorizationCodeRequestUrl CreateAuthorizationCodeRequest(string redirectUri)
        {
            return new GoogleAuthorizationCodeRequestUrl(
                new Uri(AuthorizationServerUrl))
            {
                ClientId = ClientSecrets.ClientId,
                Scope = string.Join(" ", Scopes),
                RedirectUri = redirectUri,
                AccessType = "offline",
                ApprovalPrompt = "force"
            };
        }
    }
}
