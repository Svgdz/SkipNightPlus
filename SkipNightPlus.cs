using Newtonsoft.Json;
using Oxide.Core;
using Oxide.Core.Libraries.Covalence;
using Oxide.Core.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Oxide.Game.Rust.Cui;


namespace Oxide.Plugins
{
    [Info("SkipNightPlus", "ElSalvaje", "1.0.0")]
    [Description("Allows players to vote to skip night time")]
    public class SkipNightPlus : CovalencePlugin
    {
        #region Config
        private ConfigData _config;
        private Timer nightCheckTimer;
        private Timer voteTimer;
        private Timer customDayTimer;
        private Timer customNightTimer;
        private HashSet<ulong> currentVotes = new HashSet<ulong>();
        private bool isNightTime = false;
        private bool hasAnnouncedNight = false;
        private float voteTimeRemaining = 0f;
        private bool isVoteTimerActive = false;
        private bool isCustomCycleActive = false;

        private class ConfigData
        {
            [JsonProperty(PropertyName = "GeneralSettings")]
            public GeneralSettingsData GeneralSettings { get; set; } = new GeneralSettingsData();

            [JsonProperty(PropertyName = "ChatSettings")]
            public ChatSettingsData ChatSettings { get; set; } = new ChatSettingsData();

            [JsonProperty(PropertyName = "UISettings")]
            public UISettingsData UISettings { get; set; } = new UISettingsData();

            [JsonProperty(PropertyName = "DayCycleSettings")]
            public DayCycleSettingsData DayCycleSettings { get; set; } = new DayCycleSettingsData();

            [JsonProperty(PropertyName = "Version")]
            public Version Version { get; set; } = null;
        }

        private class GeneralSettingsData
        {
            [JsonProperty(PropertyName = "VotesNeeded")]
            public int VotesNeeded { get; set; } = 3;

            [JsonProperty(PropertyName = "UsePlayerPopulationPercentage")]
            public bool UsePlayerPopulationPercentage { get; set; } = false;

            [JsonProperty(PropertyName = "PlayerPopulationPercentage")]
            public float PlayerPopulationPercentage { get; set; } = 50.0f;

            [JsonProperty(PropertyName = "NightStartHour")]
            public float NightStartHour { get; set; } = 18.0f;

            [JsonProperty(PropertyName = "NightEndHour")]
            public float NightEndHour { get; set; } = 6.0f;

            [JsonProperty(PropertyName = "SkipToHour")]
            public float SkipToHour { get; set; } = 8.0f;

            [JsonProperty(PropertyName = "EnableNightSkipVoting")]
            public bool EnableNightSkipVoting { get; set; } = true;


        }

        private class ChatSettingsData
        {
            [JsonProperty(PropertyName = "EnableChatMessages")]
            public bool EnableChatMessages { get; set; } = true;

            [JsonProperty(PropertyName = "EnableChatCommands")]
            public bool EnableChatCommands { get; set; } = true;

            [JsonProperty(PropertyName = "ChatPrefix")]
            public string ChatPrefix { get; set; } = "<color=#00FF00>[SkipNight]</color>";

            [JsonProperty(PropertyName = "NightAnnouncement")]
            public string NightAnnouncement { get; set; } = "Night time has arrived! Type <color=#FFFF00>/skipnightplus</color> or <color=#FFFF00>/snp</color> to vote to skip the night.";

            [JsonProperty(PropertyName = "NightAnnouncementUIOnly")]
            public string NightAnnouncementUIOnly { get; set; } = "Night time has arrived! Use the UI to vote to skip the night.";

            [JsonProperty(PropertyName = "VoteMessage")]
            public string VoteMessage { get; set; } = "{player} voted to skip the night!";

            [JsonProperty(PropertyName = "NightSkippedMessage")]
            public string NightSkippedMessage { get; set; } = "Night has been skipped! Welcome to the morning!";

            [JsonProperty(PropertyName = "AlreadyVotedMessage")]
            public string AlreadyVotedMessage { get; set; } = "You have already voted to skip the night!";

