using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using OwlApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace OwlApi.Helpers
{
    public class KeycloakClient
    {
        public string authServerUrl;
        public string authServerUsername;
        public string authServerPassword;
        public string authAudience;

        public KeycloakClient(IConfiguration configuration)
        {
            authServerUrl = configuration["Authentication:AuthServerUrl"];
            authServerUsername = configuration["Authentication:Username"];
            authServerPassword = configuration["Authentication:Password"];
            authAudience = configuration["Authentication:Audience"];
        }

        public class KeycloakUser
        {
            public string Id { get; set; }
            public string Email { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public List<KeycloakRole> Roles { get; set; }
        }

        public async Task<KeycloakUser> GetUserData(string realm, string userId, string authToken)
        {
            var kcUrl = $"{authServerUrl}/auth/admin/realms/{realm}/users/{userId}";
            var res = await HttpHelper.JsonGetRequest(kcUrl, authToken);
            var user = JObject.Parse(res.response);
            var roles = await GetUserRoles(realm, userId, authToken);
            return new KeycloakUser()
            {
                Id = user["id"].ToString(),
                Email = user["email"].ToString(),
                FirstName = user["firstName"].ToString(),
                LastName = user["lastName"].ToString(),
                Roles = roles
            };
        }

        public async Task<KeycloakUser> CreateUserForCompany(string realm, string email, string password, string firstName, string lastName, string authToken, string[] roles)
        {
            if (authToken == null)
            {
                authToken = "Bearer " + await GetAdminAccessToken();
            }

            var userId = await CreateUser(realm, email, password, firstName, lastName, authToken);
            var rolesToSet = new List<string>();
            rolesToSet.AddRange(roles);
            await SetRolesForUser(realm, userId, rolesToSet.ToArray(), authToken);
            return await GetUserData(realm, userId, authToken);
        }

        private async Task<string> CreateUser(string realm, string email, string password, string firstName, string lastName, string authToken)
        {
            var realmEndpoint = $"{authServerUrl}/auth/admin/realms/{realm}/users";

            var body = PrepareUserJObject(email, password, firstName, lastName);

            var response = await HttpHelper.JsonPostRequest(realmEndpoint, body, authToken);
            var locationHeader = response.headers.Location.ToString();

            var match = Regex.Match(locationHeader, "\\/users\\/(.+?)$");
            var userId = match.Groups[1].Value;
            return userId;
        }

        public async Task<KeycloakUser> UpdateUserForCompany(Company company, string userId, string password, string firstName, string lastName, string authToken, string[] roles)
        {
            await UpdateUser(company.RealmName, userId, password, firstName, lastName, authToken);
            if (roles != null)
            {
                var rolesToSet = new List<string>();
                rolesToSet.AddRange(roles);
                await SetRolesForUser(company.RealmName, userId, rolesToSet.ToArray(), authToken);
            }
            return await GetUserData(company.RealmName, userId, authToken);
        }

        private async Task UpdateUser(string realm, string userId, string password, string firstName, string lastName, string authToken)
        {
            var realmEndpoint = $"{authServerUrl}/auth/admin/realms/{realm}/users/{userId}";

            var body = PrepareUserJObject(null, password, firstName, lastName);
            if (!body.HasValues)
            {
                return;
            }

            var response = await HttpHelper.JsonPutRequest(realmEndpoint, body, authToken);
        }

        private JObject PrepareUserJObject(string email, string password, string firstName, string lastName)
        {
            JObject body = new JObject();

            if (email != null)
            {
                var emailTrimmed = email.Trim();
                body.Add("username", emailTrimmed);
                body.Add("email", emailTrimmed);
                body.Add("emailVerified", true);
            }

            if (firstName != null)
            {
                body.Add("firstName", firstName);
            }

            if (lastName != null)
            {
                body.Add("lastName", lastName);
            }


            if (password != null)
            {
                JArray credentials = new JArray();
                JObject passwordCredential = new JObject();
                passwordCredential.Add("temporary", false);
                passwordCredential.Add("type", "password");
                passwordCredential.Add("value", password);
                credentials.Add(passwordCredential);
                body.Add("credentials", credentials);
            }

            if (body.HasValues)
            {
                body.Add("enabled", true);
            }

            return body;
        }

        public async Task DeleteUser(string realm, string userId, string authToken)
        {
            var realmEndpoint = $"{authServerUrl}/auth/admin/realms/{realm}/users/{userId}";
            var body = new JObject();
            var response = await HttpHelper.JsonDeleteRequest(realmEndpoint, body, authToken);
        }

        public async Task SetRolesForUser(string realm, string userId, string[] rolesToSet, string authToken, string[] rolesToDelete = null)
        {
            var userRoles = await GetUserRoles(realm, userId, authToken);
            var realmRoles = await GetRealmRoles(realm, authToken);

            var rolesToAdd = GetUserRolesToAdd(rolesToSet, userRoles, realmRoles);
            var rolesToRemove = new List<KeycloakRole>();
            if (rolesToDelete != null)
            {
                rolesToRemove = stringRolesToKeycloakRoles(rolesToDelete.ToList(), realmRoles);
            }
            else
            {
                rolesToRemove = GetUserRolesToRemove(rolesToSet, userRoles);
            }

            await AddRolesToUser(realm, userId, rolesToAdd, authToken);
            await RemoveRolesFromUser(realm, userId, rolesToRemove, authToken);
        }

        private async Task AddRolesToUser(string realm, string userId, List<KeycloakRole> rolesToAdd, string authToken)
        {
            var kcUrl = $"{authServerUrl}/auth/admin/realms/{realm}/users/{userId}/role-mappings/realm";
            JArray roles = RoleListToRoleJArray(rolesToAdd);
            await HttpHelper.JsonPostRequest(kcUrl, roles, authToken);
        }


        private async Task RemoveRolesFromUser(string realm, string userId, List<KeycloakRole> rolesToAdd, string authToken)
        {
            var kcUrl = $"{authServerUrl}/auth/admin/realms/{realm}/users/{userId}/role-mappings/realm";
            JArray roles = RoleListToRoleJArray(rolesToAdd);
            await HttpHelper.JsonDeleteRequest(kcUrl, roles, authToken);
        }

        private JArray RoleListToRoleJArray(List<KeycloakRole> roles)
        {
            JArray rolesJArray = new JArray();
            foreach (var role in roles)
            {
                JObject roleObj = new JObject();
                roleObj.Add("id", role.Id);
                roleObj.Add("name", role.Name);
                rolesJArray.Add(roleObj);
            }

            return rolesJArray;
        }

        public class KeycloakRole
        {
            public string Name { get; set; }
            public string Id { get; set; }
        }

        private async Task<List<KeycloakRole>> GetRealmRoles(string realm, string authToken)
        {
            var kcUrl = $"{authServerUrl}/auth/admin/realms/{realm}/roles";
            var res = await HttpHelper.JsonGetRequest(kcUrl, authToken);
            var rolesJsonArr = JArray.Parse(res.response);
            return RolesJArrayToRoles(rolesJsonArr);
        }

        private List<KeycloakRole> RolesJArrayToRoles(JArray rolesJsonArr)
        {
            var roles = new List<KeycloakRole>();
            foreach (JObject role in rolesJsonArr)
            {
                roles.Add(new KeycloakRole()
                {
                    Id = role["id"].ToString(),
                    Name = role["name"].ToString()
                });
            }

            return roles;
        }

        private async Task<List<KeycloakRole>> GetUserRoles(string realm, string userId, string authToken)
        {
            var kcUrl = $"{authServerUrl}/auth/admin/realms/{realm}/users/{userId}/role-mappings/realm";
            var res = await HttpHelper.JsonGetRequest(kcUrl, authToken);
            var rolesJsonArr = JArray.Parse(res.response);
            return RolesJArrayToRoles(rolesJsonArr);
        }

        private List<KeycloakRole> GetUserRolesToAdd(string[] rolesToSet, List<KeycloakRole> currentRoles, List<KeycloakRole> realmRoles)
        {
            var rolesToAddString = new List<string>();
            foreach (var roleToSet in rolesToSet)
            {
                var alreadyHasRole = currentRoles.Any(currentRole => currentRole.Name == roleToSet);
                if (!alreadyHasRole)
                {
                    rolesToAddString.Add(roleToSet);
                }
            }

            return stringRolesToKeycloakRoles(rolesToAddString, realmRoles);
        }

        private List<KeycloakRole> stringRolesToKeycloakRoles(List<string> stringRoles, List<KeycloakRole> realmRoles)
        {
            List<KeycloakRole> roles = stringRoles
                .Select(role => realmRoles.Find(realmRole => realmRole.Name == role))
                .Where(role => role != null)
                .ToList();
            return roles;
        }

        private List<KeycloakRole> GetUserRolesToRemove(string[] rolesToSet, List<KeycloakRole> currentRoles)
        {
            var rolesToRemove = new List<KeycloakRole>();
            foreach (var role in currentRoles)
            {
                var shouldKeepRole = rolesToSet.Any(roleToSet => roleToSet == role.Name);
                if (!shouldKeepRole)
                {
                    rolesToRemove.Add(role);
                }
            }

            return rolesToRemove;
        }

        public class KeycloakClientRole
        {
            public KeycloakRealmClient Client;
            public KeycloakRole Role;
        }

        public class KeycloakRealmClient
        {
            public string Id;
            public string ClientId;
        }

        private async Task<List<KeycloakRealmClient>> GetRealmClients(string realm, string authToken)
        {
            var kcUrl = $"{authServerUrl}/auth/admin/realms/{realm}/clients";
            var res = await HttpHelper.JsonGetRequest(kcUrl, authToken);
            var clientsArray = JArray.Parse(res.response);
            var clients = new List<KeycloakRealmClient>();
            foreach (JObject client in clientsArray)
            {
                clients.Add(new KeycloakRealmClient()
                {
                    Id = client["id"].ToString(),
                    ClientId = client["clientId"].ToString()
                });
            }

            return clients;
        }

        private async Task<List<KeycloakRole>> GetClientRoles(string realm, KeycloakRealmClient client, string authToken)
        {
            var kcUrl = $"{authServerUrl}/auth/admin/realms/{realm}/clients/{client.Id}/roles";
            var res = await HttpHelper.JsonGetRequest(kcUrl, authToken);
            var rolesArray = JArray.Parse(res.response);
            return RolesJArrayToRoles(rolesArray);
        }

        public async Task<string> GetAdminAccessToken()
        {
            var kcUrl = $"{authServerUrl}/auth/realms/master/protocol/openid-connect/token";

            var content = new FormUrlEncodedContent(new[] {
                new KeyValuePair<string, string>("grant_type", "password"),
                new KeyValuePair<string, string>("client_id", "admin-cli"),
                new KeyValuePair<string, string>("username", authServerUsername),
                new KeyValuePair<string, string>("password", authServerPassword),
            });

            var client = new HttpClient();

            // Execute post method
            using (var response = await client.PostAsync(kcUrl, content))
            {
                string responseStr = await response.Content.ReadAsStringAsync();

                Console.WriteLine(responseStr);

                var parsedKcData = JObject.Parse(responseStr);

                if (parsedKcData.ContainsKey("access_token"))
                {
                    var token = (string)parsedKcData.SelectToken("access_token");
                    return token;
                }
            }

            throw new ApplicationException("Authentication exception");
        }
    }
}
