# What is Z3roVR
Z3roVR is a framework for easy multiplayer VR using Photon.VR
# Setup
## Requirements
Get ALL the cloudscripts in the cloudscripts file and add them to your playfab revision.

Import the package into your project, (requires PlayFab and Photon.Pun and Photon.Voice to be inside)

## Step 1 - Rules Setup

Go to your rules "Automation\Rules" and add a rule (any name) and with the event of 'com.playfab.player_created', this simply
allows the userdata to be created, to avoid any issues further on, while this issue does resolve its self when the player purchases something, it may error first.

## Step 2 - Project Setup
In the Z3roVR\Scenes folder, you'll find a template scene for the use of testing.
Make sure ur set ur player head, L hand, and R Hand.
Take 'Z3roPunManager' out of there and add it to your scene.

The network player is inside 'Prefabs/NetworkPlayerPrefab' setup your stuff there.

Photon Voice is not setup here, you gotta do that your self, i got a bit lazy. will be in the next version, along with a leaderboard and PROPER muting.

## Step 3 - Player Setup
The Player Has Lists, 1 for all cosmetics.

Put ALL of your cosmetics on there, and make sure they're the same name as the item on playfab. (Requirement)

# Code
## PlayFabManager
there is a pre setup playfab manager with some values, feel free to modify it, but don't dumb it up with stupid code. (plz)

## Z3roPunManager
The NetworkManager, handles all photon things, including the player cosmetics lookups.

## CosmeticButton
Use this for your cosmetic enable buttons, has a string for the tag of the hand, the cosmetic name, and if this is the enable button.

# Better Documentation Soon.
