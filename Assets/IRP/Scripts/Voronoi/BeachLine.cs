using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeachLine
{
    // Edges with slopes steeper than this are treated as vertical
    const double verticalThreshold = 0.0000001f;

    //Arc object for beach line
    public class Arc
    {
        public Arc parentArc;
        public Arc leftArc;
        public Arc rightArc;
        public Arc previousArc;
        public Arc nextArc;
        public Cell cell;
        public vEdge leftEdge;
        public vEdge rightEdge;
        public CircleQueue.Node circleNode;
    }

    Arc root;
    Stack<Arc> recycledArcs;
    CircleQueue circleQueue;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="a_cq"></param>
    public BeachLine(CircleQueue a_cq)
    {
        circleQueue = a_cq;
        recycledArcs = new Stack<Arc>();
    }

    /// <summary>
    /// Load the first arc into the beachline
    /// </summary>
    /// <param name="a_cell"></param>
    public void Initialize(Cell a_cell)
    {
        root = new Arc();
        root.parentArc = null;
        root.leftArc = null;
        root.rightArc = null;
        root.previousArc = null;
        root.nextArc = null;
        root.cell = a_cell;
        root.leftEdge = null;
        root.rightEdge = null;
        root.circleNode = null;
    }

    /// <summary>
    /// Add a new arc into the beachline. This will split the 
    /// beachline arc. The arc which is split is the arc above the 
    /// new site
    /// </summary>
    /// <param name="a_newCell"></param>
    public void ProcessSiteEvent(Cell a_newCell)
    {
        //Find arc to split
        Arc splitArc = Search(a_newCell.Site);

        //If there is a circle event with the split arc, delete it
        if(splitArc.circleNode != null)
        {
            circleQueue.Delete(splitArc.circleNode);
        }

        //Create new edges between the new arcs
        vEdge edge0 = new vEdge();
        vEdge edge1 = new vEdge();

        //set the edges cells
        edge0.Cell = splitArc.cell;
        edge1.Cell = a_newCell;

        //set the twin edges
        edge0.Twin = edge1;
        edge1.Twin = edge0;

        //check if the arc to be split cell has an edge
        //if it does not set it's edge to edge0
        if (splitArc.cell.Edge == null)
        {
            splitArc.cell.Edge = edge0;
        }
        //check if newCell has an edge. If not set it's 
        //edge to edge1
        if (a_newCell.Edge == null)
        {
            a_newCell.Edge = edge1;
        }

        //Make a new arc
        Arc newLeftArc;
        //if there are any previous arc reuse then
        if (!recycledArcs.IsEmpty())
        {
            newLeftArc = recycledArcs.Pop();
        }
        else
        {
            newLeftArc = new Arc();
        }

        //set all the values for the new left arc
        newLeftArc.parentArc = splitArc;
        newLeftArc.leftArc = splitArc.leftArc;
        newLeftArc.rightArc = null;
        newLeftArc.previousArc = splitArc.previousArc;
        newLeftArc.nextArc = splitArc;
        newLeftArc.cell = splitArc.cell;
        newLeftArc.leftEdge = splitArc.leftEdge;
        newLeftArc.rightEdge = edge0;
        newLeftArc.circleNode = null;

        //check if there is a leftArc. If there is then 
        //we need to set it parentArc to the newLeftArc
        //this links the arc together
        if (newLeftArc.leftArc != null)
        {
            newLeftArc.leftArc.parentArc = newLeftArc;
        }

        //Make a new arc
        Arc newRightArc;
        //if there are any previous arc reuse then
        if (!recycledArcs.IsEmpty())
        {
            newRightArc = recycledArcs.Pop();
        }
        else
        {
            newRightArc = new Arc();
        }

        //set all the values for the new left arc
        newRightArc.parentArc = splitArc;
        newRightArc.leftArc = null;
        newRightArc.rightArc = splitArc.rightArc;
        newRightArc.previousArc = splitArc;
        newRightArc.nextArc = splitArc.nextArc;
        newRightArc.cell = splitArc.cell;
        newRightArc.leftEdge = edge0;
        newRightArc.rightEdge = splitArc.rightEdge;
        newRightArc.circleNode = null;

        //check if there is a rightArc. If there is then 
        //we need to set it parentArc to the newRightArc
        //this links the arc together
        if (newRightArc.rightArc != null)
        {
            newRightArc.rightArc.parentArc = newRightArc;
        }

        //Set split arc variables to the two new created arcs
        splitArc.leftArc = newLeftArc;
        splitArc.rightArc = newRightArc;
        splitArc.previousArc = newLeftArc;
        splitArc.nextArc = newRightArc;
        splitArc.cell = a_newCell;
        splitArc.leftEdge = edge1;
        splitArc.rightEdge = edge1;
        splitArc.circleNode = null;

        // Have next and previous arcs point to the new arcs. This maintains the linked list
        // through the beachline.
        if (newLeftArc.previousArc != null)
        {
            newLeftArc.previousArc.nextArc = newLeftArc;
        }
        if (newRightArc.nextArc != null)
        {
            newRightArc.nextArc.previousArc = newRightArc;
        }

        // Find new circle events for new left and right arcs. The new middle arc is always
        // diverging when it is created so we don't need to check for it.

        newLeftArc.circleNode = AddCircleEvent(a_newCell.Site.y, newLeftArc);
        newRightArc.circleNode = AddCircleEvent(a_newCell.Site.y, newRightArc);
    }

    /// <summary>
    /// Deletes an existing arc and adds a new vertex to the diagram. Each new vertex
    /// also creates a new edge between the neighboring arcs.
    /// </summary>
    /// <param name="a_cEvent"></param>
    public void ProcessCircleEvent(CircleQueue.Event a_cEvent)
    {
        Arc deleteArc = a_cEvent.arc;
        Arc previousArc = deleteArc.previousArc;
        Arc nextArc = deleteArc.nextArc;

        vEdge leftEdge = deleteArc.leftEdge;
        vEdge rightEdge = deleteArc.rightEdge;

        //Create a new vertex at the location of the circle event
        vVertex v = new vVertex(a_cEvent.vertex);

        //Create a new edge that emerges from the event
        vEdge edge0 = new vEdge();
        vEdge edge1 = new vEdge();

        //setup the new edges
        edge0.Cell = leftEdge.Twin.Cell;
        edge1.Cell = rightEdge.Twin.Cell;
        edge0.Twin = edge1;
        edge1.Twin = edge0;

        //Set all edges to point to the new vertex
        leftEdge.v0 = v;
        leftEdge.Twin.v1 = v;

        rightEdge.v1 = v;
        rightEdge.Twin.v0 = v;

        edge0.v0 = v;
        edge1.v1 = v;

        //Stich the edge together
        leftEdge.Previous = rightEdge;
        rightEdge.Next = leftEdge;

        edge0.Previous = leftEdge.Twin;
        edge0.Previous.Next = edge0;

        edge1.Next = rightEdge.Twin;
        edge1.Next.Previous = edge1;

        // If there's a circle event associated with a neighboring arc, delete it
        if (previousArc.circleNode != null)
        {
            circleQueue.Delete(previousArc.circleNode);
        }
        if(nextArc.circleNode != null)
        {
            circleQueue.Delete(nextArc.circleNode);
        }

        //Set the neighboring arcs to point to the new edge
        previousArc.rightEdge = edge0;
        nextArc.leftEdge = edge1;

        //Delete the completed arc
        Delete(deleteArc);

        //Calculate new circle events for the site arcs
        previousArc.circleNode = AddCircleEvent(a_cEvent.yLock, previousArc);
        nextArc.circleNode = AddCircleEvent(a_cEvent.yLock, nextArc);
    }

    /// <summary>
    /// After the last event has been run. Check for parallel edges
    /// If any edges are parallel then link them together to create
    /// linked list
    /// </summary>
    public void CloseOpenEdge()
    {
        Arc arc = root;
        while(arc.leftArc != null)
        {
            arc = arc.leftArc;
        }
        do
        {
            //if the arc has valid arcs to the left and right
            if (arc.leftEdge != null && arc.rightEdge != null)
            {
                //check if the leftArc.next is null and rightArc.previous are is null
                //link them
                if (arc.leftEdge.Next == null && arc.rightEdge.Previous == null)
                {
                    arc.leftEdge.Next = arc.rightEdge;
                    arc.rightEdge.Previous = arc.leftEdge;
                }
                //check if the leftEdge.previous is null and rightEdge.next is null
                //link them
                if (arc.leftEdge.Previous == null && arc.rightEdge.Next == null)
                {
                    arc.leftEdge.Previous = arc.rightEdge;
                    arc.rightEdge.Next = arc.leftEdge;
                }
            }
            //move to the next arc
            arc = arc.nextArc;
        } while (arc != null);
    }

    /// <summary>
    /// Calculates the point at which the arc shrinks to nothing
    /// </summary>
    /// <param name="a_sweepLine"></param>
    /// <param name="a_arc"></param>
    /// <returns></returns>
    CircleQueue.Node AddCircleEvent(double a_sweepLine, Arc a_arc)
    {
        DVector2 center;
        //No event for leftmost and right mose arcs in beach line
        if(a_arc.previousArc == null  || a_arc.nextArc == null)
        {
            return null;
        }

        //No event if sites are expanding
        if(!Circumcenter(a_arc.cell.Site, a_arc.previousArc.cell.Site, a_arc.nextArc.cell.Site, out center))
        {
            return null;
        }

        //create new Circle event
        CircleQueue.Event cEvent;
        cEvent.yLock = center.y - (a_arc.cell.Site - center).Magnitude();
        cEvent.vertex = center;
        cEvent.arc = a_arc;

        return circleQueue.Push(cEvent);
    }

    /// <summary>
    /// Calculates the center of a circle that intersects abc. Returns false if abc are colinear, or if the
    /// arc is diverging.  Divergence is determine by whether the points are ordered clockwise or ccw around
    /// the center of the circle
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <param name="c"></param>
    /// <param name="d"></param>
    /// <returns></returns>
    bool Circumcenter(DVector2 a, DVector2 b, DVector2 c, out DVector2 d)
    {
        DVector2 ba = b - a;
        DVector2 ca = c - a;
        double baLength = ba.SqrMagnitude();
        double caLength = ca.SqrMagnitude();
        double denominator = 2d * (ba.x * ca.y - ba.y * ca.x);
        if (denominator <= 0d)
        {
            d = b;
            return false;
        }
        d.x = a.x + (ca.y * baLength - ba.y * caLength) / denominator;
        d.y = a.y + (ba.x * caLength - ca.x * baLength) / denominator;
        return true;
    }

    /// <summary>
    /// Searches the beach line for the arc that is above the site
    /// </summary>
    /// <param name="a_site"></param>
    /// <returns></returns>
    Arc Search(DVector2 a_site)
  {
        Arc splitArc = root;
      
        //Find which are to split
        while(true)
        {
            //Check to see if split is to th left of this arc
            if(splitArc.leftArc != null)
            {
                //Find a point on the line bisecting the two sites and a vector pints along it
                DVector2 midPoint = Midpoint(splitArc.cell.Site, splitArc.previousArc.cell.Site);
                DVector2 bisector = Bisector(splitArc.cell.Site, splitArc.previousArc.cell.Site);

                //Check to see if the bisector is vertical
                bool isVertical = false;
                if(Math.Abs(bisector.x) <= Math.Abs(bisector.y))
                {
                    if(bisector.y == 0d || Math.Abs(bisector.x / bisector.y) < verticalThreshold)
                    {
                        isVertical = true;
                    }
                }

                if(isVertical)
                {
                    // If the bisector is vertical we can determine if the split is to the left of this arc
                    // simply by testing if the new site lies to the left of a point on the bisector
                    if(a_site.x < midPoint.x)
                    {
                        splitArc = splitArc.leftArc;
                        continue;
                    }
                }
                else
                {
                    //If the bisector isn't vertical we have to do more work
                    //We project a line vertically from the new site and see where it intersects the bisector
                    DVector2 intercept;
                    intercept.x = a_site.x;
                    intercept.y = midPoint.y + (a_site.x - midPoint.x) * bisector.y / bisector.x;

                    //We calculate the height of the intercept line and the distance of the intercept point
                    //from the split site. Theses calues are EQUAL at the breakpoint between the two arcs

                    double radiusSquared = (splitArc.cell.Site - intercept).SqrMagnitude();
                    double height = intercept.y - a_site.y;
                    double heightSquared = height * height;

                    //Two different test criteria depending on whether split site or left site is more recent
                    if(splitArc.previousArc.cell.Site.y < splitArc.cell.Site.y)
                    {
                        if(a_site.x < splitArc.previousArc.cell.Site.x || heightSquared > radiusSquared)
                        {
                            splitArc = splitArc.leftArc;
                            continue;
                        }
                    }
                    else
                    {
                        if(a_site.x < splitArc.cell.Site.x && heightSquared < radiusSquared)
                        {
                            splitArc = splitArc.leftArc;
                            continue;
                        }
                    }
                }
            }

            //Check to see if split is to the right of the arc
            if (splitArc.rightArc != null)
            {
                //Find a point on the line bisecting the two sites and a vector pints along it
                DVector2 midPoint = Midpoint(splitArc.cell.Site, splitArc.nextArc.cell.Site);
                DVector2 bisector = Bisector(splitArc.cell.Site, splitArc.nextArc.cell.Site);

                //Check to see if the bisector is vertical
                bool isVertical = false;
                if (Math.Abs(bisector.x) <= Math.Abs(bisector.y))
                {
                    if (bisector.y == 0d || Math.Abs(bisector.x / bisector.y) < verticalThreshold)
                    {
                        isVertical = true;
                    }
                }

                if (isVertical)
                {
                    // If the bisector is vertical we can determine if the split is to the right of this arc
                    // simply by testing if the new site lies to the right of a point on the bisector
                    if (a_site.x > midPoint.x)
                    {
                        splitArc = splitArc.rightArc;
                        continue;
                    }
                }
                else
                {
                    //If the bisector isn't vertical we have to do more work

                    //We project a line vertically from the new site and see where it intersects the bisector
                    DVector2 intercept;
                    intercept.x = a_site.x;
                    intercept.y = midPoint.y + (a_site.x - midPoint.x) * bisector.y / bisector.x;

                    //We calculate the height of the intercept line and the distance of the intercept point
                    //from the split site. Theses calues are EQUAL at the breakpoint between the two arcs
                    double radiusSquared = (splitArc.cell.Site - intercept).SqrMagnitude();
                    double height = intercept.y - a_site.y;
                    double heightSquared = height * height;

                    //Two different test criteria depending on whether split site or right site is more recent
                    if (splitArc.nextArc.cell.Site.y < splitArc.cell.Site.y)
                    {
                        if (a_site.x > splitArc.nextArc.cell.Site.x || heightSquared > radiusSquared)
                        {
                            splitArc = splitArc.rightArc;
                            continue;
                        }
                    }
                    else
                    {
                        if (a_site.x > splitArc.cell.Site.x && heightSquared < radiusSquared)
                        {
                            splitArc = splitArc.rightArc;
                            continue;
                        }
                    }
                }
            }

            break;
        }
        return splitArc;
    }

    /// <summary>
    /// Returns the midpoint between two points
    /// </summary>
    /// <param name="a_p0"></param>
    /// <param name="a_p1"></param>
    /// <returns></returns>
    DVector2 Midpoint(DVector2 a_p0, DVector2 a_p1)
    {
        return (a_p0 + a_p1) / 2d;
    }

    /// <summary>
    /// Returns an unnormalized vector pointing along the bisector of two points
    /// </summary>
    /// <param name="a_p0"></param>
    /// <param name="a_p1"></param>
    /// <returns></returns>
    DVector2 Bisector(DVector2 a_p0, DVector2 a_p1)
    {
        DVector2 delta = a_p1 - a_p0;
        DVector2 perpendicular;
        perpendicular.x = delta.y * -1f;
        perpendicular.y = delta.x;
        return perpendicular;
    }

    /// <summary>
    /// Removes an arc from the beachline
    /// </summary>
    /// <param name="a_arc"></param>
    private void Delete(Arc a_arc)
    {
        Arc parent = a_arc.parentArc;
        Arc left = a_arc.leftArc;
        Arc right = a_arc.rightArc;
        Arc previous = a_arc.previousArc;
        Arc next = a_arc.nextArc;

        //Deleted node has 0 or 1 child
        if(left == null || right == null)
        {
            //Find child
            Arc child;
            if(left != null)
            {
                child = left;
            }
            else
            {
                child = right;
            }

            if(child != null)
            {
                child.parentArc = parent;
            }

            if(root == a_arc)
            {
                root = child;
            }
            else
            {
                if (a_arc == parent.leftArc)
                {
                    parent.leftArc = child;
                }
                else
                {
                    parent.rightArc = child;
                }
            }
        }
        //Detleted node has 2 children
        else
        {
            Arc splice = next;
            Arc spliceParent = splice.parentArc;
            Arc child = splice.rightArc;

            //check if child is not null
            //if not then set the parentArc to spliceParent
            if(child != null)
            {
                child.parentArc = spliceParent;
            }

            //check if splice is equal to the left arc on the spliceParent
            //if it is then set child to it's leftArc
            if(splice == spliceParent.leftArc)
            {
                spliceParent.leftArc = child;
            }
            else
            {
                //same for if child is the rightArc
                spliceParent.rightArc = child;
            }

            //set rest of values
            splice.parentArc = a_arc.parentArc;
            splice.leftArc = a_arc.leftArc;
            splice.rightArc = a_arc.rightArc;

            //check root
            if(root == a_arc)
            {
                root = splice;
            }
            else
            {
                //if a_arc is equal to parent.LeftArc
                //set it to splice
                if(a_arc == parent.leftArc)
                {
                    parent.leftArc = splice;
                }
                else
                {
                    //same for the rightArc
                    parent.rightArc = splice;
                }
            }
            //if splice.leftArc is not null
            //set it's parentArc to splice
            if(splice.leftArc != null)
            {
                splice.leftArc.parentArc = splice;
            }
            //if splice.rightArc is not null
            //set it's parentArc to splice
            if (splice.rightArc != null)
            {
                splice.rightArc.parentArc = splice;
            }
        }
        previous.nextArc = next;
        next.previousArc = previous;
        recycledArcs.Push(a_arc);
    }
}
