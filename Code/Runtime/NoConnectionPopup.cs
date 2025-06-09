using UnityEngine;

public class NoConnectionPopup : MonoBehaviour
{
    public bool IsOpened { get; private set; }

    private void Update()
    {
        if (IsOpened)
        {
            if (MonetizationManager.IsConnectedToNetwork())
            {
                Hide();
            }
        }
    }

    public void Show()
    {
        IsOpened = true;

        gameObject.SetActive(true);

        Time.timeScale = 0.0f;
    }

    public void Hide()
    {
        IsOpened = false;

        gameObject.SetActive(false);

        Time.timeScale = 1.0f;
    }

    private void OnLostConnection()
    {
        Show();
    }

    protected void Awake()
    {
        IsOpened = true;

        GetComponent<RectTransform>().anchoredPosition = Vector2.zero;

        MonetizationManager.OnLostConnection?.AddListener(OnLostConnection);

        Hide();

        DontDestroyOnLoad(transform.parent);
    }

    protected void OnDestroy()
    {
        MonetizationManager.OnLostConnection?.RemoveListener(OnLostConnection);
    }
}
