using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;
using TMPro;
using CardBattle.Conversation;

namespace CardBattle.Editor
{
    /// <summary>
    /// ノベル会話シーン用のセットアップ。Editor メニューから実行し、ConversationManager と UI（台詞・立ち絵用）を一括配置する。
    /// </summary>
    public static class ConversationSceneSetupEditor
    {
        private const string SetupRootName = "Conversation_SetupRoot";
        private const string PrefabsPath = "Assets/Prefabs";

        [MenuItem("Window/CardBattle/Setup Conversation Scene")]
        public static void SetupConversationScene()
        {
            var root = EnsureRootClean();
            if (root == null) return;

            var canvasData = CreateConversationCanvas(root);
            var actorPrefab = GetOrCreateStandingPictureActorPrefab();
            var managerGo = CreateConversationManager(root);

            WireConversationManager(managerGo.GetComponent<ConversationManager>(), canvasData, actorPrefab);
            EnsureCameraAndEventSystem(root);
            CreateSampleScenario(root);

            EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        }

        private static void CreateSampleScenario(GameObject root)
        {
            var go = new GameObject("SampleScenario");
            Undo.RegisterCreatedObjectUndo(go, "Create SampleScenario");
            go.transform.SetParent(root.transform, false);
            go.AddComponent<SampleConversationScenario>();
        }

        private static GameObject EnsureRootClean()
        {
            var existing = GameObject.Find(SetupRootName);
            if (existing != null)
                Undo.DestroyObjectImmediate(existing);

            var root = new GameObject(SetupRootName);
            Undo.RegisterCreatedObjectUndo(root, "Conversation Scene Setup");
            return root;
        }

        private struct ConversationCanvasData
        {
            public TextMeshProUGUI SpeakerNameText;
            public TextMeshProUGUI BodyText;
            public Transform ActorRoot;
            public Image BackgroundImage;
        }

        private static ConversationCanvasData CreateConversationCanvas(GameObject root)
        {
            var canvasGo = new GameObject("Canvas");
            Undo.RegisterCreatedObjectUndo(canvasGo, "Create Conversation Canvas");
            canvasGo.transform.SetParent(root.transform, false);

            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280, 720);
            canvasGo.AddComponent<GraphicRaycaster>();

            // 背景（最背面・画面全体）
            var backgroundGo = new GameObject("BackgroundImage");
            backgroundGo.transform.SetParent(canvasGo.transform, false);
            var backgroundRect = backgroundGo.AddComponent<RectTransform>();
            backgroundRect.anchorMin = Vector2.zero;
            backgroundRect.anchorMax = Vector2.one;
            backgroundRect.offsetMin = Vector2.zero;
            backgroundRect.offsetMax = Vector2.zero;
            var backgroundImg = backgroundGo.AddComponent<Image>();
            backgroundImg.color = Color.white;
            backgroundImg.raycastTarget = false;
            backgroundImg.sprite = null;
            backgroundImg.enabled = false;
            backgroundGo.transform.SetAsFirstSibling();

            // 立ち絵を置く親
            var actorRootGo = new GameObject("ActorRoot");
            actorRootGo.transform.SetParent(canvasGo.transform, false);
            var actorRootRect = actorRootGo.AddComponent<RectTransform>();
            actorRootRect.anchorMin = new Vector2(0f, 0f);
            actorRootRect.anchorMax = new Vector2(0.5f, 1f);
            actorRootRect.offsetMin = new Vector2(20, 20);
            actorRootRect.offsetMax = new Vector2(-20, -20);
            actorRootRect.pivot = new Vector2(0.5f, 0f);

            // 台詞エリア（下段）
            var dialoguePanelGo = new GameObject("DialoguePanel");
            dialoguePanelGo.transform.SetParent(canvasGo.transform, false);
            var dialoguePanelRect = dialoguePanelGo.AddComponent<RectTransform>();
            dialoguePanelRect.anchorMin = new Vector2(0f, 0f);
            dialoguePanelRect.anchorMax = new Vector2(1f, 0.28f);
            dialoguePanelRect.offsetMin = new Vector2(24, 12);
            dialoguePanelRect.offsetMax = new Vector2(-24, 24);
            var panelImage = dialoguePanelGo.AddComponent<Image>();
            panelImage.color = new Color(0.12f, 0.12f, 0.18f, 0.92f);

            // 話者名
            var speakerGo = new GameObject("SpeakerNameText");
            speakerGo.transform.SetParent(dialoguePanelGo.transform, false);
            var speakerRect = speakerGo.AddComponent<RectTransform>();
            speakerRect.anchorMin = new Vector2(0f, 0.75f);
            speakerRect.anchorMax = new Vector2(1f, 1f);
            speakerRect.offsetMin = new Vector2(30, 4);
            speakerRect.offsetMax = new Vector2(-30, -4);
            var speakerTmp = speakerGo.AddComponent<TextMeshProUGUI>();
            ApplyDefaultTmpStyle(speakerTmp);
            speakerTmp.text = "";
            speakerTmp.fontSize = 40;
            speakerTmp.color = new Color(1f, 0.95f, 0.85f);
            speakerTmp.alignment = TMPro.TextAlignmentOptions.MidlineLeft;

