﻿using Microsoft.AspNetCore.Authentication;

namespace TwitchManager.Auth
{
    public class TwitchManagerAuthenticationOptions : AuthenticationSchemeOptions
    {
        public const string AuthenticationScheme = "TwitchManagerAuthenticationScheme";

        public const string ConfiguredPolicy = "Configured";
    }
}
