# HorseRace MVP

This Unity MVP is a portrait mobile horse-race betting prototype built from selected local `Horsey Game` assets.

## Included Gameplay

- Four-horse short race card.
- Entertainment-chip betting only. No cash entry, withdrawal, ad monetization, or payment hook is included.
- Horse selection, stake controls, max bet, race start, animated race progress, winner detection, and payout settlement.
- Persistent local chip balance through `PlayerPrefs`.
- Bankruptcy recovery grants 200 entertainment chips so the prototype remains playable.

## Imported Horsey Game Assets

Source path used during setup:

`C:\Program Files (x86)\Steam\steamapps\common\Horsey Game`

Assets copied under `Assets/Resources/HorseyGame`:

- `sprites.png` / `sprites.xml`: race track, finish post, betting button, winning ticket, accessories.
- `furniture.png` / `furniture.xml`: small simulated horse sprite.
- `terrain.png` / `terrain.xml`: visual palette source for future track expansion.
- `locs.png` / `locs.xml`: race-track location icon.
- `biglogo2.png`: header branding art.
- `names.txt`: randomized horse names.
- Selected betting, race, hoof, win, and loss audio clips.

## TapTap-Oriented Notes

- Runtime forces `ScreenOrientation.Portrait`.
- Use `HorseRace/Apply TapTap Android Settings` in the Unity menu before Android packaging.
- Current package id is configured by that menu as `com.codexprototype.horseracemvp`.
- Keep the betting loop as fictional in-game chips unless product/legal review explicitly approves a different design.
