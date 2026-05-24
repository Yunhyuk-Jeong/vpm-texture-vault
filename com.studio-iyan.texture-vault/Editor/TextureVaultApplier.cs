using System;
using System.Collections.Generic;
using UnityEditor;

namespace StudioIyan.TextureVault
{
    public static class TextureVaultApplier
    {
        public static TextureVaultOperationSummary ApplyBc7MaxSize4096(IReadOnlyList<TextureVaultScannedTexture> textures)
        {
            if (textures == null || textures.Count == 0)
            {
                TextureVaultLogger.Warning("No scanned textures to apply BC7 / Max Size 4096.");
                return new TextureVaultOperationSummary(0, 0, 0);
            }

            int applied = 0;
            int skipped = 0;
            int failed = 0;

            AssetDatabase.StartAssetEditing();
            try
            {
                for (int index = 0; index < textures.Count; index++)
                {
                    TextureVaultScannedTexture textureRecord = textures[index];
                    string path = textureRecord != null ? textureRecord.assetPath : string.Empty;
                    EditorUtility.DisplayProgressBar("Texture Vault", $"Applying BC7 / Max Size 4096: {path}", (float)index / textures.Count);

                    if (textureRecord == null || string.IsNullOrEmpty(path))
                    {
                        skipped++;
                        continue;
                    }

                    try
                    {
                        var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                        if (importer == null)
                        {
                            skipped++;
                            TextureVaultLogger.Warning($"Apply skipped non-texture importer: {path}");
                            continue;
                        }

                        if (importer.textureType == TextureImporterType.NormalMap)
                        {
                            TextureVaultLogger.Warning($"Applying BC7 to normal map for MVP profile: {path}");
                            // TODO: optional smart profile: NormalMap -> BC5, masks -> linear, albedo -> sRGB BC7.
                        }

                        TextureImporterPlatformSettings settings = importer.GetPlatformTextureSettings(TextureVaultConstants.StandalonePlatformName);
                        settings.name = TextureVaultConstants.StandalonePlatformName;
                        settings.overridden = true;
                        settings.maxTextureSize = 4096;
                        settings.format = TextureImporterFormat.BC7;
                        settings.textureCompression = TextureImporterCompression.CompressedHQ;
                        settings.compressionQuality = 100;
                        settings.crunchedCompression = false;

                        importer.SetPlatformTextureSettings(settings);
                        importer.SaveAndReimport();
                        applied++;
                        TextureVaultLogger.Log($"Applied BC7 / 4096 to {path}.");
                    }
                    catch (Exception exception)
                    {
                        failed++;
                        TextureVaultLogger.Error($"Apply failed for {path}: {exception.Message}");
                    }
                }
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
                EditorUtility.ClearProgressBar();
            }

            TextureVaultLogger.Log($"Apply BC7 / Max Size 4096 finished. applied={applied}, skipped={skipped}, failed={failed}");
            return new TextureVaultOperationSummary(applied, skipped, failed);
        }
    }
}
