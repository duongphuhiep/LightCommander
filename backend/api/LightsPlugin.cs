namespace Api;
using System.ComponentModel;
using Microsoft.SemanticKernel;

public class LightsPlugin(ILightRepository lightRepository)
{
    [KernelFunction("create_light")]
    [Description("Creates a new light with the given name")]
    public async Task<LightModel> CreateNewLight(string lightName)
    {
        return await lightRepository.CreateNewLight(lightName);
    }
    
    [KernelFunction("delete_light")]
    [Description("Delete a light, return true if success")]
    public async Task<bool> RemoveLight(string lightId)
    {
        return await lightRepository.DeleteLightById(lightId);
    }
    
    [KernelFunction("get_lights")]
    [Description("Gets the list of all lights and their current state")]
    public async Task<List<LightModel>> GetLights()
    {
        return await lightRepository.GetAllLights();
    }

    [KernelFunction("get_light")]
    [Description("Gets a light information from the specified id")]
    public async Task<LightModel> GetLight(string lightId)
    {
        return await lightRepository.GetLightById(lightId);
    }
    
    [KernelFunction("change_state")]
    [Description("Changes the state of the light")]
    public async Task<LightModel?> ChangeStateAsync(int id, bool isOn)
    {
        return await lightRepository.UpdateOnOffState(id, isOn);
    }
    
    [KernelFunction("change_temperature")]
    [Description("""
                 Changes the temperature of the light, the value of the temperature is between 1000 and 27000.
                 Temperature	Source
                 1,000 K	Most commercial electric heating elements
                 1,700 K	Match flame, low-pressure sodium lamps (LPS/SOX)
                 1,850 K	Candle flame, sunset/sunrise
                 2,400 K	Standard incandescent lamps
                 2,550 K	Soft white incandescent lamps
                 2,700 K	"Soft white" compact fluorescent and LED lamps
                 3,000 K	Warm white compact fluorescent and LED lamps
                 3,200 K	Studio lamps, photofloods, etc.
                 3,350 K	Studio "CP" light
                 5,000 K	Horizon daylight, tubular fluorescent lamps or cool white/daylight compact fluorescent lamps (CFL)
                 5,500–6,000 K	Vertical daylight, electronic flash
                 6,200 K	Xenon short-arc lamp[10]
                 6,500 K	Daylight, overcast, daylight LED lamps
                 6,500–9,500 K	LCD or CRT screens
                 15,000–27,000 K	Clear blue poleward sky
                 """)]
    public async Task<LightModel?> ChangeLightTemperature(int id, long temperature)
    {
        return await lightRepository.UpdateLightTemperature(id, temperature);
    }
}