            // 本文
            var bodyGo = new GameObject("BodyText");
            bodyGo.transform.SetParent(dialoguePanelGo.transform, false);
            var bodyRect = bodyGo.AddComponent<RectTransform>();
            bodyRect.anchorMin = new Vector2(0f, 0f);
            bodyRect.anchorMax = new Vector2(1f, 0.72f);
            bodyRect.offsetMin = new Vector2(30, 4);
            bodyRect.offsetMax = new Vector2(-30, -4);
            var bodyTmp = bodyGo.AddComponent<TextMeshProUGUI>();
            ApplyDefaultTmpStyle(bodyTmp);
            bodyTmp.text = "";
            bodyTmp.fontSize = 35;
            bodyTmp.overflowMode = TextOverflowModes.Overflow;
            bodyTmp.textWrappingMode = TextWrappingModes.Normal;

            return new ConversationCanvasData
            {
                SpeakerNameText = speakerTmp,
                BodyText = bodyTmp,
                ActorRoot = actorRootGo.transform,
                BackgroundImage = backgroundImg
            };
        }

        private static void ApplyDefaultTmpStyle(TextMeshProUGUI tmp)
        {
            var fontAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/NotoSansJP-Medium SDF.asset");
            if (fontAsset != null)
                tmp.font = fontAsset;
        }

        private static GameObject CreateConversationManager(GameObject root)
        {
            var go = new GameObject("ConversationManager");
            Undo.RegisterCreatedObjectUndo(go, "Create ConversationManager");
            go.transform.SetParent(root.transform, false);
            go.AddComponent<ConversationManager>();
            return go;
        }

        private static void WireConversationManager(ConversationManager manager, ConversationCanvasData canvasData, StandingPictureActor actorPrefab)
        {
            if (manager == null) return;
            var so = new SerializedObject(manager);
            so.FindProperty("speakerNameText").objectReferenceValue = canvasData.SpeakerNameText;
            so.FindProperty("bodyText").objectReferenceValue = canvasData.BodyText;
            so.FindProperty("actorRoot").objectReferenceValue = canvasData.ActorRoot;
            so.FindProperty("actorPrefab").objectReferenceValue = actorPrefab;
            so.FindProperty("backgroundImage").objectReferenceValue = canvasData.BackgroundImage;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static StandingPictureActor GetOrCreateStandingPictureActorPrefab()
        {
            var path = $"{PrefabsPath}/StandingPictureActor.prefab";
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (existing != null)
            {
                var actor = existing.GetComponent<StandingPictureActor>();
                if (actor != null) return actor;
            }

            if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
                AssetDatabase.CreateFolder("Assets", "Prefabs");

            var go = new GameObject("StandingPictureActor");
            var rect = go.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0f);
            rect.anchorMax = new Vector2(0.5f, 0f);
            rect.pivot = new Vector2(0.5f, 0f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = new Vector2(300, 500);

            var imageGo = new GameObject("Image");
            imageGo.transform.SetParent(go.transform, false);
            var imageRect = imageGo.AddComponent<RectTransform>();
            imageRect.anchorMin = Vector2.zero;
            imageRect.anchorMax = Vector2.one;
            imageRect.offsetMin = Vector2.zero;
            imageRect.offsetMax = Vector2.zero;
            var image = imageGo.AddComponent<Image>();
            image.color = Color.white;
            image.raycastTarget = false;
            image.preserveAspect = true;

            var cg = go.AddComponent<CanvasGroup>();

            var actorComp = go.AddComponent<StandingPictureActor>();
            var so = new SerializedObject(actorComp);
            so.FindProperty("uiImage").objectReferenceValue = image;
            so.FindProperty("canvasGroup").objectReferenceValue = cg;
            so.ApplyModifiedPropertiesWithoutUndo();

            PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
            return AssetDatabase.LoadAssetAtPath<GameObject>(path).GetComponent<StandingPictureActor>();
        }

        private static void EnsureCameraAndEventSystem(GameObject root)
        {
            if (Object.FindAnyObjectByType<Camera>() == null)
            {
                var camGo = new GameObject("Main Camera");
                camGo.tag = "MainCamera";
                camGo.AddComponent<Camera>().orthographic = true;
                camGo.AddComponent<AudioListener>();
                Undo.RegisterCreatedObjectUndo(camGo, "Create Main Camera");
            }

            if (Object.FindAnyObjectByType<EventSystem>() == null)
            {
                var esGo = new GameObject("EventSystem");
                esGo.AddComponent<EventSystem>();
                esGo.AddComponent<InputSystemUIInputModule>();
                Undo.RegisterCreatedObjectUndo(esGo, "Create EventSystem");
            }
        }
    }
}
