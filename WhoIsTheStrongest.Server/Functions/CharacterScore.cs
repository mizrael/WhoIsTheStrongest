using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Newtonsoft.Json;

namespace WhoIsTheStrongest.Server.Functions
{
    public interface ICharacterScore
    {
        void Increment();
    }

    [JsonObject(MemberSerialization.OptIn)]
    public class CharacterScore : ICharacterScore
    {
        [JsonProperty("score")]
        public int Score { get; private set; }

        [JsonProperty("lastIncrement ")]
        public DateTime LastIncrement { get; private set; }

        public void Increment()
        {
            this.Score++;
            this.LastIncrement = DateTime.UtcNow;
        }

        [FunctionName(nameof(CharacterScore))]
        public static Task Run([EntityTrigger] IDurableEntityContext ctx) => ctx.DispatchAsync<CharacterScore>();
    }
}