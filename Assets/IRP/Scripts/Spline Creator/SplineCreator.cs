using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[System.Serializable]
[RequireComponent(typeof(Voronoi))]
public class SplineCreator : MonoBehaviour
{
    /*
     * Spline
     * This spline is created
     */
    private Spline _spline;
    public Spline Spline
    { get { return _spline; } set { _spline = value; } }

    /*
     * Mesh To Extrude
     * Mesh which will be extruded along the spline
     */
    [SerializeField]
    private Mesh _meshToExtrude;
    public Mesh MeshToExtrude
    { get { return _meshToExtrude; } set { _meshToExtrude = value; } }

    [SerializeField]
    private float _MeshWidthScale = 1.0f;
    public float MeshWidthScale { get { return _MeshWidthScale; } set { _MeshWidthScale = value > 0.0f ? value : 1.0f; } }

    /*
     * Curb Mesh
     */
    [SerializeField]
    private Mesh curbMesh;
    public Mesh CurbMesh
    { get { return curbMesh; } set { curbMesh = value; } }

    /*
     * Material which is applied to the track mesh 
     */
    [SerializeField]
    private Material _trackMaterial;
    public Material TrackMaterial
    { get { return _trackMaterial; } set { _trackMaterial = value; } }

    /*
    * Material which is applied to the curb mesh 
    */
    [SerializeField]
    private Material _curbMaterial;
    public Material CurbMaterial
    { get { return _curbMaterial; } set { _curbMaterial = value; } }

    /*
     * Local mesh which is made when generating the track 
     */
    private Mesh masterMesh;

    /*
     * Voronoi
     * Voronoi class 
     */
    [SerializeField]
    private Voronoi _voronoi;
    public Voronoi Voronoi
    { get { return _voronoi; } set { _voronoi = value; } }

    /*
     * Should the Track be made with a random seed value
     */
    [SerializeField]
    private bool _randomSeed;
    public bool RandomSeed
    { get { return _randomSeed; } set { _randomSeed = value; } }

    /*
     * Manully seed value (Open to change if RandomSeed is false
     */
    [SerializeField]
    private int _seed;
    public int Seed
    { get { return _seed; }  set { _seed = value; } }

    /*
     * Store a GameObject for the track
     */
    [SerializeField]
    private Mesh trackMesh;
    [SerializeField]
    private Mesh curbsMesh;

    /*
     * Class to store all the procedural data
     * Vertices
     * Triangles
     * UVs
     */
    ProMesh proMesh;

    /// <summary>
    /// Create the track
    /// </summary>
    public void CreateTrack()
    {
        if (Spline.AllSmoothPoints.Count > 0)
        {
            proMesh = new ProMesh();
            ProTwoDShape pTwoDShape = CreateTwoDShape(MeshToExtrude);
            CreateTrackVertices();
            trackMesh = CreateTackMesh(pTwoDShape);
            CreateCurbVertices();

            GenerateTrackObject();
        }
        else
        {
            Debug.Log("SPLICE CREATOR: There is no spline. Make sure there is a Voronoi diagram gnerated. Then press the" +
                " 'Generate Spline'");
        }
    }

    /// <summary>
    /// Generate the voronoi diagram
    /// </summary>
    public void GenerateVoronoi()
    {
        /*
            * Test seeds
            * 1056279151
            * 1188430406
            * 916508404
            * 1161899265
            * 1969582979
            * 380612368
            * 1517822890
            * 1720479531
            * 1817852180
            * 571502800
            * 981687587
            * 1884839221
            * 1869074966
            * 90542208
            * 1961817730
            * 1342973776
            * 1654753252
            * 355825481
            * 1316400197
            * 1401185548
            * 924199008
        */

        Random.InitState((int)System.DateTime.Now.Ticks);

        //check if we need to generate a random seed
        if (RandomSeed)
        {
            Seed = Random.Range(0, int.MaxValue);
        }
        //Setup the Voronoi with sites
        _voronoi.RandomCells(Voronoi.SiteCount, Voronoi.GridSize, Seed);
        //Calculate the voronoi diagram
        _voronoi.Calculate();
        //clip the voronoi diagram
        _voronoi.Clip(Voronoi.GridSize);
    }

