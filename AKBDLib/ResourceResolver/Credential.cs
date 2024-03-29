﻿using Newtonsoft.Json;

namespace AKBDLib.ResourceResolver
{
    public class Credential
    {
        [JsonProperty(Required = Required.Always)]
        public string Name { get; set; }

        [JsonProperty(Required = Required.Always)]
        public string Username { get; set; }

        [JsonProperty(Required = Required.Always)]
        public string Password { get; set; }
    }
}