using Microsoft.Extensions.Caching.Memory;
using TerraformingMarsTrackerApi.Models;

namespace TerraformingMarsTrackerApi
{
    public class GameStore
    {
        private readonly IMemoryCache _memoryCache;
        public GameStore(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
        }

        public (bool, GameState?) StartGame(string gameName, string userName, string userId)
        {
            if (!_memoryCache.TryGetValue(gameName, out GameState gameState))
            {
                var newGame = new GameState(gameName);
                var newBoard = new BoardState()
                {
                    Player = new Player()
                    {
                        PlayerName = userName,
                        PlayerId = userId
                    }
                };
                newGame.Boards.Add(newBoard);
                newGame.Messages.Insert(0, $"{newBoard.Player.PlayerName} created the game " + gameName);
                SaveGameState(newGame);
                return (true, newGame);
            }
            return (false, null);
        }


        private void SaveGameState(GameState gameState)
        {
            var options = new MemoryCacheEntryOptions();
            options.SlidingExpiration = TimeSpan.FromHours(12);
            _memoryCache.Set(gameState.GameCode, gameState, options);
        }

        public GameState UpdateGame(UpdateModel updateModel, string playerId)
        {
            if (_memoryCache.TryGetValue(updateModel.GameCode, out GameState gameState))
            {
                var playerBoard = gameState.Boards.First(x => x.Player.PlayerId == playerId);
                if (updateModel.Resource.Equals("MegaCredits", StringComparison.OrdinalIgnoreCase))
                {
                    UpdateResource(gameState, playerBoard.MegaCredits, updateModel, playerId, "MC");                 
                }
                if (updateModel.Resource.Equals("Steel", StringComparison.OrdinalIgnoreCase))
                {
                    UpdateResource(gameState, playerBoard.Steel, updateModel, playerId, "steel");
                }
                if (updateModel.Resource.Equals("Energy", StringComparison.OrdinalIgnoreCase))
                {
                    UpdateResource(gameState, playerBoard.Energy, updateModel, playerId, "energy");
                }
                if (updateModel.Resource.Equals("Titanium", StringComparison.OrdinalIgnoreCase))
                {
                    UpdateResource(gameState, playerBoard.Titanium, updateModel, playerId, "titanium");
                }
                if (updateModel.Resource.Equals("Heat", StringComparison.OrdinalIgnoreCase))
                {
                    UpdateResource(gameState, playerBoard.Heat, updateModel, playerId, "heat");
                }
                if (updateModel.Resource.Equals("Plants", StringComparison.OrdinalIgnoreCase))
                {
                    UpdateResource(gameState, playerBoard.Plants, updateModel, playerId, "plants");
                }
                if (updateModel.Resource.Equals("TR", StringComparison.OrdinalIgnoreCase))
                {
                    playerBoard.TerraformRating = playerBoard.TerraformRating + updateModel.AdjustmentAmount;
                    if (gameState.Started)
                    {
                        gameState.Messages.Insert(0, $"{playerBoard.Player.PlayerName.Trim()}'s TR changed by {updateModel.AdjustmentAmount}");
                    }
                }
                SaveGameState(gameState);
                return gameState;
            }
            throw new Exception("Could not find Game Board for that code!");
        }

        private void UpdateResource(GameState gameState, Resource resource, UpdateModel updateModel, string playerId, string name)
        {
            string message = "";
            var board = gameState.Boards.First(x => x.Player.PlayerId == playerId);
            if (updateModel.Production)
            {
                resource.Production = resource.Production + updateModel.AdjustmentAmount;
                message = $"{board.Player.PlayerName.Trim()}'s {name} Production changed by {updateModel.AdjustmentAmount}";
            }
            else
            {
                resource.Amount = resource.Amount + updateModel.AdjustmentAmount;
                message = $"{board.Player.PlayerName.Trim()}'s {name} changed by {updateModel.AdjustmentAmount}";
            }
            if (gameState.Started && !string.IsNullOrWhiteSpace(message))
            {
                gameState.Messages.Insert(0, message);
            }
        }

