﻿using Newtonsoft.Json;

namespace Bugsnag.Message.App
{
    public class AppInfo
    {
        [JsonProperty("version")]
        public string Version { get; set; }

        [JsonProperty("releaseStage")]
        public string ReleaseStage { get; set; }
    }
}