            [JsonProperty(PropertyName = "NotNightTimeMessage")]
            public string NotNightTimeMessage { get; set; } = "You can only vote to skip night during night time!";

            [JsonProperty(PropertyName = "AdminSkippedMessage")]
            public string AdminSkippedMessage { get; set; } = "{admin} has skipped the night! Welcome to the morning!";

            [JsonProperty(PropertyName = "NoPermissionMessage")]
            public string NoPermissionMessage { get; set; } = "You don't have permission to use this command!";

            [JsonProperty(PropertyName = "NotNightTimeAdminMessage")]
            public string NotNightTimeAdminMessage { get; set; } = "It's not currently night time. Use this command during night to skip it.";

            [JsonProperty(PropertyName = "YourVoteCountedMessage")]
            public string YourVoteCountedMessage { get; set; } = "Your vote has been counted!";

            [JsonProperty(PropertyName = "VoteTimerExpiredMessage")]
            public string VoteTimerExpiredMessage { get; set; } = "Vote timer expired! Night continues...";

            [JsonProperty(PropertyName = "VotingPeriodEndedMessage")]
            public string VotingPeriodEndedMessage { get; set; } = "Voting period has ended!";

            [JsonProperty(PropertyName = "ChatCommandsDisabledMessage")]
            public string ChatCommandsDisabledMessage { get; set; } = "Chat commands are disabled. Please use the UI to vote!";
        }

        private class DayCycleSettingsData
        {
            [JsonProperty(PropertyName = "EnableCustomDayCycle")]
            public bool EnableCustomDayCycle { get; set; } = false;

            [JsonProperty(PropertyName = "DayLengthMinutes")]
            public float DayLengthMinutes { get; set; } = 45.0f;

            [JsonProperty(PropertyName = "NightLengthMinutes")]
            public float NightLengthMinutes { get; set; } = 15.0f;
        }

        private class UISettingsData
        {
            [JsonProperty(PropertyName = "Enabled")]
            public bool Enabled { get; set; } = true;

            [JsonProperty(PropertyName = "AnchorMin")]
            public string AnchorMin { get; set; } = "1.0 1.0";

            [JsonProperty(PropertyName = "AnchorMax")]
            public string AnchorMax { get; set; } = "1.0 1.0";

            [JsonProperty(PropertyName = "OffsetMin")]
            public string OffsetMin { get; set; } = "-200 -140";

            [JsonProperty(PropertyName = "OffsetMax")]
            public string OffsetMax { get; set; } = "-20 -20";

            [JsonProperty(PropertyName = "BackgroundColor")]
            public string BackgroundColor { get; set; } = "0.08 0.08 0.08 0.95";

            [JsonProperty(PropertyName = "HeaderColor")]
            public string HeaderColor { get; set; } = "0.15 0.15 0.15 1.0";

            [JsonProperty(PropertyName = "VoteButtonColor")]
            public string VoteButtonColor { get; set; } = "0.2 0.7 0.2 1.0";
            
            [JsonProperty(PropertyName = "VoteButtonHoverColor")]
            public string VoteButtonHoverColor { get; set; } = "0.25 0.8 0.25 1.0";

            [JsonProperty(PropertyName = "VoteButtonTextColor")]
            public string VoteButtonTextColor { get; set; } = "1.0 1.0 1.0 1.0";

            [JsonProperty(PropertyName = "VotedButtonColor")]
            public string VotedButtonColor { get; set; } = "0.4 0.4 0.4 1.0";

            [JsonProperty(PropertyName = "InfoTextColor")]
            public string InfoTextColor { get; set; } = "0.9 0.9 0.9 1.0";

            [JsonProperty(PropertyName = "HeaderTextColor")]
            public string HeaderTextColor { get; set; } = "1.0 1.0 1.0 1.0";

            [JsonProperty(PropertyName = "TimerTextColor")]
            public string TimerTextColor { get; set; } = "1.0 0.6 0.2 1.0";

