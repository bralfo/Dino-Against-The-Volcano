using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelFinishTrigger : MonoBehaviour
{
    private const string GameplaySceneName = "GameScene";
    private const string EndingSceneName = "EndScene";

    private bool isLoadingEnding;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void RegisterSceneBootstrap()
    {
        SceneManager.sceneLoaded -= ConfigureLevelFinish;
        SceneManager.sceneLoaded += ConfigureLevelFinish;
    }

    private static void ConfigureLevelFinish(Scene scene, LoadSceneMode mode)
    {
        if (scene.name != GameplaySceneName)
            return;

        GameObject heart = GameObject.Find("heart-ui");

        if (heart == null)
        {
            Debug.LogWarning("O coração final 'heart-ui' não foi encontrado na GameScene.");
            return;
        }

        int defaultLayer = LayerMask.NameToLayer("Default");

        if (defaultLayer >= 0)
            heart.layer = defaultLayer;

        CircleCollider2D trigger = heart.GetComponent<CircleCollider2D>();

        if (trigger == null)
            trigger = heart.AddComponent<CircleCollider2D>();

        SpriteRenderer spriteRenderer = heart.GetComponent<SpriteRenderer>();

        if (spriteRenderer != null && spriteRenderer.sprite != null)
        {
            Vector2 extents = spriteRenderer.sprite.bounds.extents;
            trigger.radius = Mathf.Max(extents.x, extents.y) * 0.8f;
        }

        trigger.isTrigger = true;

        if (heart.GetComponent<LevelFinishTrigger>() == null)
            heart.AddComponent<LevelFinishTrigger>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isLoadingEnding || other.GetComponentInParent<PlayerHealth>() == null)
            return;

        isLoadingEnding = true;
        Time.timeScale = 1f;
        SceneManager.LoadScene(EndingSceneName);
    }
}
