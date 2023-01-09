using Microsoft.Extensions.Caching.Memory;
using TerraformingMarsTrackerApi.Models;

namespace TerraformingMarsTrackerApi
{
    public class GameStore
    {
        private CosmosDbClient _cosmosClinet;
        public GameStore(CosmosDbClient cosmosClient)
        {
            _cosmosClinet = cosmosClient;
        }



        public async Task<(bool, GameState?)> StartGame(string gameName, string userName, string userId)
        {
            var (success, gameState) = await _cosmosClinet.Get(gameName);
            if (!success)
            {
                gameState = new GameState(gameName);
                var newBoard = new BoardState()
                {
                    Player = new Player()
                    {
                        PlayerName = userName,
                        PlayerId = userId
                    }
                };
                gameState.Boards.Add(newBoard);
                gameState.Messages.Insert(0, $"{newBoard.Player.PlayerName} created the game " + gameName);
                await _cosmosClinet.Create(gameState);
                return (true, gameState);
            }
            return (false, null);
        }


        //private void SaveGameState(GameState gameState)
        //{
        //    var options = new MemoryCacheEntryOptions();
        //    options.SlidingExpiration = TimeSpan.FromHours(12);
        //    _memoryCache.Set(gameState.GameCode, gameState, options);
        //}

        public async Task<GameState> UpdateGame(UpdateModel updateModel, string playerId)
        {
            var (success, gameState) = await _cosmosClinet.Get(updateModel.GameCode);
            if (success && gameState != null)
            {
                await UpdateGameState(updateModel, playerId, gameState);
                return gameState;
            }
            throw new Exception("Could not find Game Board for that code!");
        }

        public async Task<GameState> UpdateGameById(UpdateModelNew updateModel, string playerId)
        {
            var (success, gameState) = await _cosmosClinet.GetById(updateModel.GameId);
            if (success && gameState != null)
            {
                await UpdateGameState(updateModel, playerId, gameState);
                return gameState;
            }
            throw new Exception("Could not find Game Board for that code!");
        }

        private async Task<GameState> UpdateGameState(IUpdateModel updateModel, string playerId, GameState gameState)
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
            await _cosmosClinet.Update(gameState);
            return gameState;
        }

        internal async Task<GameState> LeaveGame(string gameId, string userId)
        {
            var (success, gameState) = await _cosmosClinet.GetById(gameId);
            if (success && gameState != null)
            {
                var boardToRemove = gameState.Boards.FirstOrDefault(x => x.Player.PlayerId == userId);
                if (boardToRemove != null)
                {
                    gameState.Boards.Remove(boardToRemove);
                }
                await _cosmosClinet.Update(gameState);
                return gameState;
            }
            throw new Exception("Could not find that game state");
        }

        private void UpdateResource(GameState gameState, Resource resource, IUpdateModel updateModel, string playerId, string name)
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

        public async Task<GameState> TryJoinGame(string gameName, string userName, string userId)
        {
            if (gameName == "TEST_GAME")
            {
                var testGame = await HandleTestGame(gameName, userName, userId);
                return testGame;
            }
            var (success, gameState) = await _cosmosClinet.Get(gameName);
            if (success && gameState != null)
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
                    await _cosmosClinet.Update(gameState);
                }
                return gameState;
            }
            
            throw new Exception("Could not find game!");
        }

        private async Task<GameState> HandleTestGame(string gameName, string userName, string userId)
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
            await _cosmosClinet.Delete("TEST_GAME");
            await _cosmosClinet.Create(newGame);
            return newGame;
        }

        public async Task<GameState> SetReady(string gameName, string userId)
        {
            async Task<GameState> SetGameStateReady(string userId, GameState gameState)
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
                await _cosmosClinet.Update(gameState);
                return gameState;
            }


            if (Guid.TryParse(gameName, out Guid gameId))
            {
                var (success, gameState) = await _cosmosClinet.GetById(gameId.ToString());
                if (success && gameState != null)
                {
                    return await SetGameStateReady(userId, gameState);
                }
            }
            else
            {
                var (success, gameState) = await _cosmosClinet.Get(gameName);
                if (success && gameState != null)
                {
                    return await SetGameStateReady(userId, gameState);
                }
            }
            
            throw new Exception("Could not find game");
        }

        public async Task<GameState> SetReadyToProduce(string gameName, string userId)
        {
            async Task<GameState> SetGameStateReadyToProduce(string userId, GameState gameState)
            {
                var callingBoard = gameState.Boards.FirstOrDefault(x => x.Player.PlayerId == userId);
                if (callingBoard == null)
                {
                    throw new Exception("Could not find player");
                }
                callingBoard.Player.ReadyToProduce = true;
                gameState.Messages.Insert(0, $"{callingBoard.Player.PlayerName} is ready to produce");
                if (gameState.Boards.All(x => x.Player.ReadyToProduce) || gameState.GameCode == "TEST_GAME")
                {
                    gameState.Produce();
                    gameState.Messages.Insert(0, $"Production Done, turn {gameState.Turn}");
                }
                await _cosmosClinet.Update(gameState);
                return gameState;
            }


            if (Guid.TryParse(gameName, out var gameId))
            {
                var (success, gameState) = await _cosmosClinet.GetById(gameId.ToString());
                if (success && gameState != null)
                {
                    return await SetGameStateReadyToProduce(userId, gameState);
                }
            }
            else
            {
                var (success, gameState) = await _cosmosClinet.Get(gameName);
                if (success && gameState != null)
                {
                    return await SetGameStateReadyToProduce(userId, gameState);
                }
            }            
            throw new Exception("Could not find game");
        }

        
    }
}