            [JsonProperty(PropertyName = "TimerBackgroundColor")]
            public string TimerBackgroundColor { get; set; } = "0.2 0.2 0.2 0.9";

            [JsonProperty(PropertyName = "VoteButtonText")]
            public string VoteButtonText { get; set; } = "VOTE TO SKIP";

            [JsonProperty(PropertyName = "VoteCountedText")]
            public string VoteCountedText { get; set; } = "VOTE COUNTED";

            [JsonProperty(PropertyName = "HeaderText")]
            public string HeaderText { get; set; } = "SKIP NIGHT";

            [JsonProperty(PropertyName = "InfoText")]
            public string InfoText { get; set; } = "{votes} / {needed} votes";

            [JsonProperty(PropertyName = "TimerEnabled")]
            public bool TimerEnabled { get; set; } = true;

            [JsonProperty(PropertyName = "TimerDuration")]
            public float TimerDuration { get; set; } = 120f;

            [JsonProperty(PropertyName = "TimerText")]
            public string TimerText { get; set; } = "Time remaining: {time}";

            [JsonProperty(PropertyName = "HeaderFontSize")]
            public int HeaderFontSize { get; set; } = 14;

            [JsonProperty(PropertyName = "TimerFontSize")]
            public int TimerFontSize { get; set; } = 18;

            [JsonProperty(PropertyName = "ButtonFontSize")]
            public int ButtonFontSize { get; set; } = 12;

            [JsonProperty(PropertyName = "UIWidth")]
            public int UIWidth { get; set; } = 180;

            [JsonProperty(PropertyName = "UIHeight")]
            public int UIHeight { get; set; } = 120;
        }

        private class Version
        {
            [JsonProperty(PropertyName = "Major")]
            public int Major { get; set; } = 1;

            [JsonProperty(PropertyName = "Minor")]
            public int Minor { get; set; } = 0;

            [JsonProperty(PropertyName = "Patch")]
            public int Patch { get; set; } = 0;
        }

        protected override void LoadConfig()
        {
            base.LoadConfig();
            try
            {
                _config = Config.ReadObject<ConfigData>();
                if (_config == null)
                {
                    LoadDefaultConfig();
                }
                else
                {
                    if (_config.Version == null)
                    {
                        HandleFirstTimeVersionTracking();
                    }
                    else if (IsConfigOutdated())
                    {
                        MigrateConfig();
                    }
                }
            }
            catch
            {
                LoadDefaultConfig();
            }
        }

        protected override void LoadDefaultConfig()
        {
            _config = new ConfigData();
            _config.Version = new Version(); 
            SaveConfig();
        }

        protected override void SaveConfig() => Config.WriteObject(_config);

        private void HandleFirstTimeVersionTracking()
        {
            var currentGeneralSettings = _config.GeneralSettings;
            var currentChatSettings = _config.ChatSettings;
        
            _config.Version = new Version { Major = 1, Minor = 0, Patch = 0 };
            
            if (_config.GeneralSettings == null)
            {
                _config.GeneralSettings = new GeneralSettingsData();
            }
            if (_config.ChatSettings == null)
            {
                _config.ChatSettings = new ChatSettingsData();
            }
            
            SaveConfig();
        }

        private bool IsConfigOutdated()
        {
            var currentVersion = new Version();
            var configVersion = _config.Version;
            
            return IsVersionOlder(configVersion, currentVersion);
        }

        private bool IsVersionOlder(Version version1, Version version2)
        {
            if (version1.Major < version2.Major) return true;
            if (version1.Major > version2.Major) return false;
            
            if (version1.Minor < version2.Minor) return true;
            if (version1.Minor > version2.Minor) return false;
            
            return version1.Patch < version2.Patch;
        }

        private void MigrateConfig()
        {
            _config.Version = new Version();
            SaveConfig();
        }
        #endregion

        #region UI
        private const string UIPanelName = "SkipNightUI";