        public GameState TryJoinGame(string gameName, string userName, string userId)
        {
            if (gameName == "TEST_GAME")
            {
                var testGame = HandleTestGame(gameName, userName, userId);
                return testGame;
            }
            if (_memoryCache.TryGetValue(gameName, out GameState gameState))
            {                
                if (!gameState.Boards.Any(x => x.Player.PlayerId == userId))
                {
                    if (gameState.Started)
                    {
                        throw new Exception("Cannot join a game that has already started");
                    }
                    var newPlayer = new Player()
                    {
                        PlayerName = userName,
                        PlayerId = userId,
                    };
                    gameState.Boards.Add(new BoardState
                    {
                        Player = newPlayer
                    });
                    gameState.Messages.Insert(0, $"{userName} has joined the game");
                    SaveGameState(gameState);
                }
                return gameState;
            }
            
            throw new Exception("Could not find game!");
        }

        private GameState HandleTestGame(string gameName, string userName, string userId)
        {
            var newGame = new GameState(gameName);
            var newBoard = new BoardState()
            {
                Titanium = new Resource { Amount = 1, Production = 1 },
                Energy = new Resource() { Production = 2 },
                Plants = new Resource() { Amount = 1, Production = 2 },
                Player = new Player()
                {
                    PlayerName = userName,
                    PlayerId = userId
                }
            };
            var p2 = new BoardState()
            {
                Steel = new Resource { Amount = 3, Production = 2},
                Energy = new Resource() { Production = 1},
                Player = new Player()
                {
                    PlayerName = "Dummy Player 1",
                    PlayerId = "Dummy Player 1"
                }
            };
            var p3 = new BoardState()
            {
                Player = new Player()
                {
                    PlayerName = "Dummy Player 2",
                    PlayerId = "Dummy Player 2"
                }
            };
            newGame.Boards.Add(newBoard);
            newGame.Boards.Add(p2);
            newGame.Boards.Add(p3);
            newGame.Messages.Insert(0, $"{newBoard.Player.PlayerName} created the game " + gameName);
            SaveGameState(newGame);
            return newGame;
        }

        public GameState SetReady(string gameName, string userId)
        {
            if (_memoryCache.TryGetValue(gameName, out GameState gameState))
            {
                var board = gameState.Boards.FirstOrDefault(x => x.Player.PlayerId == userId);
                if (board == null)
                {
                    throw new Exception("Could not find player");
                }
                board.Player.ReadyToStart = true;
                var playerBoard = gameState.Boards.First(x => x.Player.PlayerId == userId);
                gameState.Messages.Insert(0, $"{board.Player.PlayerName} is ready to start. MC {playerBoard.MegaCredits}, Steel: {playerBoard.Steel}, " +
                    $"Titanium: {playerBoard.Titanium}, Plants: {playerBoard.Plants}, Energy: {playerBoard.Energy}, Heat: {playerBoard.Heat}, TR: {playerBoard.TerraformRating}");
                if ((gameState.Boards.Count > 1 && gameState.Boards.All(x => x.Player.ReadyToStart)) || gameState.GameCode == "TEST_GAME")
                {
                    gameState.Started = true;
                    gameState.Messages.Insert(0, "Game started!");
                }
                SaveGameState(gameState);
                return gameState;
            }
            throw new Exception("Could not find game");
        }

        public GameState SetReadyToProduce(string gameName, string userId)
        {
            if (_memoryCache.TryGetValue(gameName, out GameState gameState))
            {
                var callingBoard = gameState.Boards.FirstOrDefault(x => x.Player.PlayerId == userId);
                if (callingBoard == null)
                {
                    throw new Exception("Could not find player");
                }
                callingBoard.Player.ReadyToProduce = true;
                gameState.Messages.Insert(0, $"{callingBoard.Player.PlayerName} is ready to produce");
                if (gameState.Boards.All(x => x.Player.ReadyToProduce) || gameName == "TEST_GAME")
                {
                    gameState.Produce();
                    gameState.Messages.Insert(0, $"Production Done, turn {gameState.Turn}");
                }
                SaveGameState(gameState);
                return gameState;
            }
            throw new Exception("Could not find game");
        }
    }
}
