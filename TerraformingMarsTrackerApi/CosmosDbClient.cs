using Microsoft.Azure.Cosmos;
using Azure.Identity;
using Microsoft.Extensions.Options;
using TerraformingMarsTrackerApi.Models;
using Microsoft.Extensions.Caching.Memory;

namespace TerraformingMarsTrackerApi
{
    public class CosmosDbClient
    {
        private readonly CosmosConfig _cosmosConfig;
        private readonly CosmosClientOptions _options;
        private readonly ILogger<CosmosDbClient> _logger;
        //private readonly IMemoryCache _memoryCache;
        private Dictionary<string, SemaphoreSlim> _semaphores;
        private readonly object _lock = new object();
        private readonly Container _container;

        public CosmosDbClient(IOptions<CosmosConfig> cosmosConfig, ILogger<CosmosDbClient> logger)
        {
            _cosmosConfig = cosmosConfig.Value;
            _options = new CosmosClientOptions()
            {
                SerializerOptions = new CosmosSerializationOptions()
                {
                    PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase
                }
            };
            _logger = logger;
            //_memoryCache = memoryCache;
            _semaphores = new Dictionary<string, SemaphoreSlim>();
            CosmosClient client = new(accountEndpoint: _cosmosConfig.Endpoint, _cosmosConfig.Key, _options);
            Database database = client.GetDatabase("fantasy-calendar");
            _container = database.GetContainer("tm-tracker");
        }

        private SemaphoreSlim GetSemaphore(string code)
        {
            if (_semaphores.ContainsKey(code))
            {
                return _semaphores[code];
            }
            lock (_lock)
            {
                if (_semaphores.ContainsKey(code))
                {
                    return _semaphores[code];
                }
                _semaphores.Add(code, new SemaphoreSlim(1));
                return _semaphores[code];
            }
        }

        public async Task<(bool, GameState?)> Get(string gameCode)
        {
            try
            {
                //if (_memoryCache.TryGetValue(gameCode, out GameState gameState))
                //{
                //    return (true, gameState);
                //}

                //var response = await container.ReadItemAsync<GameState>(gameCode, new PartitionKey(gameCode));
                var query = new QueryDefinition(
                    query: "SELECT * FROM games g WHERE g.gameCode = @gameCode"
                ).WithParameter("@gameCode", gameCode);

                using (var feed = _container.GetItemQueryIterator<GameState>(query))
                {
                    FeedResponse<GameState> response = await feed.ReadNextAsync();
                    foreach (GameState item in response)
                    {
                        return (true, item);
                    }
                }
                return (false, null);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Error Getting game");
                throw;
            }
        }

        public async Task Create(GameState gameState)
        {
            var semaphore = GetSemaphore(gameState.GameCode);
            try
            {
                await semaphore.WaitAsync();
                //SaveToCache(gameState);
                await _container.CreateItemAsync<GameState>(gameState, new PartitionKey(gameState.Id.ToString()));
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Error Setting game");
                throw;
            }
            finally
            {
                semaphore.Release();
            }
        }

        public async Task Update(GameState gameState)
        {
            var semaphore = GetSemaphore(gameState.GameCode);
            try
            {
                await semaphore.WaitAsync();
                //SaveToCache(gameState);
                await _container.ReplaceItemAsync<GameState>(gameState, gameState.Id.ToString(), new PartitionKey(gameState.Id.ToString()));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error Setting game");
                throw;
            }
            finally
            {
                semaphore.Release();
            }
        }

        public async Task Delete(string gameCode)
        {
            var semaphore = GetSemaphore(gameCode);
            try
            {
                await semaphore.WaitAsync();
                var (succes, existingState) = await Get(gameCode);
                if (succes && existingState != null)
                {
                    var id = existingState.Id.ToString();
                    await _container.DeleteItemAsync<GameState>(id, new PartitionKey(id));
                }
                //_memoryCache.Remove(gameCode);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error Deleting game");
                throw;
            }
            finally
            {
                semaphore.Release();
            }
        }

        //private void SaveToCache(GameState gameState)
        //{
        //    var options = new MemoryCacheEntryOptions();
        //    options.SlidingExpiration = TimeSpan.FromMinutes(30);
        //    _memoryCache.Set(gameState.GameCode, gameState, options);
        //}

    }


}
