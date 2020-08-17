using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using WhoIsTheStrongest.Server.DTOs;
using WhoIsTheStrongest.Server.Functions.Commands;

namespace WhoIsTheStrongest.Server.Functions
{
    public static class ScoreTrigger
    {
        [FunctionName("Increment")]
        public static async Task Increment([QueueTrigger("scores", Connection = "AzureWebJobsStorage")]string message,
            [DurableClient] IDurableEntityClient client, 
            ILogger log)
        {
            log.LogInformation($"processing incoming message: {message}");
            var command = JsonConvert.DeserializeObject<Increment>(message);

            var entityId = new EntityId(nameof(CharacterScore), command.CharacterId);
            await client.SignalEntityAsync<ICharacterScore>(entityId, e => e.Increment());
        }

        [FunctionName("GetScores")]
        public static async Task<IActionResult> GetCounters(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "scores")] HttpRequest req,
            [DurableClient] IDurableEntityClient client)
        {
            var counters = new List<ScoreDto>();

            using CancellationTokenSource source = new CancellationTokenSource();
            var token = source.Token;

            var query = new EntityQuery
            {
                PageSize = 100,
                FetchState = true,
                EntityName = nameof(CharacterScore)
            };

            do
            {
                var result = await client.ListEntitiesAsync(query, token);
                if (null == result?.Entities) 
                    break;
                
                foreach (var e in result.Entities)
                {
                    if (null == e.State)
                        continue;

                    try
                    {
                        var c = e.State.ToObject<CharacterScore>();
                        counters.Add(new ScoreDto(e.EntityId.EntityKey, c.Score, c.LastIncrement));
                    }
                    catch 
                    { 
                        // logging
                    }
                }

                query.ContinuationToken = result.ContinuationToken;
            }
            while (query.ContinuationToken != null);

            return new OkObjectResult(counters.OrderByDescending(c => c.Value).ThenByDescending(c=> c.LastIncrement));
        }
    }
}