    /// <summary>
    /// Create a new ProTwoDShape Object to store 
    /// all the data for the mesh to extrude along the spline 
    /// </summary>
    /// <returns></returns>
    private ProTwoDShape CreateTwoDShape(Mesh a_mesh)
    {
        //ref to futhest vertex on the left and right
        Vector2 farLeftV = Vector2.zero;
        Vector2 farRightV = Vector2.zero;
        //Create List of all vertices
        List<Vector2> twoDShape = new List<Vector2>();

        for (int i = 0; i < a_mesh.vertexCount;)
        {
            //Add each vertex to twoDShape List
            twoDShape.Add(new Vector2(a_mesh.vertices[i].x,
                                      a_mesh.vertices[i].y));
            //Check if current vertex is further left then any other
            if (twoDShape[twoDShape.Count - 1].x < farLeftV.x)
            {
                farLeftV = twoDShape[twoDShape.Count - 1];
            }
            //Check if current vertex is further right then any other
            if (twoDShape[twoDShape.Count - 1].x > farRightV.x)
            {
                farRightV = twoDShape[twoDShape.Count - 1];
            }

            //if we are on the first vertex
            //then inceremnt i by 3
            if (i == 0)
            {
                i += 3;
            }
            //otherwist incerment i by 2
            //this done due to how the vertices are layed out
            //
            //1     2       4
            //
            //0     3       5
            else
            {
                i += 2;
            }
        }

        //Get the overall width of the 2d shape
        float twoDShapeWidth = Mathf.Abs(farLeftV.x - farRightV.x);

        return new ProTwoDShape(twoDShape, twoDShapeWidth);
    }

    /// <summary>
    /// Generate the spline 
    /// </summary>
    public void GenerateSpline()
    {
        Spline.CreateSpline(_voronoi.SplineOutterEdge());
    }

    /// <summary>
    /// Create all the vertices for the mesh
    /// </summary>
    private void CreateTrackVertices()
    {
        ProTwoDShape pTwoDShape = CreateTwoDShape(MeshToExtrude);

        //Create an array of OrientedPoint
        OrientedPoint[] oPoints = new OrientedPoint[_spline.SmoothNumPoints];
        //Assign each oPoint 
        for (int i = 0; i < oPoints.Length; i++)
        {
            //Get the point before current point
            Vector3 lastPoint = _spline.AllSmoothPoints[((i - 1) + oPoints.Length) % oPoints.Length];
            //Get current point
            Vector3 currentPoint = _spline.AllSmoothPoints[i];
            //Get the next point 
            Vector3 nextPoint = _spline.AllSmoothPoints[((i + 1) + oPoints.Length) % oPoints.Length];

            //Get the direction from currentPoint to lastPoint
            Vector3 lc = currentPoint - lastPoint;
            //Get the direction from nextPoint to currentPoint
            Vector3 cn = nextPoint - currentPoint;

            //Avgrage the start and end points for each curve
            //Set the points direction. This direction is the forward vector 
            Vector3 dir = (lc + cn) * 0.5f;
            dir.Normalize();

            //Get the right Vector 
            dir = Vector3.Cross(dir, Vector3.up).normalized;

            //new the oPoint
            oPoints[i] = new OrientedPoint(_spline.AllSmoothPoints[i],
                                           Quaternion.LookRotation(dir, Vector3.up));
        }

        //Generate all the center points on the mesh
        //This will also set the forward rotation
        //and UV for each point
        for (int i = 0; i < oPoints.Length; i++)
        {
            for (int j = 0; j < pTwoDShape.Vertices.Count; j++)
            {
                //get the forward vector
                Vector3 fwd = oPoints[i].Rotation * Vector3.forward;
                //get the position
                Vector3 pos = oPoints[i].Position + (pTwoDShape.Vertices[j].x * fwd) * _MeshWidthScale;
                pos.y = oPoints[i].Position.y + pTwoDShape.Vertices[j].y;

                //add the new vertex to the proMesh.Vertices list
                proMesh.Vertices.Add(pos);

                //Setup the uv coords for this vertex
                float x = (float)j / (pTwoDShape.Vertices.Count - 1);
                float y = (float)i / oPoints.Length;
                Vector2 uv = new Vector2(x, y);
                proMesh.Uvs.Add(uv);
            }
        }
    }

