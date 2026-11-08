using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Xml_lab2.Cloud
{
    public class GoogleDriveManager
    {
        private const string CredentialsFileName = "google_credentials.json";

        private static readonly HttpClient _httpClient = new HttpClient();

        private string _clientId = "";
        private string _clientSecret = "";
        private string _accessToken = "";

        public bool IsAuthenticated
        {
            get { return !string.IsNullOrEmpty(_accessToken); }
        }

        private void LoadCredentials()
        {
            if (!string.IsNullOrEmpty(_clientId))
            {
                return;
            }

            string path = Path.Combine(AppContext.BaseDirectory, CredentialsFileName);
            if (!File.Exists(path))
            {
                throw new Exception("Не знайдено файл " + CredentialsFileName + " з ключами Google. Покладіть його в папку проєкту і перезберіть програму.");
            }

            try
            {
                using (JsonDocument doc = JsonDocument.Parse(File.ReadAllText(path)))
                {
                    _clientId = doc.RootElement.GetProperty("ClientId").GetString();
                    _clientSecret = doc.RootElement.GetProperty("ClientSecret").GetString();
                }
            }
            catch (Exception)
            {
                _clientId = "";
                _clientSecret = "";
                throw new Exception("Файл " + CredentialsFileName + " пошкоджений. Він має містити поля ClientId і ClientSecret.");
            }
        }

        public async Task<bool> AuthenticateAsync()
        {
            if (IsAuthenticated)
            {
                return true;
            }

            LoadCredentials();

            int port = new Random().Next(50000, 60000);
            string redirectUri = "http://127.0.0.1:" + port + "/";

            HttpListener listener = new HttpListener();
            listener.Prefixes.Add(redirectUri);
            listener.Start();

            string scope = Uri.EscapeDataString("https://www.googleapis.com/auth/drive");
            string authUrl = "https://accounts.google.com/o/oauth2/v2/auth?client_id=" + _clientId + "&redirect_uri=" + Uri.EscapeDataString(redirectUri) + "&response_type=code&scope=" + scope + "&prompt=select_account";

            ProcessStartInfo startInfo = new ProcessStartInfo(authUrl);
            startInfo.UseShellExecute = true;
            Process.Start(startInfo);

            HttpListenerContext context = await listener.GetContextAsync();
            string code = context.Request.QueryString["code"];

            string html = "<html><head><meta charset='utf-8'></head><body style='font-family:sans-serif;text-align:center;margin-top:50px;'><h2>Авторизація в Google Drive успішна!</h2><p>Тепер ви можете закрити цю вкладку і повернутися у програму.</p></body></html>";
            byte[] buffer = Encoding.UTF8.GetBytes(html);
            context.Response.ContentLength64 = buffer.Length;
            await context.Response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
            context.Response.OutputStream.Close();
            listener.Stop();

            if (string.IsNullOrEmpty(code))
            {
                return false;
            }

            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["code"] = code;
            parameters["client_id"] = _clientId;
            parameters["client_secret"] = _clientSecret;
            parameters["redirect_uri"] = redirectUri;
            parameters["grant_type"] = "authorization_code";

            FormUrlEncodedContent tokenRequest = new FormUrlEncodedContent(parameters);
            HttpResponseMessage response = await _httpClient.PostAsync("https://oauth2.googleapis.com/token", tokenRequest);
            string responseText = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("Помилка авторизації Google: " + responseText);
            }

            using (JsonDocument doc = JsonDocument.Parse(responseText))
            {
                JsonElement tokenElem;
                if (doc.RootElement.TryGetProperty("access_token", out tokenElem))
                {
                    string tokenValue = tokenElem.GetString();
                    if (tokenValue != null)
                    {
                        _accessToken = tokenValue;
                        return true;
                    }
                }
            }

            return false;
        }

        public async Task UploadFileAsync(string fileName, string fileContent, string mimeType)
        {
            string boundary = "XmlLab2Boundary";
            string metadata = "{\"name\": \"" + fileName + "\", \"mimeType\": \"" + mimeType + "\"}";

            StringBuilder bodyBuilder = new StringBuilder();
            bodyBuilder.AppendLine("--" + boundary);
            bodyBuilder.AppendLine("Content-Type: application/json; charset=UTF-8");
            bodyBuilder.AppendLine();
            bodyBuilder.AppendLine(metadata);
            bodyBuilder.AppendLine("--" + boundary);
            bodyBuilder.AppendLine("Content-Type: " + mimeType + "; charset=UTF-8");
            bodyBuilder.AppendLine();
            bodyBuilder.AppendLine(fileContent);
            bodyBuilder.AppendLine("--" + boundary + "--");

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "https://www.googleapis.com/upload/drive/v3/files?uploadType=multipart");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);

            StringContent content = new StringContent(bodyBuilder.ToString(), Encoding.UTF8);
            content.Headers.ContentType = MediaTypeHeaderValue.Parse("multipart/related; boundary=" + boundary);
            request.Content = content;

            HttpResponseMessage response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                string err = await response.Content.ReadAsStringAsync();
                throw new Exception("Помилка вивантаження на Google Drive: " + err);
            }
        }

        public async Task<string> DownloadFileAsync(string fileId)
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "https://www.googleapis.com/drive/v3/files/" + fileId + "?alt=media");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);

            HttpResponseMessage response = await _httpClient.SendAsync(request);
            byte[] bytes = await response.Content.ReadAsByteArrayAsync();
            string content = Encoding.UTF8.GetString(bytes).TrimStart('﻿');

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("Помилка завантаження з Google Drive: " + content);
            }

            return content;
        }

        public async Task<Dictionary<string, string>> GetFilesListAsync(string extension)
        {
            Dictionary<string, string> filesDict = new Dictionary<string, string>();
            string query = Uri.EscapeDataString("name contains '" + extension + "' and trashed=false");
            string url = "https://www.googleapis.com/drive/v3/files?q=" + query + "&fields=files(id,name)&pageSize=100";

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);

            HttpResponseMessage response = await _httpClient.SendAsync(request);
            string responseText = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("Помилка отримання списку файлів Google Drive: " + responseText);
            }

            using (JsonDocument doc = JsonDocument.Parse(responseText))
            {
                JsonElement filesArray;
                if (doc.RootElement.TryGetProperty("files", out filesArray))
                {
                    foreach (JsonElement item in filesArray.EnumerateArray())
                    {
                        string id = item.GetProperty("id").GetString();
                        string name = item.GetProperty("name").GetString();
                        if (id != null && name != null && name.ToLower().EndsWith(extension))
                        {
                            string key = name;
                            if (filesDict.ContainsKey(key))
                            {
                                key = name + " (" + id.Substring(0, 5) + ")";
                            }
                            filesDict[key] = id;
                        }
                    }
                }
            }

            return filesDict;
        }
    }
}
