using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

//AssetDatabase使用相对路径，io使用绝对路径
public class BatchingTool : EditorWindow
{
    private static BatchingTool s_window = null;
    private string mPath;

    [MenuItem("Tools/BatchingResoureces")]
    public static void PrecomputedLighting()
    {
        s_window = EditorWindow.GetWindow<BatchingTool>(false,"BatchingTool", true);
        s_window.Show();
    }

    void OnGUI()
    {
        EditorGUILayout.LabelField("Path:");
        mPath = RelativeAssetPathTextField(mPath);

        if(GUILayout.Button("绘制结果"))
        {
            string[] LookFor = {mPath};
            //t：想要的类型
            string[] guids = AssetDatabase.FindAssets("t:material", LookFor);

            foreach(string guid in guids)
            {
                string MaterialPath = AssetDatabase.GUIDToAssetPath(guid);
                string TextureAPath = MaterialPath.Remove(MaterialPath.LastIndexOf("/")) + "/A.jpg";
                string TextureCPath = MaterialPath.Remove(MaterialPath.LastIndexOf("/")) + "/C.jpg";
                
                string TextureGUID_A = AssetDatabase.AssetPathToGUID(TextureAPath);
                string TextureGUID_C = AssetDatabase.AssetPathToGUID(TextureCPath);

                string MaterialAbsolutePath = ConvertRelativePathToAbsolutePath(MaterialPath);

                if(File.Exists(MaterialAbsolutePath))
                {
                    string MaterialAssetContent = File.ReadAllText(MaterialAbsolutePath);
                    string MaterialAssetContentModified = "";
                    MaterialAssetContentModified = MaterialAssetContent;
                    MaterialAssetContentModified = MaterialAssetContentModified.Replace(TextureGUID_A,TextureGUID_C);
                    File.WriteAllText(MaterialAbsolutePath, MaterialAssetContentModified)
                }
            }
            AssetDatabase.Refresh();
            AssetDatabase.SaveAssets();
        }
    }

    public GUIStyle TextFieldRoundEdge;
    public GUIStyle TextFieldRoundEdgeCancelButton;
    public GUIStyle TextFieldRoundEdgeCancelButtonEmpty;
    public GUIStyle TransparentTextField;

    private string RelativeAssetPathTextField(string path)
    {
        
        if(TextFieldRoundEdge == null)
        {
            TextFieldRoundEdge = new GUIStyle("SearchTextField");
            TextFieldRoundEdgeCancelButton = new GUIStyle("SearchCancelButton");
            TextFieldRoundEdgeCancelButtonEmpty = new GUIStyle("SearchCancelButtonEmpty");
            TransparentTextField = new GUIStyle(EditorStyles.whiteLabel);
            TransparentTextField.normal.textColor = EditorStyles.textField.normal.textColor;
        }

        Rect position = EditorGUILayout.GetControlRect();
        GUIStyle textFieldRoundEdge = TextFieldRoundEdge;
        GUIStyle transparentTextField = TransparentTextField;
        GUIStyle gUIStyle = (path != "") ? TextFieldRoundEdgeCancelButton : TextFieldRoundEdgeCancelButtonEmpty;
        position.width -= gUIStyle.fixedWidth;
        if(Event.current.type == EventType.Repaint)
        {
            GUI.contentColor = (EditorGUIUtility.isProSkin ? Color.black : new Color(0f, 0f, 0f, 0.5f));
            textFieldRoundEdge.Draw(position, new GUIContent("Assets/"), 0);
            GUI.contentColor = Color.white;
        }
        Rect rect = position;
        float num = textFieldRoundEdge.CalcSize(new GUIContent("Assets/")).x -2f;
        rect.x += num;
        rect.y += 1f;
        rect.width -= num;
        EditorGUI.BeginChangeCheck();
        path = EditorGUI.TextField(rect, path,transparentTextField);
        if(EditorGUI.EndChangeCheck())
            path = path.Replace('\\', '/');
        
        position.x += position.width;
        position.width = gUIStyle.fixedWidth;
        position.height = gUIStyle.fixedHeight;
        if(GUI.Button(position, GUIContent.none, gUIStyle) && path != "")
        {
            path = "";
            GUI.changed = true;
            GUIUtility.keyboardControl = 0;
        }
        
        return path;
    }

    //相对路径->绝对路径
    private string ConvertRelativePathToAbsolutePath(string RelativePath)
    {
        string DataPath = Application.dataPath;
        string AbsolutePath = DataPath.Remove(DataPath.LastIndexOf("/")) + "/" + RelativePath;
        RelativePath = AbsolutePath.Replace("/", @"\");

        return AbsolutePath;
    }

    //通过text格式copy资产，路径为绝对路径
    private void CopyFileByText(string OldPath, string NewPath)
    {
        string OldAssetContent = File.ReadAllText(OldPath);
        File.WriteAllText(NewPath, OldAssetContent);
    }
}