    /// <summary>
    /// Create the mesh 
    /// </summary>
    /// <param name="a_vertices"></param>
    /// <param name="a_shapeToExtrude"></param>
    /// <returns></returns>
    private Mesh CreateTackMesh(ProTwoDShape a_twoDShape)
    {
        //loop though all the vertices
        for (int i = 0; i < proMesh.Vertices.Count; i++)
        {
            //this is to check if we are on the last vertex of the twoDShape
            //e.x ((1 + 1) / 2) % 1 = 1
            //we are on the end of the twoDShape
            //we can not add trianles
            //so continue
            float value = ((i + 1) / (float)a_twoDShape.Vertices.Count) % 1f;
            if (i != 0 && value == 0)
            {
                continue;
            }

            //Make tri
            proMesh.Triangles.Add(i);
            proMesh.Triangles.Add(i + a_twoDShape.Vertices.Count);
            proMesh.Triangles.Add(i + a_twoDShape.Vertices.Count + 1);

            proMesh.Triangles.Add(i);
            proMesh.Triangles.Add(i + a_twoDShape.Vertices.Count + 1);
            proMesh.Triangles.Add(i + 1);
        }

        //create new lists for vertices, triangles and uvs
        List<Vector3> meshVer = new List<Vector3>();
        List<int> meshTri = new List<int>();
        List<Vector2> meshUV = new List<Vector2>();

        //store the current vertex/triangle count
        int currentVertexCount = 0;
        int currentTriangleCount = 0;

        for (int i = 0; i < (proMesh.Vertices.Count / a_twoDShape.Vertices.Count) + 1; i++)
        {
            //if the currentVertexCount is greater than proMesh.Vertices.Count
            //then break
            if (currentVertexCount > proMesh.Vertices.Count - 1)
            {
                break;
            }

            //Add vertex line
            for (int j = 0; j < a_twoDShape.Vertices.Count; j++)
            {
                meshVer.Add(proMesh.Vertices[j + currentVertexCount]);
                meshUV.Add(proMesh.Uvs[j + currentVertexCount]);
            }
            //incerment currentVertexCount
            currentVertexCount += a_twoDShape.Vertices.Count;

            //add triangle line
            if (currentVertexCount >= a_twoDShape.Vertices.Count * 2)
            {
                for (int j = 0; j < ((a_twoDShape.Vertices.Count * 2) - 2) * 3; j++)
                {
                    meshTri.Add(proMesh.Triangles[j + currentTriangleCount]);
                }
                //incerment currentTriangleCount
                currentTriangleCount += ((a_twoDShape.Vertices.Count * 2) - 2) * 3;
            }
        }

        //Check if the spline has been closed. If it is 
        //then do nothing. Otherwise close it
        if (_spline.SmoothSplineClosed)
        {
            int lastIndex = proMesh.Vertices.Count - 1;

            meshTri.Add(lastIndex - 1);
            meshTri.Add(0);
            meshTri.Add(1);

            meshTri.Add(lastIndex - 1);
            meshTri.Add(1);
            meshTri.Add(lastIndex);
        }

        //create a new mesh and assign the vertices, triangles and uvs
        Mesh mesh = new Mesh();
        mesh.vertices = meshVer.ToArray();
        mesh.triangles = meshTri.ToArray();
        mesh.uv = meshUV.ToArray();
        mesh.RecalculateNormals();

        //return mesh
        return mesh;
    }

