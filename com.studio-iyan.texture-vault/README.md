# Studio Iyan Texture Vault

Studio Iyan Texture Vault is a Unity Editor-only tool for saving Texture Import Settings to JSON, applying a temporary Standalone BC7 / Max Size 4096 import profile, and restoring the original settings later.

This package is intended for Unity 2022.3 or newer and does not require runtime code.

## What It Does

- Scans a selected root GameObject and all child Renderers.
- Finds Texture2D assets used by Material shader texture properties.
- Uses a safe serialized Material fallback for saved texture properties that are not exposed by the current shader.
- Deduplicates textures by asset GUID.
- Saves original TextureImporter and platform settings to JSON.
- Applies a temporary Standalone BC7 / Max Size 4096 profile.
- Restores Texture Import Settings from a saved JSON snapshot.

## What It Does Not Do

- This is not a texture upscaler.
- It does not convert source images to true 4K.
- It does not edit, duplicate, move, or destroy source image files.
- It only changes Unity Texture Import Settings, especially platform override settings.

## Installation

### VCC / VPM

1. Open VCC.
2. Add the VPM listing URL:
   `https://Yunhyuk-Jeong.github.io/vpm-texture-vault/index.json`
3. Add `Studio Iyan Texture Vault` to your Unity project.

### Manual Install

1. Download the release zip from GitHub Releases.
2. Extract the package folder into your Unity project's `Packages` folder, or add it through Unity Package Manager from disk.
3. Open the Unity project.

## Usage

1. Open `Studio Iyan/Tools/Texture Vault`.
2. Assign a root GameObject.
3. Click `Scan`.
4. Click `Save Snapshot JSON`.
5. Click `Apply BC7 / Max Size 4096`.
6. Click `Restore From Snapshot` when you need to restore the original import settings.

## Warnings

- BC7 is intended for Standalone/PC use.
- This does not upscale textures.
- Quest/Android should use a separate profile in the future.
- For the MVP, normal maps can still receive the BC7 profile, but the tool logs a warning. A future smart profile can map normal maps to BC5, masks to linear formats, and albedo textures to sRGB BC7.

## Release

1. Update `package.json` version.
2. Update `CHANGELOG.md`.
3. Commit the changes.
4. Create a tag like `v1.0.0`.
5. Push the tag.

The release workflow validates that the tag version matches `package.json`, builds a clean VPM/UPM-compatible zip, generates a SHA256 checksum, and uploads both as GitHub Release assets.

## VPM Listing

This repository uses the same-repository listing approach. The `build-vpm-listing.yml` workflow generates `public/index.json`, preserves old versions from the existing published listing when available, adds the current package version, and deploys it to GitHub Pages.

Final VPM URL:

`https://Yunhyuk-Jeong.github.io/vpm-texture-vault/index.json`

## Testing

See [TESTING.md](TESTING.md).

## Changelog

See [CHANGELOG.md](CHANGELOG.md).

## License

MIT. See [LICENSE](LICENSE).