        private void CreateOrUpdateUIForAll()
        {
            if (!_config.UISettings.Enabled || !isNightTime) return;

            foreach (var player in BasePlayer.activePlayerList)
            {
                CreateOrUpdateUI(player);
            }
        }

        private void CreateOrUpdateUI(BasePlayer player)
        {
            if (!_config.UISettings.Enabled || player == null) return;

            CuiHelper.DestroyUi(player, UIPanelName);

            var ui = _config.UISettings;
            var container = new CuiElementContainer();
            var hasVoted = currentVotes.Contains(player.userID);

            var offsetMinX = -ui.UIWidth;
            var offsetMinY = -ui.UIHeight;
            var offsetMaxX = -20;
            var offsetMaxY = -20;

            container.Add(new CuiPanel
            {
                Image = { Color = ui.BackgroundColor },
                RectTransform = { AnchorMin = ui.AnchorMin, AnchorMax = ui.AnchorMax, OffsetMin = $"{offsetMinX} {offsetMinY}", OffsetMax = $"{offsetMaxX} {offsetMaxY}" }
            }, "Overlay", UIPanelName);

            container.Add(new CuiPanel
            {
                Image = { Color = ui.HeaderColor },
                RectTransform = { AnchorMin = "0 0.7", AnchorMax = "1 1" }
            }, UIPanelName, UIPanelName + "_Header");

            container.Add(new CuiLabel
            {
                Text = { Text = ui.HeaderText, FontSize = ui.HeaderFontSize, Align = TextAnchor.MiddleCenter, Color = ui.HeaderTextColor, Font = "robotocondensed-bold.ttf" },
                RectTransform = { AnchorMin = "0 0", AnchorMax = "1 1" }
            }, UIPanelName + "_Header");

            if (ui.TimerEnabled && isVoteTimerActive)
            {
                var minutes = Mathf.FloorToInt(voteTimeRemaining / 60f);
                var seconds = Mathf.FloorToInt(voteTimeRemaining % 60f);
                var timerDisplay = $"{minutes:00}:{seconds:00}";

                container.Add(new CuiPanel
                {
                    Image = { Color = ui.TimerBackgroundColor },
                    RectTransform = { AnchorMin = "0.1 0.4", AnchorMax = "0.9 0.65" }
                }, UIPanelName, UIPanelName + "_TimerPanel");

                container.Add(new CuiLabel
                {
                    Text = { 
                        Text = timerDisplay, 
                        FontSize = ui.TimerFontSize, 
                        Align = TextAnchor.MiddleCenter, 
                        Color = "1.0 1.0 1.0 1.0"
                    },
                    RectTransform = { AnchorMin = "0 0", AnchorMax = "1 1" }
                }, UIPanelName + "_TimerPanel");
            }

            var buttonText = hasVoted ? ui.VoteCountedText : ui.VoteButtonText;
            var buttonColor = hasVoted ? ui.VotedButtonColor : ui.VoteButtonColor;
            var buttonCommand = hasVoted ? "" : "skipnightplus.vote";

            container.Add(new CuiButton
            {
                Button = { Command = buttonCommand, Color = buttonColor },
                RectTransform = { AnchorMin = "0.1 0.05", AnchorMax = "0.9 0.35" },
                Text = { Text = buttonText, FontSize = ui.ButtonFontSize, Align = TextAnchor.MiddleCenter, Color = ui.VoteButtonTextColor, Font = "robotocondensed-bold.ttf" }
            }, UIPanelName);

            CuiHelper.AddUi(player, container);
        }

        private void DestroyUIForAll()
        {
            if (!_config.UISettings.Enabled) return;

            foreach (var player in BasePlayer.activePlayerList)
            {
                CuiHelper.DestroyUi(player, UIPanelName);
            }
        }
        #endregion

        #region Plugin Hooks
        private const string PermissionAdmin = "skipnightplus.admin";

