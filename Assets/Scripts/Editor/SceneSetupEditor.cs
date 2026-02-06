using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;
using CardBattle.Managers;
using CardBattle.Battle;
using CardBattle.AI;
using CardBattle.UI;

namespace CardBattle.Editor
{
    public static class SceneSetupEditor
    {
        private const string SetupRootName = "CardBattle_SetupRoot";
        private const string PrefabsPath = "Assets/Prefabs";

        [MenuItem("Window/CardBattle/Setup Scene")]
        public static void SetupScene()
        {
            var root = EnsureRootClean();
            if (root == null) return;

            var gameSystems = CreateGameSystems(root);
            var canvasData = CreateCanvasHierarchy(root);
            var cardPrefab = GetOrCreateCardViewPrefab();
            var unitPrefab = GetOrCreateUnitViewPrefab();

            WireReferences(root, gameSystems, canvasData, cardPrefab, unitPrefab);
            EnsureCameraAndEventSystem(root);

            EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        }

        private static GameObject EnsureRootClean()
        {
            var existing = GameObject.Find(SetupRootName);
            if (existing != null)
            {
                Undo.DestroyObjectImmediate(existing);
            }

            var root = new GameObject(SetupRootName);
            Undo.RegisterCreatedObjectUndo(root, "CardBattle Setup");
            return root;
        }

        private static GameObject CreateGameSystems(GameObject root)
        {
            var go = new GameObject("GameSystems");
            Undo.RegisterCreatedObjectUndo(go, "Create GameSystems");
            go.transform.SetParent(root.transform, false);

            go.AddComponent<GameFlowManager>();
            go.AddComponent<PlayerManager>();
            go.AddComponent<PartnerManager>();
            go.AddComponent<UnitManager>();
            go.AddComponent<ActionQueueManager>();
            go.AddComponent<BattleManager>();
            go.AddComponent<AIController>();
            go.AddComponent<DialogueManager>();
            go.AddComponent<GameVisualManager>();
            go.AddComponent<GameBootstrap>();

            return go;
        }

        private struct CanvasData
        {
            public Transform DeckAnchor;
            public HandVisualizer HandVisualizer;
            public FieldVisualizer FieldVisualizer;
            public RectTransform FieldAreaRect;
        }

        private static CanvasData CreateCanvasHierarchy(GameObject root)
        {
            var canvasGo = new GameObject("Canvas");
            Undo.RegisterCreatedObjectUndo(canvasGo, "Create Canvas");
            canvasGo.transform.SetParent(root.transform, false);

            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGo.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasGo.AddComponent<GraphicRaycaster>();

            var deckGo = new GameObject("DeckAnchor");
            deckGo.transform.SetParent(canvasGo.transform, false);
            var deckRect = deckGo.AddComponent<RectTransform>();
            deckRect.anchorMin = new Vector2(0.1f, 0.1f);
            deckRect.anchorMax = new Vector2(0.15f, 0.3f);
            deckRect.offsetMin = Vector2.zero;
            deckRect.offsetMax = Vector2.zero;

            var handGo = new GameObject("HandArea");
            handGo.transform.SetParent(canvasGo.transform, false);
            var handRect = handGo.AddComponent<RectTransform>();
            handRect.anchorMin = new Vector2(0, 0);
            handRect.anchorMax = new Vector2(1, 0.25f);
            handRect.offsetMin = new Vector2(20, 10);
            handRect.offsetMax = new Vector2(-20, 60);
            var handVisualizer = handGo.AddComponent<HandVisualizer>();

            var fieldGo = new GameObject("FieldArea");
            fieldGo.transform.SetParent(canvasGo.transform, false);
            var fieldRect = fieldGo.AddComponent<RectTransform>();
            fieldRect.anchorMin = new Vector2(0.1f, 0.35f);
            fieldRect.anchorMax = new Vector2(0.9f, 0.65f);
            fieldRect.offsetMin = Vector2.zero;
            fieldRect.offsetMax = Vector2.zero;
            var fieldVisualizer = fieldGo.AddComponent<FieldVisualizer>();

            var fieldAreaGo = new GameObject("FieldDropZone");
            fieldAreaGo.transform.SetParent(fieldGo.transform, false);
            var fieldAreaRect = fieldAreaGo.AddComponent<RectTransform>();
            fieldAreaRect.anchorMin = Vector2.zero;
            fieldAreaRect.anchorMax = Vector2.one;
            fieldAreaRect.offsetMin = Vector2.zero;
            fieldAreaRect.offsetMax = Vector2.zero;

            CreatePlayerInfo(canvasGo.transform, "Player0Info", new Vector2(0, 0.85f), new Vector2(0.3f, 1));
            CreatePlayerInfo(canvasGo.transform, "Player1Info", new Vector2(0.7f, 0.85f), new Vector2(1, 1));

            return new CanvasData
            {
                DeckAnchor = deckRect,
                HandVisualizer = handVisualizer,
                FieldVisualizer = fieldVisualizer,
                FieldAreaRect = fieldAreaRect
            };
        }

