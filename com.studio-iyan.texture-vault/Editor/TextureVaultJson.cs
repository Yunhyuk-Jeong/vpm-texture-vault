using System;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace StudioIyan.TextureVault
{
    public static class TextureVaultJson
    {
        public static string Save(TextureVaultSnapshotData snapshot, string path)
        {
            if (snapshot == null)
            {
                throw new ArgumentNullException(nameof(snapshot));
            }

            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentException("Snapshot path is empty.", nameof(path));
            }

            string fullPath = ToFullPath(path);
            string directory = Path.GetDirectoryName(fullPath);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            string json = JsonUtility.ToJson(snapshot, true);
            File.WriteAllText(fullPath, json);
            AssetDatabase.Refresh();
            TextureVaultLogger.Log($"Saved snapshot JSON: {path}");
            return path;
        }

        public static TextureVaultSnapshotData Load(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentException("Snapshot path is empty.", nameof(path));
            }

            string fullPath = ToFullPath(path);
            if (!File.Exists(fullPath))
            {
                throw new FileNotFoundException("Snapshot JSON was not found.", fullPath);
            }

            string json = File.ReadAllText(fullPath);
            var snapshot = JsonUtility.FromJson<TextureVaultSnapshotData>(json);
            if (snapshot == null)
            {
                throw new InvalidDataException("Snapshot JSON could not be parsed.");
            }

            if (snapshot.textures == null)
            {
                snapshot.textures = new System.Collections.Generic.List<TextureVaultTextureRecord>();
            }

            TextureVaultLogger.Log($"Loaded snapshot JSON: {path}");
            return snapshot;
        }

        public static string BuildDefaultSnapshotAssetPath(string rootObjectName, DateTime utcNow)
        {
            string safeRootName = SanitizeFileName(string.IsNullOrEmpty(rootObjectName) ? "Root" : rootObjectName);
            string fileName = $"TextureVaultSnapshot_{safeRootName}_{utcNow:yyyyMMdd_HHmmss}.json";
            return $"{TextureVaultConstants.SnapshotFolder}/{fileName}";
        }

        public static string ToFullPath(string path)
        {
            if (Path.IsPathRooted(path))
            {
                return path;
            }

            return Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), path));
        }

        private static string SanitizeFileName(string value)
        {
            string invalidChars = Regex.Escape(new string(Path.GetInvalidFileNameChars()));
            string invalidPattern = $"[{invalidChars}]";
            string sanitized = Regex.Replace(value, invalidPattern, "_");
            sanitized = Regex.Replace(sanitized, "\\s+", "_");
            return string.IsNullOrWhiteSpace(sanitized) ? "Root" : sanitized;
        }
    }
}
