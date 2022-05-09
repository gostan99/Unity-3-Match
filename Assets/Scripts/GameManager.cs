using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    #region Grid Settings

    [Header("Grid Settings")]
    [SerializeField] private int _columns;

    [SerializeField] private int _rows;

    #endregion Grid Settings

    #region Cell Settings

    [Header("Cell Settings")]
    [SerializeField] private Sprite _cellBackground;

    [Tooltip("Remember to set alpha value to 1 in order to see background!")]
    [SerializeField] private Color[] _cellBackgroundColors;

    [SerializeField] private Sprite[] _cellSprites;

    [SerializeField] private Vector2 _cellDimension;

    [SerializeField] private float _swapDuration;
    [SerializeField] private float _fallDuration;
    [SerializeField] private float _fadeFrom = 0.3f;
    [SerializeField] private float _fadeDuration;

    #endregion Cell Settings

    private float _cellSpriteAlpha = 1;
    private int _leanTweenFadingEffectID;

    private GameObject _selectedCell;
    private List<GameObject> _cellsList = new List<GameObject>();

    // Start is called before the first frame update
    private void Awake()
    {
        GenerateCellsAndCellBackgrounds();
        _leanTweenFadingEffectID = LeanTween.value(_fadeFrom, _cellSpriteAlpha, _fadeDuration).setOnUpdate(CellSpriteFadingEffect).setLoopPingPong().id;
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
        if (!hit)
        {
            if (_selectedCell)
            {
                StopSelectedCellSpriteFadingEffect();
            }
            _selectedCell = null;
            return;
        }

        if (!_selectedCell && hit.transform.tag == "Cell")
        {
            _selectedCell = hit.transform.gameObject;
            LeanTween.resume(_leanTweenFadingEffectID);
            return;
        }

        if (!AreNeighbors(_selectedCell, hit.transform.gameObject))
        {
            ResetSelectedCellSpriteAlpha();
            _selectedCell = hit.transform.gameObject;
            return;
        }

        SwapCell(_selectedCell, hit.transform.gameObject);
        if (TryExplode())
        {
            FallDownToFillUpEmtySpace();
        }
        StopSelectedCellSpriteFadingEffect();
        _selectedCell = null;
    }

    // Perform explosion if can and return true otherwise return false
    private bool TryExplode()
    {
        return true;
    }

    private void FallDownToFillUpEmtySpace()
    {
    }

    private void StopSelectedCellSpriteFadingEffect()
    {
        LeanTween.pause(_leanTweenFadingEffectID);
        ResetSelectedCellSpriteAlpha();
    }

    private void CellSpriteFadingEffect(float val)
    {
        if (!_selectedCell) return;
        var renderer = _selectedCell.GetComponent<SpriteRenderer>();
        var color = renderer.color;
        renderer.color = new Color(color.r, color.g, color.b, val);
    }

    private void ResetSelectedCellSpriteAlpha()
    {
        var renderer = _selectedCell.GetComponent<SpriteRenderer>();
        var color = renderer.color;
        renderer.color = new Color(color.r, color.g, color.b, _cellSpriteAlpha);
    }

    private void OnCellEmpty(GameObject cell)
    {
    }

    private void SwapCell(GameObject cellA, GameObject cellB)
    {
        int indexA = _cellsList.IndexOf(cellA);
        int indexB = _cellsList.IndexOf(cellB);
        _cellsList[indexA] = cellB;
        _cellsList[indexB] = cellA;

        // Swap animation
        var cellBPos = cellB.transform.position;
        LeanTween.move(cellB, cellA.transform.position, _swapDuration);
        LeanTween.move(cellA, cellBPos, _swapDuration);
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

    private void GenerateCellsAndCellBackgrounds()
    {
        #region Assert

        Debug.Assert(_columns > 0 && _rows > 0, "Grid: No cell was made! Both row and column need to be greater than 0.");
        Debug.Assert(_cellDimension.x > 0 && _rows > 0, "Grid: Width and height of cell need to be greater than 0.");
        Debug.Assert(_cellBackground, "Grid: Cell background sprite is none!");
        Debug.Assert(_cellBackgroundColors.Length > 0, "Grid: Cell background colors are none!");
        Debug.Assert(_cellSprites.Length > 0, "Grid: Cell sprites are none!");
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
                var collider = cell.AddComponent<BoxCollider2D>(); // for Raycast2D
                var cellRenderer = cell.AddComponent<SpriteRenderer>();
                var cellPos = new Vector3(transform.position.x + _cellDimension.x * col, transform.position.y + _cellDimension.y * row, transform.position.z);
                cell.transform.position = cellPos;
                cell.tag = "Cell";
                collider.size = _cellDimension;
                cellRenderer.sortingOrder = 1;
                cellRenderer.sprite = GetRandomCellSprite();
                _cellsList.Add(cell);
                cell.name = "Cell" + (_cellsList.Count - 1);

                // Instantiate cell background
                var cellBackground = new GameObject();
                cellBackground.transform.position = cellPos;
                var bgRenderer = cellBackground.AddComponent<SpriteRenderer>();
                bgRenderer.sortingOrder = 0;
                bgRenderer.sprite = _cellBackground;
                bgRenderer.color = _cellBackgroundColors[colorIndex++ % _cellBackgroundColors.Length];
            }
            colorIndex++;
        }
    }

    private Sprite GetRandomCellSprite()
    {
        int index = Random.Range(0, _cellSprites.Length);
        return _cellSprites[index];
    }
}