        private static void CreatePlayerInfo(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rect = go.AddComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = new Vector2(10, -80);
            rect.offsetMax = new Vector2(-10, -10);

            var hpText = CreateText(go.transform, "HPText", "15", new Vector2(0, 0.6f), new Vector2(0.5f, 1));
            var hpSlider = CreateSlider(go.transform, "HPSlider", new Vector2(0, 0.35f), new Vector2(1, 0.55f));
            var mpText = CreateText(go.transform, "MPText", "0/0", new Vector2(0, 0.1f), new Vector2(1, 0.3f));
            var mpSlider = CreateSlider(go.transform, "MPSlider", new Vector2(0, 0), new Vector2(1, 0.05f));

            var playerInfoView = go.AddComponent<PlayerInfoView>();
            var so = new SerializedObject(playerInfoView);
            so.FindProperty("hpSlider").objectReferenceValue = hpSlider;
            so.FindProperty("hpText").objectReferenceValue = hpText;
            so.FindProperty("mpSlider").objectReferenceValue = mpSlider;
            so.FindProperty("mpText").objectReferenceValue = mpText;
            so.FindProperty("ppText").objectReferenceValue = mpText;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static Text CreateText(Transform parent, string name, string content, Vector2 anchorMin, Vector2 anchorMax)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rect = go.AddComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            var text = go.AddComponent<Text>();
            text.text = content;
            text.fontSize = 14;
            return text;
        }

        private static Slider CreateSlider(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rect = go.AddComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            var bg = go.AddComponent<Image>();
            bg.color = new Color(0.2f, 0.2f, 0.2f);
            var fillArea = new GameObject("Fill Area");
            fillArea.transform.SetParent(go.transform, false);
            var fillAreaRect = fillArea.AddComponent<RectTransform>();
            fillAreaRect.anchorMin = Vector2.zero;
            fillAreaRect.anchorMax = Vector2.one;
            fillAreaRect.offsetMin = new Vector2(5, 5);
            fillAreaRect.offsetMax = new Vector2(-5, -5);
            var fill = new GameObject("Fill");
            fill.transform.SetParent(fillArea.transform, false);
            var fillRect = fill.AddComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;
            var fillImage = fill.AddComponent<Image>();
            fillImage.color = new Color(0.3f, 0.7f, 0.3f);
            var slider = go.AddComponent<Slider>();
            slider.fillRect = fillRect;
            return slider;
        }

        private static CardView GetOrCreateCardViewPrefab()
        {
            var path = $"{PrefabsPath}/CardView.prefab";
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (existing != null)
            {
                return existing.GetComponent<CardView>();
            }

            if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
            {
                AssetDatabase.CreateFolder("Assets", "Prefabs");
            }

            var go = new GameObject("CardView");
            var rect = go.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(100, 140);
            go.AddComponent<Image>().color = new Color(0.9f, 0.85f, 0.7f);
            go.AddComponent<CanvasGroup>();

            var costGo = new GameObject("Cost");
            costGo.transform.SetParent(go.transform, false);
            var costRect = costGo.AddComponent<RectTransform>();
            costRect.anchorMin = new Vector2(0, 1);
            costRect.anchorMax = new Vector2(0, 1);
            costRect.anchoredPosition = new Vector2(15, -15);
            costRect.sizeDelta = new Vector2(30, 20);
            var costText = costGo.AddComponent<Text>();
            costText.text = "0";
            costText.fontSize = 14;

            var attackGo = new GameObject("Attack");
            attackGo.transform.SetParent(go.transform, false);
            var attackRect = attackGo.AddComponent<RectTransform>();
            attackRect.anchorMin = new Vector2(0, 0);
            attackRect.anchorMax = new Vector2(0, 0);
            attackRect.anchoredPosition = new Vector2(15, 15);
            attackRect.sizeDelta = new Vector2(30, 20);
            var attackText = attackGo.AddComponent<Text>();
            attackText.text = "0";
            attackText.fontSize = 14;

            var hpGo = new GameObject("HP");
            hpGo.transform.SetParent(go.transform, false);
            var hpRect = hpGo.AddComponent<RectTransform>();
            hpRect.anchorMin = new Vector2(1, 0);
            hpRect.anchorMax = new Vector2(1, 0);
            hpRect.anchoredPosition = new Vector2(-15, 15);
            hpRect.sizeDelta = new Vector2(30, 20);
            var hpText = hpGo.AddComponent<Text>();
            hpText.text = "0";
            hpText.fontSize = 14;

            var artworkGo = new GameObject("Artwork");
            artworkGo.transform.SetParent(go.transform, false);
            var artworkRect = artworkGo.AddComponent<RectTransform>();
            artworkRect.anchorMin = new Vector2(0.1f, 0.25f);
            artworkRect.anchorMax = new Vector2(0.9f, 0.95f);
            artworkRect.offsetMin = Vector2.zero;
            artworkRect.offsetMax = Vector2.zero;
            artworkGo.AddComponent<Image>().color = new Color(0.5f, 0.5f, 0.5f);

            var cardView = go.AddComponent<CardView>();
            var so = new SerializedObject(cardView);
            so.FindProperty("cost").objectReferenceValue = costText;
            so.FindProperty("attack").objectReferenceValue = attackText;
            so.FindProperty("hp").objectReferenceValue = hpText;
            so.FindProperty("canvasGroup").objectReferenceValue = go.GetComponent<CanvasGroup>();
            so.ApplyModifiedPropertiesWithoutUndo();

            PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
            return AssetDatabase.LoadAssetAtPath<GameObject>(path).GetComponent<CardView>();
        }

