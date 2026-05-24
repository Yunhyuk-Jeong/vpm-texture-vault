using UnityEditor;

namespace StudioIyan.TextureVault
{
    internal enum TextureVaultLanguage
    {
        Japanese = 0,
        Korean = 1,
        English = 2
    }

    internal static class TextureVaultLocalization
    {
        private const string EditorPrefsKey = "StudioIyan.TextureVault.Language";
        public static readonly string[] ToolbarLabels = { "日本語", "한국어", "English" };

        public static TextureVaultLanguage Current
        {
            get
            {
                int savedValue = EditorPrefs.GetInt(EditorPrefsKey, (int)TextureVaultLanguage.English);
                if (savedValue < (int)TextureVaultLanguage.Japanese || savedValue > (int)TextureVaultLanguage.English)
                {
                    return TextureVaultLanguage.English;
                }

                return (TextureVaultLanguage)savedValue;
            }
            set => EditorPrefs.SetInt(EditorPrefsKey, (int)value);
        }

        public static int ToolbarIndex
        {
            get => (int)Current;
            set => Current = (TextureVaultLanguage)value;
        }

        public static string T(string key)
        {
            switch (Current)
            {
                case TextureVaultLanguage.Japanese:
                    return Japanese(key);
                case TextureVaultLanguage.Korean:
                    return Korean(key);
                default:
                    return English(key);
            }
        }

        private static string English(string key)
        {
            switch (key)
            {
                case "HeaderDescription": return "Save texture import settings, apply a temporary Standalone BC7 / Max Size 4096 profile, and restore the original settings.";
                case "VersionLabel": return "Version {0}";
                case "RootGameObject": return "Root GameObject";
                case "IncludeInactive": return "Include inactive children";
                case "SkipPackageCache": return "Skip PackageCache textures";
                case "SkipBuiltIn": return "Skip built-in or non-Assets textures";
                case "ShowConfirmation": return "Show confirmation before reimport";
                case "SerializedFallback": return "Use safe serialized material texture fallback";
                case "Scan": return "Scan";
                case "SaveSnapshot": return "Save Snapshot JSON";
                case "LoadSnapshot": return "Load Snapshot JSON";
                case "ApplyProfile": return "Apply BC7 / Max Size 4096";
                case "RestoreSnapshot": return "Restore From Snapshot";
                case "FoundRenderers": return "Found renderers";
                case "FoundMaterials": return "Found materials";
                case "FoundTextures": return "Found textures";
                case "SkippedTextures": return "Skipped textures";
                case "LastSnapshotPath": return "Last snapshot path";
                case "None": return "(none)";
                case "Results": return "Results";
                case "NoScanResults": return "No scan results yet.";
                case "Default": return "Default";
                case "AssetPath": return "Asset path";
                case "WidthHeight": return "Width x height";
                case "StandaloneMaxSize": return "Current Standalone max size";
                case "StandaloneFormat": return "Current Standalone format";
                case "UsedBy": return "Used by";
                case "StandaloneMaxShort": return "Standalone Max";
                case "FormatShort": return "Format";
                case "UsagesShort": return "Usages";
                case "SelectTextureTooltip": return "Select and ping this texture asset";
                case "AssignRootBeforeScan": return "Assign a root GameObject before scanning.";
                case "AssignRootFirst": return "Assign a root GameObject first.";
                case "SaveSnapshotTitle": return "Save Texture Vault Snapshot";
                case "SaveSnapshotPanelMessage": return "Choose where to save the Texture Vault snapshot JSON.";
                case "LoadSnapshotTitle": return "Load Texture Vault Snapshot";
                case "LoadSnapshotFailed": return "Failed to load snapshot JSON:";
                case "NoTexturesFound": return "No textures were found in the current scan.";
                case "NoSnapshotWarning": return "No snapshot has been saved for the current scan.";
                case "ReimportWarning": return "This will reimport {0} textures and modify Texture Import Settings. Source image files will not be edited.";
                case "SaveSnapshotFirst": return "Save Snapshot First";
                case "ApplyWithoutSnapshot": return "Apply Without Snapshot";
                case "Cancel": return "Cancel";
                case "ApplyConfirmBody": return "This will reimport {0} textures.\n\nThis modifies Texture Import Settings only. Source image files will not be edited.";
                case "ApplyFinished": return "Apply finished.\nApplied: {0}\nSkipped: {1}\nFailed: {2}";
                case "SnapshotEmpty": return "The loaded snapshot does not contain texture records.";
                case "RestoreTitle": return "Restore Import Settings";
                case "RestoreConfirmBody": return "This will reimport {0} textures from the loaded snapshot.\n\nThis modifies Texture Import Settings only. Source image files will not be edited.";
                case "RestoreFinished": return "Restore finished.\nRestored: {0}\nSkipped: {1}\nFailed: {2}";
                case "Ok": return "OK";
                case "NoUsageRecords": return "(no usage records)";
                default: return key;
            }
        }

