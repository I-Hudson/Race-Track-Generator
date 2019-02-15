using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Vertex class. Stores all needed information about each vertex within the
 * voronoi diagram.
 */
public class vVertex
{
    //position of this vertex
    public DVector2 Site;
    //ID for this cell( Optional )
    public int ID;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="a_site"></param>
    public vVertex(DVector2 a_site)
    {
        Site = a_site;
    }
}
