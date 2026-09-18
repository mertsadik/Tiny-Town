# Tiny Town

Tiny Town is a relaxing casual mobile game made with Unity. You place hexagon tiles one by one and grow your own little town. Every tile earns points from the tiles next to it, so the goal is to find the best spot for each tile before your deck runs out.

## Features

- Hexagon tile placement with instant neighbor scoring
- Tile preview: tap an empty slot to see the tile and the points it will earn, tap again to place it
- Quests on tile groups that give you extra tiles when completed
- Two unlockable tile types (Mill and Harbor)
- Daily Town mode: everyone plays with the same deck on the same day
- Save system: an unfinished game is saved automatically and can be continued later
- High score, games played and daily best score
- Drag to move the camera, pinch (or mouse wheel) to zoom
- Low poly 3D tiles, drop animations, particle effects and sounds
- Portrait layout that works on phones and tablets, including notched screens

## How to Play

1. Tap an empty (light) slot next to your town. The next tile appears there as a preview with its score.
2. Tap the same slot again to place the tile.
3. Each tile scores points based on its neighbors (see the table below).
4. The next three tiles are shown at the bottom of the screen, so you can plan ahead.
5. The game ends when your deck is empty.

### Scoring

| Neighbors | Points |
|---|---|
| House + Field | +2 |
| House + Lake | +1 |
| House + House | +1 |
| Forest + Forest | +1 |
| Lake + Lake | +1 |
| Field + Lake | +1 |
| Mill + Field | +2 |
| Mill + House | +1 |
| Harbor + Lake | +2 |
| Harbor + House | +1 |

Grass tiles give no points but they fit anywhere.

### Quests

Some tiles get a quest badge like `3/5`. Grow that group of connected tiles of the same type to the target size and 5 extra tiles are added to your deck. If the group gets fully surrounded and can not grow anymore, the quest is closed. There can be up to 3 quests at the same time.

### Unlocking New Tiles

Points from all your games are added together. New tile types join the deck when you reach:

- Mill: 100 total points
- Harbor: 250 total points

Daily Town mode always uses only the five basic tile types so every player has the same chance.

## Controls

| Action | Phone | Editor |
|---|---|---|
| Preview / place tile | Tap / tap again | Click / click again |
| Move camera | Drag with one finger | Drag with mouse |
| Zoom | Pinch with two fingers | Mouse wheel |

## Requirements

- Unity 6000.5.9f1 (Unity 6)
- Universal Render Pipeline (URP)
- Input System package (the old Input Manager is not used)

## Getting Started

1. Clone the repository:
   ```
   git clone https://github.com/mertsadik/Tiny-Town.git
   ```
2. Open the project folder with Unity Hub (Unity 6000.5.9f1).
3. Open the scene `Assets/Scenes/Oyun.unity`.
4. Set the Game view to a portrait resolution (for example 1080x1920) and press Play.

The scene only contains one `Game` object. The camera, light and all UI are created from code when the game starts, so the scene looks empty before you press Play.

## Project Structure

```
Assets/
  Resources/
    Fonts/          Poppins font
    Modeller/       Kenney hexagon tile models
    Parcaciklar/    particle material and texture
    Sesler/         sound effects
    UI/             rounded panel sprite
  Scenes/
    Oyun.unity      main scene
  Scripts/
    KasabaOyunu.cs  main game logic (input, tiles, camera, save/load)
    Arayuz.cs       all UI (menu, score, preview, rules, game over)
    Ayarlar.cs      balance values (deck sizes, points, camera, animations)
    GorevSistemi.cs quests
    Puanlama.cs     scoring rules
    HexGrid.cs      hexagon grid math
    OyunKaydi.cs    save data and high scores
    ...
    Editor/
      TinyTownKurulum.cs  "TinyTown" menu: set up scene and run a self test
```

Most of the code and comments are written in Turkish.

## Changing the Balance

All numbers that affect gameplay are in `Assets/Scripts/Ayarlar.cs`: number of each tile in the deck, points for each neighbor pair, quest settings, unlock scores, camera and animation values. Change a value and press Play to try it.

You can check the core math (grid, scoring, groups, save data) from the Unity menu: `TinyTown > Kendini Test Et`. The result is written to the Console.

## Credits

- 3D models: [Hexagon Kit](https://kenney.nl/assets/hexagon-kit) by Kenney (CC0)
- Sound effects: [Impact Sounds](https://kenney.nl/assets/impact-sounds), [Interface Sounds](https://kenney.nl/assets/interface-sounds) and [Music Jingles](https://kenney.nl/assets/music-jingles) by Kenney (CC0)
- Font: [Poppins](https://fonts.google.com/specimen/Poppins) by Indian Type Foundry (SIL Open Font License 1.1)

A full list of the assets used is in `Assets/Resources/LICENSES.txt`.

## Author

Mert Sadık