        private void Init()
        {
            permission.RegisterPermission(PermissionAdmin, this);

            AddCovalenceCommand("skipnightplus", nameof(SkipNightCommand));
            AddCovalenceCommand("sn", nameof(SkipNightCommand));
            AddCovalenceCommand("skipnightplusnow", nameof(AdminSkipCommand));
            AddCovalenceCommand("forcenightskip", nameof(AdminSkipCommand));
            AddCovalenceCommand("toggledaycycle", nameof(ToggleDayCycleCommand));
            
            AddCovalenceCommand("skipnightplus.vote", nameof(UIVoteCommand));

            nightCheckTimer = timer.Every(30f, CheckNightTime);
        }

        private void OnServerInitialized()
        {
            CheckNightTime();
            
            if (_config.DayCycleSettings.EnableCustomDayCycle)
            {
                StartCustomDayCycle();
            }
        }

        private void Unload()
        {
            nightCheckTimer?.Destroy();
            voteTimer?.Destroy();
            customDayTimer?.Destroy();
            customNightTimer?.Destroy();
            DestroyUIForAll();
        }
        #endregion

        #region Commands
        private void SkipNightCommand(IPlayer player, string command, string[] args)
        {
            if (player.IsServer) return;

            var basePlayer = player.Object as BasePlayer;
            if (basePlayer == null) return;

            if (!_config.ChatSettings.EnableChatCommands && !player.HasPermission(PermissionAdmin) && !player.IsAdmin)
            {
                if (_config.ChatSettings.EnableChatMessages)
                {
                    SendReply(player, _config.ChatSettings.ChatCommandsDisabledMessage);
                }
                return;
            }

            basePlayer.SendConsoleCommand("skipnightplus.vote");
        }

        private void ToggleDayCycleCommand(IPlayer player, string command, string[] args)
        {
            if (!player.HasPermission(PermissionAdmin) && !player.IsAdmin)
            {
                if (_config.ChatSettings.EnableChatMessages)
                {
                    SendReply(player, _config.ChatSettings.NoPermissionMessage);
                }
                return;
            }

            _config.DayCycleSettings.EnableCustomDayCycle = !_config.DayCycleSettings.EnableCustomDayCycle;
            SaveConfig();

            if (_config.DayCycleSettings.EnableCustomDayCycle)
            {
                StartCustomDayCycle();
                SendReply(player, "Custom day cycle enabled!");
            }
            else
            {
                StopCustomDayCycle();
                SendReply(player, "Custom day cycle disabled!");
            }
        }

        private void UIVoteCommand(IPlayer player, string command, string[] args)
        {
            if (player.IsServer) return;

            var basePlayer = player.Object as BasePlayer;
            if (basePlayer == null) return;

            if (!_config.GeneralSettings.EnableNightSkipVoting)
            {
                if (_config.ChatSettings.EnableChatMessages)
                {
                    SendReply(player, "Night skip voting is currently disabled!");
                }
                return;
            }

            try
            {
                if (!isNightTime)
                {
                    if (_config.ChatSettings.EnableChatMessages)
                    {
                        SendReply(player, _config.ChatSettings.NotNightTimeMessage);
                    }
                    return;
                }

                if (_config.UISettings.TimerEnabled && !isVoteTimerActive)
                {
                    if (_config.ChatSettings.EnableChatMessages)
                    {
                        SendReply(player, _config.ChatSettings.VotingPeriodEndedMessage);
                    }
                    return;
                }

                if (currentVotes.Contains(basePlayer.userID))
                {
                    if (_config.ChatSettings.EnableChatMessages)
                    {
                        SendReply(player, _config.ChatSettings.AlreadyVotedMessage);
                    }
                    return;
                }

                currentVotes.Add(basePlayer.userID);

                if (_config.ChatSettings.EnableChatMessages)
                {
                    SendReply(player, _config.ChatSettings.YourVoteCountedMessage);
            
                    var voteMessage = _config.ChatSettings.VoteMessage
                        .Replace("{player}", basePlayer.displayName)
                        .Replace("{votes}", currentVotes.Count.ToString())
                        .Replace("{needed}", GetRequiredVotes().ToString());
            
                    BroadcastMessage(voteMessage);
                }

                if (_config.UISettings.Enabled)
                {
                    CreateOrUpdateUIForAll();
                }

                if (currentVotes.Count >= GetRequiredVotes())
                {
                    SkipNightTime();
                }
            }
            catch (Exception ex)
            {
                PrintError($"An error occurred in UIVoteCommand for player {basePlayer.displayName}: {ex.ToString()}");
            }
        }

