using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class HelpWindow : EditorWindow
{
    //Get the help window
    public static void Init()
    {
        HelpWindow window = EditorWindow.GetWindow<HelpWindow>();
        window.Show();
    }

    /// <summary>
    /// Display all the information need
    /// </summary>
    private void OnGUI()
    {
        //set the label wordWrap to true.
        //This will wrap all label field around 
        EditorStyles.label.wordWrap = true;

        //Call each on the sections
        GeneralInfo();
        SplineInfo();
        VoronoiInfo();
    }

    /// <summary>
    /// All General Info
    /// </summary>
    private void GeneralInfo()
    {
        //General information
        EditorGUILayout.BeginVertical();
        {
            //Title
            EditorGUILayout.LabelField("General Information", EditorStyles.boldLabel);

            //general info
            EditorGUILayout.LabelField("This is the General Information. Any information about " +
                                       "this tool is displayed within this window. It is recomended " +
                                       "that many of the settings in the inspector are not changed. Any " +
                                       "setting which can be changed will say in the tooltip of the property.");

            //Tooltop
            EditorGUILayout.LabelField("All the exposed variables in the inspector will have a tooltip. " +
                                        "The tooltip will give a quick overview of what that variable is for.");

            //title
            EditorGUILayout.LabelField("Generation Information", EditorStyles.boldLabel);

            //general info on generation
            EditorGUILayout.LabelField("Before we can generate a new race track, you just first press the " +
                                       "'Generate Voronoi Diagram' button. This will generate a Voronoi diagram. " +
                                       "Then you must press the 'Generate Spline' button. This will generate a Spline. "+
                                       "Finally we can press the 'Generate Track Mesh' button. This will generate the track.");

            EditorGUILayout.LabelField("If this is not done then no track will be generated.");
            EditorGUILayout.LabelField("If done correctly. Then a track will be made and attached to a new GameObject.");
        }
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space();
    }

    /// <summary>
    /// All Spline Info
    /// </summary>
    private void SplineInfo()
    {
        //Spline information
        EditorGUILayout.BeginVertical();
        {
            //Title
            EditorGUILayout.LabelField("Spline Information", EditorStyles.boldLabel);

            //general info
            EditorGUILayout.LabelField("This section is for all the Spline information. When the 'Generate Spline' " +
                "button is pressed then a spline shall be generated (This does require a vaild Voronoi Diagram).");

            //foldout
            EditorGUILayout.LabelField("There is a foldout in the inspector. This will show all the setting for the spline. " + 
                "All of these setting can be changed. However it is recommended to only interact with the " +
                "'Display' toggles");

            //change spline
            EditorGUILayout.LabelField("If the spline/mesh which is generated is not what you want. You have the ability " +
                "to edit the spline control points.");

            //how to change spline
            EditorGUILayout.LabelField("This can be done by enabling 'Display Spline Control Points', then enablig 'Control are Modifiable'. This " +
                "will allow you to move the control points. This is useful for if the mesh is overlaping you can edit " +
                "the spline to make the angle of the curve bigger.");

            //save new control points/spline
            EditorGUILayout.LabelField("If you have adjusted the control points on the spline then a new button will apear. " + 
                "The button shall be called 'Apply new control points'. THIS BUTTON MUST BE PRESSED TO CHANGE THE SPLINE. " +
                "If you click off the GameObejct with the Spline Creator script on it the spline will not be changed.");

        }
        EditorGUILayout.EndVertical();
    }

    /// <summary>
    /// All Voronoi Info
    /// </summary>
    private void VoronoiInfo()
    {
        //Voronoi information
        EditorGUILayout.BeginVertical();
        {
            //Title
            EditorGUILayout.LabelField("Voronoi Information", EditorStyles.boldLabel);

            //general info
            EditorGUILayout.LabelField("This section is for all the Voronoi information. When the 'Generate Voronoi Diagram' " +
                "button is pressed then a Voronoi diagram shall be generated.");

            //foldout
            EditorGUILayout.LabelField("There is a foldout in the inspector. This will show all the setting for the Voronoi. " +
                "All of these setting can be changed. The defualt settings are optomised to work with each other.");
        }
        EditorGUILayout.EndVertical();
    }
}
