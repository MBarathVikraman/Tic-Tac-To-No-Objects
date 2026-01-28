using UnityEngine;

public class InputDisabler : MonoBehaviour
{
    private Cell[] cells;
    public bool InputEnabled { get; private set; } = true;

    private void Awake()
    {
        cells = FindObjectsByType<Cell>(FindObjectsInactive.Include,FindObjectsSortMode.None);
        SetInputEnabled(true);
    }

    public void RefreshCells()
    {
        cells = FindObjectsByType<Cell>(FindObjectsInactive.Include,FindObjectsSortMode.None);
    }

    public void SetInputEnabled(bool enabled)
    {
        InputEnabled = enabled;

        if (cells == null) return;
        foreach (var c in cells)
            c.SetInteractable(enabled);
    }
}
