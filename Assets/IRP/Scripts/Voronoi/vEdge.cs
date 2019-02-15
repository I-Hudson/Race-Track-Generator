using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Edge class. This wil store all information needed for each edge.
 * Edges are store as linked lists. 
 */
public class vEdge
{
    //cell this edge is connected two
    public Cell Cell;
    //edge which shares the same space but is not connect to the same cell
    public vEdge Twin;
    //next edge connect to this edge
    public vEdge Next;
    //previous edge connect to this edge
    public vEdge Previous;
    //vertex 0 of edge
    public vVertex v0;
    //vertex 1 of edge
    public vVertex v1;
    //ID for this cell( Optional )
    public int id;
}
