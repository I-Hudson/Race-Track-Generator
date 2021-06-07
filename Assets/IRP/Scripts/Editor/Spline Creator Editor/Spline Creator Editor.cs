using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SplineCreator))]
[System.Serializable]
public class SplineCreatorEditor : Editor
{
    //Ref to spline creator
    SplineCreator sc;

    //Are there changes that need to be saved
    [SerializeField]
    private bool unSavedChanges = false;
    //Store all the unsave control points which have been edited
    [SerializeField]
    private List<Vector3> unApplliedControlPoints;

    private void OnEnable()
    {
        //set target
        sc = (SplineCreator)target;
    }

    /// <summary>
    /// Inspector GUI
    /// </summary>
    public override void OnInspectorGUI()
    {
        //Title
        EditorGUILayout.LabelField("Spline Creator", EditorStyles.boldLabel);

        //Call each control section
        GeneralControl();
        SplineControl();
        VoronoiControls();
        GenerateControls();

        //Create a help button
        //This button will bring you a editor window and 
        //explain what the settings do
        if(GUILayout.Button("HELP"))
        {
            HelpWindow.Init();
        }

        //Repaint the inspector
        Repaint();
    }

    /// <summary>
    /// Enables the Editor to handle an event in the Scene view
    /// </summary>
    private void OnSceneGUI()
    {
        if (sc.Spline != null)
        {
            //if DisplayRawPoints is true then display the raw points of the spline
            if (sc.Spline.DisplayRawPoints)
            {
                DisplaySplineRawPoints();
            }

            //if DisplaySmoothPoints is true then display the smoothed points of the spline
            if (sc.Spline.DisplaySmoothPoints)
            {
                DisplaySplineSmoothPoints();
            }

            //if DisplayControlPoints is true then display the control points of the spline
            if (sc.Spline.DisplayControlPoints)
            {
                DisplaySplineControlPoints();
            }

            //if DisplayVoronoi is true then display the voronoi diagram
            if (sc.Voronoi.DisplayVoronoi)
            {
                DisplayVoronoiDiagram();
            }
            //reapint the scene view
            SceneView.RepaintAll();
        }
    }

    /// <summary>
    /// Control any variable that is not spefic for spline or voronoi
    /// </summary>
    private void GeneralControl()
    {
        EditorGUILayout.BeginVertical();
        {
            //Mesh To Extrude
            sc.MeshToExtrude = (Mesh)EditorGUILayout.ObjectField(new GUIContent("Track Mesh", 
                                                        "This mesh will be extrude along the spline from the center " +
                                                        "(Not recommended to be changed)"),
                                                          sc.MeshToExtrude, typeof(Mesh), true);

            //Mesh Width Scale
            sc.MeshWidthScale = EditorGUILayout.FloatField(new GUIContent("Track Mesh Width Scale",
                                                        "Scale of the width for the track mesh(if 1 then the generated track will be the same width as the MeshToExtrude."),
                                                          sc.MeshWidthScale);

            //Curb mesh
            sc.CurbMesh = (Mesh)EditorGUILayout.ObjectField(new GUIContent("Curb Mesh",
                                                          "This is the curb mesh (Not recommended to be changed)"),
                                                          sc.CurbMesh, typeof(Mesh), true);

            //Track Material
            sc.TrackMaterial = (Material)EditorGUILayout.ObjectField(new GUIContent("Track Material", 
                                                                           "Material added to the track mesh when generated" +
                                                                           "(Not recommended to be changed)"),
                                                                           sc.TrackMaterial, typeof(Material), true);

            //Curb Material
            sc.CurbMaterial = (Material)EditorGUILayout.ObjectField(new GUIContent("Curb Material", 
                                                                           "Material added to the curb mesh when generated " +
                                                                           "(Not recommended to be changed)"),
                                                                           sc.CurbMaterial, typeof(Material), true);

            //Voronoi Class
            sc.Voronoi = (Voronoi)EditorGUILayout.ObjectField(new GUIContent("Voronoi", 
                                                                   "Reference to Voronoi script"),
                                                                   sc.Voronoi, typeof(Voronoi), true);
            //Random seed value
            sc.RandomSeed = EditorGUILayout.Toggle(new GUIContent("Random Seed", "If true then a seed will be randomly generated"), sc.RandomSeed);

            //Check if randomSeed is false or true. If it is false then
            //allow the user to enter there own seed
            if (!sc.RandomSeed)
            {
                sc.Seed = EditorGUILayout.IntField("Seed", sc.Seed);
            }
            //Display seed value made
            EditorGUILayout.IntField("Track Seed", sc.Seed);
            //Save seed value

            //Display Mesh made
            //Save mesh to assets
        }
        EditorGUILayout.EndVertical();
    }

