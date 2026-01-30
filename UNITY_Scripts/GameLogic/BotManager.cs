using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BotManager : MonoBehaviour
{
    [SerializeField] private float delay = 0.35f;

    private GameManager board;
    [SerializeField]private InputDisabler input;
    private bool gameOver;

    private void OnEnable()
    {
        board = GameManager.Instance;
        board.OnMoveMade += OnMoveMade;
        board.OnGameFinished += OnGameFinished;
        board.OnReset += OnReset;
        input.SetInputEnabled(true);
        gameOver = false;
    }

    private void OnDisable()
    {
        if (board != null)
        {
            board.OnMoveMade -= OnMoveMade;
            board.OnGameFinished -= OnGameFinished;
            board.OnReset -= OnReset;
        }

        StopAllCoroutines();
        board = null;
        input = null;
    }

    private void OnReset()
    {
        gameOver = false;
        input.RefreshCells();
        input.SetInputEnabled(true);
        StopAllCoroutines();
    }

    private void OnGameFinished(int winner, bool isWin, int[] winline)
    {
        gameOver = true;
        StopAllCoroutines();
    }

    private void OnMoveMade(int cellId, int value)
    {
        if (gameOver) return;

        if (!GameManager.Instance.GetTurn())
        {
            input.SetInputEnabled(false);
            StopAllCoroutines();
            StartCoroutine(BotMoveRoutine());
        }
        else
        {
            input.SetInputEnabled(true);
        }
    }

    private IEnumerator BotMoveRoutine()
    {
        yield return new WaitForSeconds(delay);
        if (gameOver) yield break;

        int move = ChooseMove();
        if (move == -1)
        {
            yield break;
        }

        board.TryPlayMove(move, 2);
    }

    private int ChooseMove()
    {
        int[] b = board.GetBoardCopy();

        List<int> empty = new();
        for (int i = 0; i < b.Length; i++)
            if (b[i] == 0)
                empty.Add(i);

        if (empty.Count == 0) return -1;

        const int O = 2; // bot
        const int X = 1; // player

        //win if possible
        int winMove = FindLineCompletionMove(b, O);
        if (winMove != -1) return winMove;

        //block player
        int blockMove = FindLineCompletionMove(b, X);
        if (blockMove != -1) return blockMove;

        //random
        return empty[Random.Range(0, empty.Count)];
    }


    private static readonly int[][] winConditions =
{
    new[] { 0, 1, 2 },
    new[] { 3, 4, 5 },
    new[] { 6, 7, 8 },
    new[] { 0, 3, 6 },
    new[] { 1, 4, 7 },
    new[] { 2, 5, 8 },
    new[] { 0, 4, 8 },
    new[] { 2, 4, 6 }
};

private int FindLineCompletionMove(int[] b, int target)
{
    foreach (var line in winConditions)
    {
        int a = line[0], c = line[1], d = line[2];

        int targetCount = 0;
        int emptyIndex = -1;

        if (b[a] == target) targetCount++;
        else if (b[a] == 0) emptyIndex = a;

        if (b[c] == target) targetCount++;
        else if (b[c] == 0) emptyIndex = c;

        if (b[d] == target) targetCount++;
        else if (b[d] == 0) emptyIndex = d;

        // exactly 2 in line + 1 empty
        if (targetCount == 2 && emptyIndex != -1)
            return emptyIndex;
    }

    return -1;
}

    /*
    private int ChooseRandomMove()
    {
        List<Cell> cells = board.GetCells();
        List<int> empty = new();

        for (int i = 0; i < cells.Count; i++)
        {
            if (cells[i].IsInteractive && cells[i].Value == 0)
                empty.Add(i);
        }
        if (empty.Count == 0) return -1;
        return empty[Random.Range(0, empty.Count)];
    }
    */

}
