﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace DiscordCloudBot
{
    public class SpeechKitService
    {
        public async Task<Stream> SpeakAsync(string text)
        {
            var iamToken = Environment.GetEnvironmentVariable("YANDEX_IAM_TOKEN");
            var folderId = Environment.GetEnvironmentVariable("YANDEX_FOLDER_ID");
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + iamToken);
            var values = new Dictionary<string, string>
            {
                {"text", text},
                {"folderId", folderId},
                {"format", "oggopus"}
            };
            var content = new FormUrlEncodedContent(values);
            var response =
                await client.PostAsync(@"https://tts.api.cloud.yandex.net/speech/v1/tts:synthesize", content);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStreamAsync();
        }
    }
}