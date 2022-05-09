using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid : MonoBehaviour
{
    [Header("Grid Settings")]
    [SerializeField] private Sprite _cellBackground;

    [Tooltip("Remember to set alpha value to 1 in order to see background!")]
    [SerializeField] private Color[] _cellBackgroundColors;

    [SerializeField] private int _columns;
    [SerializeField] private int _rows;

    [Header("Cell Settings")]
    [SerializeField] private Vector2 _cellDimension;

    [SerializeField] private float _swapDuration;
    [SerializeField] private float _fallDuration;

    private GameObject _selectedCell;
    private List<GameObject> _cellsList = new List<GameObject>();

    // Start is called before the first frame update
    private void Awake()
    {
        GenerateCells();
    }

    // Update is called once per frame
    private void Update()
    {
        HandleInput();
    }

    private void HandleInput()
    {
        if (Input.GetMouseButtonDown(0)) OnClick();
    }

    private void OnClick()
    {
        var mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        var hit = Physics2D.Raycast(mouseRay.origin, mouseRay.direction, Vector2.Distance(transform.position, mouseRay.origin));
        if (hit)
        {
            if (_selectedCell != null && AreNeighbors(_selectedCell, hit.transform.gameObject))
                SwapCell(_selectedCell, hit.transform.gameObject);
            else
                _selectedCell = hit.transform.gameObject;
        }
        else
            _selectedCell = null;
    }

    private void SwapCell(GameObject cellA, GameObject cellB)
    {
        var cellBPos = cellB.transform.position;
        LeanTween.move(cellB, cellA.transform.position, _swapDuration);
        LeanTween.move(cellA, cellBPos, _swapDuration);

        int indexA = _cellsList.IndexOf(cellA);
        int indexB = _cellsList.IndexOf(cellB);
        _cellsList[indexA] = cellB;
        _cellsList[indexB] = cellA;
    }

    // return true if cellA and cellB are neighbors to each other
    private bool AreNeighbors(GameObject cellA, GameObject cellB)
    {
        int aIndex = _cellsList.IndexOf(cellA);
        int bIndex = _cellsList.IndexOf(cellB);

        int northAIndex = aIndex - _columns;
        int southAIndex = aIndex + _columns;
        int westAIndex = aIndex - 1;
        int eastAIndex = aIndex + 1;

        return bIndex == northAIndex || bIndex == southAIndex || bIndex == westAIndex || bIndex == eastAIndex;
    }

    private void GenerateCells()
    {
        #region Assert

        Debug.Assert(_columns > 0 && _rows > 0, "Grid: No cell was made! Both row and column need to be greater than 0.");
        Debug.Assert(_cellDimension.x > 0 && _rows > 0, "Grid: Width and height of cell need to be greater than 0.");
        Debug.Assert(_cellBackground, "Grid: Cell background sprite is none!");
        Debug.Assert(_cellBackgroundColors.Length > 0, "Grid: Cell background colors are none!");
#if DEBUG
        if (_cellBackgroundColors.Length > 0)
            foreach (var color in _cellBackgroundColors)
                Debug.Assert(color.a > 0, "Grid: Some cell background colors are set to 0!");
#endif

        #endregion Assert

        int colorIndex = 0;
        for (int row = 0; row < _rows; row++)
        {
            for (int col = 0; col < _columns; col++)
            {
                // Instantiate cell
                var cell = new GameObject();
                _cellsList.Add(cell);
                var cellPos = new Vector3(transform.position.x + _cellDimension.x * col, transform.position.y + _cellDimension.y * row, transform.position.z);
                cell.transform.position = cellPos;
                cell.name = "Cell" + (_cellsList.Count - 1);
                var collider = cell.AddComponent<BoxCollider2D>(); // for Raycast2D
                collider.size = _cellDimension;
                cell.AddComponent<SpriteRenderer>();

                // Instantiate cell background
                var cellBackground = new GameObject();
                cellBackground.transform.position = cellPos;
                var renderer = cellBackground.AddComponent<SpriteRenderer>();
                renderer.sprite = _cellBackground;
                renderer.color = _cellBackgroundColors[colorIndex++ % _cellBackgroundColors.Length];
            }
            colorIndex++;
        }
    }
}