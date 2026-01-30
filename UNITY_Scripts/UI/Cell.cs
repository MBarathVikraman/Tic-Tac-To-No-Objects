using UnityEngine;
using UnityEngine.UI;

public class Cell : MonoBehaviour
{
    [SerializeField] private int cellIndex;
    [SerializeField] private Button button;
    [SerializeField] private Image image;
    [SerializeField] private MultiplayerManager mp;

    [SerializeField] private Color defaultColor;
    [SerializeField] private Color winColor;
    [SerializeField] private Color failedColor;

    [SerializeField] private Sprite xImage;
    [SerializeField] private Sprite oImage;
    [SerializeField] private Sprite blankImage;

    private GameManager board;
    private void Awake()
    {
        if (button == null) button = GetComponent<Button>();
        if (image == null) image = GetComponent<Image>();

        if(xImage==null) xImage = Resources.Load<Sprite>("x");
        if(oImage==null) oImage = Resources.Load<Sprite>("o");
        if(blankImage==null) blankImage = Resources.Load<Sprite>("b");

        if (mp == null) mp = FindFirstObjectByType<MultiplayerManager>();
    }

    private void OnEnable()
    {
        board = GameManager.Instance;

        button.onClick.AddListener(OnButtonClick);

        if (board != null)
        {
            board.OnCellChanged += OnCellChanged;
            board.OnGameFinished += OnGameFinished;
            board.OnReset += OnReset;
        }
    }

    private void OnDisable()
    {
        button.onClick.RemoveListener(OnButtonClick);

        if (board != null)
        {
            board.OnCellChanged -= OnCellChanged;
            board.OnGameFinished -= OnGameFinished;
            board.OnReset -= OnReset;
        }

        board = null;
    }

    private void OnReset()
    {
        image.color = defaultColor;
    }

    private void OnCellChanged(int index, int newValue)
    {
        if (index != cellIndex) return;

        if (newValue == 0)
        {
            image.sprite = blankImage;
            image.color = defaultColor;
            return;
        }

        image.sprite = (newValue == 1) ? xImage : oImage;
        image.color = defaultColor;
    }

    private void OnGameFinished(int winnerValue, bool isWin, int[] winLine)
    {
        if (!isWin || winLine == null)
        {
            image.color = failedColor;
            return;
        }
        foreach (int i in winLine)
        {
            if (i == cellIndex)
            {
                image.color = winColor;
                return;
            }
        }
        image.color = failedColor;
    }


    public void SetInteractable(bool enabled)
    {
        button.interactable = enabled;
    }

    private void OnButtonClick()
    {
        if (board == null) return;
        if (GameModeConfig.Mode == GameMode.Online)
        {
            mp.SendMove(cellIndex);
            return;
        }
        bool isXTurn = GameManager.Instance.GetTurn();
        int value = isXTurn ? 1 : 2;
        board.TryPlayMove(cellIndex, value);
    }
}