    /// <summary>
    /// Generate a new GameObject and add the track to it
    /// </summary>
    private void GenerateTrackObject()
    {
        //if both the trackMesh and curbMesh are not null
        if (trackMesh != null && curbsMesh != null)
        {
            //then create a new GameObject add a mesh filter, mesh renderer and PGSave to it
            GameObject track = new GameObject("Track Seed: " + Seed);
            track.AddComponent<MeshFilter>();
            track.AddComponent<MeshRenderer>();
            track.AddComponent<PGSave>();

            //set the mesh filter and mesh renderer
            track.GetComponent<MeshFilter>().sharedMesh = trackMesh;
            track.GetComponent<MeshRenderer>().sharedMaterial = TrackMaterial;

            //then create a new GameObject add a mesh filter and mesh renderer to it
            GameObject curb = new GameObject("Curbs");
            curb.transform.SetParent(track.transform);
            curb.AddComponent<MeshFilter>();
            curb.AddComponent<MeshRenderer>();

            //set the mesh filter and mesh renderer
            curb.GetComponent<MeshFilter>().sharedMesh = curbsMesh;
            curb.GetComponent<MeshRenderer>().sharedMaterial = CurbMaterial;
        }
    }

    /// <summary>
    /// Create all the vertices for the curbs around the track
    /// </summary>
    private void CreateCurbVertices()
    {
        ProTwoDShape curbMesh = CreateTwoDShape(CurbMesh);
        for (int i = 0; i < curbMesh.Vertices.Count; i++)
        {
            Vector2 v = curbMesh.Vertices[i];
            v *= 100;
            curbMesh.Vertices[i] = v;
        }
        curbMesh.ShapeWidth *= 100;

        //Store all the curb meshes made
        List<CombineInstance> allCurbMeshes = new List<CombineInstance>();
        if (Spline.SplineCurveType == CurveType.Bezier)
        {
            //Go though all the vertices in the track mesh
            for (int i = 0; i < proMesh.Vertices.Count; i++)
            {
                //Store all the vertices for the current curb
                List<Vector3> curbVertices = new List<Vector3>();
                //Store all the uvs for the current curb
                List<Vector2> curbUvs = new List<Vector2>();
                //Store all the triangle for the current curb
                List<int> curbTriangles = new List<int>();

                //Get the number of segmnets each spline curve it broken
                //up into
                int segStart = ((Spline.SmoothSplineResoultion + 1) * 2);

                //if we are at the start of a curve 
                if (i % segStart == 0)
                {
                    //Get the distance for the left side of the spline
                    float splineLeft = (proMesh.Vertices[i] -
                                        proMesh.Vertices[Helper.LoopIndex(i, segStart - 2, proMesh.Vertices.Count)]).magnitude;
                    //Get the distance for the right side of the spline
                    float splineRight = (proMesh.Vertices[Helper.ALoopIndex(i, proMesh.Vertices.Count)] -
                                        proMesh.Vertices[Helper.LoopIndex(i, segStart - 1, proMesh.Vertices.Count)]).magnitude;

                    //check which side of the track mesh is smaller
                    //this is the side which will have the curb made for
                    if (splineLeft < splineRight)
                    {
                        curbMesh.Vertices.Reverse();
                        //Setup the vertices
                        for (int j = 0; j < segStart; j += 2)
                        {
                            for (int k = 0; k < curbMesh.Vertices.Count; k++)
                            {
                                //ROTATE VERTICES
                                Vector3 cV = proMesh.Vertices[Helper.LoopIndex(i, j, proMesh.Vertices.Count)];
                                Vector3 cVT = proMesh.Vertices[Helper.LoopIndex(i, j + 1, proMesh.Vertices.Count)];
                                Vector3 dir = cV -
                                              cVT;

                                //Set the new vertex position
                                Vector3 pos = cV + (curbMesh.Vertices[k].x * dir.normalized);
                                pos += (curbMesh.ShapeWidth) * dir.normalized;

                                //if we are on the first or last row of vertices for this curb
                                //clamp the y value to 0
                                if (j == 0 || j == (segStart - 2))
                                {
                                    pos.y = 0;
                                }
                                else
                                {
                                    pos.y = 0 + curbMesh.Vertices[k].y;
                                }

                                //add curb to vertex list
                                curbVertices.Add(pos);

                                //Set the new vertex uv
                                float x = (float)j / (curbMesh.Vertices.Count - 1);
                                float y = (float)i / Spline.SmoothSplineResoultion + 1;
                                Vector2 uv = new Vector2(x, y);
                                curbUvs.Add(uv);
                            }
                        }
                        //Setup the triangles
                        for (int j = 0; j < curbVertices.Count - (curbMesh.Vertices.Count); j++)
                        {
                            float value = (j + 1) / (float)curbMesh.Vertices.Count % 1f;
                            if (value != 0)
                            {
                                curbTriangles.Add(j);
                                curbTriangles.Add(j + curbMesh.Vertices.Count);
                                curbTriangles.Add(j + curbMesh.Vertices.Count + 1);
                                curbTriangles.Add(j);
                                curbTriangles.Add(j + curbMesh.Vertices.Count + 1);
                                curbTriangles.Add(j + 1);
                            }
                        }
                        curbMesh.Vertices.Reverse();
                    }
                    else if (splineLeft > splineRight)
                    {
                        for (int j = 1; j < segStart; j += 2)
                        {
                            for (int k = 0; k < curbMesh.Vertices.Count; k++)
                            {
                                //ROTATE VERTICES
                                Vector3 cV = proMesh.Vertices[Helper.LoopIndex(i, j, proMesh.Vertices.Count)];
                                Vector3 cVT = proMesh.Vertices[Helper.LoopIndex(i, j - 1, proMesh.Vertices.Count)];
                                Vector3 dir = cV -
                                              cVT;

                                Vector3 pos = cV + (curbMesh.Vertices[k].x * dir.normalized);
                                pos += (curbMesh.ShapeWidth) * dir.normalized;

                                //if we are on the first or last row of vertices for this curb
                                //clamp the y value to 0
                                if (j == 1 || j == (segStart - 1))
                                {
                                    pos.y = 0;
                                }
                                else
                                {
                                    pos.y = 0 + curbMesh.Vertices[k].y;
                                }

                                //add curb inner vertex first
                                //curbVertices.Add(cV + (dir.normalized * 2));
                                //curbVertices.Add(cV);
                                curbVertices.Add(pos);

                                float x = (float)j / (curbMesh.Vertices.Count - 1);
                                float y = (float)i / segStart;
                                Vector2 uv = new Vector2(x, y);
                                curbUvs.Add(uv);
                            }
                        }
                        //Setup the triangles
                        for (int j = 0; j < curbVertices.Count - (curbMesh.Vertices.Count + 2); j++)
                        {
                            float value = (j + 1) / (float)curbMesh.Vertices.Count % 1f;
                            if (value != 0)
                            {
                                curbTriangles.Add(j);
                                curbTriangles.Add(j + curbMesh.Vertices.Count);
                                curbTriangles.Add(j + curbMesh.Vertices.Count + 1);
                                
                                curbTriangles.Add(j);
                                curbTriangles.Add(j + curbMesh.Vertices.Count + 1);
                                curbTriangles.Add(j + 1);
                            }
                        }
                    }

                    CombineInstance ci = new CombineInstance();
                    Mesh m = new Mesh();
                    m.vertices = curbVertices.ToArray();
                    m.uv = curbUvs.ToArray();
                    m.triangles = curbTriangles.ToArray();
                    ci.mesh = m;
                    ci.transform = transform.localToWorldMatrix;
                    allCurbMeshes.Add(ci);
                }
            }
        }
        else if (Spline.SplineCurveType == CurveType.CatmullRom)
        {
            //create the curbs from the control points. This assumes that the sharepest 
            //part of the curve is the control point

            //Go though all the vertices in the track mesh
            for (int i = 0; i < proMesh.Vertices.Count; i++)
            {
                //Store all the vertices for the current curb
                List<Vector3> curbVertices = new List<Vector3>();
                //Store all the uvs for the current curb
                List<Vector2> curbUvs = new List<Vector2>();
                //Store all the triangle for the current curb
                List<int> curbTriangles = new List<int>();


                //Get the number of segmnets each spline curve it broken
                //up into
                int segStart = ((Spline.SmoothSplineResoultion + 1) * 2);

                if(i % segStart == 0)
                {
                    //Get the overall offset value form the control point
                    //this is the number of vertices from the control point
                    int offset = (segStart * 2) / 8;

                    //Get the start and end vertex point for the left side of the mesh
                    int leftStartIndex = Helper.LoopIndex(i, -(offset + 2), proMesh.Vertices.Count);
                    int leftEndIndex = Helper.LoopIndex(i, (offset), proMesh.Vertices.Count);


                    //Get the distance for the left side of the spline
                    float splineLeft = (proMesh.Vertices[leftStartIndex] -
                                        proMesh.Vertices[leftEndIndex]).magnitude;

                    //Get the start and end vertex point for the right side of the mesh
                    int rightStartIndex = Helper.LoopIndex(i, -(offset + 1), proMesh.Vertices.Count);
                    int rightEndIndex = Helper.LoopIndex(i, (offset + 1), proMesh.Vertices.Count);

                    //Get the distance for the right side of the spline
                    float splineRight = (proMesh.Vertices[rightStartIndex] -
                                        proMesh.Vertices[rightEndIndex]).magnitude;

                    //check which side of the track mesh is smaller
                    //this is the side which will have the curb made for
                    if (splineLeft < splineRight)
                    {
                        int cIndex = leftStartIndex;
                        curbMesh.Vertices.Reverse();
                        //create mesh
                        for (int j = 0; j < (offset * 2 + 1); j += 2)
                        {
                            cIndex = Helper.LoopIndex(leftStartIndex, j, proMesh.Vertices.Count);
                            for (int k = 0; k < curbMesh.Vertices.Count; k++)
                            {
                                //ROTATE VERTICES
                                Vector3 cV = proMesh.Vertices[cIndex];
                                Vector3 cVT = proMesh.Vertices[Helper.ALoopIndex(cIndex, proMesh.Vertices.Count)];
                                Vector3 dir = cV -
                                              cVT;

                                //Set the new vertex position
                                Vector3 pos = cV + (curbMesh.Vertices[k].x * dir.normalized);
                                pos += (curbMesh.ShapeWidth) * dir.normalized;

                                //if we are on the first or last row of vertices for this curb
                                //clamp the y value to 0
                                if (j == 0 || j == (offset * 2 + 1) - 1)
                                {
                                    pos.y = 0;
                                }
                                else
                                {
                                    pos.y = 0 + curbMesh.Vertices[k].y;
                                }

                                //add curb to vertex list
                                curbVertices.Add(pos);

                                //Set the new vertex uv
                                float x = (float)j / (curbMesh.Vertices.Count - 1);
                                float y = (float)i / Spline.SmoothSplineResoultion + 1;
                                Vector2 uv = new Vector2(x, y);
                                curbUvs.Add(uv);
                            }
                        }

                        //Setup the triangles
                        for (int j = 0; j < curbVertices.Count - (curbMesh.Vertices.Count + 1); j++)
                        {
                            float value = (j + 1) / (float)curbMesh.Vertices.Count % 1f;
                            if (value != 0)
                            {
                                curbTriangles.Add(j);
                                curbTriangles.Add(j + curbMesh.Vertices.Count);
                                curbTriangles.Add(j + curbMesh.Vertices.Count + 1);
                                curbTriangles.Add(j);
                                curbTriangles.Add(j + curbMesh.Vertices.Count + 1);
                                curbTriangles.Add(j + 1);
                            }
                        }
                        curbMesh.Vertices.Reverse();

                        //Add new mesh to the combine instance list
                        CombineInstance ci = new CombineInstance();
                        Mesh m = new Mesh();
                        m.vertices = curbVertices.ToArray();
                        m.uv = curbUvs.ToArray();
                        m.triangles = curbTriangles.ToArray();
                        ci.mesh = m;
                        ci.transform = transform.localToWorldMatrix;
                        allCurbMeshes.Add(ci);
                    }
                    else
                    {
                        int cIndex = rightStartIndex;

                        //create mesh
                        for (int j = 0; j < (offset * 2 + 1); j += 2)
                        {
                            cIndex = Helper.LoopIndex(rightStartIndex, j, proMesh.Vertices.Count);

                            for (int k = 0; k < curbMesh.Vertices.Count; k++)
                            {
                                Vector3 cV = proMesh.Vertices[cIndex];
                                Vector3 cVT = proMesh.Vertices[Helper.SLoopIndex(cIndex, proMesh.Vertices.Count)];
                                Vector3 dir = cV -
                                              cVT;

                                //Set the new vertex position
                                Vector3 pos = cV + (curbMesh.Vertices[k].x * dir.normalized);
                                pos += (curbMesh.ShapeWidth) * dir.normalized;

                                //if we are on the first or last row of vertices for this curb
                                //clamp the y value to 0
                                if (j == 0 || j == (offset * 2 + 1) - 1)
                                {
                                    pos.y = 0;
                                }
                                else
                                {
                                    pos.y = 0 + curbMesh.Vertices[k].y;
                                }

                                //add curb to vertex list
                                curbVertices.Add(pos);

                                //Set the new vertex uv
                                float x = (float)j / (curbMesh.Vertices.Count - 1);
                                float y = (float)i / Spline.SmoothSplineResoultion + 1;
                                Vector2 uv = new Vector2(x, y);
                                curbUvs.Add(uv);
                            }
                        }
                        //Setup the triangles
                        for (int j = 0; j < curbVertices.Count - (curbMesh.Vertices.Count + 1); j++)
                        {
                            float value = (j + 1) / (float)curbMesh.Vertices.Count % 1f;
                            if (value != 0)
                            {
                                curbTriangles.Add(j);
                                curbTriangles.Add(j + curbMesh.Vertices.Count);
                                curbTriangles.Add(j + curbMesh.Vertices.Count + 1);
                                curbTriangles.Add(j);
                                curbTriangles.Add(j + curbMesh.Vertices.Count + 1);
                                curbTriangles.Add(j + 1);
                            }
                        }

                        //Add new mesh to the combine instance list
                        CombineInstance ci = new CombineInstance();
                        Mesh m = new Mesh();
                        m.vertices = curbVertices.ToArray();
                        m.uv = curbUvs.ToArray();
                        m.triangles = curbTriangles.ToArray();
                        ci.mesh = m;
                        ci.transform = transform.localToWorldMatrix;
                        allCurbMeshes.Add(ci);
                    }
                }
            }
        }
        curbsMesh = new Mesh();
        //Combine all curb meshs in to one
        curbsMesh.CombineMeshes(allCurbMeshes.ToArray());

        //Clean up all the meshes by destroying them
        for (int i = 0; i < allCurbMeshes.Count; i++)
        {
            allCurbMeshes[i].mesh.Clear();
        }
    }