        private static string Korean(string key)
        {
            switch (key)
            {
                case "HeaderDescription": return "텍스처 임포트 설정을 JSON으로 저장하고, 임시 Standalone BC7 / Max Size 4096 프로필을 적용한 뒤 원래 설정으로 복원합니다.";
                case "VersionLabel": return "버전 {0}";
                case "RootGameObject": return "루트 GameObject";
                case "IncludeInactive": return "비활성 자식 포함";
                case "SkipPackageCache": return "PackageCache 텍스처 건너뛰기";
                case "SkipBuiltIn": return "내장 또는 Assets 외부 텍스처 건너뛰기";
                case "ShowConfirmation": return "리임포트 전 확인 표시";
                case "SerializedFallback": return "안전한 직렬화 Material 텍스처 fallback 사용";
                case "Scan": return "스캔";
                case "SaveSnapshot": return "스냅샷 JSON 저장";
                case "LoadSnapshot": return "스냅샷 JSON 불러오기";
                case "ApplyProfile": return "BC7 / Max Size 4096 적용";
                case "RestoreSnapshot": return "스냅샷에서 복원";
                case "FoundRenderers": return "찾은 Renderer";
                case "FoundMaterials": return "찾은 Material";
                case "FoundTextures": return "찾은 Texture";
                case "SkippedTextures": return "건너뛴 Texture";
                case "LastSnapshotPath": return "마지막 스냅샷 경로";
                case "None": return "(없음)";
                case "Results": return "결과";
                case "NoScanResults": return "아직 스캔 결과가 없습니다.";
                case "Default": return "기본값";
                case "AssetPath": return "에셋 경로";
                case "WidthHeight": return "너비 x 높이";
                case "StandaloneMaxSize": return "현재 Standalone 최대 크기";
                case "StandaloneFormat": return "현재 Standalone 포맷";
                case "UsedBy": return "사용 위치";
                case "StandaloneMaxShort": return "Standalone 최대";
                case "FormatShort": return "포맷";
                case "UsagesShort": return "사용";
                case "SelectTextureTooltip": return "이 텍스처 에셋 선택 및 ping";
                case "AssignRootBeforeScan": return "스캔하기 전에 루트 GameObject를 지정하세요.";
                case "AssignRootFirst": return "먼저 루트 GameObject를 지정하세요.";
                case "SaveSnapshotTitle": return "Texture Vault 스냅샷 저장";
                case "SaveSnapshotPanelMessage": return "Texture Vault 스냅샷 JSON을 저장할 위치를 선택하세요.";
                case "LoadSnapshotTitle": return "Texture Vault 스냅샷 불러오기";
                case "LoadSnapshotFailed": return "스냅샷 JSON을 불러오지 못했습니다:";
                case "NoTexturesFound": return "현재 스캔에서 텍스처를 찾지 못했습니다.";
                case "NoSnapshotWarning": return "현재 스캔에 대해 저장된 스냅샷이 없습니다.";
                case "ReimportWarning": return "텍스처 {0}개를 리임포트하고 Texture Import Settings를 수정합니다. 원본 이미지 파일은 편집하지 않습니다.";
                case "SaveSnapshotFirst": return "먼저 스냅샷 저장";
                case "ApplyWithoutSnapshot": return "스냅샷 없이 적용";
                case "Cancel": return "취소";
                case "ApplyConfirmBody": return "텍스처 {0}개를 리임포트합니다.\n\nTexture Import Settings만 수정하며 원본 이미지 파일은 편집하지 않습니다.";
                case "ApplyFinished": return "적용 완료.\n적용: {0}\n건너뜀: {1}\n실패: {2}";
                case "SnapshotEmpty": return "불러온 스냅샷에 텍스처 기록이 없습니다.";
                case "RestoreTitle": return "임포트 설정 복원";
                case "RestoreConfirmBody": return "불러온 스냅샷에서 텍스처 {0}개를 리임포트합니다.\n\nTexture Import Settings만 수정하며 원본 이미지 파일은 편집하지 않습니다.";
                case "RestoreFinished": return "복원 완료.\n복원: {0}\n건너뜀: {1}\n실패: {2}";
                case "Ok": return "확인";
                case "NoUsageRecords": return "(사용 기록 없음)";
                default: return English(key);
            }
        }