        private void AdminSkipCommand(IPlayer player, string command, string[] args)
        {
            if (!player.HasPermission(PermissionAdmin) && !player.IsAdmin)
            {
                if (_config.ChatSettings.EnableChatMessages)
                {
                    SendReply(player, _config.ChatSettings.NoPermissionMessage);
                }
                return;
            }

            if (!isNightTime)
            {
                if (_config.ChatSettings.EnableChatMessages)
                {
                    SendReply(player, _config.ChatSettings.NotNightTimeAdminMessage);
                }
                return;
            }

            AdminSkipNight(player.Name);
        }
        #endregion

        #region Core Functionality
        private int GetRequiredVotes()
        {
            if (!_config.GeneralSettings.UsePlayerPopulationPercentage)
            {
                return _config.GeneralSettings.VotesNeeded;
            }

            var onlinePlayerCount = BasePlayer.activePlayerList.Count;
            var requiredVotes = Mathf.CeilToInt(onlinePlayerCount * (_config.GeneralSettings.PlayerPopulationPercentage / 100f));
            var finalVotes = Mathf.Max(1, requiredVotes);
            
            return finalVotes;
        }

        private void CheckNightTime()
        {
            if (_config.DayCycleSettings.EnableCustomDayCycle && isCustomCycleActive)
            {
                return;
            }
            
            var currentTime = TOD_Sky.Instance.Cycle.Hour;
            var nightStart = _config.GeneralSettings.NightStartHour;
            var nightEnd = _config.GeneralSettings.NightEndHour;
            
            bool wasNightTime = isNightTime;
            
            if (nightEnd < nightStart)
            {
                isNightTime = currentTime >= nightStart || currentTime <= nightEnd;
            }
            else
            {
                isNightTime = currentTime >= nightStart && currentTime <= nightEnd;
            }
            
            if (isNightTime && !wasNightTime)
            {
                OnNightStarted();
            }
            
            if (!isNightTime && wasNightTime)
            {
                OnDayStarted();
            }
        }

        private void OnNightStarted()
        {
            if (hasAnnouncedNight) return;
            
            hasAnnouncedNight = true;
            currentVotes.Clear();
            

            
            var requiredVotes = GetRequiredVotes();
            var onlineCount = BasePlayer.activePlayerList.Count;
            
            if (_config.ChatSettings.EnableChatMessages)
            {
                var announcement = _config.ChatSettings.NightAnnouncement;
                if (!_config.ChatSettings.EnableChatCommands)
                {
                    announcement = _config.ChatSettings.NightAnnouncementUIOnly;
                }
                BroadcastMessage(announcement);
            }

            if (_config.UISettings.Enabled && _config.GeneralSettings.EnableNightSkipVoting)
            {
                if (_config.UISettings.TimerEnabled)
                {
                    StartVoteTimer();
                }
                CreateOrUpdateUIForAll();
            }
        }

        private void OnDayStarted()
        {
            hasAnnouncedNight = false;
            currentVotes.Clear();
            StopVoteTimer();
            

            
            if (_config.UISettings.Enabled)
            {
                DestroyUIForAll();
            }
        }

        private void SkipNightTime()
        {
            TOD_Sky.Instance.Cycle.Hour = _config.GeneralSettings.SkipToHour;
            
            if (_config.ChatSettings.EnableChatMessages)
            {
                BroadcastMessage(_config.ChatSettings.NightSkippedMessage);
            }
            
            currentVotes.Clear();
            isNightTime = false;
            hasAnnouncedNight = false;
            StopVoteTimer();
            if (_config.UISettings.Enabled)
            {
                DestroyUIForAll();
            }
        }

