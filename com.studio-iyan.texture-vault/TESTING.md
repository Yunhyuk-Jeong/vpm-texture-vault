# Testing Checklist

## Unity Tests

- Open a Unity 2022.3 project.
- Add the package through a local folder in VCC or Unity Package Manager.
- Open `Studio Iyan/Tools/Texture Vault`.
- Select an avatar or root GameObject.
- Click `Scan`.
- Confirm materials and textures are listed.
- Click `Save Snapshot JSON`.
- Click `Apply BC7 / Max Size 4096`.
- Check Texture Import Settings on a texture and confirm Standalone override uses BC7 and Max Size 4096.
- Click `Restore From Snapshot`.
- Check that the original import settings are restored.

## GitHub Tests

- Push a normal commit to `main`; no release should be created.
- Push tag `v1.0.0`; release zip should be created.
- Push a tag that does not match `package.json` version; the release workflow should fail.
- Confirm VPM listing `index.json` includes the package version.
- Add the listing URL to VCC and install the package.
