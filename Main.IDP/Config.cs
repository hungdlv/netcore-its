using IdentityServer4.Models;
using IdentityServer4.Test;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Main.IDP
{
    public static class Config
    {
        public static List<TestUser> GetUsers() {
            return new List<TestUser>
            {
                new TestUser
                {
                    SubjectId = "d860efca-22d9-47fd-8249-791ba61b07c7",
                    Username = "coc",
                    Password = "password",

                    Claims = new List<Claim>
                    {
                        new Claim("given_name", "Frank"),
                        new Claim("family_name", "Underwood"),
                        new Claim("address", "11 Huge street"),
                        new Claim("role", "FreeUser")
                    }
                },
                new TestUser
                {
                    SubjectId = "b7539694-97e7-4dfe-84da-b4256e1ff5c7",
                    Username = "xoai",
                    Password = "password",

                    Claims = new List<Claim>
                    {
                        new Claim("given_name", "Claire"),
                        new Claim("family_name", "Underwood"),
                        new Claim("address", "12 Big street"),
                        new Claim("role", "PayingUser")
                    }
                }
            };

        }

        public static IEnumerable<IdentityResource> GetIdentityResources()
        {
            return new List<IdentityResource>
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile(),
                new IdentityResources.Address(),
                new IdentityResource("roles", "Your role(s)", new List<string>() { "role" })
            };
        }

        public static IEnumerable<Client> GetClients()
        {
            return new List<Client>() {
                new Client{
                     ClientName = "Image Gallary",
                     ClientId = "imagegallaryclient",
                     AllowedGrantTypes = new string[] { GrantType.Hybrid },
                     RedirectUris = new List<string>{
                         "https://localhost:44398/signin-oidc"
                     },
                     AllowedScopes = new List<string>{
                         IdentityServer4.IdentityServerConstants.StandardScopes.OpenId,
                         IdentityServer4.IdentityServerConstants.StandardScopes.Profile,
                         IdentityServer4.IdentityServerConstants.StandardScopes.Address,
                         "roles"
                     },
                     ClientSecrets = {
                        new Secret("secret".Sha256())
                     },
                     //AlwaysIncludeUserClaimsInIdToken = true
                     PostLogoutRedirectUris = new List<string>{
                         "https://localhost:44398/signout-callback-oidc"
                     }
                }
            };
        }


    }
}