        private void AdminSkipNight(string adminName)
        {
            TOD_Sky.Instance.Cycle.Hour = _config.GeneralSettings.SkipToHour;
            
            if (_config.ChatSettings.EnableChatMessages)
            {
                var adminMessage = _config.ChatSettings.AdminSkippedMessage;
                if (adminMessage.Contains("{admin}"))
                {
                    adminMessage = adminMessage.Replace("{admin}", adminName);
                }
                BroadcastMessage(adminMessage);
            }
            
            currentVotes.Clear();
            isNightTime = false;
            hasAnnouncedNight = false;
            StopVoteTimer();
            if (_config.UISettings.Enabled)
            {
                DestroyUIForAll();
            }
        }

        private void StartVoteTimer()
        {
            if (!_config.UISettings.TimerEnabled) 
            {
                return;
            }
            
            StopVoteTimer();
            voteTimeRemaining = _config.UISettings.TimerDuration;
            isVoteTimerActive = true;
            
            voteTimer = timer.Every(1f, () =>
            {
                voteTimeRemaining -= 1f;
                
                if (voteTimeRemaining <= 0f)
                {
                    OnVoteTimerExpired();
                    return;
                }
                
                if (_config.UISettings.Enabled)
                {
                    CreateOrUpdateUIForAll();
                }
            });
        }

        private void StopVoteTimer()
        {
            voteTimer?.Destroy();
            voteTimer = null;
            isVoteTimerActive = false;
            voteTimeRemaining = 0f;
        }

        private void OnVoteTimerExpired()
        {
            StopVoteTimer();
            if (_config.ChatSettings.EnableChatMessages)
            {
                BroadcastMessage(_config.ChatSettings.VoteTimerExpiredMessage);
            }
            
            if (_config.UISettings.Enabled)
            {
                DestroyUIForAll();
            }
        }

        private void StartCustomDayCycle()
        {
            if (!_config.DayCycleSettings.EnableCustomDayCycle) return;
            
            StopCustomDayCycle();
            
            isCustomCycleActive = true;
            TOD_Sky.Instance.Cycle.Hour = 12.0f;
            isNightTime = false;
            hasAnnouncedNight = false;
            
            var dayDurationSeconds = _config.DayCycleSettings.DayLengthMinutes * 60f;
            customDayTimer = timer.Once(dayDurationSeconds, () =>
            {
                if (isCustomCycleActive)
                {
                    TOD_Sky.Instance.Cycle.Hour = 0.0f;
                    isNightTime = true;
                    hasAnnouncedNight = false;
                    OnNightStarted();
                    
                    var nightDurationSeconds = _config.DayCycleSettings.NightLengthMinutes * 60f;
                    customNightTimer = timer.Once(nightDurationSeconds, () =>
                    {
                        if (isCustomCycleActive)
                        {
                            StartCustomDayCycle();
                        }
                    });
                }
            });
        }

        private void StopCustomDayCycle()
        {
            customDayTimer?.Destroy();
            customDayTimer = null;
            customNightTimer?.Destroy();
            customNightTimer = null;
            isCustomCycleActive = false;
        }

        private void SetDayTime()
        {
            TOD_Sky.Instance.Cycle.Hour = 12f;
            if (isNightTime)
            {
                isNightTime = false;
                OnDayStarted();
            }
        }

        private void SetNightTime()
        {
            TOD_Sky.Instance.Cycle.Hour = 0f;
            if (!isNightTime)
            {
                isNightTime = true;
                OnNightStarted();
            }
        }

        private void BroadcastMessage(string message)
        {
            var formattedMessage = $"{_config.ChatSettings.ChatPrefix} {message}";
            foreach (var player in players.Connected)
            {
                player.Message(formattedMessage);
            }
        }

        private void SendReply(IPlayer player, string message)
        {
            var formattedMessage = $"{_config.ChatSettings.ChatPrefix} {message}";
            player.Reply(formattedMessage);
        }


        #endregion
    }
}
