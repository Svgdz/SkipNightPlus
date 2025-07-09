# SkipNightPlus

**SkipNight** is a Rust plugin that allows players to vote to skip the night cycle and transition quickly to morning. It features a robust voting system, chat and UI integration, and customizable day/night cycles, giving server owners full control over how and when nights can be skipped.

## Features

- Players can vote to skip night using chat commands or an interactive UI
- Configurable vote requirements (fixed number or percentage of online players)
- Admins can instantly skip night with special commands
- Customizable day and night start/end hours and skip-to hour
- Optional voting timer with UI countdown
- Fully configurable chat messages and UI appearance
- Optional custom day/night cycle lengths (override Rust’s default)
- Automatic UI updates for all players during voting
- Permission-based admin controls for skipping night and toggling custom cycles
- All settings are easily adjustable via the config file

## Quick Start

### Basic Setup (All Players):

Players can vote to skip night by typing `/skipnight` or `/sn` in chat (if chat commands are enabled), or by using the on-screen UI button during night time.

### Admin Setup:

Grant admin permissions to allow instant night skipping and cycle toggling:
```
oxide.grant user  skipnight.admin
```

## Permissions

This plugin uses the permission system.  
To assign a permission, use:
```
oxide.grant   
```
To remove a permission, use:
```
oxide.revoke   
```

- **skipnight.admin** — Grants access to admin-only commands: instant night skip and toggling custom day/night cycles.

## Commands

### Player Commands

- `/skipnight` or `/sn`  
  Vote to skip the night (if chat commands are enabled).  
  Triggers the UI vote as if the player pressed the UI button.

- `/skipnight.vote`  
  Internal UI command, triggered by the UI vote button.

### Admin Commands

- `/skipnightnow` or `/forcenightskip`  
  Instantly skips night, regardless of votes (admin only).

- `/toggledaycycle`  
  Enables or disables the custom day/night cycle (admin only).

## Configuration

SkipNight comes with several configuration options to tailor the plugin’s behavior:

```json
{
  "GeneralSettings": {
    "VotesNeeded": 3,
    "UsePlayerPopulationPercentage": false,
    "PlayerPopulationPercentage": 50.0,
    "NightStartHour": 18.0,
    "NightEndHour": 6.0,
    "SkipToHour": 8.0,
    "EnableNightSkipVoting": true
  },
  "ChatSettings": {
    "EnableChatMessages": true,
    "EnableChatCommands": true,
    "ChatPrefix": "[SkipNight]",
    "NightAnnouncement": "Night time has arrived! Type /skipnight or /sn to vote to skip the night.",
    "NightAnnouncementUIOnly": "Night time has arrived! Use the UI to vote to skip the night.",
    "VoteMessage": "{player} voted to skip the night!",
    "NightSkippedMessage": "Night has been skipped! Welcome to the morning!",
    "AlreadyVotedMessage": "You have already voted to skip the night!",
    "NotNightTimeMessage": "You can only vote to skip night during night time!",
    "AdminSkippedMessage": "{admin} has skipped the night! Welcome to the morning!",
    "NoPermissionMessage": "You don't have permission to use this command!",
    "NotNightTimeAdminMessage": "It's not currently night time. Use this command during night to skip it.",
    "YourVoteCountedMessage": "Your vote has been counted!",
    "VoteTimerExpiredMessage": "Vote timer expired! Night continues...",
    "VotingPeriodEndedMessage": "Voting period has ended!",
    "ChatCommandsDisabledMessage": "Chat commands are disabled. Please use the UI to vote!"
  },
  "UISettings": {
    "Enabled": true,
    "AnchorMin": "1.0 1.0",
    "AnchorMax": "1.0 1.0",
    "OffsetMin": "-200 -140",
    "OffsetMax": "-20 -20",
    "BackgroundColor": "0.08 0.08 0.08 0.95",
    "HeaderColor": "0.15 0.15 0.15 1.0",
    "VoteButtonColor": "0.2 0.7 0.2 1.0",
    "VoteButtonHoverColor": "0.25 0.8 0.25 1.0",
    "VoteButtonTextColor": "1.0 1.0 1.0 1.0",
    "VotedButtonColor": "0.4 0.4 0.4 1.0",
    "InfoTextColor": "0.9 0.9 0.9 1.0",
    "HeaderTextColor": "1.0 1.0 1.0 1.0",
    "TimerTextColor": "1.0 0.6 0.2 1.0",
    "TimerBackgroundColor": "0.2 0.2 0.2 0.9",
    "VoteButtonText": "VOTE TO SKIP",
    "VoteCountedText": "VOTE COUNTED",
    "HeaderText": "SKIP NIGHT",
    "InfoText": "{votes} / {needed} votes",
    "TimerEnabled": true,
    "TimerDuration": 120.0,
    "TimerText": "Time remaining: {time}",
    "HeaderFontSize": 14,
    "TimerFontSize": 18,
    "ButtonFontSize": 12,
    "UIWidth": 180,
    "UIHeight": 120
  },
  "DayCycleSettings": {
    "EnableCustomDayCycle": false,
    "DayLengthMinutes": 45.0,
    "NightLengthMinutes": 15.0
  },
  "Version": {
    "Major": 1,
    "Minor": 0,
    "Patch": 0
  }
}
```