        private static UnitView GetOrCreateUnitViewPrefab()
        {
            var path = $"{PrefabsPath}/UnitView.prefab";
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (existing != null)
            {
                return existing.GetComponent<UnitView>();
            }

            var go = new GameObject("UnitView");
            var rect = go.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(80, 100);
            var bodyImage = go.AddComponent<Image>();
            bodyImage.color = new Color(0.7f, 0.6f, 0.5f);

            var attackGo = new GameObject("AttackText");
            attackGo.transform.SetParent(go.transform, false);
            var attackRect = attackGo.AddComponent<RectTransform>();
            attackRect.anchorMin = new Vector2(0, 0);
            attackRect.anchorMax = new Vector2(0, 0);
            attackRect.anchoredPosition = new Vector2(15, 10);
            attackRect.sizeDelta = new Vector2(30, 20);
            var attackText = attackGo.AddComponent<Text>();
            attackText.text = "0";
            attackText.fontSize = 12;

            var hpGo = new GameObject("HPText");
            hpGo.transform.SetParent(go.transform, false);
            var hpRect = hpGo.AddComponent<RectTransform>();
            hpRect.anchorMin = new Vector2(1, 0);
            hpRect.anchorMax = new Vector2(1, 0);
            hpRect.anchoredPosition = new Vector2(-15, 10);
            hpRect.sizeDelta = new Vector2(30, 20);
            var hpText = hpGo.AddComponent<Text>();
            hpText.text = "0";
            hpText.fontSize = 12;

            var unitView = go.AddComponent<UnitView>();
            var so = new SerializedObject(unitView);
            so.FindProperty("attackText").objectReferenceValue = attackText;
            so.FindProperty("hpText").objectReferenceValue = hpText;
            so.FindProperty("bodyImage").objectReferenceValue = bodyImage;
            so.ApplyModifiedPropertiesWithoutUndo();

            PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
            return AssetDatabase.LoadAssetAtPath<GameObject>(path).GetComponent<UnitView>();
        }

        private static void WireReferences(GameObject root, GameObject gameSystems, CanvasData canvasData,
            CardView cardPrefab, UnitView unitPrefab)
        {
            var gameVisualManager = gameSystems.GetComponent<GameVisualManager>();
            var fieldVisualizer = canvasData.FieldVisualizer;

            var gvmSo = new SerializedObject(gameVisualManager);
            gvmSo.FindProperty("cardPrefab").objectReferenceValue = cardPrefab;
            gvmSo.FindProperty("deckTransform").objectReferenceValue = canvasData.DeckAnchor;
            gvmSo.FindProperty("handVisualizer").objectReferenceValue = canvasData.HandVisualizer;
            gvmSo.FindProperty("fieldVisualizer").objectReferenceValue = fieldVisualizer;
            gvmSo.FindProperty("unitPrefab").objectReferenceValue = unitPrefab;
            gvmSo.ApplyModifiedPropertiesWithoutUndo();

            var fvSo = new SerializedObject(fieldVisualizer);
            fvSo.FindProperty("fieldAreaRect").objectReferenceValue = canvasData.FieldAreaRect;
            fvSo.ApplyModifiedPropertiesWithoutUndo();
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
