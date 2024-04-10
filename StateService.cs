
using System.Text.Json;
using Microsoft.Extensions.Logging;

public interface IStateService
{
    State State { get; set; }
    State Load();
    void Save();
}
public class StateService : IStateService
{
    private State _state;
    public State State
    {
        get
        {
            if (_state == null)
                Load();
            return _state;
        }
        set { _state = value; }
    }

    ILogger<StateService> _logger { get; }
    readonly JsonSerializerOptions _indentedJsonSerializerOptions;
    public StateService(ILogger<StateService> logger)
    {
        _indentedJsonSerializerOptions = new JsonSerializerOptions { WriteIndented = true };
        _logger = logger;
    }
    public State Load()
    {
        if (File.Exists("state.json"))
            try
            {
                using var reader = new StreamReader("state.json");
                _state = JsonSerializer.Deserialize<State>(reader.BaseStream);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Corrupted `state.json`. Using fresh state");
                CreateFreshState();
            }
        else
        {
            CreateFreshState();
            Save();
        }
        return _state;
    }

    private void CreateFreshState()
    {
        State = new State
        {
            LevelAndShip = null,
            Session = new Session{
                ServerId = Guid.Empty,
                UserId = Guid.Empty,
                Token = Guid.Empty
            },
            IsServerStarted = false,
            PlayerCount = 0
        };
    }

    public async void Save()
    {
        string fileName = "state.json";
        await using FileStream createStream = File.Create(fileName);
        await JsonSerializer.SerializeAsync(createStream, State, _indentedJsonSerializerOptions);
    }
}
