# Changelog

All notable changes to this mod are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/).

## [Unreleased]

### Fixed

- Double-clicking the dial toggled pinning on every rapid press after the first (a triple-click pinned and immediately unpinned again, and a quick press right after a double-click re-toggled). Only the second press of a burst now toggles.
- A drag interrupted by loss of mouse capture (for example, the game window losing focus) left the dial where it was dragged but did not save that position.
- Left presses and releases on a pinned dial are now consumed like every other click on it, instead of bubbling up through the game's UI.

## [1.0.1] - 2026-09-12

### Changed

- Verified compatible with Timberborn 1.1 (tested on 1.1.2.4). No code changes were needed: the mod's only game dependencies (`CameraService`, `UILayout`, the Bindito `Game`/`MapEditor` contexts, and the manifest format) are unchanged in 1.1, and the compass loads and renders correctly. Timberborn 1.0 remains supported (`MinimumGameVersion` is unchanged).

## [1.0] - 2026-08-20

### Added

- Initial release.
- HUD compass whose needle always points to true north, updating live as you pan, zoom, and rotate the camera.
- Drag anywhere on the dial to reposition it.
- Double-click to pin it in place.
- Position and pinned state persist between sessions.
- Visual style matched to the game's own UI theme.
