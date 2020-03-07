using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Selenium
{
    public class Config
    {
        /// <summary>
        /// The bot token.
        /// </summary>
        [JsonProperty("token")] public string Token;

        /// <summary>
        /// The snapshot version.
        /// </summary>
        [JsonProperty("snapshot")] public string Snapshot;
    }
}
