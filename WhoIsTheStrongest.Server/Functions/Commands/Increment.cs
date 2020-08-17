using Newtonsoft.Json;

namespace WhoIsTheStrongest.Server.Functions.Commands
{
    public class Increment
    {
        [JsonConstructor]
        public Increment(string characterId)
        {
            CharacterId = characterId;
        }

        public string CharacterId { get; }
    }
}