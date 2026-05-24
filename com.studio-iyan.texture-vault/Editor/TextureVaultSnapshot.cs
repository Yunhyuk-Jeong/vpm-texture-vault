using System;
using UnityEditor;
using UnityEngine;

namespace StudioIyan.TextureVault
{
    public static class TextureVaultSnapshot
    {
        public static TextureVaultSnapshotData Create(GameObject rootObject, TextureVaultScanResult scanResult)
        {
            var snapshot = new TextureVaultSnapshotData
            {
                snapshotVersion = 1,
                toolVersion = TextureVaultConstants.ToolVersion,
                unityVersion = Application.unityVersion,
                createdAtUtc = DateTime.UtcNow.ToString("o"),
                rootObjectName = rootObject != null ? rootObject.name : string.Empty,
                rootObjectGlobalId = TryGetGlobalObjectId(rootObject),
                scenePath = rootObject != null && rootObject.scene.IsValid() ? rootObject.scene.path : string.Empty
            };

            if (scanResult == null)
            {
                return snapshot;
            }

            foreach (TextureVaultScannedTexture scannedTexture in scanResult.textures)
            {
                if (scannedTexture == null || scannedTexture.importer == null)
                {
                    continue;
                }

                var record = new TextureVaultTextureRecord
                {
                    textureGuid = scannedTexture.guid,
                    texturePath = scannedTexture.assetPath,
                    textureName = scannedTexture.textureName,
                    width = scannedTexture.width,
                    height = scannedTexture.height,
                    importerSettings = CaptureImporterSettings(scannedTexture.importer)
                };

                record.usages.AddRange(scannedTexture.usages);

                foreach (string platformName in TextureVaultConstants.PreservedPlatformNames)
                {
                    record.platformSettings.Add(CapturePlatformSettings(scannedTexture.importer, platformName));
                }

                snapshot.textures.Add(record);
            }

            return snapshot;
        }

        public static TextureVaultImporterSettings CaptureImporterSettings(TextureImporter importer)
        {
            return new TextureVaultImporterSettings
            {
                textureType = (int)importer.textureType,
                textureShape = (int)importer.textureShape,
                sRGBTexture = importer.sRGBTexture,
                alphaSource = (int)importer.alphaSource,
                alphaIsTransparency = importer.alphaIsTransparency,
                mipmapEnabled = importer.mipmapEnabled,
                mipmapFilter = (int)importer.mipmapFilter,
                isReadable = importer.isReadable,
                streamingMipmaps = importer.streamingMipmaps,
                npotScale = (int)importer.npotScale,
                wrapMode = (int)importer.wrapMode,
                filterMode = (int)importer.filterMode,
                anisoLevel = importer.anisoLevel,
                textureCompression = (int)importer.textureCompression,
                compressionQuality = importer.compressionQuality,
                crunchedCompression = importer.crunchedCompression
            };
        }

        public static TextureVaultPlatformSettings CapturePlatformSettings(TextureImporter importer, string platformName)
        {
            TextureImporterPlatformSettings platformSettings = platformName == "DefaultTexturePlatform"
                ? importer.GetDefaultPlatformTextureSettings()
                : importer.GetPlatformTextureSettings(platformName);

            return FromUnityPlatformSettings(platformSettings, platformName);
        }

        public static TextureVaultPlatformSettings FromUnityPlatformSettings(TextureImporterPlatformSettings settings, string fallbackName)
        {
            return new TextureVaultPlatformSettings
            {
                name = string.IsNullOrEmpty(settings.name) ? fallbackName : settings.name,
                overridden = settings.overridden,
                maxTextureSize = settings.maxTextureSize,
                resizeAlgorithm = (int)settings.resizeAlgorithm,
                format = (int)settings.format,
                textureCompression = (int)settings.textureCompression,
                compressionQuality = settings.compressionQuality,
                crunchedCompression = settings.crunchedCompression,
                allowsAlphaSplitting = settings.allowsAlphaSplitting,
                androidETC2FallbackOverride = (int)settings.androidETC2FallbackOverride
            };
        }

        public static void ApplyImporterSettings(TextureImporter importer, TextureVaultImporterSettings settings)
        {
            if (importer == null || settings == null)
            {
                return;
            }

            importer.textureType = (TextureImporterType)settings.textureType;
            importer.textureShape = (TextureImporterShape)settings.textureShape;
            importer.sRGBTexture = settings.sRGBTexture;
            importer.alphaSource = (TextureImporterAlphaSource)settings.alphaSource;
            importer.alphaIsTransparency = settings.alphaIsTransparency;
            importer.mipmapEnabled = settings.mipmapEnabled;
            importer.mipmapFilter = (TextureImporterMipFilter)settings.mipmapFilter;
            importer.isReadable = settings.isReadable;
            importer.streamingMipmaps = settings.streamingMipmaps;
            importer.npotScale = (TextureImporterNPOTScale)settings.npotScale;
            importer.wrapMode = (TextureWrapMode)settings.wrapMode;
            importer.filterMode = (FilterMode)settings.filterMode;
            importer.anisoLevel = settings.anisoLevel;
            importer.textureCompression = (TextureImporterCompression)settings.textureCompression;
            importer.compressionQuality = settings.compressionQuality;
            importer.crunchedCompression = settings.crunchedCompression;
        }

        public static TextureImporterPlatformSettings ToUnityPlatformSettings(TextureVaultPlatformSettings savedSettings)
        {
            var settings = new TextureImporterPlatformSettings
            {
                name = savedSettings.name,
                overridden = savedSettings.overridden,
                maxTextureSize = savedSettings.maxTextureSize,
                resizeAlgorithm = (TextureResizeAlgorithm)savedSettings.resizeAlgorithm,
                format = (TextureImporterFormat)savedSettings.format,
                textureCompression = (TextureImporterCompression)savedSettings.textureCompression,
                compressionQuality = savedSettings.compressionQuality,
                crunchedCompression = savedSettings.crunchedCompression,
                allowsAlphaSplitting = savedSettings.allowsAlphaSplitting,
                androidETC2FallbackOverride = (AndroidETC2FallbackOverride)savedSettings.androidETC2FallbackOverride
            };

            return settings;
        }

        private static string TryGetGlobalObjectId(UnityEngine.Object target)
        {
            if (target == null)
            {
                return string.Empty;
            }

            try
            {
                return GlobalObjectId.GetGlobalObjectIdSlow(target).ToString();
            }
            catch (Exception exception)
            {
                TextureVaultLogger.Warning($"Could not capture root object GlobalObjectId: {exception.Message}");
                return string.Empty;
            }
        }
    }
}
