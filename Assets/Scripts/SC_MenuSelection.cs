using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// メニューの選択状態を管理するスクリプト
/// 選択中のボタンだけFrameを表示する
/// </summary>
public class SC_MenuSelection : MonoBehaviour,
    ISelectHandler,
    IDeselectHandler,
    IPointerEnterHandler
{
    // 選択中に表示する枠
    public GameObject frame;

    // ボタンが選択された時
    public void OnSelect(BaseEventData eventData)
    {
        frame.SetActive(true);
    }

    // ボタンの選択が外れた時
    public void OnDeselect(BaseEventData eventData)
    {
        frame.SetActive(false);
    }

    // マウスを乗せたら選択状態にする
    public void OnPointerEnter(PointerEventData eventData)
    {
        EventSystem.current.SetSelectedGameObject(gameObject);
    }
}