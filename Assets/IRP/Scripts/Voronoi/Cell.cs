using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Cell class. Contain all the information needed for each cell within the Voronoi
 * diagram
 */
public class Cell
{
    //The site position for this ccell
    public DVector2 Site;
    //Thr first edge for this cell
    public vEdge Edge;
    //ID for this cell( Optional )
    public int ID;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="newSite"></param>
    public Cell(DVector2 newSite)
    {
        Site = newSite;
    }
}