        private static string Japanese(string key)
        {
            switch (key)
            {
                case "HeaderDescription": return "テクスチャのインポート設定をJSONに保存し、一時的なStandalone BC7 / Max Size 4096プロファイルを適用して、元の設定へ復元します。";
                case "VersionLabel": return "バージョン {0}";
                case "RootGameObject": return "ルートGameObject";
                case "IncludeInactive": return "非アクティブな子を含める";
                case "SkipPackageCache": return "PackageCacheのテクスチャをスキップ";
                case "SkipBuiltIn": return "ビルトインまたはAssets外のテクスチャをスキップ";
                case "ShowConfirmation": return "再インポート前に確認を表示";
                case "SerializedFallback": return "安全なシリアライズ済みMaterialテクスチャfallbackを使用";
                case "Scan": return "スキャン";
                case "SaveSnapshot": return "スナップショットJSONを保存";
                case "LoadSnapshot": return "スナップショットJSONを読み込み";
                case "ApplyProfile": return "BC7 / Max Size 4096を適用";
                case "RestoreSnapshot": return "スナップショットから復元";
                case "FoundRenderers": return "検出したRenderer";
                case "FoundMaterials": return "検出したMaterial";
                case "FoundTextures": return "検出したTexture";
                case "SkippedTextures": return "スキップしたTexture";
                case "LastSnapshotPath": return "最後のスナップショットパス";
                case "None": return "（なし）";
                case "Results": return "結果";
                case "NoScanResults": return "まだスキャン結果がありません。";
                case "Default": return "デフォルト";
                case "AssetPath": return "アセットパス";
                case "WidthHeight": return "幅 x 高さ";
                case "StandaloneMaxSize": return "現在のStandalone最大サイズ";
                case "StandaloneFormat": return "現在のStandaloneフォーマット";
                case "UsedBy": return "使用箇所";
                case "StandaloneMaxShort": return "Standalone最大";
                case "FormatShort": return "フォーマット";
                case "UsagesShort": return "使用数";
                case "SelectTextureTooltip": return "このテクスチャアセットを選択してping";
                case "AssignRootBeforeScan": return "スキャン前にルートGameObjectを指定してください。";
                case "AssignRootFirst": return "先にルートGameObjectを指定してください。";
                case "SaveSnapshotTitle": return "Texture Vaultスナップショットを保存";
                case "SaveSnapshotPanelMessage": return "Texture VaultスナップショットJSONの保存先を選択してください。";
                case "LoadSnapshotTitle": return "Texture Vaultスナップショットを読み込み";
                case "LoadSnapshotFailed": return "スナップショットJSONの読み込みに失敗しました:";
                case "NoTexturesFound": return "現在のスキャンでテクスチャが見つかりませんでした。";
                case "NoSnapshotWarning": return "現在のスキャンに対して保存されたスナップショットがありません。";
                case "ReimportWarning": return "{0}個のテクスチャを再インポートし、Texture Import Settingsを変更します。元の画像ファイルは編集されません。";
                case "SaveSnapshotFirst": return "先にスナップショットを保存";
                case "ApplyWithoutSnapshot": return "スナップショットなしで適用";
                case "Cancel": return "キャンセル";
                case "ApplyConfirmBody": return "{0}個のテクスチャを再インポートします。\n\nTexture Import Settingsのみを変更し、元の画像ファイルは編集されません。";
                case "ApplyFinished": return "適用完了。\n適用: {0}\nスキップ: {1}\n失敗: {2}";
                case "SnapshotEmpty": return "読み込んだスナップショットにテクスチャ記録がありません。";
                case "RestoreTitle": return "インポート設定を復元";
                case "RestoreConfirmBody": return "読み込んだスナップショットから{0}個のテクスチャを再インポートします。\n\nTexture Import Settingsのみを変更し、元の画像ファイルは編集されません。";
                case "RestoreFinished": return "復元完了。\n復元: {0}\nスキップ: {1}\n失敗: {2}";
                case "Ok": return "OK";
                case "NoUsageRecords": return "（使用記録なし）";
                default: return English(key);
            }
        }
    }
}
