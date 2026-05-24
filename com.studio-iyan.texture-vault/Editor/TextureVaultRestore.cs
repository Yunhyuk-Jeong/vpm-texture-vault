using System;
using UnityEditor;

namespace StudioIyan.TextureVault
{
    public static class TextureVaultRestore
    {
        public static TextureVaultOperationSummary RestoreFromSnapshot(TextureVaultSnapshotData snapshot)
        {
            if (snapshot == null || snapshot.textures == null || snapshot.textures.Count == 0)
            {
                TextureVaultLogger.Warning("No snapshot textures to restore.");
                return new TextureVaultOperationSummary(0, 0, 0);
            }

            int restored = 0;
            int skipped = 0;
            int failed = 0;

            AssetDatabase.StartAssetEditing();
            try
            {
                for (int index = 0; index < snapshot.textures.Count; index++)
                {
                    TextureVaultTextureRecord record = snapshot.textures[index];
                    string displayPath = record != null ? record.texturePath : string.Empty;
                    EditorUtility.DisplayProgressBar("Texture Vault", $"Restoring import settings: {displayPath}", (float)index / snapshot.textures.Count);

                    if (record == null)
                    {
                        skipped++;
                        continue;
                    }

                    try
                    {
                        string path = ResolveTexturePath(record);
                        if (string.IsNullOrEmpty(path))
                        {
                            skipped++;
                            TextureVaultLogger.Warning($"Restore skipped missing texture GUID: {record.textureGuid}");
                            continue;
                        }

                        var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                        if (importer == null)
                        {
                            skipped++;
                            TextureVaultLogger.Warning($"Restore skipped non-texture importer: {path}");
                            continue;
                        }

                        TextureVaultSnapshot.ApplyImporterSettings(importer, record.importerSettings);

                        if (record.platformSettings != null)
                        {
                            foreach (TextureVaultPlatformSettings savedPlatformSettings in record.platformSettings)
                            {
                                if (savedPlatformSettings == null || string.IsNullOrEmpty(savedPlatformSettings.name))
                                {
                                    continue;
                                }

                                importer.SetPlatformTextureSettings(TextureVaultSnapshot.ToUnityPlatformSettings(savedPlatformSettings));
                            }
                        }

                        importer.SaveAndReimport();
                        restored++;
                        TextureVaultLogger.Log($"Restored import settings for {path}.");
                    }
                    catch (Exception exception)
                    {
                        failed++;
                        TextureVaultLogger.Error($"Restore failed for {displayPath}: {exception.Message}");
                    }
                }
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
                EditorUtility.ClearProgressBar();
            }

            TextureVaultLogger.Log($"Restore finished. restored={restored}, skipped={skipped}, failed={failed}");
            return new TextureVaultOperationSummary(restored, skipped, failed);
        }

        private static string ResolveTexturePath(TextureVaultTextureRecord record)
        {
            if (!string.IsNullOrEmpty(record.textureGuid))
            {
                string guidPath = AssetDatabase.GUIDToAssetPath(record.textureGuid);
                if (!string.IsNullOrEmpty(guidPath))
                {
                    return guidPath;
                }
            }

            if (!string.IsNullOrEmpty(record.texturePath) && AssetDatabase.LoadAssetAtPath<UnityEngine.Texture2D>(record.texturePath) != null)
            {
                return record.texturePath;
            }

            return string.Empty;
        }
    }
}
