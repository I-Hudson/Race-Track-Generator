/*
 *  Description : Class for defining the mesh spline
 *  for the race track. This will use the edges from 
 *  the voronoi class generation. 
 *  This class will hold two splines. The first being the
 *  raw spline. The scond spline being the spline which
 *  has been smoothed
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Define the different curve types
/// </summary>
public enum CurveType
{
    Bezier,
    CatmullRom
}

[System.Serializable]
public class Spline
{
    /*
     * All Raw data 
     */
    [SerializeField]
    private List<Vector3> allRawPoints;
    public List<Vector3> AllRawPoints
    { get { return allRawPoints; } }
    public int RawNumSegments
    { get { return allRawPoints.Count; } }
    public int RawNumPoints
    { get { return RawNumSegments; } }

    /*
     * All smooth data
     */
    [SerializeField]
    private List<Vector3> allSmoothPoints;
    public List<Vector3> AllSmoothPoints
    { get { return allSmoothPoints; } }
    public int SmoothNumSegments
    { get { return (allSmoothPoints.Count / 2) - 1; } }
    public int SmoothNumPoints
    { get { return allSmoothPoints.Count; } }

    /*
     * Control points for this spline
     */
    [SerializeField]
    private List<Vector3> controlPoints;
    public List<Vector3> ControlPoints
    { get { return controlPoints; } set { controlPoints = value; } }

    [SerializeField]
    public List<OrientedPoint> splinePoints;

    /*
     * Which curve should the spline use
     */
    private CurveType splineCurveType;
    public CurveType SplineCurveType
    { get { return splineCurveType; } set { splineCurveType = value; } }

    /*
     * Spline scale
     * Quickly change the positions of all spline points
     */
    [SerializeField]
    private float splineScale = 1;
    public float SplineScale
    { get { return splineScale; } set { splineScale = value; } }

    /*
     * Smooth Spline Segment Resoultion
     * Number of points within a segment
     */
    [SerializeField]
    private int smoothSplineSegmentResoultion = 16;
    public int SmoothSplineResoultion
    { get { return smoothSplineSegmentResoultion; }
        set
        {
            if (value < 4)
            {
                smoothSplineSegmentResoultion = 4;
            }
            else if (value > 128)
            {
                smoothSplineSegmentResoultion = 128;
            }
            else
            {
                smoothSplineSegmentResoultion = value;
            }
        }
    }

    /*
     * Smooth Spline Closed
     * Is the Smooth spline closed (The end of the spline attachs to the begining of the spline)
     */
    private bool smoothSplineClosed = true;
    public bool SmoothSplineClosed
    { get { return smoothSplineClosed; } set { smoothSplineClosed = value; } }

    /*
     * Should the editor display setting which can be changed
     * in this spline
     */
    [SerializeField]
    private bool editorFoldOut;
    public bool EditorFoldOut
    { get { return editorFoldOut; } set { editorFoldOut = value; } }

    /*
     * Display all the un smoothed points 
     */
    [SerializeField]
    private bool displayRawPoints;
    public bool DisplayRawPoints
    { get { return displayRawPoints; } set { displayRawPoints = value; } }

    /*
    * Display all the smoothed points 
    */
    [SerializeField]
    private bool displaySmoothPoints;
    public bool DisplaySmoothPoints
    { get { return displaySmoothPoints; } set { displaySmoothPoints = value; } }

    /*
     * Display all the control points
     */
    [SerializeField]
    private bool displayControlPoints;
    public bool DisplayControlPoints
    { get { return displayControlPoints; } set { displayControlPoints = value; } }

    /*
     * Should the control points be able to be modified 
     * by the user user
     */
    [SerializeField]
    private bool controlPointsModifiable = false;
    public bool ControlPointsModifiable
    { get { return controlPointsModifiable; } set { controlPointsModifiable = value; } }

    /*
     * Should the spline try and sort out edge cases
     */
    [SerializeField]
    private bool applyAutoModifcations = false;
    public bool ApplyAutoModifcations
    { get { return applyAutoModifcations; } set { applyAutoModifcations = value; } }

    /*
     * List of vEdge of all the outter edges from the voronoi diagram
     */
    private List<vEdge> splineOutterEdges;

    /// <summary>
    /// Constructor
    /// </summary>
    public Spline()
    {
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="a_outterEdge"></param>
    public Spline(List<vEdge> a_outterEdge)
    {
        CreateRawSpline(a_outterEdge);
        CreateSmoothSpline();
        SmoothSplineNoEdit();
    }

    /// <summary>
    /// Create the spline
    /// </summary>
    /// <param name="a_outterEdge"></param>
    public void CreateSpline(List<vEdge> a_outterEdge)
    {
        if (a_outterEdge != null)
        {
            splineOutterEdges = a_outterEdge;
            CreateRawSpline(splineOutterEdges);
            CreateSmoothSpline();
            SmoothSplineNoEdit();
        }
    }

    /// <summary>
    /// Create the spline
    /// </summary>
    public void CreateSplineFromControlPoints(List<Vector3> a_newControlPoints)
    {
        //CreateSmoothSpline();
        SmoothSplineFromControlPoints(a_newControlPoints);
    }

    /// <summary>
    /// Generate spline for all the control points
    /// </summary>
    public void CreateSplineFromControlPoints()
    {
        CreateSmoothSpline();
        SmoothSplineFromControlPoints(ControlPoints);
    }

    /// <summary>
    /// Create the allRawSpline List
    /// </summary>
    /// <param name="a_outterEdge"></param>
    private void CreateRawSpline(List<vEdge> a_outterEdge)
    {
        //new allRawPoints
        allRawPoints = new List<Vector3>();

        if (a_outterEdge != null)
        {
            //assign all our outter edge points to allRawPoints
            for (int i = 0; i < a_outterEdge.Count; i++)
            {
                //add new point
                allRawPoints.Add(a_outterEdge[i].v0.Site.Vector3() * splineScale);
                //center points 
                //allRawPoints[i] -= (a_outterEdge[0].v0.Site.Vector3() * splineScale);
            }
        }
    }

    /// <summary>
    /// Create the allSmoothSpline List
    /// </summary>
    private void CreateSmoothSpline()
    {
        //new allSmoothPoints
        allSmoothPoints = new List<Vector3>();
        if (allRawPoints != null && RawNumPoints > 0)
        {
            //asign each rawPoint to the allSmoothPoints list
            for (int i = 0; i < allRawPoints.Count; i++)
            {
                allSmoothPoints.Add(allRawPoints[i]);
            }
            //allSmoothPoints.Add(allRawPoints[0]);
            //allSmoothPoints.Add(allRawPoints[0] + (new Vector3(0,0,2.5f) * splineScale));

            //allSmoothPoints.Add(allRawPoints[1] + (new Vector3(0, 0, 2.5f) * splineScale));
            //allSmoothPoints.Add(allRawPoints[1]);

            ////assign all our outter edge points to allRawPoints
            //for (int i = 2; i < allRawPoints.Count; i++)
            //{
            //allSmoothPoints.Add(allSmoothPoints[allSmoothPoints.Count - 1] * 2 - allSmoothPoints[allSmoothPoints.Count - 2]);
            //allSmoothPoints.Add(allSmoothPoints[allSmoothPoints.Count - 1] + allRawPoints[i] * 0.5f);
            //allSmoothPoints.Add(allRawPoints[i]);
            //}
        }
    }

    /// <summary>
    /// Smooth out all the corners in allSmoothSpline list
    /// </summary>
    private void SmoothSplineNoEdit()
    {
        //List of new points. These points will be oriented
        List<OrientedPoint> splineEdge = new List<OrientedPoint>();

        //List for all the control points (Control points are the site from the voronoi, the
        //start point of the bezier curve and end point of the bezier curve  
        ControlPoints = new List<Vector3>();

        if (SplineCurveType == CurveType.Bezier)
        {
            //Loop though allSmoothPoints
            for (int i = 0; i < allSmoothPoints.Count; i++)
            {
                //Check if two site's are close and facing the same direction
                //if there are within a range then set it as one point
                Vector3 cSite = allSmoothPoints[i];
                Vector3 nSite = allSmoothPoints[((i + 1) + allSmoothPoints.Count) % allSmoothPoints.Count];

                //Get if the sites are within range
                if (ApplyAutoModifcations && (cSite - nSite).magnitude < 50)
                {
                    //if we are connecting the first and last site points 
                    //remove the first set of points 
                    if (((i + 1) + allSmoothPoints.Count) % allSmoothPoints.Count == 0)
                    {
                        splineEdge.RemoveRange(0, smoothSplineSegmentResoultion + 1);
                    }

                    //find the start point, midpoint and end point
                    Vector3 oStartPoint = (allSmoothPoints[((i - 1) + allSmoothPoints.Count) % allSmoothPoints.Count] + allSmoothPoints[i]) * 0.5f;
                    Vector3 startPoint = (oStartPoint + allSmoothPoints[i]) * 0.5f;

                    Vector3 midPoint = (allSmoothPoints[i] + allSmoothPoints[((i + 1) +
                                        allSmoothPoints.Count) % allSmoothPoints.Count]) * 0.5f;

                    Vector3 oEndPoint = (allSmoothPoints[((i + 2) + allSmoothPoints.Count) % allSmoothPoints.Count] +
                                        allSmoothPoints[((i + 1) + allSmoothPoints.Count) % allSmoothPoints.Count]) * 0.5f;
                    Vector3 endPoint = (oEndPoint + allSmoothPoints[((i + 1) + allSmoothPoints.Count) % allSmoothPoints.Count]) * 0.5f;


                    //Add points to controlpoints
                    ControlPoints.Add(startPoint);
                    ControlPoints.Add(midPoint);
                    ControlPoints.Add(endPoint);

                    //Get the average direction
                    Vector3 dir = (((midPoint - oStartPoint) + (oEndPoint - midPoint)) * 0.5f).normalized;

                    //Make a new list for the curve
                    List<Vector3> newOPoints = new List<Vector3>();
                    for (int j = 0; j <= smoothSplineSegmentResoultion; j++)
                    {
                        //get the new point in the bezier curve
                        Vector3 pos = BezierCurve(startPoint, midPoint, midPoint, endPoint, j * (1f / smoothSplineSegmentResoultion));
                        OrientedPoint p = new OrientedPoint(pos, Quaternion.LookRotation(
                                                                 (midPoint - pos).normalized, Vector3.up));
                        newOPoints.Add(p.Position);
                        splineEdge.Add(p);
                    }

                    i++;
                }
                else if(!ApplyAutoModifcations)
                {

                    //find the start point, midpoint and end point
                    Vector3 oStartPoint = (allSmoothPoints[((i - 1) + allSmoothPoints.Count) % allSmoothPoints.Count] + allSmoothPoints[i]) * 0.5f;
                    Vector3 startPoint = (oStartPoint + allSmoothPoints[i]) * 0.5f;
                    Vector3 midPoint = allSmoothPoints[i];
                    Vector3 oEndPoint = (allSmoothPoints[((i + 1) + allSmoothPoints.Count) % allSmoothPoints.Count] + allSmoothPoints[i]) * 0.5f;
                    Vector3 endPoint = (oEndPoint + allSmoothPoints[i]) * 0.5f;

                    if (ApplyAutoModifcations)
                    {
                        //check where the site is. If too close to a start or end point then the site will be moved
                        midPoint = SitePointCheck(startPoint, midPoint, endPoint);

                        //Get the average direction
                        Vector3 dir = (((allSmoothPoints[i] - startPoint) + (endPoint - allSmoothPoints[i])) * 0.5f).normalized;

                        float angle = Vector3.SignedAngle((endPoint - startPoint).normalized,
                                                                  (midPoint - startPoint).normalized,
                                                                   Vector3.up);

                        //if the angle from start to end point is greater than 75
                        //then move the site point to make the curve not as tight
                        if (Mathf.Abs(angle) > 75)
                        {
                            //Check which direction to move point in
                            Vector3 d = (midPoint - startPoint).normalized;
                            Vector3 d1 = (midPoint - endPoint).normalized;
                            Vector3 d2 = ((d + d1) * 0.5f).normalized;

                            Vector3 d3 = Vector3.Cross(d2, Vector3.up);

                            midPoint += (d3 * 8);
                        }
                    }

                    //add the start, mid and end point to the Control points list
                    ControlPoints.Add(startPoint);
                    ControlPoints.Add(midPoint);
                    ControlPoints.Add(endPoint);

                    //Make a new list for the curve
                    List<Vector3> newOPoints = new List<Vector3>();
                    for (int j = 0; j <= smoothSplineSegmentResoultion; j++)
                    {
                        //get the new point in the bezier curve
                        Vector3 pos = BezierCurve(startPoint, midPoint, midPoint, endPoint, j * (1f / smoothSplineSegmentResoultion));
                        OrientedPoint p = new OrientedPoint(pos, Quaternion.LookRotation(
                                                                 (midPoint - pos).normalized, Vector3.up));
                        newOPoints.Add(p.Position);
                        splineEdge.Add(p);
                    }
                }
            }
            //clear the allSmoothPoints
            allSmoothPoints.Clear();
            //loop though all splineEdge's 
            for (int i = 0; i < splineEdge.Count; i++)
            {
                //add each splineEdge's position to allSmoothPoints
                allSmoothPoints.Add(splineEdge[i].Position);
            }
            //set splinePoints to splineEdge
            splinePoints = splineEdge;
        }
        else if(SplineCurveType == CurveType.CatmullRom)
        {
            for (int i = 0; i < AllSmoothPoints.Count; i++)
            {
                if (ApplyAutoModifcations)
                {
                    if ((AllSmoothPoints[i] - AllSmoothPoints[Helper.ALoopIndex(i, AllSmoothPoints.Count)]).magnitude
                        < 50)
                    {
                        Vector3 pos = (AllSmoothPoints[i] + AllSmoothPoints[Helper.ALoopIndex(i, AllSmoothPoints.Count)]) * 0.5f;

                        AllSmoothPoints.Insert(i, pos);
                        AllSmoothPoints.Remove(AllSmoothPoints[i]);
                        AllSmoothPoints.Remove(AllSmoothPoints[Helper.ALoopIndex(i, AllSmoothPoints.Count)]);
                    }
                }

                //get all points needed for a spline from p1 to p2
                Vector3 p0 = AllSmoothPoints[Helper.SLoopIndex(i, AllSmoothPoints.Count)];
                Vector3 p1 = AllSmoothPoints[i];
                Vector3 p2 = AllSmoothPoints[Helper.ALoopIndex(i, AllSmoothPoints.Count)];
                Vector3 p3 = AllSmoothPoints[Helper.LoopIndex(i, 2, AllSmoothPoints.Count)];

                ControlPoints.Add(AllSmoothPoints[i]);

                //spline start position
                Vector3 lastPos = p1;

                for (int j = 0; j <= SmoothSplineResoultion; j++)
                {
                    Vector3 pos = GetCatmullRom(p0, p1, p2, p3, j * (1f / SmoothSplineResoultion));
                    lastPos = pos;

                    splineEdge.Add(new OrientedPoint(pos, Quaternion.identity));
                }

                splineEdge.Remove(splineEdge[splineEdge.Count - 1]);
            }

            //clear the allSmoothPoints
            allSmoothPoints.Clear();
            //loop though all splineEdge's 
            for (int i = 0; i < splineEdge.Count; i++)
            {
                //add each splineEdge's position to allSmoothPoints
                allSmoothPoints.Add(splineEdge[i].Position);
            }
            //set splinePoints to splineEdge
            splinePoints = splineEdge;
        }
    }

    /// <summary>
    /// Generate the spline from all the current control points
    /// </summary>
    private void SmoothSplineFromControlPoints(List<Vector3> a_newControlPoints)
    {
        //Set ControlPoints to a_newControlPoints
        ControlPoints = a_newControlPoints;

        //List of new points. These points will be oriented
        List<OrientedPoint> splineEdge = new List<OrientedPoint>();

        if (SplineCurveType == CurveType.Bezier)
        {
            //Loop though allSmoothPoints
            for (int i = 1; i < ControlPoints.Count; i += 3)
            {
                //Check if two site's are close and facing the same direction
                //if there are within a range then set it as one point
                Vector3 cSite = ControlPoints[i];
                Vector3 nSite = ControlPoints[Helper.LoopIndex(i, 3, ControlPoints.Count)];

                //Get if the sites are within range
                if ((cSite - nSite).magnitude < 50)
                {
                    //if we are connecting the first and last site points 
                    //remove the first set of points 
                    if (Helper.ALoopIndex(i, ControlPoints.Count) == 0)
                    {
                        splineEdge.RemoveRange(0, smoothSplineSegmentResoultion + 1);
                    }

                    //Get all the control points for the current curve
                    Vector3 startPoint = ControlPoints[Helper.SLoopIndex(i, ControlPoints.Count)];
                    Vector3 midPoint = ControlPoints[i];
                    Vector3 endPoint = ControlPoints[Helper.ALoopIndex(i, ControlPoints.Count)];

                    //get the average direction
                    Vector3 dir = (((midPoint - startPoint) + (endPoint - midPoint)) * 0.5f).normalized;

                    //Make a new list for the curve
                    List<Vector3> newOPoints = new List<Vector3>();
                    for (int j = 0; j <= smoothSplineSegmentResoultion; j++)
                    {
                        //get the new point in the bezier curve
                        Vector3 pos = BezierCurve(startPoint, midPoint, midPoint, endPoint, j * (1f / smoothSplineSegmentResoultion));
                        OrientedPoint p = new OrientedPoint(pos, Quaternion.LookRotation(
                                                                 (midPoint - pos).normalized, Vector3.up));
                        newOPoints.Add(p.Position);
                        splineEdge.Add(p);
                    }
                    i++;
                }
                else
                {
                    //Get all the control points for the current curve
                    Vector3 startPoint = ControlPoints[Helper.SLoopIndex(i, ControlPoints.Count)];
                    Vector3 midPoint = ControlPoints[i];
                    Vector3 endPoint = ControlPoints[Helper.ALoopIndex(i, ControlPoints.Count)];

                    //get the average direction
                    Vector3 dir = (((midPoint - startPoint) + (endPoint - midPoint)) * 0.5f).normalized;

                    //Make a new list for the curve
                    List<Vector3> newOPoints = new List<Vector3>();
                    for (int j = 0; j <= smoothSplineSegmentResoultion; j++)
                    {
                        //get the new point in the bezier curve
                        Vector3 pos = BezierCurve(startPoint, midPoint, midPoint, endPoint, j * (1f / smoothSplineSegmentResoultion));
                        OrientedPoint p = new OrientedPoint(pos, Quaternion.LookRotation(
                                                                 (midPoint - pos).normalized, Vector3.up));
                        newOPoints.Add(p.Position);
                        splineEdge.Add(p);
                    }
                }
            }
        }
        else if(SplineCurveType == CurveType.CatmullRom)
        {
            for (int i = 0; i < ControlPoints.Count; i++)
            {

                //get all points needed for a spline from p1 to p2
                Vector3 p0 = ControlPoints[Helper.SLoopIndex(i, ControlPoints.Count)];
                Vector3 p1 = ControlPoints[i];
                Vector3 p2 = ControlPoints[Helper.ALoopIndex(i, ControlPoints.Count)];
                Vector3 p3 = ControlPoints[Helper.LoopIndex(i, 2, ControlPoints.Count)];

                for (int j = 0; j <= SmoothSplineResoultion; j++)
                {
                    Vector3 pos = GetCatmullRom(p0, p1, p2, p3, j * (1f / SmoothSplineResoultion));

                    splineEdge.Add(new OrientedPoint(pos, Quaternion.identity));
                }
            }
        }

        //clear the allSmoothPoints
        allSmoothPoints.Clear();
        //loop though all splineEdge's 
        for (int i = 0; i < splineEdge.Count; i++)
        {
            //add each splineEdge's position to allSmoothPoints
            allSmoothPoints.Add(splineEdge[i].Position);
        }
        //set splinePoints to splineEdge
        splinePoints = splineEdge;
    }

    /// <summary>
    /// Check if the site needs to be moved
    /// The site is only moved if it is too close to the end point
    /// </summary>
    /// <param name="a_sPoint"></param>
    /// <param name="a_mPoint"></param>
    /// <param name="a_ePoint"></param>
    /// <returns></returns>
    private Vector3 SitePointCheck(Vector3 a_sPoint, Vector3 a_mPoint, Vector3 a_ePoint)
    {
        //get the average direction 
        Vector3 avgDir = (((a_sPoint - a_mPoint).normalized + (a_ePoint - a_mPoint).normalized)
              * 0.5f).normalized;

        //set a vector 100 unit away in the avgDir direction
        Vector3 midPointEnd = a_mPoint + (avgDir * 100f);

        Vector2 intersect;
        //Check if a line from start the end cross with a line from midPoint to midPointEnd
        Helper.LineInsterection(a_sPoint.ReturnVector2(), a_ePoint.ReturnVector2(),
                                a_mPoint.ReturnVector2(), midPointEnd.ReturnVector2(),
                                out intersect);
        
        //Get the direction from the end point to the start point
        Vector3 sToEDir = a_ePoint - a_sPoint;
        //get the distance between the start and end point
        float maxDistance = sToEDir.magnitude;
        //get the direction from the start point the the intersect point 
        Vector3 sToIDir = intersect.ReturnVector3() - a_sPoint;
        //get the distance from the start point and the intersect point
        float perDistance = sToIDir.magnitude;
        //get the value of 70% of maxDistance
        float p70 = Helper.GetPercentage(maxDistance, 70);
        //get the value of 30% of maxDistance
        float p30 = Helper.GetPercentage(maxDistance, 30);

        //check if the the intersect point is more than 70% along the start to end line
        //if it is then move it back to 70%
        if (perDistance > p70)
        {
            //set t to the midpoint
            Vector3 t = a_mPoint;
            //get the distance from t to the intersect point
            float dis = (t - intersect.ReturnVector3()).magnitude;
            //get the direction from t to the start point
            Vector3 sToMDir = t - a_sPoint;
            //set t to be 70% along the line from start to end
            t = a_sPoint + (sToMDir.normalized * (p70));
            //then move t off the line from start to mid
            t += -avgDir * dis;

            return t;
        }
        //check if the the intersect point is less than 30% along the start to end line
        //if it is then move it to 30%
        else if (perDistance < p30)
        {
            //set t to the midpoint
            Vector3 t = a_mPoint;
            //get the distance from t to the intersect point
            float dis = (t - intersect.ReturnVector3()).magnitude;
            //get the direction from t to the end point
            Vector3 MToEDir = a_mPoint - a_ePoint;
            //set t to be 70% along the line from mid to end
            //this is 30% from start point
            t = a_ePoint + (MToEDir.normalized * (p70));
            //then move t off the line from end to mid
            t += -avgDir * dis;
            return t;

        }
        //if the intersect point is between 30% - 70% then return the midpoint
        else
        {
            return a_mPoint;
        }
    }

    /// <summary>
    /// Bezier curve with one control point
    /// </summary>
    /// <param name="a_P0"></param>
    /// <param name="a_P1"></param>
    /// <param name="a_P2"></param>
    /// <param name="a_t"></param>
    /// <returns></returns>
    private Vector3 GetPointInSmoothSpline(Vector3 a_P0, Vector3 a_P1, Vector3 a_P2, float a_t)
    {
        return Vector3.Lerp(Vector3.Lerp(a_P0, a_P1, a_t), Vector3.Lerp(a_P1, a_P2, a_t), a_t);
    }

    /// <summary>
    /// Return a vector3 position from four other vector3 with the CatmullRom algorithm 
    /// </summary>
    /// <param name="a_p0"></param>
    /// <param name="a_p1"></param>
    /// <param name="a_p2"></param>
    /// <param name="a_p3"></param>
    /// <param name="a_t"></param>
    /// <returns></returns>
    private Vector3 GetCatmullRom(Vector3 a_p0, Vector3 a_p1, Vector3 a_p2, Vector3 a_p3, float a_t)
    {
        Vector3 a = 2f * a_p1;
        Vector3 b = a_p2 - a_p0;
        Vector3 c = 2f * a_p0 - 5f * a_p1 + 4f * a_p2 - a_p3;
        Vector3 d = -a_p0 + 3f * a_p1 - 3f * a_p2 + a_p3;

        Vector3 p = 0.5f * (a + (b * a_t) + (c * a_t * a_t) + (d * a_t * a_t * a_t));
        return p;
    }

    /// <summary>
    /// Check that all the points are a certain distance from each other
    /// if they are too close move then away from eachother
    /// </summary>
    private void CatmullRomCheckPoints()
    {
        for (int i = 0; i < allSmoothPoints.Count; i++)
        {
            for (int j = 0; j < allSmoothPoints.Count; j++)
            {
                if (i != j && (allSmoothPoints[i] - allSmoothPoints[j]).magnitude < 50)
                {

                    Vector3 dir = (allSmoothPoints[i] - allSmoothPoints[j]).normalized;
                    allSmoothPoints[i] += dir * 10;
                    allSmoothPoints[((i + 1) + allSmoothPoints.Count) % allSmoothPoints.Count] -= dir * 10;
                    i = 0;
                    j = 0;
                }
            }
        }
    }

    /// <summary>
    /// Get a point in a bezier curve segment
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <param name="c"></param>
    /// <param name="t"></param>
    /// <returns></returns>
    private Vector3 BezierSegment(Vector3 a, Vector3 b, Vector3 c, float t)
    {
        Vector3 p0 = Vector3.Lerp(a, b, t);
        Vector3 p1 = Vector3.Lerp(b, c, t);
        return Vector3.Lerp(p0, p1, t);
    }

    /// <summary>
    /// Return a point within a bezier curve
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <param name="c"></param>
    /// <param name="d"></param>
    /// <param name="t"></param>
    /// <returns></returns>
    private Vector3 BezierCurve(Vector3 a, Vector3 b, Vector3 c, Vector3 d, float t)
    {
        Vector3 p0 = BezierSegment(a, b, c, t);
        Vector3 p1 = BezierSegment(b, c, d, t);
        return Vector3.Lerp(p0, p1, t);
    }
}