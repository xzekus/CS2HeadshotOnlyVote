using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Utils;

namespace HeadshotOnlyVote
{
    public class HeadshotOnlyVote : BasePlugin, IPluginConfig<Config>
    {
        public override string ModuleName => "HeadshotOnlyVote";
        public override string ModuleVersion => "1.0";
        public override string ModuleAuthor => "Zekus";

        public Config Config { get; set; }

        private bool _isVoteActive = false;
        private bool _isHeadshotOnly = false;
        private int _currentRound = 0;
        private int _voteYesCount = 0;
        private int _voteNoCount = 0;

        private const string ConfigFilePath = "headshot_config.json";

        public void OnConfigParsed(Config config)
        {
            Config = config;
        }

        public override void Load(bool hotReload)
        {
            LoadConfig();

            AddCommand("start_hs_vote", "Start headshot-only mode vote", cmd_StartVote);
            AddCommand("force_start_hs", "Force start headshot-only mode", cmd_ForceStartHeadshot);
            AddCommand("force_stop_hs", "Force stop headshot-only mode", cmd_ForceStopHeadshot);

            RegisterEventHandler<EventRoundStart>((@event, info) =>
            {
                _currentRound++;
                if (_currentRound % Config.HeadshotVoteRounds == 0)
                {
                    StartVote();
                }
                return HookResult.Continue;
            });

            RegisterEventHandler<EventPlayerHurt>((@event, info) =>
            {
                if (_isHeadshotOnly)
                {
                    CCSPlayerController attacker = @event.Attacker;
                    CCSPlayerController victim = @event.Userid;

                    if (attacker.IsValid && victim.IsValid && attacker.TeamNum != victim.TeamNum)
                    {
                        // Check if the hitgroup is the head (hitgroup 1)
                        if (@event.Hitgroup == 1)
                        {
                            // Apply damage only if the hitgroup is the head
                            victim.PlayerPawn.Value.Health -= @event.DmgHealth;
                            victim.PlayerPawn.Value.ArmorValue -= @event.DmgArmor;
                        }
                        else
                        {
                            // If it's not a headshot, prevent any damage
                            @event.DmgHealth = 0;
                            @event.DmgArmor = 0;
                        }
                    }
                }
                return HookResult.Continue;
            });
        }

        private void LoadConfig()
        {
            if (File.Exists(ConfigFilePath))
            {
                string json = File.ReadAllText(ConfigFilePath);
                Config = JsonSerializer.Deserialize<Config>(json);
            }
            else
            {
                Config = new Config();
                SaveConfig();
            }
        }

        private void SaveConfig()
        {
            string json = JsonSerializer.Serialize(Config, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(ConfigFilePath, json);
        }

        private void StartVote()
        {
            if (_isVoteActive)
                return;

            _isVoteActive = true;
            _voteYesCount = 0;
            _voteNoCount = 0;

            Server.PrintToChatAll("Vote for headshot-only mode has started! Type !yes or !no to vote.");

            Task.Delay(Config.HeadshotVoteDurationSeconds * 1000).ContinueWith((task) =>
            {
                _isVoteActive = false;
                if (_voteYesCount > _voteNoCount)
                {
                    _isHeadshotOnly = true;
                    Server.PrintToChatAll("Headshot-only mode activated!");
                }
                else
                {
                    Server.PrintToChatAll("Vote for headshot-only mode failed.");
                }
            });
        }

        private void cmd_StartVote(CCSPlayerController? player, CommandInfo commandInfo)
        {
            if (player == null)
                return;

            if (_isVoteActive)
            {
                player.PrintToChat("A vote is already in progress.");
                return;
            }

            StartVote();
        }

        private void cmd_ForceStartHeadshot(CCSPlayerController? player, CommandInfo commandInfo)
        {
            if (player == null || !AdminManager.PlayerHasPermissions(player, Config.AdminFlagtoForceHsOnly))
                return;

            _isHeadshotOnly = true;
            Server.PrintToChatAll("Headshot-only mode activated by admin.");
        }

        private void cmd_ForceStopHeadshot(CCSPlayerController? player, CommandInfo commandInfo)
        {
            if (player == null || !AdminManager.PlayerHasPermissions(player, Config.AdminFlagtoForceHsOnly))
                return;

            _isHeadshotOnly = false;
            Server.PrintToChatAll("Headshot-only mode deactivated by admin.");
        }
    }

    public class Config : BasePluginConfig
    {
        [JsonPropertyName("HeadshotVoteRounds")]
        public int HeadshotVoteRounds { get; set; } = 5;

        [JsonPropertyName("HeadshotVoteDurationSeconds")]
        public int HeadshotVoteDurationSeconds { get; set; } = 10;

        [JsonPropertyName("AdminCanForceStartHeadshot")]
        public bool AdminCanForceStartHeadshot { get; set; } = true;

        [JsonPropertyName("AdminCanForceStopHeadshot")]
        public bool AdminCanForceStopHeadshot { get; set; } = true;

        [JsonPropertyName("AdminFlagtoForceHsOnly")]
        public string AdminFlagtoForceHsOnly { get; set; } = "@css/root";
    }
}
