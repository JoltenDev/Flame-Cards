using System;
using System.Collections.Generic;
using UnityEngine;

public class GridHighlight : MonoBehaviour
{
    [SerializeField] Material defaultGrid;
    [SerializeField] Material avaliableGrid;
    [SerializeField] Material unavaliableGrid;

    GridBuilder gridBuilder;

    void Awake()
    {
        gridBuilder = GetComponent<GridBuilder>();
    }

    public void HighlightBlocks(int position, int fSteps, int bSteps, int lSteps, int rSteps, int flSteps, int frSteps, int blSteps, int brSteps)
    {
        var blocks = gridBuilder.GetBlocks();

        position--;

        HighlightDirection(blocks, position, 1, 0, fSteps);
        HighlightDirection(blocks, position, -1, 0, bSteps);
        HighlightDirection(blocks, position, 0, -1, lSteps);
        HighlightDirection(blocks, position, 0, 1, rSteps);

        HighlightDirection(blocks, position, 1, -1, flSteps);
        HighlightDirection(blocks, position, 1, 1, frSteps);
        HighlightDirection(blocks, position, -1, -1, blSteps);
        HighlightDirection(blocks, position, -1, 1, brSteps);
    }

    private void HighlightDirection(List<Transform> blocks, int position, int rowDirection, int columnDirection, int steps)
    {
        int length = GetLength();
        int totalRows = Mathf.CeilToInt((float)blocks.Count / length);

        int startRow = position / length;
        int startColumn = position % length;

        bool blocked = false;

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

            if (!blocked)
            {
                blocked = blocks[index].GetComponent<GridBlock>().HasCard();
            }

            HighlightBlock(blocks, index, blocked);
        }
    }

    private int GetLength()
    {
        return gridBuilder.GetLength();
    }

    private void HighlightBlock(List<Transform> blocks, int index, bool blocked)
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

    public void UnHighlightBlocks()
    {
        var blocks = gridBuilder.GetBlocks();

        for (int i = 0; i < blocks.Count; i++)
        {
            blocks[i].GetComponent<MeshRenderer>().material = defaultGrid;
        }
    }

    public void MakeBlocksAvailable()
    {
        var blocks = gridBuilder.GetBlocks();

        for (int i = 0; i < blocks.Count; i++)
        {
            blocks[i].GetComponent<GridBlock>().Available = true;
        }
    }

    public void MakeBlocksUnavailable()
    {
        var blocks = gridBuilder.GetBlocks();

        for (int i = 0; i < blocks.Count; i++)
        {
            blocks[i].GetComponent<GridBlock>().Available = false;
        }
    }
}
