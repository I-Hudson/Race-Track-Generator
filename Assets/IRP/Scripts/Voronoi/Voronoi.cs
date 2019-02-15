using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Voronoi : MonoBehaviour
{
    //Box struct
    public struct Box
    {
        public double top;
        public double left;
        public double bottom;
        public double right;
    }

    /*
     * Should the voronoi controls be displayed
     */
    [SerializeField]
    private bool voronoiFoldOut = false;
    public bool VoronoiFoldOut
    { get { return voronoiFoldOut; } set { voronoiFoldOut = value; } }

    /*
     * Should the voornoi diagram be displayed to the user
     */
    [SerializeField]
    private bool displayVoronoi = false;
    public bool DisplayVoronoi
    { get { return displayVoronoi; } set { displayVoronoi = value; } }

    /*
     * Num of cells to include in spline to get the outter edge
     */
    [SerializeField]
    private int splineCellCount = 4;
    public int SplineCellCount
    { get { return splineCellCount; } set { splineCellCount = value; } }

    /*
     * Number of sites to generate
     */
    [SerializeField]
    private int siteCount = 100;
    public int SiteCount
    { get { return siteCount; } set { siteCount = value; } }

    /*
     * Size of the diagram
     */
    [SerializeField]
    private float gridSize = 1000;
    public float GridSize
    { get { return gridSize; } set { gridSize = value; } }

    /*
     * Num of total cells
     */
    [SerializeField]
    private int cellCount;
    public int CellCount
    { get { return cellCount; } }

    /*
     * Num of total edges
     */
    [SerializeField]
    private int edgeCount;
    public int EdgeCount
    { get { return edgeCount; } }

    /*
     * Num of total nodes
     */
    [SerializeField]
    private int nodeCount;
    public int NodeCount
    { get { return nodeCount; } }

    /*
     * All the cells on the diagram
     */
    [SerializeField]
    private Cell[] cells;
    public Cell[] Cells
    { get { return cells; } }

    /// <summary>
    /// Creates a new set of cells with the specified center sites
    /// </summary>
    /// <param name="a_newCellCount"></param>
    /// <param name="a_newSites"></param>
    public void SetCells(int a_newCellCount, DVector2[] a_newSites)
    {
        cellCount = a_newCellCount;
        cells = new Cell[cellCount];
        for (int i = 0; i < cellCount; i++)
        {
            cells[i] = new Cell(a_newSites[i]);
        }
    }

    /// <summary>
    /// Creates a new set of cells with random center sites contained within a bounding box
    /// </summary>
    /// <param name="a_newCellCount"></param>
    /// <param name="a_boxSize"></param>
    /// <param name="a_seed"></param>
    public void RandomCells(int a_newCellCount, float a_boxSize, int a_seed)
    {
        //set siteCount, cellCount and cells
        siteCount = a_newCellCount;
        cellCount = a_newCellCount;
        cells = new Cell[cellCount];

        //Init Unity's random
        UnityEngine.Random.InitState(a_seed);
        //get the x and y size of the box the diagram is in
        double xSize = a_boxSize - -a_boxSize;
        double ySize = a_boxSize - -a_boxSize;
        for (int i = 0; i < cellCount; i++)
        {
            //Create a new site
            DVector2 newSite;
            newSite.x = -a_boxSize + xSize * UnityEngine.Random.value;
            newSite.y = -a_boxSize + ySize * UnityEngine.Random.value;

            //check if two sites are too close
            bool siteWithinRange = false;
            for (int j = 0; j < cellCount; j++)
            {
                if(cells[j] != null && (newSite - cells[j].Site).Magnitude() < 35)
                {
                    //two sites are two close 
                    siteWithinRange = true;
                    //break from loop
                    break;
                }
            }
            //if siteWithinRange is true redo the current site
            if (siteWithinRange)
            {
                i--;
            }
            else
            {
                //sites not too close. Add new site
                cells[i] = new Cell(newSite);
            }
        }
    }

    /// <summary>
    /// Calculate the Voronoi graph from the current set of cells
    /// </summary>
    public void Calculate()
    {
        if(cellCount == 0)
        {
            return;
        }

        //new the siteQueue, circleQueue and beachLine
        SiteQueue siteQueue = new SiteQueue(cellCount);
        CircleQueue circleQueue = new CircleQueue();
        BeachLine beachLine = new BeachLine(circleQueue);

        //Add all the cells to the site queue
        for (int i = 0; i < cellCount; i++)
        {
            cells[i].Edge = null;
            siteQueue.Push(cells[i]);
        }

        //Get the first cell
        Cell firstCell = siteQueue.Pop();

        // check if the first two sites are the same on the y axis
        //if there are then move the first one up
        if (!siteQueue.IsEmpty())
        {
            Cell secondCell = siteQueue.Top();
            if(firstCell.Site.y.Equals(secondCell.Site.y))
            {
                firstCell.Site.y *= 1.00001f;
            }
        }

        //Ini the beachLine with the first cell
        beachLine.Initialize(firstCell);

        // Loop through the site queue and the circle queue, handling
        // whichever one has the next event

        //while there are sites to calculate
        while(!siteQueue.IsEmpty())
        {
            //check if the cicrcleQueu is empty or if the event on the top
            //of the circleQueue y value is less than the next site. (The cicrle event
            //has not passed the next site)
            if(circleQueue.IsEmpty() || (circleQueue.Top().yLock < siteQueue.Top().Site.y))
            {
                beachLine.ProcessSiteEvent(siteQueue.Pop());
            }
            else
            {
                beachLine.ProcessCircleEvent(circleQueue.Pop());
            }
        }

        //Finish off any remaining events in the circle queue
        while(!circleQueue.IsEmpty())
        {
            beachLine.ProcessCircleEvent(circleQueue.Pop());
        }

        //Find any parallel edges and link them tpgether
        beachLine.CloseOpenEdge();
    }

    /// <summary>
    /// Assigns an id to each cell, edge, and node.
    /// Diagram must be clipped
    /// </summary>
    public void SetIDs()
    {
        edgeCount = 0;
        nodeCount = 0;

        //Clear all the old IDs
        for (int i = 0; i < cellCount; i++)
        {
            vEdge firstEdge = cells[i].Edge;
            vEdge edge = firstEdge;
            do
            {
                edge.id = -1;
                edge.v0.ID = -1;
                edge = edge.Next;
            } while (edge != firstEdge);
        }

        //Set the IDs
        for (int i = 0; i < cellCount; i++)
        {
            Cell cell = cells[i];
            cell.ID = i;
            vEdge firstEdge = cell.Edge;
            vEdge edge = firstEdge;
            do
            {
                edge.id = edgeCount++;
                if (edge.v0.ID == -1)
                {
                    edge.v0.ID = nodeCount++;
                }
                edge = edge.Next;
            } while (edge != firstEdge);
        }
    }

    /// <summary>
    /// Clip the voronoi diagram to the bounding box
    /// </summary>
    /// <param name="a_size"></param>
    public void Clip(float a_size)
    {
        Box b = new Box();
        b.top = a_size;
        b.right = a_size;
        b.bottom = -a_size;
        b.left = -a_size;

        Clip(b);
    }

    /*
     * Clipping code
     */

    /// <summary>
    /// Clip the voronoi diagram to the bounding box
    /// </summary>
    /// <param name="a_bounds"></param>
    public void Clip(Box a_bounds)
    {
        //If there are no cells then return
        //There is are no cells to clip
        if(cellCount == 0)
        {
            return;
        }

        //Get the top left of the Bounding box
        DVector2 topLeft;
        topLeft.x = a_bounds.left;
        topLeft.y = a_bounds.top;

        //Get the top right of the Bounding box
        DVector2 topRight;
        topRight.x = a_bounds.right;
        topRight.y = a_bounds.top;

        //Get the bottom right of the Bounding box
        DVector2 bottomRight;
        bottomRight.x = a_bounds.right;
        bottomRight.y = a_bounds.bottom;

        //Get the bottom left of the Bounding box
        DVector2 bottomLeft;
        bottomLeft.x = a_bounds.left;
        bottomLeft.y = a_bounds.bottom;

        //Setup each bounds vertex 
        vVertex tlVer = new vVertex(topLeft);
        vVertex trVer = new vVertex(topRight);
        vVertex blVer = new vVertex(bottomLeft);
        vVertex brVer = new vVertex(bottomRight);

        Corners corners = FindCorners(a_bounds);

        DVector2 v;
        Cell cell;
        vEdge firstEdge;
        vEdge cornerEdge;
        vEdge clipEdge;
        vEdge nextCrossing;
        vEdge previousCrossing;
        vVertex vertex0;
        vVertex vertex1;

        //Handle special case. If there is only on cell
        if(cellCount == 1)
        {
            //Create a new edge to go from top left
            //to top right
            clipEdge = new vEdge();
            clipEdge.Cell = cells[0];
            clipEdge.v0 = tlVer;
            clipEdge.v1 = trVer;

            firstEdge = clipEdge;
            cornerEdge = clipEdge;

            //Create a new edge to go from top right
            //to bottom right
            clipEdge = new vEdge();
            clipEdge.Cell = cells[0];
            clipEdge.v0 = trVer;
            clipEdge.v1 = brVer;
            //Set the previous edge
            clipEdge.Previous = cornerEdge;
            //set the next edge
            cornerEdge.Next = clipEdge;

            cornerEdge = clipEdge;

            //Create a new edge to go from bottom right
            //to bottom left
            clipEdge = new vEdge();
            clipEdge.Cell = cells[0];
            clipEdge.v0 = brVer;
            clipEdge.v1 = blVer;
            clipEdge.Previous = cornerEdge;
            cornerEdge.Next = clipEdge;

            cornerEdge = clipEdge;

            //Create a new edge to go from bottom left
            //to top left
            clipEdge = new vEdge();
            clipEdge.Cell = cells[0];
            clipEdge.v0 = blVer;
            clipEdge.v1 = tlVer;
            clipEdge.Previous = cornerEdge;
            cornerEdge.Next = clipEdge;
            clipEdge.Next = firstEdge;
            firstEdge.Previous = clipEdge;

            //Set the cells edge to the firstEdge
            cells[0].Edge = firstEdge;

            return;
        }

        //Clip to the top
        
        //Check if the same cell which is in the top left 
        //is at the top right 
        if (corners.topLeft == corners.topRight)
        {
            //If the same cell is at topLeft and topRight
            //then create an edge to go from topLeft
            //to topRight
            cell = corners.topLeft;
            clipEdge = new vEdge();
            clipEdge.Cell = cell;
            clipEdge.Next = null;
            clipEdge.Previous = null;
            clipEdge.Twin = null;
            clipEdge.v0 = tlVer;
            clipEdge.v1 = trVer;

            firstEdge = clipEdge;
            cornerEdge = clipEdge;
        }
        //If the topLeft and topRight are differnt cells
        //then work from left to right
        //clipping all edges that go outside of the bounds
        else
        {
            //Set the inital cell to topLeft
            cell = corners.topLeft;
            nextCrossing = DownCrossing(a_bounds.top, cell);
            v.x = IntersectY(a_bounds.top, nextCrossing);
            v.y = a_bounds.top;
            vertex1 = new vVertex(v);

            //Set clipEdge to a new edge
            clipEdge = new vEdge();
            clipEdge.Cell = cell;
            //Set clipEdge's next edge to the edge
            //which has crossing to top boundary
            clipEdge.Next = nextCrossing;
            //Set clipEdge's previous edge to 
            //the same previous edge as the edge which has crossed 
            //the boundarys
            clipEdge.Previous = nextCrossing.Previous;
            clipEdge.Twin = null;
            //Set the vertexs of clipEdge
            clipEdge.v0 = tlVer;
            clipEdge.v1 = vertex1;

            //set firstEdge to clipEdge
            firstEdge = clipEdge;

            //if the edge previous to the clipped edge
            //is not null
            if (clipEdge.Previous != null)
            {
                //Set the previous's next edge to the clipped edge 
                //This is done so the previous edge.next is the 
                //clipped edge
                clipEdge.Previous.Next = clipEdge;
            }

            //set the nextCrossing edge previous edge
            //to clipEdge
            nextCrossing.Previous = clipEdge;
            //Set the nextCrossing vertex 0 to vertex1
            nextCrossing.v0 = vertex1;
            //set the cell's.edge to clipEdge
            cell.Edge = clipEdge;

            //Set vertex 0 to vertex 1
            //This sets are start vertex to the previous 
            //finish vertex
            vertex0 = vertex1;
            //set the cell to nexCrossing's twin cell
            cell = nextCrossing.Twin.Cell;

            //While cell is not the cell at the topRight of the 
            //diagram keep clipping the diagram
            while (cell != corners.topRight)
            {
                //Set the previous edge which had crossed the 
                //top boundary
                previousCrossing = nextCrossing.Twin;
                //Get the next edge which crossing the boundary
                nextCrossing = DownCrossing(a_bounds.top, cell);

                //Set v.x to where the edge intersects with the
                //top boundary
                v.x = IntersectY(a_bounds.top, nextCrossing);
                v.y = a_bounds.top;
                vertex1 = new vVertex(v);

                //Setup the new cliped edge 
                clipEdge = new vEdge();
                clipEdge.Cell = cell;
                clipEdge.Next = nextCrossing;
                clipEdge.Previous = previousCrossing;
                clipEdge.Twin = null;
                clipEdge.v0 = vertex0;
                clipEdge.v1 = vertex1;

                //Set the previousCrossing next edge to 
                //the new clipped edge
                previousCrossing.Next = clipEdge;
                //Set the nextCrossing previous edge to
                //the new clipped edge
                nextCrossing.Previous = clipEdge;

                //Set the previousCrossing vertexs
                previousCrossing.v1 = vertex0;
                nextCrossing.v0 = vertex1;
                //Set the cell's edge to the clipped edge
                cell.Edge = clipEdge;

                //Set vertex 0 to vertex1
                vertex0 = vertex1;
                //set the nextCrossing twin's cell to cell
                cell = nextCrossing.Twin.Cell;

            }

            //Set the previousCrossing to the twin edge
            //of nextCrossing
            previousCrossing = nextCrossing.Twin;

            //Create a new cornerEdge
            cornerEdge = new vEdge();
            //Set the corner's cell
            cornerEdge.Cell = cell;
            //Set the next edge
            cornerEdge.Next = previousCrossing.Next;
            //Set the previous edge
            cornerEdge.Previous = previousCrossing;
            //Set the twin to null as there is no 
            //twin edge(No neighbour cell)
            cornerEdge.Twin = null;
            //Set v0
            cornerEdge.v0 = vertex0;
            //Set v1
            cornerEdge.v1 = trVer;

            //Set the previousCrossing edge's next to
            //cornerEdge
            previousCrossing.Next = cornerEdge;
            //Set prviousCrossing edge v1 to vertex0
            previousCrossing.v1 = vertex0;

        }

        //Clip to the right

        //If the topRight cell is the same all the bottomRight cell
        //create a new edge to go from topRight to bottomRight
        if (corners.topRight == corners.bottomRight)
        {
            cell = corners.topRight;
            clipEdge = new vEdge();
            clipEdge.Cell = cell;
            clipEdge.Next = null;
            clipEdge.Previous = cornerEdge;
            clipEdge.Twin = null;
            clipEdge.v0 = trVer;
            clipEdge.v1 = brVer;

            cornerEdge.Next = clipEdge;
            cornerEdge = clipEdge;
        }
        else
        {
            //Set the inital cell to topRight
            cell = corners.topRight;
            nextCrossing = LeftCrossing(a_bounds.right, cell);
            v.x = a_bounds.right;
            v.y = IntersectX(a_bounds.right, nextCrossing);
            vertex1 = new vVertex(v);

            //Set clipEdge to a new edge
            clipEdge = new vEdge();
            clipEdge.Cell = cell;
            //Set clipEdge's next edge to the edge
            //which has crossing to top boundary
            clipEdge.Next = nextCrossing;
            //Set clipEdge's previous edge to 
            //the same previous edge as the edge which has crossed 
            //the boundarys
            clipEdge.Previous = cornerEdge;
            clipEdge.Twin = null;
            //Set the vertexs of clipEdge
            clipEdge.v0 = trVer;
            clipEdge.v1 = vertex1;

            //Set cornerEdge.next to clipEdge
            cornerEdge.Next = clipEdge;

            //set the nextCrossing edge previous edge
            //to clipEdge
            nextCrossing.Previous = clipEdge;
            //Set the nextCrossing vertex 0 to vertex1
            nextCrossing.v0 = vertex1;
            //set the cell's.edge to clipEdge
            cell.Edge = clipEdge;

            //Set vertex 0 to vertex 1
            //This sets are start vertex to the previous 
            //finish vertex
            vertex0 = vertex1;
            //set the cell to nexCrossing's twin cell
            cell = nextCrossing.Twin.Cell;

            //While cell is not the cell at the bottomRight of the 
            //diagram keep clipping the diagram
            while (cell != corners.bottomRight)
            {
                //Set the previous edge
                previousCrossing = nextCrossing.Twin;
                //Get the next edge which crossing the boundary
                nextCrossing = LeftCrossing(a_bounds.right, cell);

                //Set v.x to where the edge intersects with the
                //top boundary
                v.x = a_bounds.right;
                v.y = IntersectX(a_bounds.right, nextCrossing);
                vertex1 = new vVertex(v);

                //Setup the new cliped edge 
                clipEdge = new vEdge();
                clipEdge.Cell = cell;
                clipEdge.Next = nextCrossing;
                clipEdge.Previous = previousCrossing;
                clipEdge.Twin = null;
                clipEdge.v0 = vertex0;
                clipEdge.v1 = vertex1;

                //Set the previousCrossing next edge to 
                //the new clipped edge
                previousCrossing.Next = clipEdge;
                //Set the nextCrossing previous edge to
                //the new clipped edge
                nextCrossing.Previous = clipEdge;

                //Set the previousCrossing vertexs
                previousCrossing.v1 = vertex0;
                nextCrossing.v0 = vertex1;
                //Set the cell's edge to the clipped edge
                cell.Edge = clipEdge;

                //Set vertex 0 to vertex1
                vertex0 = vertex1;
                //set the nextCrossing twin's cell to cell
                cell = nextCrossing.Twin.Cell;

            }
            //Set the previousCrossing to the twin edge
            //of nextCrossing
            previousCrossing = nextCrossing.Twin;

            //Create a new cornerEdge
            cornerEdge = new vEdge();
            //Set the corner's cell
            cornerEdge.Cell = cell;
            //Set the next edge
            cornerEdge.Next = previousCrossing.Next;
            //Set the previous edge
            cornerEdge.Previous = previousCrossing;
            //Set the twin to null as there is no 
            //twin edge(No neighbour cell)
            cornerEdge.Twin = null;
            //Set v0
            cornerEdge.v0 = vertex0;
            //Set v1
            cornerEdge.v1 = brVer;

            //Set the previousCrossing edge's next to
            //cornerEdge
            previousCrossing.Next = cornerEdge;
            //Set prviousCrossing edge v1 to vertex0
            previousCrossing.v1 = vertex0;

        }

        //Clip to the bottom

        //If the bottomRight cell is the same all the bottomLeft cell
        //create a new edge to go from bottomRight to bottomLeft
        if (corners.bottomRight == corners.bottomLeft)
        {
            cell = corners.bottomRight;
            clipEdge = new vEdge();
            clipEdge.Cell = cell;
            clipEdge.Next = null;
            clipEdge.Previous = cornerEdge;
            clipEdge.Twin = null;
            clipEdge.v0 = brVer;
            clipEdge.v1 = blVer;

            cornerEdge.Next = clipEdge;
            cornerEdge = clipEdge;
        }
        else
        {
            //Set the inital cell to bottomRight
            cell = corners.bottomRight;
            nextCrossing = UpCrossing(a_bounds.bottom, cell);
            v.x = IntersectY(a_bounds.bottom, nextCrossing);
            v.y = a_bounds.bottom;
            vertex1 = new vVertex(v);

            //Set clipEdge to a new edge
            clipEdge = new vEdge();
            clipEdge.Cell = cell;
            //Set clipEdge's next edge to the edge
            //which has crossing to top boundary
            clipEdge.Next = nextCrossing;
            //Set clipEdge's previous edge to 
            //the same previous edge as the edge which has crossed 
            //the boundarys
            clipEdge.Previous = cornerEdge;
            clipEdge.Twin = null;
            //Set the vertexs of clipEdge
            clipEdge.v0 = brVer;
            clipEdge.v1 = vertex1;

            //Set cornerEdge.next to clipEdge
            cornerEdge.Next = clipEdge;

            //set the nextCrossing edge previous edge
            //to clipEdge
            nextCrossing.Previous = clipEdge;
            //Set the nextCrossing vertex 0 to vertex1
            nextCrossing.v0 = vertex1;
            //set the cell's.edge to clipEdge
            cell.Edge = clipEdge;

            //Set vertex 0 to vertex 1
            //This sets are start vertex to the previous 
            //finish vertex
            vertex0 = vertex1;
            //set the cell to nexCrossing's twin cell
            cell = nextCrossing.Twin.Cell;


            //While cell is not the cell at the bottomRight of the 
            //diagram keep clipping the diagram
            while (cell != corners.bottomLeft)
            {
                //Set the previous edge
                previousCrossing = nextCrossing.Twin;
                //Get the next edge which crossing the boundary
                nextCrossing = UpCrossing(a_bounds.bottom, cell);

                //Set v.x to where the edge intersects with the
                //top boundary
                v.x = IntersectY(a_bounds.bottom, nextCrossing);
                v.y = a_bounds.bottom;
                vertex1 = new vVertex(v);

                //Setup the new cliped edge 
                clipEdge = new vEdge();
                clipEdge.Cell = cell;
                clipEdge.Next = nextCrossing;
                clipEdge.Previous = previousCrossing;
                clipEdge.Twin = null;
                clipEdge.v0 = vertex0;
                clipEdge.v1 = vertex1;

                //Set the previousCrossing next edge to 
                //the new clipped edge
                previousCrossing.Next = clipEdge;
                //Set the nextCrossing previous edge to
                //the new clipped edge
                nextCrossing.Previous = clipEdge;

                //Set the previousCrossing vertexs
                previousCrossing.v1 = vertex0;
                nextCrossing.v0 = vertex1;
                //Set the cell's edge to the clipped edge
                cell.Edge = clipEdge;

                //Set vertex 0 to vertex1
                vertex0 = vertex1;
                //set the nextCrossing twin's cell to cell
                cell = nextCrossing.Twin.Cell;

            }
            //Set the previousCrossing to the twin edge
            //of nextCrossing
            previousCrossing = nextCrossing.Twin;

            //Create a new cornerEdge
            cornerEdge = new vEdge();
            //Set the corner's cell
            cornerEdge.Cell = cell;
            //Set the next edge
            cornerEdge.Next = previousCrossing.Next;
            //Set the previous edge
            cornerEdge.Previous = previousCrossing;
            //Set the twin to null as there is no 
            //twin edge(No neighbour cell)
            cornerEdge.Twin = null;
            //Set v0
            cornerEdge.v0 = vertex0;
            //Set v1
            cornerEdge.v1 = blVer;

            //Set the previousCrossing edge's next to
            //cornerEdge
            previousCrossing.Next = cornerEdge;
            //Set prviousCrossing edge v1 to vertex0
            previousCrossing.v1 = vertex0;

        }

        //Clip to the Left

        //If the bottomLeft cell is the same all the topLeft cell
        //create a new edge to go from bottomLeft to topLeft
        if (corners.bottomLeft == corners.topLeft)
        {
            cell = corners.bottomRight;
            clipEdge = new vEdge();
            clipEdge.Cell = cell;
            clipEdge.Next = firstEdge;
            clipEdge.Previous = cornerEdge;
            clipEdge.Twin = null;
            clipEdge.v0 = blVer;
            clipEdge.v1 = tlVer;

            cornerEdge.Next = clipEdge;
            cornerEdge = clipEdge;
        }
        else
        {
            //Set the inital cell to bottomLeft
            cell = corners.bottomLeft;
            nextCrossing = RightCrossing(a_bounds.left, cell);
            v.x = a_bounds.left;
            v.y = IntersectX(a_bounds.left, nextCrossing);
            vertex1 = new vVertex(v);

            //Set clipEdge to a new edge
            clipEdge = new vEdge();
            clipEdge.Cell = cell;
            //Set clipEdge's next edge to the edge
            //which has crossing to top boundary
            clipEdge.Next = nextCrossing;
            //Set clipEdge's previous edge to 
            //the same previous edge as the edge which has crossed 
            //the boundarys
            clipEdge.Previous = cornerEdge;
            clipEdge.Twin = null;
            //Set the vertexs of clipEdge
            clipEdge.v0 = blVer;
            clipEdge.v1 = vertex1;

            //Set cornerEdge.next to clipEdge
            cornerEdge.Next = clipEdge;

            //set the nextCrossing edge previous edge
            //to clipEdge
            nextCrossing.Previous = clipEdge;
            //Set the nextCrossing vertex 0 to vertex1
            nextCrossing.v0 = vertex1;
            //set the cell's.edge to clipEdge
            cell.Edge = clipEdge;

            //Set vertex 0 to vertex 1
            //This sets are start vertex to the previous 
            //finish vertex
            vertex0 = vertex1;
            //set the cell to nexCrossing's twin cell
            cell = nextCrossing.Twin.Cell;

            //While cell is not the cell at the bottomRight of the 
            //diagram keep clipping the diagram
            while (cell != corners.topLeft)
            {
                //Set the previous edge
                previousCrossing = nextCrossing.Twin;
                //Get the next edge which crossing the boundary
                nextCrossing = RightCrossing(a_bounds.left, cell);

                //Set v.x to where the edge intersects with the
                //top boundary
                v.x = a_bounds.left;
                v.y = IntersectX(a_bounds.left, nextCrossing);
                vertex1 = new vVertex(v);

                //Setup the new cliped edge 
                clipEdge = new vEdge();
                clipEdge.Cell = cell;
                clipEdge.Next = nextCrossing;
                clipEdge.Previous = previousCrossing;
                clipEdge.Twin = null;
                clipEdge.v0 = vertex0;
                clipEdge.v1 = vertex1;

                //Set the previousCrossing next edge to 
                //the new clipped edge
                previousCrossing.Next = clipEdge;
                //Set the nextCrossing previous edge to
                //the new clipped edge
                nextCrossing.Previous = clipEdge;

                //Set the previousCrossing vertexs
                previousCrossing.v1 = vertex0;
                nextCrossing.v0 = vertex1;
                //Set the cell's edge to the clipped edge
                cell.Edge = clipEdge;

                //Set vertex 0 to vertex1
                vertex0 = vertex1;
                //set the nextCrossing twin's cell to cell
                cell = nextCrossing.Twin.Cell;
            }
            //Set the previousCrossing to the twin edge
            //of nextCrossing
            previousCrossing = nextCrossing.Twin;

            //Create a new cornerEdge
            cornerEdge = new vEdge();
            //Set the corner's cell
            cornerEdge.Cell = cell;
            //Set the next edge
            cornerEdge.Next = firstEdge;
            //Set the previous edge
            cornerEdge.Previous = previousCrossing;
            //Set the twin to null as there is no 
            //twin edge(No neighbour cell)
            cornerEdge.Twin = null;
            //Set v0
            cornerEdge.v0 = vertex0;
            //Set v1
            cornerEdge.v1 = tlVer;

            //Set the previousCrossing edge's next to
            //cornerEdge
            previousCrossing.Next = cornerEdge;
            //Set prviousCrossing edge v1 to vertex0
            previousCrossing.v1 = vertex0;
        }
        firstEdge.Previous = cornerEdge;
    }

    /// <summary>
    /// Get the intersect X position
    /// </summary>
    /// <param name="a_x"></param>
    /// <param name="a_edge"></param>
    /// <returns></returns>
    private double IntersectX(double a_x, vEdge a_edge)
    {
        //Get both sites
        DVector2 site0 = a_edge.Cell.Site;
        DVector2 site1 = a_edge.Twin.Cell.Site;
        //Get the mid point between both sites
        DVector2 midPoint = (site0 + site1) / 2d;
        //Get the bisector point
        DVector2 bisector;
        bisector.x = (site1.y - site0.y);
        bisector.y = (site1.x - site0.x) * -1d;
        return midPoint.y + (a_x - midPoint.x) * bisector.y / bisector.x;
    }

    /// <summary>
    /// Get the intersect Y position
    /// </summary>
    /// <param name="a_x"></param>
    /// <param name="a_edge"></param>
    /// <returns></returns>
    private double IntersectY(double a_y, vEdge a_edge)
    {
        //Get both sites
        DVector2 site0 = a_edge.Cell.Site;
        DVector2 site1 = a_edge.Twin.Cell.Site;
        //Get the mid point between both sites
        DVector2 midPoint = (site0 + site1) / 2d;
        //Get the bisector point
        DVector2 bisector;
        bisector.x = (site1.y - site0.y);
        bisector.y = (site1.x - site0.x) * -1d;
        return midPoint.x + (a_y - midPoint.y) * bisector.x / bisector.y;
    }

    /// <summary>
    /// Finds the edge in a Cell that crosses the y bounds
    /// </summary>
    /// <param name="a_y"></param>
    /// <param name="a_cell"></param>
    /// <returns></returns>
    private vEdge UpCrossing(double a_y, Cell a_cell)
    {
        //Get the first edge from a_cell
        vEdge firstEdge = FirstEdge(a_cell);
        vEdge edge = firstEdge;
        //Check if the first edge is crossing the y bound
        //If it is then return it
        if(IsUpCrossing(a_y, edge))
        {
            return edge;
        }
        //Loop until a next edge is null or the next edge is equal to
        //the first edge
        while(edge.Next != null && edge.Next != firstEdge)
        {
            //Set edge to edge.next
            edge = edge.Next;
            //Check if edge is crossing the y bounds
            if(IsUpCrossing(a_y, edge))
            {
                return edge;
            }
        }
        return null;
    }

    /// <summary>
    /// Finds the edge in a Cell that crosses the y bounds
    /// </summary>
    /// <param name="a_y"></param>
    /// <param name="a_cell"></param>
    /// <returns></returns>
    private vEdge DownCrossing(double a_y, Cell a_cell)
    {
        //Get the first edge from a_cell
        vEdge firstEdge = FirstEdge(a_cell);
        vEdge edge = firstEdge;
        //Check if the first edge is crossing the y bound
        //If it is then return it
        if (IsDownCrossing(a_y, edge))
        {
            return edge;
        }
        //Loop until a next edge is null or the next edge is equal to
        //the first edge
        while (edge.Next != null && edge.Next != firstEdge)
        {
            //Set edge to edge.next
            edge = edge.Next;
            //Check if edge is crossing the y bounds
            if (IsDownCrossing(a_y, edge))
            {
                return edge;
            }
        }
        return null;
    }

    /// <summary>
    /// Finds the edge in a cell that crosses the x bounds 
    /// </summary>
    /// <param name="a_x"></param>
    /// <param name="a_cell"></param>
    /// <returns></returns>
    private vEdge RightCrossing(double a_x, Cell a_cell)
    {
        //Get the first edge from a_cell
        vEdge firstEdge = FirstEdge(a_cell);
        vEdge edge = firstEdge;
        //Check if the first edge is crossing the x bound
        //If it is then return it
        if (IsRightCrossing(a_x, edge))
        {
            return edge;
        }
        //Loop until a next edge is null or the next edge is equal to
        //the first edge
        while (edge.Next != null && edge.Next != firstEdge)
        {
            //Set edge to edge.next
            edge = edge.Next;
            //Check if edge is crossing the x bounds
            if (IsRightCrossing(a_x, edge))
            {
                return edge;
            }
        }
        return null;
    }

    /// <summary>
    /// Finds the edge in a cell that crosses the x bounds 
    /// </summary>
    /// <param name="a_x"></param>
    /// <param name="a_cell"></param>
    /// <returns></returns>
    private vEdge LeftCrossing(double a_x, Cell a_cell)
    {
        //Get the first edge from a_cell
        vEdge firstEdge = FirstEdge(a_cell);
        vEdge edge = firstEdge;
        //Check if the first edge is crossing the x bound
        //If it is then return it
        if (IsLeftCrossing(a_x, edge))
        {
            return edge;
        }
        //Loop until a next edge is null or the next edge is equal to
        //the first edge
        while (edge.Next != null && edge.Next != firstEdge)
        {
            //Set edge to edge.next
            edge = edge.Next;
            //Check if edge is crossing the x bounds
            if (IsLeftCrossing(a_x, edge))
            {
                return edge;
            }
        }
        return null;
    }

    #region IsCrossing Functions

    /// <summary>
    /// Check if an edge is heading up and crossing a bound
    /// </summary>
    /// <param name="a_y"></param>
    /// <param name="a_edge"></param>
    /// <returns></returns>
    private bool IsUpCrossing(double a_y, vEdge a_edge)
    {
        if (a_edge.Twin == null)
        {
            return false;
        }
        //If the edge's vertexs are not null
        //then check if the y value is not within
        //the y bounds
        if (a_edge.v0 != null && a_edge.v1 != null)
        {
            if(a_edge.v0.Site.y < a_y && a_edge.v1.Site.y > a_y)
            {
                return true;
            }
        }
        //If edge's cell x position is greater than 
        //the edge's Twin's cell x position
        //then we are moving right along the diagram. This is due to the twin cell being 
        //on our left
        else if(a_edge.Cell.Site.x > a_edge.Twin.Cell.Site.x)
        {
            if(a_edge.v0 != null && a_edge.v0.Site.y < a_y)
            {
                return true;
            }
            if(a_edge.v1 != null && a_edge.v1.Site.y > a_y)
            {
                return true;
            }
            if(a_edge.v0 == null && a_edge.v1 == null)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Check if an edge is heading down and crossing a bound
    /// </summary>
    /// <param name="a_y"></param>
    /// <param name="a_edge"></param>
    /// <returns></returns>
    private bool IsDownCrossing(double a_y, vEdge a_edge)
    {
        if (a_edge.Twin == null)
        {
            return false;
        }
        //If the edge's vertexs are not null
        //then check if the y value is not within
        //the y bounds
        if (a_edge.v0 != null && a_edge.v1 != null)
        {
            if (a_edge.v0.Site.y > a_y && a_edge.v1.Site.y < a_y)
            {
                return true;
            }
        }
        //If edge's cell x position is less than 
        //the edge's Twin's cell x position
        //then we are moving left along the diagram. This is due to the twin cell being 
        //on our right 
        else if (a_edge.Cell.Site.x < a_edge.Twin.Cell.Site.x)
        {
            if (a_edge.v0 != null && a_edge.v0.Site.y > a_y)
            {
                return true;
            }
            if (a_edge.v1 != null && a_edge.v1.Site.y < a_y)
            {
                return true;
            }
            if (a_edge.v0 == null && a_edge.v1 == null)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Check if an edge is heading right and crossing a bound
    /// </summary>
    /// <param name="a_x"></param>
    /// <param name="a_edge"></param>
    /// <returns></returns>
    private bool IsRightCrossing(double a_x, vEdge a_edge)
    {
        if (a_edge.Twin == null)
        {
            return false;
        }
        //If the edge's vertexs are not null
        //then check if the x value is not within
        //the x bounds
        if (a_edge.v0 != null && a_edge.v1 != null)
        {
            if (a_edge.v0.Site.x < a_x && a_edge.v1.Site.x > a_x)
            {
                return true;
            }
        }
        //If edge's cell y position is less than 
        //the edge's Twin's cell y position
        //then we are moving down along the diagram. This is due to the twin's cell
        //being above us
        else if (a_edge.Cell.Site.y < a_edge.Twin.Cell.Site.y)
        {
            if (a_edge.v0 != null && a_edge.v0.Site.x < a_x)
            {
                return true;
            }
            if (a_edge.v1 != null && a_edge.v1.Site.x > a_x)
            {
                return true;
            }
            if (a_edge.v0 == null && a_edge.v1 == null)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Check if an edge is heading left and crossing a bound
    /// </summary>
    /// <param name="a_x"></param>
    /// <param name="a_edge"></param>
    /// <returns></returns>
    private bool IsLeftCrossing(double a_x, vEdge a_edge)
    {
        if (a_edge.Twin == null)
        {
            return false;
        }
        //If the edge's vertexs are not null
        //then check if the x value is not within
        //the x bounds
        if (a_edge.v0 != null && a_edge.v1 != null)
        {
            if (a_edge.v0.Site.x > a_x && a_edge.v1.Site.x < a_x)
            {
                return true;
            }
        }
        //If edge's cell y position is greater than 
        //the edge's Twin's cell y position
        //then we are moving up along the diagram. This is due to our twin cell
        //being below us
        else if (a_edge.Cell.Site.y > a_edge.Twin.Cell.Site.y)
        {
            if (a_edge.v0 != null && a_edge.v0.Site.x > a_x)
            {
                return true;
            }
            if (a_edge.v1 != null && a_edge.v1.Site.x < a_x)
            {
                return true;
            }
            if (a_edge.v0 == null && a_edge.v1 == null)
            {
                return true;
            }
        }
        return false;
    }

    #endregion

    /// <summary>
    /// Find each cell in the four corners of the diagram
    /// </summary>
    /// <param name="bounds"></param>
    /// <returns></returns>
    private Corners FindCorners(Box bounds)
    {
        //create DVector2 for the 4 corners
        DVector2 topLeft;
        DVector2 topRight;
        DVector2 bottomLeft;
        DVector2 bottomRight;
        DVector2 site = cells[0].Site;

        //set each DVector to the correct corner
        topLeft.x = bounds.left;
        topLeft.y = bounds.top;
        topRight.x = bounds.right;
        topRight.y = bounds.top;
        bottomLeft.x = bounds.left;
        bottomLeft.y = bounds.bottom;
        bottomRight.x = bounds.right;
        bottomRight.y = bounds.bottom;

        //Get the distance from each corner to the site
        double tlDistance = (topLeft - site).SqrMagnitude();
        double trDistance = (topRight - site).SqrMagnitude();
        double blDistance = (bottomLeft - site).SqrMagnitude();
        double brDistance = (bottomRight - site).SqrMagnitude();
        double testDistance;

        //set each corner site to cells[0]
        Corners corners;
        corners.topLeft = cells[0];
        corners.topRight = cells[0];
        corners.bottomLeft = cells[0];
        corners.bottomRight = cells[0];

        for (int i = 1; i < cellCount; i++)
        {
            site = cells[i].Site;

            //get the distance from the topLeft to site
            testDistance = (topLeft - site).SqrMagnitude();
            //if testDistance is less than tlDistance
            if (testDistance < tlDistance)
            {
                //set tlDistance to testDistance
                tlDistance = testDistance;
                //set corner.topLeft to the current cells
                corners.topLeft = cells[i];
            };

            //get the distance from the topRight to site
            testDistance = (topRight - site).SqrMagnitude();
            //if testDistance is less than trDistance
            if (testDistance < trDistance)
            {
                //set trDistance to testDistance
                trDistance = testDistance;
                //set corner.topRight to the current cells
                corners.topRight = cells[i];
            };

            //get the distance from the bottomLeft to site
            testDistance = (bottomLeft - site).SqrMagnitude();
            //if testDistance is less than blDistance
            if (testDistance < blDistance)
            {
                //set blDistance to testDistance
                blDistance = testDistance;
                //set corner.bottomLeft to the current cells
                corners.bottomLeft = cells[i];
            };

            //get the distance from the bottomRight to site
            testDistance = (bottomRight - site).SqrMagnitude();
            //if testDistance is less than brDistance
            if (testDistance < brDistance)
            {
                //set brDistance to testDistance
                brDistance = testDistance;
                //set corner.bottomRight to the current cells
                corners.bottomRight = cells[i];
            };

        };

        return corners;

    }

    /// <summary>
    /// Find the first edge of a cell
    /// </summary>
    /// <param name="cell"></param>
    /// <returns></returns>
    public vEdge FirstEdge(Cell cell)
    {
        vEdge first = cell.Edge;
        vEdge edge = first;
        while (edge.Previous != null)
        {
            edge = edge.Previous;
            if (edge == first)
            {
                return first;
            }
        }
        return edge;
    }

    /// <summary>
    /// Get all the outter edges of random cells
    /// </summary>
    /// <returns></returns>
    public List<vEdge> SplineOutterEdge()
    {
        if(cellCount == 0 || cells == null)
        {
            Debug.Log("VORONOI: No cells have been generated. Please click the 'Generate Voronoi Diagram'");
            return null;
        }

        //Create a list of cells for all the selected cells
        List<Cell> selectedCells = new List<Cell>();
        //ref the number of cells added to the selectedCells list
        int cellsAddedCount = 0;

        //while cellsAddedCount is less than numOfCells
        //add more cells
        while (cellsAddedCount < SplineCellCount)
        {
            //if selectedCells.Count to 0
            //we are on our first cell
            if (selectedCells.Count == 0)
            {
                Cell c = null;
                do
                {
                    //pick a random cell from all the cells
                    c = cells[UnityEngine.Random.Range(0, cells.Length)];
                    //check that all the edges are vaild
                } while(!NULLCheckCellEdges(c));
                //add
                selectedCells.Add(c);
            }
            else
            {
                //store all the edges on the next cell 
                List<vEdge> allCellEdges = new List<vEdge>();
                //get the first edge from the latest cell is selectedCells
                vEdge beginEdge = FirstEdge(selectedCells[selectedCells.Count - 1]);
                vEdge currentEdge = beginEdge;
                do
                {
                    allCellEdges.Add(currentEdge);
                    currentEdge = currentEdge.Next;
                } while (currentEdge != beginEdge);

                //ref to the edge which twin cell could be added
                vEdge edgeToAdd;
                //bool for if the cell should be added
                bool edgeToAddCheck = true;
                do
                {
                    edgeToAddCheck = true;
                    //pick a random edge from all the current edges and get the twin edge
                    edgeToAdd = allCellEdges[UnityEngine.Random.Range(0, allCellEdges.Count)].Twin;
                    
                    //null check
                    if(edgeToAdd != null)
                    { 
                        //get the first edge
                        beginEdge = FirstEdge(edgeToAdd.Cell);
                        currentEdge = beginEdge;
                        do
                        {
                            //check if all the vertex are not null
                            if (currentEdge.v0 != null && currentEdge.v1 != null &&
                               currentEdge.Next.v0 != null && currentEdge.Next.v1 != null)
                            {
                                currentEdge = currentEdge.Next;
                            }
                            else
                            {
                                //if any vertex is null then break
                                edgeToAddCheck = false;
                                break;
                            }
                        } while (currentEdge != beginEdge);

                        //if edgeToAdd.Cell is not null, selectedCell does not contain edgeToAdd.Cell
                        //and edgeToAddCheck is true then add edgeToAdd.Cell
                        //otherwise don't
                        if (edgeToAdd.Cell != null && !selectedCells.Contains(edgeToAdd.Cell) && edgeToAddCheck)
                        {
                            selectedCells.Add(edgeToAdd.Cell);
                        }
                    }
                    else
                    {
                        edgeToAddCheck = false;
                    }
                    //loop if edgeToAddCheck is false or edgeToAdd's cell is null
                } while (!edgeToAddCheck || edgeToAdd.Cell == null);
                cellsAddedCount += 1;
            }
        }
        //Debug.Log("<color=blue>All Cells Selected</color>");

        List<vEdge> outterEdges = GetOutterEdge(selectedCells);
        //Debug.Log("<color=blue>Outter Edge Found</color>");

        List<vEdge> sorttedEdge = SortEdges(outterEdges);
        //Debug.Log("<color=blue>Outter Edges Sortted</color>");


        return sorttedEdge;
    }

    /// <summary>
    /// sort edges
    /// Sort all the edges so they are connected. This will make
    /// one line from the start to the end
    /// </summary>
    /// <param name="a_edges"></param>
    /// <returns></returns>
    private List<vEdge> SortEdges(List<vEdge> a_edges)
    {
        List<vEdge> sorttedEdges = new List<vEdge>();
        vEdge startEdge = a_edges[0];
        vEdge sEdge = startEdge;
        sorttedEdges.Add(sEdge);
        do
        {
            for (int i = 0; i < a_edges.Count; i++)
            {
                //if sEdge is not equal to current edge
                if (sEdge != a_edges[i])
                {
                    //check if sEdge vertex1 (end pos) is equal to the current edge vertex0 (start pos)
                    if (sEdge.v1.Site == a_edges[i].v0.Site)
                    {
                        sEdge = a_edges[i];
                        sorttedEdges.Add(sEdge);
                        break;
                    }
                }
            }
        } while (sEdge != startEdge);

        sorttedEdges.RemoveAt(sorttedEdges.Count - 1);

        return sorttedEdges;
    }

    /// <summary>
    /// Return a list of vEdge for the outter edges 
    /// of a list of cells
    /// </summary>
    /// <param name="a_cells"></param>
    /// <returns></returns>
    private List<vEdge> GetOutterEdge(List<Cell> a_cells)
    {
        //get all the edge centers from all the cells within a_cells
        List<EdgeCenter> center = GetAllEdgeCenters(a_cells);
        List<vEdge> outterEdges = new List<vEdge>();
        for (int i = 0; i < center.Count; i++)
        {
            //get the first edge center 
            EdgeCenter firstEdgeCenter = center[i];
            int sameCount = 0;
            for (int j = 0; j < center.Count; j++)
            {
                //get another edgecenter
                EdgeCenter secondEdgeCenter = center[j];
                //if i and j are not equal (this makes sure we do not evaluate the same edge center)
                if (i != j)
                {
                    //if the positions are the same (edges are overlapping. Edges are not on the out side)
                    if (firstEdgeCenter.position == secondEdgeCenter.position)
                    {
                        //incerment sameCount
                        sameCount += 1;
                    }
                }
            }

            if (sameCount >= 1)
            {
                //Debug Draw
            }
            else
            {
                outterEdges.Add(firstEdgeCenter.edge);
            }
        }
        return outterEdges;
    }

    /// <summary>
    /// Return List of EdgeCenter for all the cells passed in
    /// This will return all the center positions of each edge 
    /// from all the cells
    /// </summary>
    /// <param name="a_allSelectedCells"></param>
    /// <returns></returns>
    private List<EdgeCenter> GetAllEdgeCenters(List<Cell> a_allSelectedCells)
    {
        List<EdgeCenter> center = new List<EdgeCenter>();
        for (int i = 0; i < a_allSelectedCells.Count; i++)
        {
            //get the first edge in the current cell
            vEdge firstEdge = FirstEdge(a_allSelectedCells[i]);
            vEdge edge = firstEdge;
            do
            {
                //add a new edgeCenter
                center.Add(new EdgeCenter
                {
                    position = new Vector3(((float)edge.v0.Site.x + (float)edge.v1.Site.x) * 0.5f,
                    0.0f,
                    ((float)edge.v0.Site.y + (float)edge.v1.Site.y) * 0.5f),
                    edge = edge
                });
                //set edge to the next edge
                edge = edge.Next;
            } while (edge != firstEdge);
        }
        return center;
    }

    /// <summary>
    /// Check if all the edges around a cell are valid
    /// </summary>
    /// <param name="a_cell"></param>
    /// <returns></returns>
    private bool NULLCheckCellEdges(Cell a_cell)
    {
        bool result = true;
        if (a_cell != null)
        {
            //get the first edge
            vEdge firstEdge = FirstEdge(a_cell);
            vEdge edge = firstEdge;
            do
            {
                //check that all the vertexs on edge and the next edge
                //are not null
                if (edge.v0 != null && edge.v1 != null &&
                   edge.Next.v0 != null && edge.Next.v1 != null)
                {
                    edge = edge.Next;
                }
                else
                {
                    //if any vertex is null 
                    //set result to false
                    result = false;
                    //and break from loop
                    break;
                }
            } while (edge != firstEdge);
        }
        else
        {
            result = false;
        }
        return result;
    }

    /// <summary>
    /// Struct to store position and the corresponding edge
    /// </summary>
    public struct EdgeCenter
    {
        public Vector3 position;
        public vEdge edge;
    };
    
    /// <summary>
    /// store each cell for all the corners
    /// </summary>
    struct Corners
    {
        public Cell topLeft;
        public Cell topRight;
        public Cell bottomLeft;
        public Cell bottomRight;
    }
}