    /// <summary>
    /// Control all variables associated with the spline 
    /// </summary>
    private void SplineControl()
    {
        //if there is no spline then create one
        if(sc.Spline == null)
        {
            sc.Spline = new Spline();
        }

        sc.Spline.EditorFoldOut = EditorGUILayout.Foldout(sc.Spline.EditorFoldOut, 
                                  new GUIContent("Spline Controls", "Display all the control which can be changed for the spline"));

        if(sc.Spline.EditorFoldOut)
        {
            EditorGUILayout.BeginVertical();
            {
                //curve type
                sc.Spline.SplineCurveType = (CurveType)EditorGUILayout.EnumPopup(new GUIContent("Spline Curve Type", 
                                            "Choose which curve method to use. (Bezier or Catmull-Rom)"), 
                                            sc.Spline.SplineCurveType);

                //spline scale
                sc.Spline.SplineScale = EditorGUILayout.FloatField(new GUIContent("Spline Scale", 
                                        "Spline scale in relation to the voronoi diagram."), 
                                        sc.Spline.SplineScale);

                //Spline resoulation
                int res = EditorGUILayout.IntField(new GUIContent("Spline Resolution", 
                                                    "Num of points each curve is smoothed along. " +
                                                    "(Higher Num = Smoother Curve. Recomened to keep around 16 - 32)"),
                                                    sc.Spline.SmoothSplineResoultion);
                if(res != sc.Spline.SmoothSplineResoultion)
                {
                    sc.Spline.SmoothSplineResoultion = res;
                    sc.Spline.CreateSplineFromControlPoints();
                }

                sc.Spline.ApplyAutoModifcations = EditorGUILayout.Toggle(new GUIContent("Try auto fix", 
                                                  "With this option on the spline will try and reduce any issue with the spline/mesh." +
                                                  "An example being the mesh overlapping. This 'fixed' by moving the control points." +
                                                  "(If you want full control please untick this option)"),
                                                  sc.Spline.ApplyAutoModifcations);

                sc.Spline.SmoothSplineClosed = EditorGUILayout.Toggle(new GUIContent("Spline Closed", 
                                               "Should the spline be connected from start to finish." +
                                               "(It is recommended to keep this true)"),
                                               sc.Spline.SmoothSplineClosed);
                //draw raw points
                sc.Spline.DisplayRawPoints = EditorGUILayout.Toggle(new GUIContent("Display Spline Raw Points",
                                               "Displays all the site points on the spline"),
                                               sc.Spline.DisplayRawPoints);

                //draw smoothed out points
                sc.Spline.DisplaySmoothPoints = EditorGUILayout.Toggle(new GUIContent("Display Spline Smooth Points",
                                               "Display the current spline which has been smoothed. (When true " + 
                                               "a green line will be displayed. This is the current spline)"),
                                               sc.Spline.DisplaySmoothPoints);

                //Draw control points
                sc.Spline.DisplayControlPoints = EditorGUILayout.Toggle(new GUIContent("Display Spline Control Points",
                                               "Display all the control points on the spline"),
                                               sc.Spline.DisplayControlPoints);
                //if the control points are being displayed
                if (sc.Spline.DisplayControlPoints)
                {
                    //Display the toggle to allow for the control points to be modified 
                    sc.Spline.ControlPointsModifiable = EditorGUILayout.Toggle(new GUIContent("Control are Modifiable",
                                                        "Allow the user to edit the control points positions"),
                                                        sc.Spline.ControlPointsModifiable);

                    //if there are unsaved changes
                    if(unSavedChanges)
                    {
                        //allow the user to apply them
                        if(GUILayout.Button("Apply new control points"))
                        {
                            sc.Spline.CreateSplineFromControlPoints(unApplliedControlPoints);
                            unSavedChanges = false;
                        }
                    }
                }

            }
            EditorGUILayout.EndVertical();
        }
    }

