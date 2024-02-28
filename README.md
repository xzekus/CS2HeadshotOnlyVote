# CS2HeadshotOnlyVote
CS2 plugin that creates a vote for headshot only mode.

## Features:
This plugin creates a vote for haedshot only mode after X amount of rounds (by default, its set to 5). <br>
Admin can force start the headshot mode.<br>
Admin can force stop the headshot mode.

## Commands
`css_hs_vote` - Starts the vote for headshot only mode.<br>
`css_starths` - Starts the headshot only mode (Admin command).<br>
`css_stophs` - Stops the headshot only mode (Admin command).

## Configuration
```
{
  {
    "HeadshotVoteRounds": 5,  // - After 5 rounds the headshot vote will be started
    "HeadshotVoteDurationSeconds": 10, // - Set the votes duration in seconds
    "HeadshotModeRounds": 3, // - How long does the HeadshotMode last
    "AdminCanForceStartHeadshot": true, // Toggles if admin can force start headshot mode
    "AdminCanForceStopHeadshot": true, // Toggles if admin can force stop headshot mode
    "AdminFlagtoForceHsOnly": "@css/root" // Admin flag Which can force start/stop Headshot Only Mode 
  }

}
```



