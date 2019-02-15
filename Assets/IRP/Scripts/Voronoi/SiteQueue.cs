using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Site queue class. Store all the sites from highest y value to lowest
 */
public class SiteQueue
{
    int count;
    Cell[] cells;

    /// <summary>
    /// constructor
    /// </summary>
    /// <param name="a_size"></param>
    public SiteQueue(int a_size)
    {
        cells = new Cell[a_size];
    }

    /// <summary>
    /// Check if thje queue is empty
    /// </summary>
    /// <returns></returns>
    public bool IsEmpty()
    {
        return count == 0;
    }

    /// <summary>
    ///  Push new Cell into cells array
    /// </summary>
    /// <param name="a_newCell"></param>
    public void Push(Cell a_newCell)
    {
        int i = count++;
        // Sort cell depending on y value
        while((i>0) && (cells[i >> 1].Site.y < a_newCell.Site.y))
        {
            cells[i] = cells[i >> 1];
            i = i >> 1;
        };
        cells[i] = a_newCell;
    }

    /// <summary>
    ///  Return the top cell in cells
    /// </summary>
    /// <returns></returns>
    public Cell Top()
    {
        return cells[0];
    }

    /// <summary>
    /// Return top cell and remove it from cells
    /// </summary>
    /// <returns></returns>
    public Cell Pop()
    {
        Cell maxElement = cells[0];
        cells[0] = cells[--count];
        Heapify(0);
        return maxElement;
    }

    /// <summary>
    /// Return the number of sites in the queue
    /// </summary>
    /// <returns></returns>
    public int Count()
    {
        return count;
    }

    /// <summary>
    /// Sort cells into a heap structure
    /// </summary>
    /// <param name="a_i"></param>
    void Heapify(int a_i)
    {
        int largest = a_i;
        int left = a_i << 1;
        int right = left + 1;
        if ((left < count) && (cells[left].Site.y > cells[largest].Site.y)) largest = left;
        if ((right < count) && (cells[right].Site.y > cells[largest].Site.y)) largest = right;
        if(largest == a_i)
        {
            return;
        }
        Cell swap = cells[a_i];
        cells[a_i] = cells[largest];
        cells[largest] = swap;
        Heapify(largest);
    }
}
