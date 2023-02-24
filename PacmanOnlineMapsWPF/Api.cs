using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace PacmanOnlineMapsWPF
{
    class Api
    {
        public static string HOST = "http://[2a00:6d42:1242:1c00:0000:0000:0000:0015]:1980/";
        public static Api INSTANCE { get; } = new();
        private HttpClient httpClient = new();
        public Constants Constants { get; private set; }


        private Api()
        {
            this.Constants = this.GetConstants();
        }


        private void HandleResponse<T>(Response<T> response)
        {

        }


        public SearchedWorld[] GetRandomWorlds()
        {
            var raw_reply = this.httpClient.PostAsync(Api.HOST + "get_random_worlds", new StringContent(string.Empty)).Result;
            var raw_text = raw_reply.Content.ReadAsStringAsync().Result;
            var response = JsonSerializer.Deserialize<Response<SearchedWorld[]>>(raw_text)!;
            this.HandleResponse<SearchedWorld[]>(response);

            return response.result;
        }

        public SearchedWorld[] SearchWorlds(SearchReq searchReq, int offset)
        {
            var raw_msg = JsonSerializer.Serialize(searchReq)!;
            raw_msg = raw_msg.Substring(0, raw_msg.Length - 1) + $", \"offset\": {offset}}}";

            var msg = new StringContent(raw_msg, Encoding.UTF8, "application/json");

            var raw_reply = this.httpClient.PostAsync(Api.HOST + "search_worlds", msg).Result;

            var response = JsonSerializer.Deserialize<Response<SearchedWorld[]>>(raw_reply.Content.ReadAsStringAsync().Result)!;
            this.HandleResponse<SearchedWorld[]>(response);

            return response.result;
        }

        public byte[] GetWorld(string world_id)
        {
            var raw_reply = this.httpClient.GetAsync(Api.HOST + "get_world?world_id=" + world_id).Result;

            var response = JsonSerializer.Deserialize<Response<byte[]>>(raw_reply.Content.ReadAsStringAsync().Result)!;
            this.HandleResponse<byte[]>(response);

            return response.result;
        }

        public Constants GetConstants()
        {
            var raw_reply = this.httpClient.GetAsync(Api.HOST + "get_constants").Result;

            var response = JsonSerializer.Deserialize<Response<Constants>>(raw_reply.Content.ReadAsStringAsync().Result)!;
            this.HandleResponse<Constants>(response);

            this.Constants = response.result;
            return response.result;
        }

        public async Task<Constants> GetConstantsAsync()
        {
            var raw_reply = await this.httpClient.GetAsync(Api.HOST + "get_constants");
            var body = await raw_reply.Content.ReadAsStringAsync();
            var response = JsonSerializer.Deserialize<Response<Constants>>(body)!;
            this.HandleResponse<Constants>(response);

            this.Constants = response.result;
            return response.result;
        }
    }
}