    /// <summary>
    /// Control all the variables associated with the Voronoi diagram
    /// </summary>
    private void VoronoiControls()
    {
        if (sc.Voronoi == null)
        {
            return;
        }

        //Are the control foldedout
        sc.Voronoi.VoronoiFoldOut = EditorGUILayout.Foldout(sc.Voronoi.VoronoiFoldOut,
                                    new GUIContent("Voronoi Controls",
                                    "Display all the controls for the Voronoi Diagram"));
        //If the foldout is true
        if (sc.Voronoi.VoronoiFoldOut)
        {
            //If the voronoi is no null
            if (sc.Voronoi)
            {
                //Number of sites to generate
                sc.Voronoi.SiteCount = EditorGUILayout.IntField(new GUIContent("Num of Sites to generate",
                                       ""),
                                       sc.Voronoi.SiteCount);

                //Size of the voronoi diagram
                sc.Voronoi.GridSize = EditorGUILayout.FloatField(new GUIContent("Size of Voronoi",
                                       ""),
                                       sc.Voronoi.GridSize);

                //Number of cells to include in getting the outter edge for the spline
                sc.Voronoi.SplineCellCount = EditorGUILayout.IntField(new GUIContent("Num of Cells to include in spline",
                                       "Higher numbers include more cells to get the outter edge from. (It is recommended" +
                                       "to keep this number below 6. All testing was done with 4)"),
                                       sc.Voronoi.SplineCellCount);

                sc.Voronoi.DisplayVoronoi = EditorGUILayout.Toggle(new GUIContent("Display Voronoi Diagram",
                                        ""),
                                        sc.Voronoi.DisplayVoronoi);
            }
            else
            {
                //If the voronoi is null then promted the user to click the button
                EditorGUILayout.LabelField(new GUIContent("Please Click the \"" + "Generate Voronoi Diagram Button" + "\""));
            }
        }
    }

    /// <summary>
    /// Control all the generate methods 
    /// </summary>
    private void GenerateControls()
    {
        //Generate the voronoi diagram
        if (GUILayout.Button(new GUIContent("Generate Voronoi Diagram", "")))
        {
            sc.GenerateVoronoi();
        }

        //Generate the voronoi diagram
        if (GUILayout.Button(new GUIContent("Generate Spline", "")))
        {
            sc.GenerateSpline();
            unApplliedControlPoints = sc.Spline.ControlPoints;
        }

        //Generate the track Mesh (If this is called before track spline 
        //the spline will be generated 
        if (GUILayout.Button(new GUIContent("Generate Track Mesh", "")))
        {
            sc.CreateTrack();
        }

    }

    /// <summary>
    /// Display all the raw points of the spline
    /// </summary>
    private void DisplaySplineRawPoints()
    {
        //check if AllRawPoints is not null
        if (sc.Spline.AllRawPoints != null)
        {
            //if AllRawPoints is valid then loop though all the points
            //drawing then and connecting them to the next point
            for (int i = 0; i < sc.Spline.AllRawPoints.Count; i++)
            {
                Handles.color = Color.red;
                Handles.CylinderHandleCap(0, sc.Spline.AllRawPoints[i], 
                                          Quaternion.AngleAxis(90, Vector3.right), 25f, 
                                          EventType.Repaint);

                Handles.color = Color.magenta;
                Handles.DrawLine(sc.Spline.AllRawPoints[i], 
                                sc.Spline.AllRawPoints[Helper.ALoopIndex(i, sc.Spline.AllRawPoints.Count)]);
            }
        }
    }

    /// <summary>
    /// Display all the smoothed points of the spline
    /// </summary>
    private void DisplaySplineSmoothPoints()
    {
        //check if AllSmoothPoints is valid
        if (sc.Spline.AllSmoothPoints != null)
        {
            //if AllSmoothPoints is valid then loop though all the points
            //drawing them and connecting them
            for (int i = 0; i < sc.Spline.AllSmoothPoints.Count; i++)
            {
                Handles.color = Color.yellow;
                Handles.CylinderHandleCap(0, sc.Spline.AllSmoothPoints[i],
                                          Quaternion.AngleAxis(90, Vector3.right), 2f,
                                          EventType.Repaint);

                Handles.color = Color.green;
                Handles.DrawLine(sc.Spline.AllSmoothPoints[i],
                                sc.Spline.AllSmoothPoints[Helper.ALoopIndex(i, sc.Spline.AllSmoothPoints.Count)]);
            }
        }
    }

