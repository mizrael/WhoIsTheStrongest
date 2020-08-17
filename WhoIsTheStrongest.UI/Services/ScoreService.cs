using Azure.Storage.Queues;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace WhoIsTheStrongest.UI.Services
{
    public class ScoreService : IScoreService
    {
        private readonly HttpClient _httpClient;
        private readonly QueueClient _queueClient;

        public ScoreService(HttpClient httpClient, string queueConnectionString)
        {
            _queueClient = new QueueClient(queueConnectionString, "scores");
            
            _httpClient = httpClient;
        }

        public async Task<IEnumerable<Score>> GetAllAsync()
        {
            var json = await _httpClient.GetStringAsync("/api/scores");
            if (string.IsNullOrWhiteSpace(json)) 
                return Enumerable.Empty<Score>();
            return Newtonsoft.Json.JsonConvert.DeserializeObject<IEnumerable<Score>>(json);
        }

        public async Task Increment(string characterName)
        {
            if (string.IsNullOrWhiteSpace(characterName))            
                throw new ArgumentNullException(nameof(characterName));

            await _queueClient.CreateIfNotExistsAsync();

            var message = new
            {
                CharacterId = characterName
            };
            var jsonMessage = Newtonsoft.Json.JsonConvert.SerializeObject(message);
            var bytes = System.Text.Encoding.UTF8.GetBytes(jsonMessage);
            var base64Message = Convert.ToBase64String(bytes);
            await _queueClient.SendMessageAsync(base64Message);
        }
    }

    public interface IScoreService
    {
        Task Increment(string characterName);
        Task<IEnumerable<Score>> GetAllAsync();
    }

    public class Score
    {
        public Score(string characterId, int value, DateTime lastIncrement)
        {
            CharacterId = characterId;
            Value = value;
            LastIncrement = lastIncrement;
        }

        public string CharacterId { get; }
        public int Value { get; }
        public DateTime LastIncrement { get; }
    }
}