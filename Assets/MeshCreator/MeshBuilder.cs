using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

public class MeshBuilder : MonoBehaviour {

    public bool autoBuild = true, showOutline = true, showWireframe = false, showHandles = true, showAxes = true, showGrid = false;
    public float area;

    public List<Corner> Corners = new List<Corner>();

    public bool verticalSymmetry = false;
    public bool horizontalSymmetry = false;
    public float symmertryDistance = 0.25f;

    public float smootFactor = 4;
    public float snap = 0;

    public float handleScale = 0.2f;

    void Start()
    {
        BuildMesh();
    }



    public void BuildMesh() {

        List<Vector3> _vertices = new List<Vector3>();

        for (int i = 0; i < Corners.Count; i++)
        {
            int nextIndex, lastIndex;
            if (i > 0) lastIndex = i - 1; else lastIndex = Corners.Count - 1;
            if (i < Corners.Count - 1) nextIndex = i + 1; else nextIndex = 0;

            /*int segCount = Mathf.CeilToInt((
                Vector2.Distance((Corners[i].position + Corners[i].controlPoint1)/2, (Corners[i].controlPoint1 + Corners[nextIndex].controlPoint0) / 2) +
                Vector2.Distance((Corners[nextIndex].position + Corners[nextIndex].controlPoint0) / 2, (Corners[i].controlPoint1 + Corners[nextIndex].controlPoint0) / 2)
                ) * 50 * smootFactor);*/

            int segCount = Mathf.CeilToInt(Mathf.Sqrt(
                Vector2.Distance(Corners[i].position, Corners[i].controlPoint1) +
                Vector2.Distance(Corners[nextIndex].controlPoint0, Corners[i].controlPoint1) +
                Vector2.Distance(Corners[nextIndex].controlPoint0, Corners[nextIndex].position)
                ) * smootFactor);

            if (Corners[i].isBezier && Corners[nextIndex].isBezier)
            {
                for (int j = 0; j < segCount; j++)
                {
                    float t = (float)j/segCount;
                    _vertices.Add(GetPointBezier(Corners[i].position, Corners[nextIndex].position, Corners[i].controlPoint1, Corners[nextIndex].controlPoint0,t));
                }
            }
            else
            {
                _vertices.Add(Corners[i].position);
            }
        }

        Vector2[] vertices2D = new Vector2[_vertices.Count];
        Vector3[] vertices = new Vector3[_vertices.Count];

        for (int i = 0; i < _vertices.Count; i++)
        {
            vertices2D[i] = _vertices[i];
            vertices[i] = _vertices[i];
        }
           

        Triangulator tr = new Triangulator(vertices2D);
        int[] indices = tr.Triangulate();


        Mesh msh = new Mesh();
        msh.vertices = vertices;
        msh.triangles = indices;
        msh.uv = vertices2D;
        msh.RecalculateNormals();
        msh.RecalculateBounds();

        GetComponent<MeshFilter>().mesh = msh;

        area = Mathf.Abs(tr.Area());

        if (GetComponent<PolygonCollider2D>())
        {
            GetComponent<PolygonCollider2D>().points = vertices2D;
        }
    }




#if UNITY_EDITOR

    public void QuickStart()
    {
        if (!GetComponent<MeshRenderer>())
        {
            gameObject.AddComponent<MeshRenderer>();
        }

        if (!GetComponent<MeshFilter>())
        {
            gameObject.AddComponent<MeshFilter>();
        }

        if (!GetComponent<PolygonCollider2D>())
        {
            gameObject.AddComponent<PolygonCollider2D>();
        }

        if (Corners.Count<2)
        {
            Corners.Add(new Corner(new Vector3(0, 1, 0),false));
            Corners.Add(new Corner(new Vector3(-1, -0.5f, 0), false));
            Corners.Add(new Corner(new Vector3(1, -0.5f, 0), false));

            BuildMesh();
        }
        if (!gameObject.GetComponent<MeshRenderer>().sharedMaterial)
        {
            gameObject.GetComponent<MeshRenderer>().material = new Material(Shader.Find("Standard"));
        }
        

    }
    
     
    public void SaveAsAsset()
    {
        string filePath =
        EditorUtility.SaveFilePanelInProject("Save Procedural Mesh", "Procedural Mesh", "asset", "");
        if (filePath == "") return;

        Mesh msh = new Mesh();

        msh.vertices = GetComponent<MeshFilter>().sharedMesh.vertices;
        msh.triangles = GetComponent<MeshFilter>().sharedMesh.triangles;
        msh.uv = GetComponent<MeshFilter>().sharedMesh.uv;

        msh.RecalculateNormals();
        msh.RecalculateBounds();

        AssetDatabase.CreateAsset(msh, filePath);

    }