    /// <summary>
    /// Check if any there are any overlapping
    /// </summary>
    private void CheckForOverLap()
    {
        //set splineIntersects
        bool splineIntersects = false;
        //set splineIntersectsStart
        int splineIntersectsStart = -1;
        //create a new list of ints
        List<int> pointsToChange = new List<int>();

        for (int i = 3; i < proMesh.Vertices.Count; i += 2)
        {
            int nLineIndex = ((i + 1) + proMesh.Vertices.Count) % proMesh.Vertices.Count;
            int nLineIndexPlusOne = ((i + 2) + proMesh.Vertices.Count) % proMesh.Vertices.Count;

            //get the before, current and next line
            Line bLine = new Line
            {
                p0 = new Vector2(proMesh.Vertices[i - 3].x, proMesh.Vertices[i - 3].z),
                p1 = new Vector2(proMesh.Vertices[i - 2].x, proMesh.Vertices[i - 2].z)
            };
            Line cLine = new Line
            {
                p0 = new Vector2(proMesh.Vertices[i - 1].x, proMesh.Vertices[i - 1].z),
                p1 = new Vector2(proMesh.Vertices[i].x, proMesh.Vertices[i].z)
            };
            Line nLine = new Line
            {
                p0 = new Vector2(proMesh.Vertices[nLineIndex].x, proMesh.Vertices[nLineIndex].z),
                p1 = new Vector2(proMesh.Vertices[nLineIndexPlusOne].x, proMesh.Vertices[nLineIndexPlusOne].z)
            };

            Vector2 intersects;
            //check for intersects from before and current line
            if (Helper.LineInsterection(bLine.p0, bLine.p1, cLine.p0, cLine.p1, out intersects))
            {
                //set splineIntersects to true
                splineIntersects = true;
                //check if splineIntersectsStart is -1
                //set splineIntersectsStart
                if (splineIntersectsStart == -1)
                {
                    splineIntersectsStart = i;
                }
                //add two points to pointsToChange
                pointsToChange.Add(i - 1);
                pointsToChange.Add(i - 3);

                //set newPos
                Vector3 newPos = (proMesh.Vertices[((i - 5) + proMesh.Vertices.Count) % proMesh.Vertices.Count] +
                                 proMesh.Vertices[nLineIndex]) * 0.5f;
            }
            else
            {
                //if splineIntersects is true 
                if (splineIntersects)
                {
                    //i - splineIntersectsStart is greater than 6 then we are on a 
                    //ne curve
                    if (i - splineIntersectsStart > 6)
                    {
                        //set splineIntersects
                        splineIntersects = false;
                        //set splineIntersectsStart
                        splineIntersectsStart = -1;
                        //create a vector3 for the center point
                        Vector3 center = Vector3.zero;

                        //add all the pointsToChange to center
                        for (int k = 0; k < pointsToChange.Count; k++)
                        {
                            center += proMesh.Vertices[pointsToChange[k]];
                        }
                        //divide center by the number of pointsToChange
                        center /= pointsToChange.Count;
                        //set all the pointsToChange to the center
                        for (int k = 0; k < pointsToChange.Count; k++)
                        {
                            proMesh.Vertices[pointsToChange[k]] = center;
                        }
                    }
                    //clear pointsToChange
                    pointsToChange.Clear();
                }
            }
        }
    }

