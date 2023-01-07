﻿namespace TerraformingMarsTrackerApi.Models
{
    public class GameState
    {
        public GameState(string gameCode)
        {
            GameCode = gameCode;
        }
        public GameState()
        {
        }

        public Guid Id { get; set; } = Guid.NewGuid();
        public bool Started { get; set; }
        public string GameCode { get; set; } = "";
        public List<BoardState> Boards { get; set; } = new List<BoardState>();
        public List<string> Messages { get; set; } = new List<string>();
        public int Turn { get; set; }


        public void Produce()
        {
            foreach(var board in Boards)
            {
                board.Produce();
            }
            Turn++;
        }
    }
}
