using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class RestoreGameplayScene
{
    private const string ScenePath = "Assets/Scenes/GameScene.unity";
    private const string PlayerPrefabPath = "Assets/Prefab/Player.prefab";
    private const string FontPath = "Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset";
    private const string HeartSpritePath = "Assets/Sprites/Decoration/heart-ui.png";

    [MenuItem("Tools/Dino Against the Volcano/Restaurar HUD e menus")]
    public static void Apply()
    {
        ConfigurePlayerPrefab();

        Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        PlayerHealth playerHealth = Object.FindAnyObjectByType<PlayerHealth>();
        Canvas hud = FindHud();

        if (playerHealth == null || hud == null)
            throw new System.InvalidOperationException("GameScene precisa conter o Player e o Canvas HUD.");

        Sprite heartSprite = ConfigureHeartSprite();
        RemoveOldUI(hud.transform);

        HealthUI healthUI = CreateHearts(hud.transform, playerHealth, heartSprite);
        GameOverUI gameOverUI = CreateGameOver(hud.transform, playerHealth);
        CreatePauseMenu(hud.transform, gameOverUI);
        OrganizeHierarchy(scene);

        EditorUtility.SetDirty(healthUI);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();

        Debug.Log("HUD, Game Over e menu de pausa restaurados na GameScene.");
    }

    [MenuItem("Tools/Dino Against the Volcano/Organizar GameScene")]
    public static void OrganizeGameScene()
    {
        Scene scene = SceneManager.GetActiveScene();

        if (scene.path != ScenePath)
            throw new System.InvalidOperationException("Abra a GameScene antes de organizar a hierarquia.");

        OrganizeHierarchy(scene);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);

        Debug.Log("Hierarquia da GameScene organizada e salva.");
    }

    private static void OrganizeHierarchy(Scene scene)
    {
        Transform systems = GetOrCreateRoot(scene, "_Systems");
        Transform environment = GetOrCreateRoot(scene, "_Environment");
        Transform gameplay = GetOrCreateRoot(scene, "_Gameplay");
        Transform ui = GetOrCreateRoot(scene, "_UI");

        Transform enemies = GetOrCreateChild(gameplay, "Enemies");
        Transform collectibles = GetOrCreateChild(gameplay, "Collectibles");

        MoveRootObjects(scene, systems, "Main Camera", "Global Light 2D", "EventSystem");
        MoveRootObjects(scene, environment, "Grid", "Platforms", "Colunas", "Floaters");
        MoveRootObjects(scene, gameplay, "Player");
        MoveRootObjects(scene, collectibles, "Coins");
        MoveRootObjects(scene, ui, "HUD");

        foreach (GameObject root in scene.GetRootGameObjects())
        {
            bool isEnemy = root.name == "Bat"
                || root.name == "bob"
                || root.name.StartsWith("Enemy")
                || root.GetComponent<EnemyController>() != null;

            if (isEnemy)
                root.transform.SetParent(enemies, true);
        }

        systems.SetSiblingIndex(0);
        environment.SetSiblingIndex(1);
        gameplay.SetSiblingIndex(2);
        ui.SetSiblingIndex(3);

        SetChildOrder(systems, "Main Camera", "Global Light 2D", "EventSystem");
        SetChildOrder(environment, "Grid", "Platforms", "Colunas", "Floaters");
        SetChildOrder(gameplay, "Player", "Enemies", "Collectibles");
    }

    private static Transform GetOrCreateRoot(Scene scene, string name)
    {
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            if (root.name == name)
                return root.transform;
        }

        GameObject group = new GameObject(name);
        SceneManager.MoveGameObjectToScene(group, scene);
        return group.transform;
    }

    private static Transform GetOrCreateChild(Transform parent, string name)
    {
        Transform child = parent.Find(name);

        if (child != null)
            return child;

        GameObject group = new GameObject(name);
        group.transform.SetParent(parent, false);
        return group.transform;
    }

    private static void MoveRootObjects(Scene scene, Transform destination, params string[] names)
    {
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            foreach (string objectName in names)
            {
                if (root.name != objectName)
                    continue;

                root.transform.SetParent(destination, true);
                break;
            }
        }
    }

    private static void SetChildOrder(Transform parent, params string[] names)
    {
        for (int index = 0; index < names.Length; index++)
        {
            Transform child = parent.Find(names[index]);

            if (child != null)
                child.SetSiblingIndex(index);
        }
    }

    private static Sprite ConfigureHeartSprite()
    {
        TextureImporter importer = AssetImporter.GetAtPath(HeartSpritePath) as TextureImporter;

        if (importer == null)
            throw new System.InvalidOperationException("The heart sprite was not found.");

        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.alphaIsTransparency = true;
        importer.mipmapEnabled = false;
        importer.SaveAndReimport();

        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(HeartSpritePath);

        if (sprite == null)
            throw new System.InvalidOperationException("The heart could not be imported as a Sprite.");

        return sprite;
    }

    private static void ConfigurePlayerPrefab()
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerPrefabPath);

        if (prefab == null)
            throw new System.InvalidOperationException("Player.prefab was not found.");

        PlayerHealth health = prefab.GetComponent<PlayerHealth>();
        PlayerController controller = prefab.GetComponent<PlayerController>();

        SerializedObject healthProperties = new SerializedObject(health);
        healthProperties.FindProperty("maxHealth").intValue = 3;
        healthProperties.ApplyModifiedPropertiesWithoutUndo();

        SerializedObject controllerProperties = new SerializedObject(controller);
        controllerProperties.FindProperty("jumpForce").floatValue = 20f;
        controllerProperties.FindProperty("doubleJumpForceMultiplier").floatValue = 0.7f;
        controllerProperties.ApplyModifiedPropertiesWithoutUndo();

        EditorUtility.SetDirty(prefab);
        PrefabUtility.SavePrefabAsset(prefab);
    }

    private static Canvas FindHud()
    {
        foreach (Canvas canvas in Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include))
        {
            if (canvas.name == "HUD")
                return canvas;
        }

        return null;
    }

    private static void RemoveOldUI(Transform hud)
    {
        string[] names = { "HealthBar", "Hearts", "GameOverPanel", "PausePanel" };

        foreach (string objectName in names)
        {
            Transform child = hud.Find(objectName);

            if (child != null)
                Object.DestroyImmediate(child.gameObject);
        }

        DestroyComponentIfPresent<HealthUI>(hud.gameObject);
        DestroyComponentIfPresent<GameOverUI>(hud.gameObject);
        DestroyComponentIfPresent<PauseMenuUI>(hud.gameObject);
    }

    private static void DestroyComponentIfPresent<T>(GameObject target) where T : Component
    {
        T component = target.GetComponent<T>();

        if (component != null)
            Object.DestroyImmediate(component);
    }

    private static HealthUI CreateHearts(Transform hud, PlayerHealth playerHealth, Sprite heartSprite)
    {
        GameObject container = CreateUIObject("Hearts", hud);
        RectTransform rect = container.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.anchoredPosition = new Vector2(45f, -35f);
        rect.sizeDelta = new Vector2(260f, 80f);

        HorizontalLayoutGroup layout = container.AddComponent<HorizontalLayoutGroup>();
        layout.spacing = 8f;
        layout.childAlignment = TextAnchor.MiddleLeft;
        layout.childControlWidth = false;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;

        Graphic[] hearts = new Graphic[3];

        for (int index = 0; index < hearts.Length; index++)
        {
            GameObject heartObject = CreateUIObject("Heart" + (index + 1), container.transform);
            RectTransform heartRect = heartObject.GetComponent<RectTransform>();
            heartRect.sizeDelta = new Vector2(72f, 72f);

            Image heart = heartObject.AddComponent<Image>();
            heart.sprite = heartSprite;
            heart.preserveAspect = true;
            heart.raycastTarget = false;
            hearts[index] = heart;
        }

        HealthUI healthUI = container.AddComponent<HealthUI>();
        SerializedObject properties = new SerializedObject(healthUI);
        properties.FindProperty("playerHealth").objectReferenceValue = playerHealth;

        SerializedProperty heartsProperty = properties.FindProperty("hearts");
        heartsProperty.arraySize = hearts.Length;

        for (int index = 0; index < hearts.Length; index++)
            heartsProperty.GetArrayElementAtIndex(index).objectReferenceValue = hearts[index];

        properties.ApplyModifiedPropertiesWithoutUndo();
        return healthUI;
    }

    private static GameOverUI CreateGameOver(Transform hud, PlayerHealth playerHealth)
    {
        GameObject panel = CreatePanel("GameOverPanel", hud, new Color(0.04f, 0.02f, 0.08f, 0.92f));
        CreateText("YouDiedText", panel.transform, "GAME OVER", 76f, new Color(1f, 0.25f, 0.2f, 1f), new Vector2(0f, 175f), new Vector2(800f, 110f));

        GameOverUI gameOverUI = hud.gameObject.AddComponent<GameOverUI>();
        Button playAgain = CreateButton("PlayAgainButton", panel.transform, "PLAY AGAIN", new Vector2(0f, 25f));
        Button menu = CreateButton("MenuButton", panel.transform, "MAIN MENU", new Vector2(0f, -95f));

        UnityEventTools.AddPersistentListener(playAgain.onClick, new UnityAction(gameOverUI.PlayAgain));
        UnityEventTools.AddPersistentListener(menu.onClick, new UnityAction(gameOverUI.GoToMenu));

        SerializedObject properties = new SerializedObject(gameOverUI);
        properties.FindProperty("playerHealth").objectReferenceValue = playerHealth;
        properties.FindProperty("gameOverPanel").objectReferenceValue = panel;
        properties.FindProperty("mainMenuSceneName").stringValue = "MainMenu";
        properties.ApplyModifiedPropertiesWithoutUndo();

        panel.SetActive(false);
        return gameOverUI;
    }

    private static void CreatePauseMenu(Transform hud, GameOverUI gameOverUI)
    {
        GameObject panel = CreatePanel("PausePanel", hud, new Color(0.02f, 0.04f, 0.08f, 0.9f));
        CreateText("PausedText", panel.transform, "PAUSED", 72f, Color.white, new Vector2(0f, 175f), new Vector2(800f, 110f));

        PauseMenuUI pauseMenu = hud.gameObject.AddComponent<PauseMenuUI>();
        Button resume = CreateButton("ResumeButton", panel.transform, "RESUME", new Vector2(0f, 25f));
        Button menu = CreateButton("MenuButton", panel.transform, "MAIN MENU", new Vector2(0f, -95f));

        UnityEventTools.AddPersistentListener(resume.onClick, new UnityAction(pauseMenu.ResumeGame));
        UnityEventTools.AddPersistentListener(menu.onClick, new UnityAction(pauseMenu.GoToMenu));

        SerializedObject properties = new SerializedObject(pauseMenu);
        properties.FindProperty("pausePanel").objectReferenceValue = panel;
        properties.FindProperty("gameOverUI").objectReferenceValue = gameOverUI;
        properties.FindProperty("mainMenuSceneName").stringValue = "MainMenu";
        properties.ApplyModifiedPropertiesWithoutUndo();

        panel.SetActive(false);
    }

    private static GameObject CreatePanel(string name, Transform parent, Color color)
    {
        GameObject panel = CreateUIObject(name, parent);
        Stretch(panel.GetComponent<RectTransform>());
        Image background = panel.AddComponent<Image>();
        background.color = color;
        return panel;
    }

    private static Button CreateButton(string name, Transform parent, string label, Vector2 position)
    {
        GameObject buttonObject = CreateUIObject(name, parent);
        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = new Vector2(440f, 84f);

        Image image = buttonObject.AddComponent<Image>();
        image.color = new Color(0.45f, 0.12f, 0.55f, 1f);

        Button button = buttonObject.AddComponent<Button>();
        button.targetGraphic = image;

        TextMeshProUGUI text = CreateText("Text", buttonObject.transform, label, 34f, Color.white);
        Stretch(text.rectTransform);
        return button;
    }

    private static TextMeshProUGUI CreateText(string name, Transform parent, string value, float fontSize, Color color)
    {
        return CreateText(name, parent, value, fontSize, color, Vector2.zero, new Vector2(400f, 100f));
    }

    private static TextMeshProUGUI CreateText(string name, Transform parent, string value, float fontSize, Color color, Vector2 position, Vector2 size)
    {
        GameObject textObject = CreateUIObject(name, parent);
        RectTransform rect = textObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;

        TextMeshProUGUI text = textObject.AddComponent<TextMeshProUGUI>();
        text.text = value;
        text.fontSize = fontSize;
        text.color = color;
        text.alignment = TextAlignmentOptions.Center;
        text.fontStyle = FontStyles.Bold;
        text.raycastTarget = false;
        text.font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontPath);
        return text;
    }

    private static GameObject CreateUIObject(string name, Transform parent)
    {
        GameObject gameObject = new GameObject(name, typeof(RectTransform));
        gameObject.layer = LayerMask.NameToLayer("UI");
        gameObject.transform.SetParent(parent, false);
        return gameObject;
    }

    private static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }
}