    /// <summary>
    /// return a new point in a bezier curve with 3 points
    /// </summary>
    /// <param name="a_P0"></param>
    /// <param name="a_P1"></param>
    /// <param name="a_P2"></param>
    /// <param name="a_t"></param>
    /// <returns></returns>
    private Vector3 GetPointInSmoothSpline(Vector3 a_P0, Vector3 a_P1, Vector3 a_P2, float a_t)
    {
        //(1 - a_t)2 P0 + 2(1-t)tP1 + tP2
        //float u = 1 - a_t;
        //float tt = a_t * a_t;
        //float uu = u - u;
        //Vector3 p = uu * a_P0;
        //p += 2 * u * a_t * a_P1;
        //p += tt * a_P2;
        //return p;
        return Vector3.Lerp(Vector3.Lerp(a_P0, a_P1, a_t), Vector3.Lerp(a_P1, a_P2, a_t), a_t);
    }
}

/// <summary>
/// Class for the 2d shape
/// This holds all the vertices and the width of the shape
/// </summary>
public class ProTwoDShape
{
    public List<Vector2> Vertices;
    public float ShapeWidth;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="a_vertices"></param>
    /// <param name="a_width"></param>
    public ProTwoDShape(List<Vector2> a_vertices, float a_width)
    {
        Vertices = a_vertices;
        ShapeWidth = a_width;
    }
}