    /// <summary>
    /// Display all the control points 
    /// </summary>
    private void DisplaySplineControlPoints()
    {
        //if ControlPoints is valid
        if (sc.Spline.ControlPoints != null)
        {
            //if unApplliedControlPoints is null
            //then set unApplliedControlPoints to the spline's control points
            if (unApplliedControlPoints == null || unApplliedControlPoints.Count != sc.Spline.ControlPoints.Count)
            {
                unApplliedControlPoints = sc.Spline.ControlPoints;
            }

            //If the control points are modifiable
            //them check for changes to the points 
            if (sc.Spline.ControlPointsModifiable)
            {
                if (sc.Spline.SplineCurveType == CurveType.Bezier)
                {
                    for (int i = 0; i < sc.Spline.ControlPoints.Count; i += 3)
                    {
                        //Draw the start and end control point
                        Handles.color = (i != 0) ? Color.blue : Color.black;
                        Vector3 newPos = Handles.FreeMoveHandle(unApplliedControlPoints[i],
                                                                Quaternion.identity, 8f, Vector3.zero,
                                                                Handles.CylinderHandleCap);
                        //if newPos is not the same as the control point on the spline 
                        if (sc.Spline.ControlPoints[i] != newPos)
                        {
                            //set unSavedChanges to true
                            unSavedChanges = true;
                            //set unApplliedControlPoints[i] to newPos
                            unApplliedControlPoints[i] = newPos;
                        }
                        newPos = Handles.FreeMoveHandle(unApplliedControlPoints[i + 2],
                                                                Quaternion.identity, 8f, Vector3.zero,
                                                                Handles.CylinderHandleCap);
                        //if newPos is not the same as the control point on the spline 
                        if (sc.Spline.ControlPoints[i + 2] != newPos)
                        {
                            //set unSavedChanges to true
                            unSavedChanges = true;
                            //set unApplliedControlPoints[i] to newPos
                            unApplliedControlPoints[i + 2] = newPos;
                        }

                        //Draw the site point
                        Handles.color = Color.red;
                        newPos = Handles.FreeMoveHandle(unApplliedControlPoints[i + 1],
                                                                Quaternion.identity, 8f, Vector3.zero,
                                                                Handles.CylinderHandleCap);
                        //if newPos is not the same as the control point on the spline 
                        if (sc.Spline.ControlPoints[i + 1] != newPos)
                        {
                            //set unSavedChanges to true
                            unSavedChanges = true;
                            //set unApplliedControlPoints[i] to newPos
                            unApplliedControlPoints[i + 1] = newPos;
                        }
                    }
                    for (int i = 0; i < sc.Spline.ControlPoints.Count; i += 3)
                    {
                        //Draw the line between the control points
                        Handles.color = Color.cyan;
                        Handles.DrawBezier(unApplliedControlPoints[i], unApplliedControlPoints[i + 2],
                                           unApplliedControlPoints[i + 1], unApplliedControlPoints[i + 1],
                                           Color.cyan, null, 2);
                        Handles.DrawLine(sc.Spline.ControlPoints[i + 2], sc.Spline.ControlPoints[Helper.LoopIndex(i, 3, sc.Spline.ControlPoints.Count)]);
                    }
                }
                else if(sc.Spline.SplineCurveType == CurveType.CatmullRom)
                {
                    for (int i = 0; i < sc.Spline.ControlPoints.Count; i++)
                    {
                        //Draw the start and end control point
                        Handles.color = Color.red;
                        Vector3 newPos = Handles.FreeMoveHandle(unApplliedControlPoints[i],
                                                                Quaternion.identity, 8f, Vector3.zero,
                                                                Handles.CylinderHandleCap);
                        //if newPos is not the same as the control point on the spline 
                        if (sc.Spline.ControlPoints[i] != newPos)
                        {
                            //set unSavedChanges to true
                            unSavedChanges = true;
                            //set unApplliedControlPoints[i] to newPos
                            unApplliedControlPoints[i] = newPos;
                        }
                    }
                    List<Vector3> points = new List<Vector3>();
                    //setup all the catmull-rom points to draw
                    for (int i = 0; i < sc.Spline.ControlPoints.Count; i++)
                    {
                        for (int j = 0; j < sc.Spline.SmoothSplineResoultion; j++)
                        {
                            points.Add(GetCatmullRom(unApplliedControlPoints[Helper.SLoopIndex(i, sc.Spline.ControlPoints.Count)],
                                                    unApplliedControlPoints[i],
                                                    unApplliedControlPoints[Helper.ALoopIndex(i, sc.Spline.ControlPoints.Count)],
                                                    unApplliedControlPoints[Helper.LoopIndex(i, 2, sc.Spline.ControlPoints.Count)],
                                                    j * (1f / sc.Spline.SmoothSplineResoultion)));
                        }
                    }
                    Handles.color = Color.cyan;
                    //draw all the points
                    for (int i = 0; i < points.Count; i++)
                    {
                        Handles.DrawLine(points[i], points[Helper.ALoopIndex(i, points.Count)]);
                    }
                }
            }
            else
            {
                if (sc.Spline.SplineCurveType == CurveType.Bezier)
                {
                    for (int i = 0; i < sc.Spline.ControlPoints.Count; i += 3)
                    {
                        //Draw the start and end control point
                        Handles.color = Color.blue;
                        Handles.CylinderHandleCap(0, sc.Spline.ControlPoints[i],
                                                  Quaternion.AngleAxis(90, Vector3.right), 8f,
                                                  EventType.Repaint);

                        Handles.CylinderHandleCap(0, sc.Spline.ControlPoints[i + 2],
                                                  Quaternion.AngleAxis(90, Vector3.right), 8f,
                                                  EventType.Repaint);


                        //Draw the site point
                        Handles.color = Color.red;
                        Handles.CylinderHandleCap(0, sc.Spline.ControlPoints[i + 1],
                                                  Quaternion.AngleAxis(90, Vector3.right), 8f,
                                                  EventType.Repaint);

                        //Draw the line between the control points
                        Handles.color = Color.cyan;
                        Handles.DrawLine(sc.Spline.ControlPoints[i], sc.Spline.ControlPoints[i + 1]);
                        Handles.DrawLine(sc.Spline.ControlPoints[i + 1], sc.Spline.ControlPoints[i + 2]);
                        Handles.DrawLine(sc.Spline.ControlPoints[i + 2], sc.Spline.ControlPoints[Helper.LoopIndex(i, 3, sc.Spline.ControlPoints.Count)]);
                    }
                }
                else if (sc.Spline.SplineCurveType == CurveType.CatmullRom)
                {
                    for (int i = 0; i < sc.Spline.ControlPoints.Count; i++)
                    {
                        //Draw the site point
                        Handles.color = Color.red;
                        Handles.CylinderHandleCap(0, sc.Spline.ControlPoints[i],
                                                Quaternion.AngleAxis(90, Vector3.right), 8f,
                                                EventType.Repaint);

                        //Draw the line between the control points
                        Handles.color = Color.cyan;
                        Handles.DrawLine(sc.Spline.ControlPoints[i], sc.Spline.ControlPoints[Helper.ALoopIndex(i , sc.Spline.ControlPoints.Count)]);
                    }
                }
            }
        }
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
    /// Display the Voronoi Diagram
    /// </summary>
    private void DisplayVoronoiDiagram()
    {
        Tools.current = Tool.None;
        //Check if the cells array in the Voronoi class is not null
        if (sc.Voronoi.Cells != null)
        {
            //Set the Handles color
            Handles.color = Color.grey;
            //Loop 
            for (int i = 0; i < sc.Voronoi.CellCount; i++)
            {
                //Check if the current cell is not null
                if (sc.Voronoi.Cells[i] != null)
                {
                    //Get the first edge in the current cell
                    vEdge fE = sc.Voronoi.FirstEdge(sc.Voronoi.Cells[i]);
                    //Set e to the first edge
                    vEdge e = fE;
                    do
                    {
                        //Draw a line 
                        Handles.DrawLine(e.v0.Site.Vector3(), e.v1.Site.Vector3());
                        //Set e to the next ege
                        e = e.Next;
                        //While e is not the first edge contuine
                        //This will allow us to go around though each 
                        //edge unitll we reach the first edge again
                    } while (e != fE);
                }
            }
        }
    }
}