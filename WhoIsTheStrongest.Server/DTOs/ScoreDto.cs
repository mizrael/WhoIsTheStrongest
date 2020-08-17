using System;

namespace WhoIsTheStrongest.Server.DTOs
{
    public class ScoreDto
    {
        public ScoreDto(string characterId, int value, DateTime lastIncrement)
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