### Configuration Sections Explained

#### GeneralSettings

- **VotesNeeded**: Number of votes required to skip night (if UsePlayerPopulationPercentage is false)
- **UsePlayerPopulationPercentage**: If true, required votes is a percentage of online players
- **PlayerPopulationPercentage**: Percentage of online players required to vote if enabled
- **NightStartHour**: Hour (24h) when night starts
- **NightEndHour**: Hour (24h) when night ends
- **SkipToHour**: Hour to advance to when night is skipped
- **EnableNightSkipVoting**: Master toggle for night skip voting system

#### ChatSettings

- **EnableChatMessages**: Enables all chat messages and notifications
- **EnableChatCommands**: Allows use of /skipnight and /sn commands
- **ChatPrefix**: Prefix for all plugin messages in chat
- **NightAnnouncement**: Message broadcast at night start (when chat commands are enabled)
- **NightAnnouncementUIOnly**: Message broadcast at night start (when chat commands are disabled)
- **VoteMessage**: Message sent when a player votes
- **NightSkippedMessage**: Message sent when night is skipped
- **AlreadyVotedMessage**: Sent to players who try to vote twice
- **NotNightTimeMessage**: Sent if a player tries to vote during the day
- **AdminSkippedMessage**: Sent when an admin skips the night
- **NoPermissionMessage**: Sent when a player lacks permission for an admin command
- **NotNightTimeAdminMessage**: Sent to admin if they try to skip day
- **YourVoteCountedMessage**: Sent to player when their vote is registered
- **VoteTimerExpiredMessage**: Sent when the voting timer runs out
- **VotingPeriodEndedMessage**: Sent if voting is attempted after timer ends
- **ChatCommandsDisabledMessage**: Sent if chat commands are disabled

#### UISettings

- **Enabled**: Enables the voting UI panel
- **AnchorMin / AnchorMax / OffsetMin / OffsetMax**: Controls where the UI appears on screen
- **BackgroundColor / HeaderColor / VoteButtonColor / VoteButtonHoverColor / VoteButtonTextColor / VotedButtonColor / InfoTextColor / HeaderTextColor / TimerTextColor / TimerBackgroundColor**: Customize colors for UI elements (RGBA format)
- **VoteButtonText**: Text for the vote button
- **VoteCountedText**: Text when player has voted
- **HeaderText**: UI header
- **InfoText**: Shows vote progress
- **TimerEnabled**: Enables the vote timer in UI
- **TimerDuration**: How long (in seconds) the vote is open
- **TimerText**: Text for timer display
- **HeaderFontSize / TimerFontSize / ButtonFontSize**: Font sizes for UI text
- **UIWidth / UIHeight**: Size of the UI panel

#### DayCycleSettings

- **EnableCustomDayCycle**: Enables custom day/night cycle lengths
- **DayLengthMinutes**: Length of the day (in minutes)
- **NightLengthMinutes**: Length of the night (in minutes)

#### Version

- **Major / Minor / Patch**: Internal version tracking for config migration

### Configuration Tips

- Backup your config before making changes
- Use `oxide.reload SkipNight` after config changes
- Adjust vote requirements and timer for your server’s population size
- Use custom day/night cycle for unique server pacing
- Disable chat commands or UI as needed for your community

## Notes

- Admin permission is required for instant night skipping and toggling custom cycles.
- All chat and UI messages are fully customizable in the config.
- UI is automatically destroyed at daybreak or when voting ends.
- Voting can be disabled entirely with EnableNightSkipVoting.
- The plugin is performance optimized and only checks night status every 30 seconds.
