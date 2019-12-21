using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace DiscordCloudBot
{
    public class TranslateService
    {
        public async Task<string> TranslateToRussianAsync(string text)
        {
            var iamToken = Environment.GetEnvironmentVariable("YANDEX_IAM_TOKEN");
            var folderId = Environment.GetEnvironmentVariable("YANDEX_FOLDER_ID");
            if (string.IsNullOrEmpty(iamToken))
                throw new Exception("Environment variable YANDEX_IAM_TOKEN is not set.");
            if (string.IsNullOrEmpty(folderId))
                throw new Exception("Environment variable YANDEX_FOLDER_ID is not set.");
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + iamToken);
            var values = new Dictionary<string, string>
            {
                {"texts", $"{text}"},
                {"targetLanguageCode", "ru" },
                {"folderId", folderId},
            };
            var content = new FormUrlEncodedContent(values);
            var response =
                await client.PostAsync(@"https://translate.api.cloud.yandex.net/translate/v2/translate", content);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }
    }
}