using Microsoft.AspNetCore.SignalR;
using TerraformingMarsTrackerApi.Models;

namespace TerraformingMarsTrackerApi.Hubs
{
    public class TfmHub : Hub
    {
        private GameStore _gameStore;
        private readonly ILogger<TfmHub> _logger;


        public TfmHub(GameStore gameStore, ILogger<TfmHub> logger)
        {
            _gameStore = gameStore;
            _logger = logger;
        }

        public async Task SendMessage(string user, string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", user, message);
        }

        public async Task JoinGroup(string groupName)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        }

        public async Task StartGame(string groupName, string userName, string userId)
        {
            try
            {
                var (success, gameState) = await _gameStore.StartGame(groupName.Trim(), userName.Trim(), userId);
                if (success)
                {
                    await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
                    await Clients.Group(groupName).SendAsync("GameUpdate", gameState);
                }
                else
                {
                    await Clients.Caller.SendAsync("ErrorMessage", "Game Already Exists");
                }
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Error starting game");
            }       
        }

        public async Task UpdateGame(UpdateModel updateModel, string userId)
        {
            GameState newBoard = await _gameStore.UpdateGame(updateModel, userId);
            await Groups.AddToGroupAsync(Context.ConnectionId, updateModel.GameCode);
            await Clients.Group(updateModel.GameCode).SendAsync("GameUpdate", newBoard);
        }

        public async Task UpdateGameById(UpdateModelNew updateModel, string userId)
        {
            GameState newBoard = await _gameStore.UpdateGameById(updateModel, userId);
            await Groups.AddToGroupAsync(Context.ConnectionId, newBoard.GameCode);
            await Clients.Group(newBoard.GameCode).SendAsync("GameUpdate", newBoard);
        }

        public async Task JoinGame(string gameCode, string userName, string userId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, gameCode);
            try
            {
                GameState newBoard = await _gameStore.TryJoinGame(gameCode.Trim(), userName.Trim(), userId);
                await Clients.Group(gameCode).SendAsync("GameUpdate", newBoard);
            }            
            catch (Exception ex)
            {
                await Clients.Caller.SendAsync("ErrorMessage", ex.Message);

            }
        }

        public async Task Ready(string gameCode, string userId)
        {
            try
            {
                GameState gamestate = await _gameStore.SetReady(gameCode, userId);
                await Groups.AddToGroupAsync(Context.ConnectionId, gamestate.GameCode);
                await Clients.Group(gamestate.GameCode).SendAsync("GameUpdate", gamestate);
            }
            catch(Exception ex)
            {
                await Clients.Caller.SendAsync("ErrorMessage", ex.Message);
            }            
        }

        public async Task ReadyToProduce(string gameCode, string userId)
        {
            try
            {
                GameState gamestate = await _gameStore.SetReadyToProduce(gameCode, userId);
                await Groups.AddToGroupAsync(Context.ConnectionId, gamestate.GameCode);
                await Clients.Group(gamestate.GameCode).SendAsync("GameUpdate", gamestate);
            }
            catch (Exception ex)
            {
                await Clients.Caller.SendAsync("ErrorMessage", ex.Message);
            }
        }

        public async Task LeaveGame(string gameId, string userId)
        {
            try
            {
                var gameState = await _gameStore.LeaveGame(gameId, userId);
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, gameState.GameCode);
                await Clients.Group(gameState.GameCode).SendAsync("GameUpdate", gameState);
            }
            catch (Exception ex)
            {
                await Clients.Caller.SendAsync("ErrorMessage", ex.Message);
            }
        }
    }
}


