using System;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace StudioIyan.TextureVault
{
    public sealed class TextureVaultWindow : EditorWindow
    {
        private GameObject _rootObject;
        private bool _includeInactiveChildren = true;
        private bool _skipPackageCacheTextures = true;
        private bool _skipBuiltInOrNonAssetsTextures = true;
        private bool _showConfirmationBeforeReimport = true;
        private bool _includeSerializedFallback = true;

        private TextureVaultScanResult _scanResult;
        private TextureVaultSnapshotData _loadedSnapshot;
        private string _lastSnapshotPath = string.Empty;
        private bool _snapshotSavedForCurrentScan;
        private Vector2 _scrollPosition;

        [MenuItem("Studio Iyan/Tools/Texture Vault")]
        public static void Open()
        {
            var window = GetWindow<TextureVaultWindow>("Texture Vault");
            window.minSize = new Vector2(780, 480);
            window.Show();
        }

        private void OnGUI()
        {
            DrawHeader();
            DrawRootAndOptions();
            DrawActions();
            DrawSummary();
            DrawResultTable();
        }

        private void DrawHeader()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                using (new EditorGUILayout.VerticalScope())
                {
                    EditorGUILayout.LabelField("Studio Iyan Texture Vault", EditorStyles.boldLabel);
                    EditorGUILayout.LabelField(string.Format(TextureVaultLocalization.T("VersionLabel"), TextureVaultConstants.ToolVersion), EditorStyles.miniLabel);
                }

                GUILayout.FlexibleSpace();
                DrawLanguageSelector();
            }

            EditorGUILayout.LabelField(TextureVaultLocalization.T("HeaderDescription"), EditorStyles.wordWrappedMiniLabel);
            EditorGUILayout.Space(8);
        }

        private void DrawLanguageSelector()
        {
            int previousIndex = TextureVaultLocalization.ToolbarIndex;
            int selectedIndex = GUILayout.Toolbar(previousIndex, TextureVaultLocalization.ToolbarLabels, EditorStyles.miniButton, GUILayout.Width(180));
            if (selectedIndex != previousIndex)
            {
                TextureVaultLocalization.ToolbarIndex = selectedIndex;
                Repaint();
            }
        }

        private void DrawRootAndOptions()
        {
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                var newRootObject = (GameObject)EditorGUILayout.ObjectField(TextureVaultLocalization.T("RootGameObject"), _rootObject, typeof(GameObject), true);
                if (newRootObject != _rootObject)
                {
                    _rootObject = newRootObject;
                    _scanResult = null;
                    _snapshotSavedForCurrentScan = false;
                }
                _includeInactiveChildren = EditorGUILayout.ToggleLeft(TextureVaultLocalization.T("IncludeInactive"), _includeInactiveChildren);
                _skipPackageCacheTextures = EditorGUILayout.ToggleLeft(TextureVaultLocalization.T("SkipPackageCache"), _skipPackageCacheTextures);
                _skipBuiltInOrNonAssetsTextures = EditorGUILayout.ToggleLeft(TextureVaultLocalization.T("SkipBuiltIn"), _skipBuiltInOrNonAssetsTextures);
                _showConfirmationBeforeReimport = EditorGUILayout.ToggleLeft(TextureVaultLocalization.T("ShowConfirmation"), _showConfirmationBeforeReimport);
                _includeSerializedFallback = EditorGUILayout.ToggleLeft(TextureVaultLocalization.T("SerializedFallback"), _includeSerializedFallback);
            }
        }

        private void DrawActions()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button(TextureVaultLocalization.T("Scan"), GUILayout.Height(28)))
                {
                    Scan();
                }

                if (GUILayout.Button(TextureVaultLocalization.T("SaveSnapshot"), GUILayout.Height(28)))
                {
                    SaveSnapshotWithDialog();
                }

                if (GUILayout.Button(TextureVaultLocalization.T("LoadSnapshot"), GUILayout.Height(28)))
                {
                    LoadSnapshotWithDialog();
                }
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button(TextureVaultLocalization.T("ApplyProfile"), GUILayout.Height(30)))
                {
                    ApplyProfile();
                }

                if (GUILayout.Button(TextureVaultLocalization.T("RestoreSnapshot"), GUILayout.Height(30)))
                {
                    RestoreFromSnapshot();
                }
            }
        }

        private void DrawSummary()
        {
            int foundTextures = _scanResult != null ? _scanResult.textures.Count : 0;
            int foundRenderers = _scanResult != null ? _scanResult.foundRenderers : 0;
            int foundMaterials = _scanResult != null ? _scanResult.foundMaterials : 0;
            int skippedTextures = _scanResult != null ? _scanResult.skippedTextures : 0;

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField(TextureVaultLocalization.T("FoundRenderers"), foundRenderers.ToString());
                EditorGUILayout.LabelField(TextureVaultLocalization.T("FoundMaterials"), foundMaterials.ToString());
                EditorGUILayout.LabelField(TextureVaultLocalization.T("FoundTextures"), foundTextures.ToString());
                EditorGUILayout.LabelField(TextureVaultLocalization.T("SkippedTextures"), skippedTextures.ToString());
                EditorGUILayout.LabelField(TextureVaultLocalization.T("LastSnapshotPath"), string.IsNullOrEmpty(_lastSnapshotPath) ? TextureVaultLocalization.T("None") : _lastSnapshotPath);
            }
        }

        private void DrawResultTable()
        {
            EditorGUILayout.LabelField(TextureVaultLocalization.T("Results"), EditorStyles.boldLabel);
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

            if (_scanResult == null || _scanResult.textures.Count == 0)
            {
                EditorGUILayout.HelpBox(TextureVaultLocalization.T("NoScanResults"), MessageType.Info);
                EditorGUILayout.EndScrollView();
                return;
            }

            foreach (TextureVaultScannedTexture texture in _scanResult.textures)
            {
                DrawTextureResultRow(texture);
            }

            EditorGUILayout.EndScrollView();
        }

        private static void DrawTextureResultRow(TextureVaultScannedTexture texture)
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox, GUILayout.MinHeight(56)))
            {
                DrawTexturePreviewButton(texture);

                using (new EditorGUILayout.VerticalScope())
                {
                    EditorGUILayout.LabelField(texture.textureName, EditorStyles.boldLabel);
                    EditorGUILayout.LabelField(BuildCurrentInfoLine(texture), EditorStyles.miniLabel);
                    EditorGUILayout.LabelField(BuildUsageSummary(texture), EditorStyles.miniLabel);
                }
            }
        }

        private static void DrawTexturePreviewButton(TextureVaultScannedTexture texture)
        {
            Texture previewTexture = null;
            if (texture != null && texture.texture != null)
            {
                previewTexture = AssetPreview.GetAssetPreview(texture.texture);
                if (previewTexture == null)
                {
                    previewTexture = AssetPreview.GetMiniThumbnail(texture.texture);
                }
            }

            var content = new GUIContent(previewTexture, TextureVaultLocalization.T("SelectTextureTooltip"));
            if (GUILayout.Button(content, GUILayout.Width(48), GUILayout.Height(48)) && texture != null && texture.texture != null)
            {
                Selection.activeObject = texture.texture;
                EditorGUIUtility.PingObject(texture.texture);
            }
        }

        private static string BuildCurrentInfoLine(TextureVaultScannedTexture texture)
        {
            string formatLabel = texture.standaloneSettings.overridden
                ? texture.standaloneSettings.format.ToString()
                : $"{TextureVaultLocalization.T("Default")} ({texture.standaloneSettings.format})";
            string sizeLabel = texture.standaloneSettings.overridden
                ? texture.standaloneSettings.maxTextureSize.ToString()
                : $"{TextureVaultLocalization.T("Default")} ({texture.standaloneSettings.maxTextureSize})";
            int usageCount = texture.usages != null ? texture.usages.Count : 0;

            return $"{texture.assetPath} | {texture.width} x {texture.height} | {TextureVaultLocalization.T("StandaloneMaxShort")}: {sizeLabel} | {TextureVaultLocalization.T("FormatShort")}: {formatLabel} | {TextureVaultLocalization.T("UsagesShort")}: {usageCount}";
        }

        private void Scan()
        {
            if (_rootObject == null)
            {
                EditorUtility.DisplayDialog("Texture Vault", TextureVaultLocalization.T("AssignRootBeforeScan"), TextureVaultLocalization.T("Ok"));
                return;
            }

            var options = new TextureVaultScanOptions
            {
                includeInactiveChildren = _includeInactiveChildren,
                skipPackageCacheTextures = _skipPackageCacheTextures,
                skipBuiltInOrNonAssetsTextures = _skipBuiltInOrNonAssetsTextures,
                includeSerializedMaterialTextureFallback = _includeSerializedFallback
            };

            _scanResult = TextureVaultScanner.Scan(_rootObject, options);
            _snapshotSavedForCurrentScan = false;
            Repaint();
        }

        private bool SaveSnapshotWithDialog()
        {
            if (!EnsureScanExists())
            {
                return false;
            }

            string defaultPath = TextureVaultJson.BuildDefaultSnapshotAssetPath(_rootObject.name, DateTime.UtcNow);
            EnsureSnapshotFolderExists();

            string savePath = EditorUtility.SaveFilePanelInProject(
                TextureVaultLocalization.T("SaveSnapshotTitle"),
                Path.GetFileName(defaultPath),
                "json",
                TextureVaultLocalization.T("SaveSnapshotPanelMessage"),
                TextureVaultConstants.SnapshotFolder);

            if (string.IsNullOrEmpty(savePath))
            {
                return false;
            }

            return SaveSnapshot(savePath);
        }

        private bool SaveSnapshot(string assetPath)
        {
            var snapshot = TextureVaultSnapshot.Create(_rootObject, _scanResult);
            TextureVaultJson.Save(snapshot, assetPath);
            _loadedSnapshot = snapshot;
            _lastSnapshotPath = assetPath;
            _snapshotSavedForCurrentScan = true;
            return true;
        }

        private void LoadSnapshotWithDialog()
        {
            string selectedPath = EditorUtility.OpenFilePanel(TextureVaultLocalization.T("LoadSnapshotTitle"), "Assets", "json");
            if (string.IsNullOrEmpty(selectedPath))
            {
                return;
            }

            string projectPath = Directory.GetCurrentDirectory().Replace("\\", "/");
            string normalizedSelectedPath = selectedPath.Replace("\\", "/");
            string assetPath = normalizedSelectedPath.StartsWith(projectPath, StringComparison.OrdinalIgnoreCase)
                ? normalizedSelectedPath.Substring(projectPath.Length).TrimStart('/')
                : selectedPath;

            try
            {
                _loadedSnapshot = TextureVaultJson.Load(assetPath);
                _lastSnapshotPath = assetPath;
                TextureVaultLogger.Log($"Snapshot loaded explicitly for restore: {assetPath}");
            }
            catch (Exception exception)
            {
                EditorUtility.DisplayDialog("Texture Vault", $"{TextureVaultLocalization.T("LoadSnapshotFailed")}\n{exception.Message}", TextureVaultLocalization.T("Ok"));
                TextureVaultLogger.Error($"Failed to load snapshot JSON: {exception.Message}");
            }
        }

        private void ApplyProfile()
        {
            if (!EnsureScanExists())
            {
                return;
            }

            int textureCount = _scanResult.textures.Count;
            if (textureCount == 0)
            {
                EditorUtility.DisplayDialog("Texture Vault", TextureVaultLocalization.T("NoTexturesFound"), TextureVaultLocalization.T("Ok"));
                return;
            }

            if (!_snapshotSavedForCurrentScan)
            {
                int choice = EditorUtility.DisplayDialogComplex(
                    "Texture Vault",
                    $"{TextureVaultLocalization.T("NoSnapshotWarning")}\n\n{string.Format(TextureVaultLocalization.T("ReimportWarning"), textureCount)}",
                    TextureVaultLocalization.T("SaveSnapshotFirst"),
                    TextureVaultLocalization.T("ApplyWithoutSnapshot"),
                    TextureVaultLocalization.T("Cancel"));

                if (choice == 0)
                {
                    string defaultPath = TextureVaultJson.BuildDefaultSnapshotAssetPath(_rootObject.name, DateTime.UtcNow);
                    EnsureSnapshotFolderExists();
                    if (!SaveSnapshot(defaultPath))
                    {
                        return;
                    }
                }
                else if (choice == 1)
                {
                    TextureVaultLogger.Warning("User explicitly chose to apply BC7 / Max Size 4096 without a current snapshot.");
                }
                else
                {
                    return;
                }
            }

            if (_showConfirmationBeforeReimport &&
                !EditorUtility.DisplayDialog(
                    "Apply BC7 / Max Size 4096",
                    string.Format(TextureVaultLocalization.T("ApplyConfirmBody"), textureCount),
                    TextureVaultLocalization.T("ApplyProfile"),
                    TextureVaultLocalization.T("Cancel")))
            {
                return;
            }

            TextureVaultOperationSummary summary = TextureVaultApplier.ApplyBc7MaxSize4096(_scanResult.textures);
            EditorUtility.DisplayDialog("Texture Vault", string.Format(TextureVaultLocalization.T("ApplyFinished"), summary.AppliedOrRestored, summary.Skipped, summary.Failed), TextureVaultLocalization.T("Ok"));
            Scan();
        }

        private void RestoreFromSnapshot()
        {
            if (_loadedSnapshot == null)
            {
                LoadSnapshotWithDialog();
                if (_loadedSnapshot == null)
                {
                    return;
                }
            }

            int textureCount = _loadedSnapshot.textures != null ? _loadedSnapshot.textures.Count : 0;
            if (textureCount == 0)
            {
                EditorUtility.DisplayDialog("Texture Vault", TextureVaultLocalization.T("SnapshotEmpty"), TextureVaultLocalization.T("Ok"));
                return;
            }

            if (_showConfirmationBeforeReimport &&
                !EditorUtility.DisplayDialog(
                    TextureVaultLocalization.T("RestoreTitle"),
                    string.Format(TextureVaultLocalization.T("RestoreConfirmBody"), textureCount),
                    TextureVaultLocalization.T("RestoreTitle"),
                    TextureVaultLocalization.T("Cancel")))
            {
                return;
            }

            TextureVaultOperationSummary summary = TextureVaultRestore.RestoreFromSnapshot(_loadedSnapshot);
            EditorUtility.DisplayDialog("Texture Vault", string.Format(TextureVaultLocalization.T("RestoreFinished"), summary.AppliedOrRestored, summary.Skipped, summary.Failed), TextureVaultLocalization.T("Ok"));
            if (_rootObject != null)
            {
                Scan();
            }
        }

        private bool EnsureScanExists()
        {
            if (_scanResult != null)
            {
                return true;
            }

            if (_rootObject == null)
            {
                EditorUtility.DisplayDialog("Texture Vault", TextureVaultLocalization.T("AssignRootFirst"), TextureVaultLocalization.T("Ok"));
                return false;
            }

            Scan();
            return _scanResult != null;
        }

        private static void EnsureSnapshotFolderExists()
        {
            string fullPath = TextureVaultJson.ToFullPath(TextureVaultConstants.SnapshotFolder);
            Directory.CreateDirectory(fullPath);
            AssetDatabase.Refresh();
        }

        private static string BuildUsageSummary(TextureVaultScannedTexture texture)
        {
            if (texture == null || texture.usages == null || texture.usages.Count == 0)
            {
                return TextureVaultLocalization.T("NoUsageRecords");
            }

            var builder = new StringBuilder();
            for (int index = 0; index < texture.usages.Count; index++)
            {
                TextureVaultUsageRecord usage = texture.usages[index];
                if (index > 0)
                {
                    builder.AppendLine();
                }

                builder.Append(usage.rendererPath);
                builder.Append(" | ");
                builder.Append(usage.materialName);
                builder.Append(" | ");
                builder.Append(usage.texturePropertyName);
                if (!string.IsNullOrEmpty(usage.texturePropertyDescription) && usage.texturePropertyDescription != usage.texturePropertyName)
                {
                    builder.Append(" (");
                    builder.Append(usage.texturePropertyDescription);
                    builder.Append(")");
                }
            }

            return builder.ToString();
        }
    }
}
