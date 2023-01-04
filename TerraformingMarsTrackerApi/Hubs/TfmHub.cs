using Microsoft.AspNetCore.SignalR;
using TerraformingMarsTrackerApi.Models;

namespace TerraformingMarsTrackerApi.Hubs
{
    public class TfmHub : Hub
    {
        private GameStore _gameStore;
        public TfmHub(GameStore gameStore)
        {
            _gameStore = gameStore;
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
            var (success, gameState) = _gameStore.StartGame(groupName.Trim(), userName.Trim(), userId);
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

        public async Task UpdateGame(UpdateModel updateModel, string userId)
        {
            var newBoard = _gameStore.UpdateGame(updateModel, userId);
            await Clients.Group(updateModel.GameCode).SendAsync("GameUpdate", newBoard);
        }

        public async Task JoinGame(string gameCode, string userName, string userId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, gameCode);
            try
            {
                var newBoard = _gameStore.TryJoinGame(gameCode.Trim(), userName.Trim(), userId);
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
                var gamestate = _gameStore.SetReady(gameCode, userId);
                await Clients.Group(gameCode).SendAsync("GameUpdate", gamestate);
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
                var gamestate = _gameStore.SetReadyToProduce(gameCode, userId);
                await Clients.Group(gameCode).SendAsync("GameUpdate", gamestate);
            }
            catch (Exception ex)
            {
                await Clients.Caller.SendAsync("ErrorMessage", ex.Message);
            }
        }
    }
}


