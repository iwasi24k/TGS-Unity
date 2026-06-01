using UnityEngine;

public class SC_ShieldLightningEffect : MonoBehaviour
{
    [Header("Ref")]
    [Tooltip("シールドのRenderer"), SerializeField]
    private Renderer targetRenderer;

    [Header("Shader Property")]
    [Tooltip("Shader Graph側のLightningSpeedのReference名"), SerializeField]
    private string lightningSpeedPropertyName = "_LightningSpeed1";

    [Tooltip("2本目のLightningSpeedを使う場合のReference名"), SerializeField]
    private string lightningSpeedBPropertyName = "_LightningSpeed2";

    [Tooltip("2本目のSpeedも変更するか"), SerializeField]
    private bool useSecondSpeed = false;

    [Header("Speed")]
    [Tooltip("通常時の稲妻速度1"), SerializeField]
    private float normalSpeed1 = -1.0f;

    [Tooltip("通常時の稲妻速度2"), SerializeField]
    private float normalSpeed2 = -2.0f;

    [Tooltip("攻撃を受けた瞬間の稲妻速度"), SerializeField]
    private float hitSpeed = -5.0f;

    [Tooltip("通常速度へ戻る速さ"), SerializeField]
    private float recoverSpeed = 1.0f;

    private MaterialPropertyBlock propertyBlock;
    private float currentSpeed1;
    private float currentSpeed2;

    private void Awake()
    {
        if (targetRenderer == null)
        {
            targetRenderer = GetComponentInChildren<Renderer>();
        }

        propertyBlock = new MaterialPropertyBlock();

        currentSpeed1 = normalSpeed1;
        currentSpeed2 = normalSpeed2;
        ApplySpeed(currentSpeed1, currentSpeed2);
    }

    private void Update()
    {
        currentSpeed1 = Mathf.Lerp(
            currentSpeed1,
            normalSpeed1,
            recoverSpeed * Time.deltaTime);

        currentSpeed2 = Mathf.Lerp(
            currentSpeed2,
            normalSpeed2,
            recoverSpeed * Time.deltaTime);


        ApplySpeed(currentSpeed1,currentSpeed2);
    }

    public void PlayHitEffect()
    {
        currentSpeed1 = hitSpeed;
        currentSpeed2 = hitSpeed;
        ApplySpeed(currentSpeed1, currentSpeed2);
    }

    private void ApplySpeed(float speed1,float speed2)
    {
        if (targetRenderer == null) return;

        targetRenderer.GetPropertyBlock(propertyBlock);

        propertyBlock.SetFloat(lightningSpeedPropertyName, speed1);

        if (useSecondSpeed)
        {
            propertyBlock.SetFloat(lightningSpeedBPropertyName, speed2);
        }

        targetRenderer.SetPropertyBlock(propertyBlock);
    }
}
