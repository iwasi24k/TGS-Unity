using System.Reflection;
using UnityEngine;

public class SC_BlinkStamina: MonoBehaviour
{
    [Header("Player")]
    [SerializeField] private SC_PlayerMove playerMove;

    [Header("Needle")]
    [SerializeField] private RectTransform needle;

    [Header("Angle")]

    // 右端
    [SerializeField] private float rightAngle = -90f;

    // 左端
    [SerializeField] private float leftAngle = 90f;

    [Header("Smooth")]
    [SerializeField] private float smoothSpeed = 8f;

    // private変数参照用
    private FieldInfo blinkTimerField;
    private FieldInfo blinkCooldownField;

    private void Start()
    {
        // Playerタグから取得
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");

        if (playerObj != null)
        {
            playerMove = playerObj.GetComponent<SC_PlayerMove>();
        }

        if (playerMove == null)
        {
            Debug.LogError("PlayerタグのSC_PlayerMoveが見つかりません");
            return;
        }

        // private float BlinkTimer
        blinkTimerField =
            typeof(SC_PlayerMove)
            .GetField(
                "BlinkTimer",
                BindingFlags.NonPublic | BindingFlags.Instance);

        // private float blinkCooldown
        blinkCooldownField =
            typeof(SC_PlayerMove)
            .GetField(
                "blinkCooldown",
                BindingFlags.NonPublic | BindingFlags.Instance);
    }

    private void Update()
    {
        if (playerMove == null) return;

        if (blinkTimerField == null) return;
        if (blinkCooldownField == null) return;

        // private変数取得
        float currentCooldown =
            (float)blinkTimerField.GetValue(playerMove);

        float maxCooldown =
            (float)blinkCooldownField.GetValue(playerMove);

        // 0除算防止
        if (maxCooldown <= 0f) return;

        // 1 → 0
        float ratio = currentCooldown / maxCooldown;

        // 左 ↔ 右
        float angle =
            Mathf.Lerp(leftAngle, rightAngle, ratio);

        Quaternion targetRotation =
            Quaternion.Euler(0, 0, angle);

        needle.localRotation =
            Quaternion.Lerp(
                needle.localRotation,
                targetRotation,
                Time.deltaTime * smoothSpeed);
    }
}