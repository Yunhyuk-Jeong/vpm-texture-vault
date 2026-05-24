using UnityEngine;

namespace StudioIyan.TextureVault
{
    internal static class TextureVaultLogger
    {
        public const string Prefix = "[Texture Vault]";

        public static void Log(string message)
        {
            Debug.Log($"{Prefix} {message}");
        }

        public static void Warning(string message)
        {
            Debug.LogWarning($"{Prefix} {message}");
        }

        public static void Error(string message)
        {
            Debug.LogError($"{Prefix} {message}");
        }
    }
}
