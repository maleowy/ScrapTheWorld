using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;

namespace Worker.RemoteDebugging
{
    public class MyClientWebSocket : IDisposable
    {
        public static dynamic Template = new ExpandoObject();
        public int ChunkSize = 8192;

        private ClientWebSocket ws = new ClientWebSocket();

        public MyClientWebSocket(string url)
        {
            Template.method = "Runtime.evaluate";

            dynamic par = new ExpandoObject();
            par.expression = "";
            par.objectGroup = "console";
            par.includeCommandLineAPI = true;
            par.doNotPauseOnExceptions = false;
            par.returnByValue = false;

            Template.@params = par;

            Template.id = 1;

            ConnectAsync(new Uri(url), CancellationToken.None).Wait();
        }

        public static List<string> GetUrls()
        {
            RestClient client = new RestClient("http://localhost:9222");
            RestRequest req = new RestRequest("/json/list");
            var resp = client.Execute(req);

            var jarray = (JArray)JsonConvert.DeserializeObject(resp.Content);

            var urls = jarray.ToList().Where(x => x["type"].ToString() == "page" && x["webSocketDebuggerUrl"] != null)
                .Select(x => x["webSocketDebuggerUrl"].ToString()).ToList();

            return urls;
        }

        private async Task ConnectAsync(Uri uri, CancellationToken token)
        {
            await ws.ConnectAsync(uri, token);
        }

        public WebSocketState GetState()
        {
            return ws.State;
        }

        public async Task DisconnectAsync(CancellationToken token)
        {
            await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, null, token);
        }

        public void Open(string url, CancellationToken token)
        {
            var clone = DeepCopy(Template);
            clone.@params.expression = $"document.location='{url}'";
            var json = JsonConvert.SerializeObject(clone);

            SendString(json, token);
        }

        public async Task OpenAsync(string url, CancellationToken token)
        {
            var clone = DeepCopy(Template);
            clone.@params.expression = $"document.location='{url}'";
            var json = JsonConvert.SerializeObject(clone);

            await SendStringAsync(json, token);
        }

        public async Task OpenNewWindowAsync(string url, CancellationToken token)
        {
            var clone = DeepCopy(Template);
            clone.@params.expression = $"window.open('{url}', '_blank');";
            var json = JsonConvert.SerializeObject(clone);

            await SendStringAsync(json, token);
        }

        public async Task FillAsync(string selector, string value, CancellationToken token)
        {
            var clone = DeepCopy(Template);
            clone.@params.expression = $"document.querySelector('{selector}').value = '{value}';";
            var json = JsonConvert.SerializeObject(clone);

            await SendStringAsync(json, token);
        }

        public async Task ClickAsync(string selector, CancellationToken token)
        {
            var clone = DeepCopy(Template);
            clone.@params.expression = $"document.querySelector('{selector}').click();";
            var json = JsonConvert.SerializeObject(clone);

            await SendStringAsync(json, token);
        }

        public async Task EvaluateAsync(string js, CancellationToken token)
        {
            var json = BuildRequest(js, false);

            await SendStringAsync(json, token);
        }

        public string EvaluateWithReturn(string js, CancellationToken token)
        {
            var json = BuildRequest(js, true);

            SendString(json, token);
            return ReadString();
        }

        public string ResultToValue(string returnString)
        {
            dynamic obj = JsonConvert.DeserializeObject(returnString);
            object value = obj.result.result.value;

            var serialized = JsonConvert.SerializeObject(value);
            serialized = serialized.Replace("\\\"", "\"").Trim('"');

            return serialized;
        }

        public void WaitForDocumentReady()
        {
            do
            {
                Thread.Sleep(1000);
            }
            while (!CheckReadyState());
        }

        private bool CheckReadyState()
        {
            var json = BuildRequest("document.readyState;", true);

            SendString(json, CancellationToken.None);
            var state = ReadString();

            if (state == "complete")
                return true;

            return false;
        }

        public async Task<string> EvaluateWithReturnAsync(string js, CancellationToken token)
        {
            var json = BuildRequest(js, true);

            await SendStringAsync(json, token);
            return await ReadStringAsync();
        }

        private void SendString(string data, CancellationToken token)
        {
            //var encoded = Encoding.UTF8.GetBytes(data);
            //var buffer = new ArraySegment<byte>(encoded, 0, encoded.Length);
            //ws.SendAsync(buffer, WebSocketMessageType.Text, true, token);

            SendStringChunkedAsync(data, token).Wait();
        }

        private Task SendStringAsync(string data, CancellationToken token)
        {
            return SendStringChunkedAsync(data, token);
        }

        private async Task SendStringChunkedAsync(string data, CancellationToken token)
        {
            var messageBuffer = Encoding.UTF8.GetBytes(data);
            var messagesCount = (int)Math.Ceiling((double)messageBuffer.Length / ChunkSize);

            for (var i = 0; i < messagesCount; i++)
            {
                var offset = ChunkSize * i;
                var count = ChunkSize;
                var lastMessage = i + 1 == messagesCount;

                if (count * (i + 1) > messageBuffer.Length)
                {
                    count = messageBuffer.Length - offset;
                }

                await ws.SendAsync(new ArraySegment<byte>(messageBuffer, offset, count), WebSocketMessageType.Text, lastMessage, token);
            }
        }

        private string ReadString()
        {
            return ReadStringAsync().Result;
        }

        private async Task<string> ReadStringAsync()
        {
            var buffer = new ArraySegment<byte>(new byte[ChunkSize]);

            using (var ms = new MemoryStream())
            {
                WebSocketReceiveResult result;

                do
                {
                    result = await ws.ReceiveAsync(buffer, CancellationToken.None);
                    ms.Write(buffer.Array, buffer.Offset, result.Count);
                }
                while (!result.EndOfMessage);

                ms.Seek(0, SeekOrigin.Begin);

                using (var reader = new StreamReader(ms, Encoding.UTF8))
                    return ResultToValue(reader.ReadToEnd());
            }
        }

        private ExpandoObject DeepCopy(ExpandoObject original)
        {
            var clone = new ExpandoObject();

            foreach (var kvp in original)
            {
                ((IDictionary<string, object>) clone).Add(kvp.Key, kvp.Value is ExpandoObject 
                    ? DeepCopy((ExpandoObject) kvp.Value) : kvp.Value);
            }

            return clone;
        }

        private string BuildRequest(string js, bool returnByValue)
        {
            var clone = DeepCopy(Template);
            clone.@params.expression = js;
            clone.@params.returnByValue = returnByValue;

            return JsonConvert.SerializeObject(clone);
        }

        public void Dispose()
        {
            ws.Dispose();
        }
    }
}