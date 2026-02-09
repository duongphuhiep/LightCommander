using System.Text.Json;
using StackExchange.Redis;

namespace Api;

public interface ILightRepository
{
    Task<LightModel> CreateNewLight(string lightName);
    Task<List<LightModel>> GetAllLights();
    Task<LightModel> GetLightById(string lightId);
    Task<LightModel> UpdateOnOffState(int id, bool isOn);
    Task<LightModel> UpdateLightTemperature(int id, long? temperature);
    Task<bool> DeleteLightById(string id);
}

class RedisLightRepository(IConnectionMultiplexer redisConnection) : ILightRepository
{
    private readonly IDatabase _db = redisConnection.GetDatabase();
    private const string IdCounterKey = "light:id:counter";
    private const string AllLightsSetKey = "lights:all";

    public async Task<LightModel> CreateNewLight(string lightName)
    {
        // Generate new ID
        var newId = (int)await _db.StringIncrementAsync(IdCounterKey);

        var light = new LightModel
        {
            Id = newId,
            Name = lightName,
            IsOn = false,
            LightTemperature = 0
        };

        var lightKey = $"light:{newId}";
        var json = JsonSerializer.Serialize(light);

        // Store JSON document
        await _db.ExecuteAsync("JSON.SET", lightKey, "$", json);
        
        // Add to index set
        await _db.SetAddAsync(AllLightsSetKey, newId);

        return light;
    }

    public async Task<List<LightModel>> GetAllLights()
    {
        var lightIds = await _db.SetMembersAsync(AllLightsSetKey);
        
        if (lightIds.Length == 0)
            return new List<LightModel>();

        // Use pipeline for efficiency
        var batch = _db.CreateBatch();
        var tasks = lightIds.Select(id => 
            batch.ExecuteAsync("JSON.GET", $"light:{id}", "$")
        ).ToArray();

        batch.Execute();
        await Task.WhenAll(tasks);

        var lights = new List<LightModel>();
        foreach (var task in tasks)
        {
            var result = await task;
            if (!result.IsNull)
            {
                var jsonArray = JsonSerializer.Deserialize<List<LightModel>>(result.ToString());
                if (jsonArray?.Count > 0)
                    lights.Add(jsonArray[0]);
            }
        }

        return lights;
    }

    public async Task<LightModel> GetLightById(string lightId)
    {
        var lightKey = $"light:{lightId}";
        var result = await _db.ExecuteAsync("JSON.GET", lightKey, "$");

        if (result.IsNull)
            return null;

        var jsonArray = JsonSerializer.Deserialize<List<LightModel>>(result.ToString());
        return jsonArray?.FirstOrDefault();
    }

    public async Task<LightModel> UpdateOnOffState(int id, bool isOn)
    {
        var lightKey = $"light:{id}";
        
        // Update specific field using JSONPath
        await _db.ExecuteAsync("JSON.SET", lightKey, "$.is_on", isOn.ToString().ToLower());

        return await GetLightById(id.ToString());
    }

    public async Task<LightModel> UpdateLightTemperature(int id, long? temperature)
    {
        var lightKey = $"light:{id}";
        var tempValue = temperature ?? 0;

        await _db.ExecuteAsync("JSON.SET", lightKey, "$.temperature", tempValue);

        return await GetLightById(id.ToString());
    }

    public async Task<bool> DeleteLightById(string id)
    {
        var lightKey = $"light:{id}";
        
        // Remove from index first
        await _db.SetRemoveAsync(AllLightsSetKey, id);
        
        // Delete JSON document
        var result = await _db.KeyDeleteAsync(lightKey);
        
        return result;
    }
}