/// <summary>
/// Pro Mesh Object. This object holds all data need for a Mesh
/// -Vertices
/// -Triangles
/// -Uvs
/// </summary>
public class ProMesh
{
    public List<Vector3> Vertices;
    public List<int> Triangles;
    public List<Vector2> Uvs;

    public ProMesh()
    {
        Vertices = new List<Vector3>();
        Triangles = new List<int>();
        Uvs = new List<Vector2>();
    }
}

/// <summary>
/// OrientedPoint class. This class stores a position and a rotation
/// </summary>
public class OrientedPoint
{
    public Vector3 Position;
    public Quaternion Rotation;

    public OrientedPoint(Vector3 a_position, Quaternion a_rotation)
    {
        Position = a_position;
        Rotation = a_rotation;
    }

    public OrientedPoint(Vector3 a_position, Vector3 a_rotation)
    {
        Position = a_position;
        Rotation = Quaternion.Euler(a_rotation.x, a_rotation.y, a_rotation.z);
    }
}

/// <summary>
/// Struct for a line object
/// This contains two points. Both are Vector2s
/// </summary>
public struct Line
{
    public Vector2 p0;
    public Vector2 p1;

    public Vector3 P0()
    {
        return new Vector3(p0.x, 0, p0.y);
    }
    public Vector3 P1()
    {
        return new Vector3(p1.x, 0, p1.y);
    }
}