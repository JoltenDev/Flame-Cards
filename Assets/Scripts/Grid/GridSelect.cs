using System.Collections.Generic;
using UnityEngine;

public class GridSelect : MonoBehaviour
{
    [SerializeField] Material defaultGrid;
    [SerializeField] Material avaliableGrid;
    [SerializeField] Material unavaliableGrid;

    GridBuilder gridBuilder;

    void Awake()
    {
        gridBuilder = GetComponent<GridBuilder>();
    }

    public int GetLength()
    {
        return gridBuilder.GetLength();
    }

    public void SelectBlocks(int position, int fSteps, int bSteps, int lSteps, int rSteps, int flSteps, int frSteps, int blSteps, int brSteps, bool elevated = false)
    {
        DeselectBlocks();
        MakeBlocksUnavailable();

        var blocks = gridBuilder.GetBlocks();

        position--;

        HighlightPath(blocks, position, 1, 0, fSteps, elevated);
        HighlightPath(blocks, position, -1, 0, bSteps, elevated);
        HighlightPath(blocks, position, 0, -1, lSteps, elevated);
        HighlightPath(blocks, position, 0, 1, rSteps, elevated);

        HighlightPath(blocks, position, 1, -1, flSteps, elevated);
        HighlightPath(blocks, position, 1, 1, frSteps, elevated);
        HighlightPath(blocks, position, -1, -1, blSteps, elevated);
        HighlightPath(blocks, position, -1, 1, brSteps, elevated);
    }

    public void DeselectBlocks()
    {
        MakeBlocksAvailable();
    }

    void HighlightPath(List<Transform> blocks, int position, int rowDirection, int columnDirection, int steps, bool elevated = false)
    {
        int length = GetLength();
        int totalRows = Mathf.CeilToInt((float)blocks.Count / length);

        int startRow = position / length;
        int startColumn = position % length;

        bool blockPath = false;

        for (int i = 1; i <= steps; i++)
        {
            int row = startRow + rowDirection * i;
            int column = startColumn + columnDirection * i;

            if (row < 0 || row >= totalRows || column < 0 || column >= length)
            {
                break;
            }

            int index = row * length + column;

            if (index < 0 || index >= blocks.Count)
                break;

            bool blockOccupied = blocks[index].GetComponent<GridBlock>().Occupied<GridItem>(out GridItem item);

            if (!blockPath && blockOccupied)
            {
                if (!(item.TryGetComponent<Wall>(out Wall wall) && elevated))
                    blockPath = true;
            }

            HighlightBlock(blocks, index, blockPath);
        }
    }

    void HighlightBlock(List<Transform> blocks, int index, bool blocked)
    {
        if (index < 0 || index >= blocks.Count)
            return;

        var gridBlock = blocks[index].GetComponent<GridBlock>();
        var renderer = blocks[index].GetComponent<MeshRenderer>();

        if (!blocked)
        {
            renderer.material = avaliableGrid;
            gridBlock.Available = true;
        }
        else
        {
            renderer.material = unavaliableGrid;
            gridBlock.Available = false;
        }
    }

    void MakeBlocksAvailable()
    {
        var blocks = gridBuilder.GetBlocks();

        for (int i = 0; i < blocks.Count; i++)
        {
            if (!blocks[i].GetComponent<GridBlock>().Occupied<GridItem>(out GridItem item))
            {
                blocks[i].GetComponent<GridBlock>().Available = true;
            }

            blocks[i].GetComponent<MeshRenderer>().material = defaultGrid;
        }
    }

    void MakeBlocksUnavailable()
    {
        var blocks = gridBuilder.GetBlocks();

        for (int i = 0; i < blocks.Count; i++)
        {
            if (!blocks[i].GetComponent<GridBlock>().Occupied<GridItem>(out GridItem item))
            {
                blocks[i].GetComponent<GridBlock>().Available = false;
            }
        }
    }
}
