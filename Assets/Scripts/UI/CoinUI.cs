using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CoinUI : MonoBehaviour
{
    [SerializeField] private CoinWallet coinWallet;
    [SerializeField] private TMP_Text coinCountText;
    [SerializeField] private Image coinIcon;

    private SpriteRenderer animatedCoinSource;

    private void OnEnable()
    {
        if (coinWallet != null)
            coinWallet.CoinsChanged += UpdateCoinText;
    }

    private void Start()
    {
        if (coinWallet != null)
            UpdateCoinText(coinWallet.CurrentCoins);

        ConfigureAnimatedIcon();
    }

    private void LateUpdate()
    {
        if (coinIcon != null && animatedCoinSource != null && animatedCoinSource.sprite != null)
            coinIcon.sprite = animatedCoinSource.sprite;
    }

    private void OnDisable()
    {
        if (coinWallet != null)
            coinWallet.CoinsChanged -= UpdateCoinText;
    }

    private void UpdateCoinText(int amount)
    {
        if (coinCountText != null)
            coinCountText.text = amount.ToString();
    }

    private void ConfigureAnimatedIcon()
    {
        if (coinIcon == null)
        {
            Transform iconTransform = transform.Find("CoinIcon");

            if (iconTransform != null)
                coinIcon = iconTransform.GetComponent<Image>();
        }

        if (coinIcon == null)
            return;

        Animator sourceAnimator = FindCoinAnimator();

        if (sourceAnimator == null || sourceAnimator.runtimeAnimatorController == null)
            return;

        GameObject animationSource = new GameObject(
            "CoinIconAnimationSource",
            typeof(SpriteRenderer),
            typeof(Animator));

        animationSource.transform.SetParent(transform, false);

        animatedCoinSource = animationSource.GetComponent<SpriteRenderer>();
        animatedCoinSource.enabled = false;
        animatedCoinSource.sprite = coinIcon.sprite;

        Animator iconAnimator = animationSource.GetComponent<Animator>();
        iconAnimator.runtimeAnimatorController = sourceAnimator.runtimeAnimatorController;
        iconAnimator.updateMode = AnimatorUpdateMode.UnscaledTime;
        iconAnimator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
    }

    private static Animator FindCoinAnimator()
    {
        foreach (Animator animator in FindObjectsByType<Animator>())
        {
            if (animator.runtimeAnimatorController == null)
                continue;

            if (animator.runtimeAnimatorController.name != "Coin")
                continue;

            if (animator.GetComponent<SpriteRenderer>() != null)
                return animator;
        }

        return null;
    }
}
