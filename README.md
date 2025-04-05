# HKMP Voice Chat
A mod and HKMP addon that implements voice chat between players.

## How it works
HKMP Voice Chat works using a few libraries to facilitate voice chat:
- OpenAL: To record and play audio positionally
- Opus: Audio codec to efficiently encode audio for networking
- RNNoise: Lightweight neural network that filters out noise from audio

HKMP Voice Chat uses the HKMP API to network audio data and the server addon takes care of delivering the audio to the
correct clients based on the server configuration.

## Install
### Client
HKMP.VoiceChat requires that it is installed on both the server and the client.
If you host from in-game, simply install the mod and start hosting.
To install it on your client, you should put the contents of the `HKMP.VoiceChat.zip` in your `Mods/` folder,
such that you have the folder `Mods/HKMP.VoiceChat/` with the file `HKMPVoiceChat.dll` in it (and a bunch of others).

The mods folder can be found in your steam installation (Beware that these are the default locations.
Your install may be in a different location):
- **Windows**: `C:\Program Files (x86)\Steam\steamapps\common\Hollow Knight\hollow_knight_Data\Managed\Mods\`
- **Mac**: `~/Library/Application Support/Steam/steamapps/common/Hollow Knight/hollow_knight.app/`,
then click "open package contents" and `content -> resources -> data -> managed -> mods`
- **Linux**: `~/.local/share/Steam/steamapps/common/Hollow Knight/hollow_knight_Data/Managed/Mods/`

Make sure that you have OpenAL installed.
It's a library program that facilitates recording and playing audio (positionally).
To install it:
- **Windows**: Go to [OpenAL.org](https://www.openal.org/downloads/) and download and run the installer.
- **Linux**: Most likely already installed with your distribution. Otherwise, use the package manager to install it.

### Server
If you are hosting a standalone server, make sure to add the following files/directories to the server directory
(these are included in the `.zip` file):
- `HKMPVoiceChat.dll`
- `HKMPVoiceChat.pdb`
- `Natives/`
- `OpenTK.dll`
- `OpenTK.dll.config`

## Usage
### Client
You can configure the voice chat with a few commands. For client configuration you can use the following:
- `/vcc mute` : Toggle mute your voice chat, such that other can(not) hear you.
- `/vcc volume <mic|speaker> <value>` : Change the input/output volume of your microphone or speakers.
- `/vcc device <list|set>` : List of set the used device for either microphone or speaker.
    - `/vcc device list <mics|speakers>` : Lists the available microphones or speakers that can be used with voice chat. The IDs given in the output can be used in the `/vcc device set ...` command.
    - `/vcc device set <mic|speaker> <value>` : Set the microphone or speaker to be used for voice chat. The value you should supply is one of the IDs from the `/vcc device list ...` command.

### Server
For the server configuration you can use the commands below. Please note that these commands require the sender to be authorized in order to execute them.
- `/vcs set <setting name> [value]` : Get or set the value of a voice chat server setting. The following settings can be used (along with their function):
    - `proximity_based_volume` (aliases: `proximity`, `prox`): Whether the volume of voice chat should be based on the proximity of the source and listener.
    - `team_voices_globally` (aliases: `teamglobal`, `teamglobally`): Whether to hear your team's voices globally independent of proximity or scenes.
    - `team_voices_only` (aliases: `teamonly`): Whether to hear only your team's voices and not other teams, even if they are in the same scene or in close proximity.
- `/vcs broadcast` : Toggle broadcasting your voice chat to the entire server.

## Copyright and license
HKMP.VoiceChat is a game modification for Hollow Knight that adds voice chat to HKMP.  
Copyright (C) 2025  Extremelyd1

    This library is free software; you can redistribute it and/or
    modify it under the terms of the GNU Lesser General Public
    License as published by the Free Software Foundation; either
    version 2.1 of the License, or (at your option) any later version.

    This library is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
    Lesser General Public License for more details.

    You should have received a copy of the GNU Lesser General Public
    License along with this library; if not, write to the Free Software
    Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301
    USA