    public void SaveAsSmartMesh()
    {
        string temporaryTextFileName = "SmartMesh";

        string filePath =
        EditorUtility.SaveFilePanelInProject("Save Procedural Mesh", temporaryTextFileName, "sm", "");
        if (filePath == "") return;


        File.WriteAllText(filePath, "<?xml version=\"1.0\" encoding=\"utf-8\"?> <corners> </corners>");

        XmlDocument xmlDoc = new XmlDocument();

        if (File.Exists(filePath))
        {
            xmlDoc.Load(filePath);

            XmlElement elmRoot = xmlDoc.DocumentElement;

            elmRoot.RemoveAll();

            for (int i = 0; i < Corners.Count; i++)
            {
                XmlElement elmNew = xmlDoc.CreateElement("corner");
                elmNew.SetAttribute("index", i.ToString());

                XmlElement X = xmlDoc.CreateElement("x"); 
                X.InnerText = Corners[i].position.x.ToString(); 

                XmlElement Y = xmlDoc.CreateElement("y");
                Y.InnerText = Corners[i].position.y.ToString(); 

                XmlElement isBezier = xmlDoc.CreateElement("isBezier");
                isBezier.InnerText = Corners[i].isBezier.ToString();


                elmNew.AppendChild(X);
                elmNew.AppendChild(Y);
                elmNew.AppendChild(isBezier);

                if (Corners[i].isBezier)
                {
                    XmlElement controlPoint0 = xmlDoc.CreateElement("controlPoint0");

                    XmlElement cp0X = xmlDoc.CreateElement("x");
                    cp0X.InnerText = Corners[i].controlPoint0.x.ToString();

                    XmlElement cp0Y = xmlDoc.CreateElement("y");
                    cp0Y.InnerText = Corners[i].controlPoint0.y.ToString();


                    XmlElement controlPoint1 = xmlDoc.CreateElement("controlPoint1");

                    XmlElement cp1X = xmlDoc.CreateElement("x");
                    cp1X.InnerText = Corners[i].controlPoint1.x.ToString();

                    XmlElement cp1Y = xmlDoc.CreateElement("y");
                    cp1Y.InnerText = Corners[i].controlPoint1.y.ToString();

                    elmNew.AppendChild(controlPoint0);
                    controlPoint0.AppendChild(cp0X);
                    controlPoint0.AppendChild(cp0Y);
                    elmNew.AppendChild(controlPoint1);
                    controlPoint1.AppendChild(cp1X);
                    controlPoint1.AppendChild(cp1Y);
                }

                elmRoot.AppendChild(elmNew); 
            }


            xmlDoc.Save(filePath);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

    }

    public void ImportSmartMesh()
    {
        string filePath =
        EditorUtility.OpenFilePanel("Select SmartMesh","Assets","sm");
        if (filePath == "") return;

        XmlDocument xmlDoc = new XmlDocument();

        if (File.Exists(filePath))
        {
            xmlDoc.Load(filePath);

            XmlNodeList corners = xmlDoc.GetElementsByTagName("corner");

            Corners = new List<Corner>();

            for (int i = 0; i < corners.Count; i++)
            {
                Corners.Add(new Corner(Vector3.zero, false));
            }

            foreach (XmlElement corner in corners)
            {
                int index = int.Parse(corner.Attributes[0].Value);

                foreach (XmlNode cornerParam in corner.ChildNodes)
                {
                    if (cornerParam.Name == "x")
                    {
                        Corners[index].position.x = float.Parse(cornerParam.InnerText);
                    }
                    if (cornerParam.Name == "y")
                    {
                        Corners[index].position.y = float.Parse(cornerParam.InnerText);
                    }
                    if (cornerParam.Name == "isBezier")
                    {
                        Corners[index].isBezier = bool.Parse(cornerParam.InnerText);
                    }

                    if (cornerParam.Name == "controlPoint0")
                    {
                        foreach (XmlNode cornerParam_ in cornerParam.ChildNodes)
                        {
                            if (cornerParam_.Name == "x")
                            {
                                Corners[index].controlPoint0.x = float.Parse(cornerParam_.InnerText);
                            }
                            if (cornerParam_.Name == "y")
                            {
                                Corners[index].controlPoint0.y = float.Parse(cornerParam_.InnerText);
                            }
                        }
                    }
                    if (cornerParam.Name == "controlPoint1")
                    {
                        foreach (XmlNode cornerParam_ in cornerParam.ChildNodes)
                        {
                            if (cornerParam_.Name == "x")
                            {
                                Corners[index].controlPoint1.x = float.Parse(cornerParam_.InnerText);
                            }
                            if (cornerParam_.Name == "y")
                            {
                                Corners[index].controlPoint1.y = float.Parse(cornerParam_.InnerText);
                            }
                        }
                    }
                }



            }


            BuildMesh();
        }

    }
    #endif

    public Vector3 GetPointBezier(Vector3 A, Vector3 B, Vector3 cpA, Vector3 cpB, float t)
    {
        Vector3 a = Vector3.Lerp(A, cpA, t);
        Vector3 b = Vector3.Lerp(cpA, cpB, t);
        Vector3 c = Vector3.Lerp(cpB, B, t);
        Vector3 d = Vector3.Lerp(a, b, t);
        Vector3 e = Vector3.Lerp(b, c, t);
        return Vector3.Lerp(d, e, t);
    }
}

[System.Serializable]
public class Corner
{
    public Vector3 position;

    public bool isBezier;

    public Vector3 controlPoint0 = new Vector3(1,1,0), controlPoint1 = new Vector3(2, 1, 0);
    //public bool cp0r = true, cp1r = true;

    public Corner(Vector3 _pos, bool _isBezier)
    {
        position = _pos;
        isBezier = _isBezier;
        
        if (_isBezier)
        {
            controlPoint0 = _pos;
            controlPoint1 = _pos;
        }

        //cp0r = true;
        //cp1r = true;
    }

}

