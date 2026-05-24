using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace StudioIyan.TextureVault
{
    [Serializable]
    public sealed class TextureVaultSnapshotData
    {
        public int snapshotVersion = 1;
        public string toolVersion = TextureVaultConstants.ToolVersion;
        public string unityVersion;
        public string createdAtUtc;
        public string rootObjectName;
        public string rootObjectGlobalId;
        public string scenePath;
        public List<TextureVaultTextureRecord> textures = new List<TextureVaultTextureRecord>();
    }

    [Serializable]
    public sealed class TextureVaultTextureRecord
    {
        public string textureGuid;
        public string texturePath;
        public string textureName;
        public int width;
        public int height;
        public List<TextureVaultUsageRecord> usages = new List<TextureVaultUsageRecord>();
        public TextureVaultImporterSettings importerSettings;
        public List<TextureVaultPlatformSettings> platformSettings = new List<TextureVaultPlatformSettings>();
    }

    [Serializable]
    public sealed class TextureVaultUsageRecord
    {
        public string rendererPath;
        public string materialAssetPath;
        public string materialName;
        public string shaderName;
        public string texturePropertyName;
        public string texturePropertyDescription;
        public string source;
    }

    [Serializable]
    public sealed class TextureVaultImporterSettings
    {
        public int textureType;
        public int textureShape;
        public bool sRGBTexture;
        public int alphaSource;
        public bool alphaIsTransparency;
        public bool mipmapEnabled;
        public int mipmapFilter;
        public bool isReadable;
        public bool streamingMipmaps;
        public int npotScale;
        public int wrapMode;
        public int filterMode;
        public int anisoLevel;
        public int textureCompression;
        public int compressionQuality;
        public bool crunchedCompression;
    }

    [Serializable]
    public sealed class TextureVaultPlatformSettings
    {
        public string name;
        public bool overridden;
        public int maxTextureSize;
        public int resizeAlgorithm;
        public int format;
        public int textureCompression;
        public int compressionQuality;
        public bool crunchedCompression;
        public bool allowsAlphaSplitting;
        public int androidETC2FallbackOverride;
    }

    public sealed class TextureVaultScanOptions
    {
        public bool includeInactiveChildren = true;
        public bool skipPackageCacheTextures = true;
        public bool skipBuiltInOrNonAssetsTextures = true;
        public bool includeSerializedMaterialTextureFallback = true;
    }

    public sealed class TextureVaultScanResult
    {
        public GameObject rootObject;
        public int foundRenderers;
        public int foundMaterials;
        public int skippedTextures;
        public readonly List<TextureVaultScannedTexture> textures = new List<TextureVaultScannedTexture>();
    }

    public sealed class TextureVaultScannedTexture
    {
        public Texture2D texture;
        public string guid;
        public string assetPath;
        public string textureName;
        public int width;
        public int height;
        public TextureImporter importer;
        public TextureImporterPlatformSettings standaloneSettings;
        public readonly List<TextureVaultUsageRecord> usages = new List<TextureVaultUsageRecord>();
    }

    public readonly struct TextureVaultOperationSummary
    {
        public TextureVaultOperationSummary(int appliedOrRestored, int skipped, int failed)
        {
            AppliedOrRestored = appliedOrRestored;
            Skipped = skipped;
            Failed = failed;
        }

        public int AppliedOrRestored { get; }
        public int Skipped { get; }
        public int Failed { get; }
    }

    internal static class TextureVaultConstants
    {
        public const string ToolVersion = "1.0.2";
        public const string SnapshotFolder = "Assets/StudioIyan/TextureVault/Snapshots";
        public const string StandalonePlatformName = "Standalone";
        public static readonly string[] PreservedPlatformNames =
        {
            "DefaultTexturePlatform",
            "Standalone",
            "Android",
            "iPhone"
        };
    }
}
