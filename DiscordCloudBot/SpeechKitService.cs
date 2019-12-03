using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace DiscordCloudBot
{
    public class SpeechKitService
    {
        public async Task<byte[]> SpeakAsync(string text)
        {
            var iamToken = Environment.GetEnvironmentVariable("YANDEX_IAM_TOKEN");
            var folderId = Environment.GetEnvironmentVariable("YANDEX_FOLDER_ID");
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + iamToken);
            var values = new Dictionary<string, string>
            {
                {"text", text},
                {"lang", "en-US"},
                {"folderId", folderId}
            };
            var content = new FormUrlEncodedContent(values);
            var response = await client.PostAsync(@"https://tts.api.cloud.yandex.net/speech/v1/tts:synthesize", content);
            return await response.Content.ReadAsByteArrayAsync();
        }
    }
}