
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

[CustomEditor(typeof(MeshBuilder))]
[CanEditMultipleObjects]
public class MeshBuilderEditor : Editor
{
    int hotID = -1, lastHotControl = -1;
    Vector3 vSymPos,hSymPos;
    Tool oldToolType = Tool.Move;

    //void OnEnable() void Init
    public void OnSceneGUI()
    {
        Event e = Event.current;
        Vector3 mousePos = HandleUtility.GUIPointToWorldRay(e.mousePosition).origin;

        var t = (target as MeshBuilder);

        float handleScale = Mathf.Sqrt(Mathf.Abs(mousePos.z - t.transform.position.z)+1);

        mousePos.z = t.transform.position.z;

        Texture2D te = new Texture2D(1, 1, TextureFormat.RGBA32, false);
        te.SetPixel(0, 0, Color.white);
        te.Apply();
        

        List<BezierPoint> lineVerts = new List<BezierPoint>();
        Vector3 v;

        float minX = 0, maxX = 0, minY = 0, maxY = 0;

        if ((EventType.KeyDown == e.type && KeyCode.C == e.keyCode))
        {
            if (Tools.current != Tool.None)
            {
                oldToolType = Tools.current;
                Tools.current = Tool.None;
            }
            else
            {
                Tools.current = oldToolType;
            }
            
        }

        if (t.Corners.Count != 0)
        {
            minX = t.Corners[0].position.x;
            maxX = t.Corners[0].position.x;
            minY = t.Corners[0].position.y;
            maxY = t.Corners[0].position.y;

            for (int i = 0; i < t.Corners.Count; i++)
            {
                if (t.Corners[i].position.x > maxX) maxX = t.Corners[i].position.x;
                else if (t.Corners[i].position.x < minX) minX = t.Corners[i].position.x;

                if (t.Corners[i].position.y > maxY) maxY = t.Corners[i].position.y;
                else if (t.Corners[i].position.y < minY) minY = t.Corners[i].position.y;

                if (t.Corners[i].isBezier)
                {
                    int nextIndex = i + 1;
                    if (nextIndex > t.Corners.Count - 1) nextIndex = 0;
                    int lastIndex = i - 1;
                    if (lastIndex < 0) lastIndex = t.Corners.Count - 1;

                    if (t.Corners[lastIndex].isBezier)
                    {
                        if (t.Corners[i].controlPoint0.x > maxX) maxX = t.Corners[i].controlPoint0.x;
                        else if (t.Corners[i].controlPoint0.x < minX) minX = t.Corners[i].controlPoint0.x;

                        if (t.Corners[i].controlPoint0.y > maxY) maxY = t.Corners[i].controlPoint0.y;
                        else if (t.Corners[i].controlPoint0.y < minY) minY = t.Corners[i].controlPoint0.y;
                    }

                    if (t.Corners[nextIndex].isBezier)
                    {
                        if (t.Corners[i].controlPoint1.x > maxX) maxX = t.Corners[i].controlPoint1.x;
                        else if (t.Corners[i].controlPoint1.x < minX) minX = t.Corners[i].controlPoint1.x;

                        if (t.Corners[i].controlPoint1.y > maxY) maxY = t.Corners[i].controlPoint1.y;
                        else if (t.Corners[i].controlPoint1.y < minY) minY = t.Corners[i].controlPoint1.y;
                    }
                }
                
            }
        }

        float sf = 1;

        if (t.snap > 0)
        {
            sf = t.snap;
        }

        if ((t.showGrid && !Event.current.shift) || (Event.current.control && Event.current.shift))
        {
            Handles.color = new Color(0.8f,0.8f,0.8f,0.5f);

            minX = RoundFloat.RoundToFloat(minX, sf);
            minY = RoundFloat.RoundToFloat(minY, sf);
            maxX = RoundFloat.RoundToFloat(maxX, sf);
            maxY = RoundFloat.RoundToFloat(maxY, sf);

            minX -= 4 * sf;
            minY -= 4 * sf;
            maxX += 4 * sf;
            maxY += 4 * sf;

            for (int i = 0; i <= Mathf.RoundToInt((maxY-minY)/sf); i++)
            {
                Vector3 sPos = t.transform.position + t.transform.rotation * new Vector3(t.transform.localScale.x * minX, t.transform.localScale.y * minY);
                Vector3 ePos = t.transform.position + t.transform.rotation * new Vector3(t.transform.localScale.x * maxX, t.transform.localScale.y * minY);

                sPos += t.transform.rotation * new Vector3(0, t.transform.localScale.y * i * sf, 0);
                ePos += t.transform.rotation * new Vector3(0, t.transform.localScale.y * i * sf, 0);

                Handles.DrawLine(sPos, ePos);
            }

            for (int i = 0; i <= Mathf.RoundToInt((maxX - minX) / sf); i++)
            {
                Vector3 sPos = t.transform.position + t.transform.rotation * new Vector3(t.transform.localScale.x * minX, t.transform.localScale.y * minY);
                Vector3 ePos = t.transform.position + t.transform.rotation * new Vector3(t.transform.localScale.x * minX, t.transform.localScale.y * maxY);

                sPos += t.transform.rotation * new Vector3(t.transform.localScale.x * i * sf, 0, 0);
                ePos += t.transform.rotation * new Vector3(t.transform.localScale.x * i * sf, 0, 0);

                Handles.DrawLine(sPos, ePos);
            }
        }


        if ((t.showAxes && !Event.current.shift) || (Event.current.control && Event.current.shift))
        {
            Handles.color = new Color(1, 1, 1, 0.75f);

            if (Event.current.control && Event.current.shift)
            {
                Handles.color = new Color(0.9f, 0, 0, 1);
            }

            Handles.DrawLine(t.transform.position + t.transform.rotation * new Vector3(0, t.transform.localScale.y * (minY - sf), 0), t.transform.position + t.transform.rotation * new Vector3(0, t.transform.localScale.y * (maxY + sf), 0));
            Handles.DrawLine(t.transform.position + t.transform.rotation * new Vector3(t.transform.localScale.x * (minX - sf), 0, 0), t.transform.position + t.transform.rotation * new Vector3(t.transform.localScale.x * (maxX + sf), 0, 0));

            if (minX / maxX > 0)
            {
                Handles.DrawLine(t.transform.position + t.transform.rotation * new Vector3(- sf, 0, 0), t.transform.position + t.transform.rotation * new Vector3(sf, 0, 0));
            }

            if (minY / maxY > 0)
            {
                Handles.DrawLine(t.transform.position + t.transform.rotation * new Vector3(0,-sf, 0), t.transform.position + t.transform.rotation * new Vector3(0, sf, 0));
            }


        }

        if (Event.current.control && Event.current.shift)
        {
            v = mousePos - t.transform.position;
            v.x /= t.transform.localScale.x;
            v.y /= t.transform.localScale.y;

            Vector3 p = Quaternion.Inverse(t.transform.rotation) * v;
            

            if (t.snap > 0)
            {
                p.x = RoundFloat.RoundToFloat(p.x, t.snap);
                p.y = RoundFloat.RoundToFloat(p.y, t.snap);
                p.z = RoundFloat.RoundToFloat(p.z, t.snap);
            }

            p.x *= t.transform.localScale.x;
            p.y *= t.transform.localScale.y;

            p = t.transform.rotation * p;

            p += t.transform.position;

            Handles.CircleCap(120, p, Quaternion.identity, 0.1f);

            v = Quaternion.Inverse(t.transform.rotation) * (p - t.transform.position);
            v.x /= t.transform.localScale.x;
            v.y /= t.transform.localScale.y;

            for (int i = 0; i < t.Corners.Count; i++)
            {
                t.Corners[i].position -= v;
                t.Corners[i].controlPoint0 -= v;
                t.Corners[i].controlPoint1 -= v;
            }

            t.transform.position = p;
            
            if(t.autoBuild)
            t.BuildMesh();

        }


        Color myColor = new Color();
        ColorUtility.TryParseHtmlString("#6FD200FF", out myColor);
        Handles.color = myColor;
        


        for (int i = 0; i < t.Corners.Count; i++)
        {
            int nextIndex = i + 1;
            if (nextIndex > t.Corners.Count - 1) nextIndex = 0;

            if (t.Corners[i].isBezier && t.Corners[nextIndex].isBezier)
            {
                /*int segCount = Mathf.CeilToInt((
                Vector2.Distance((t.Corners[i].position + t.Corners[i].controlPoint1) / 2, (t.Corners[i].controlPoint1 + t.Corners[nextIndex].controlPoint0) / 2) +
                Vector2.Distance((t.Corners[nextIndex].position + t.Corners[nextIndex].controlPoint0) / 2, (t.Corners[i].controlPoint1 + t.Corners[nextIndex].controlPoint0) / 2)
                ) * 50 * t.smootFactor);*/
                int segCount = Mathf.CeilToInt(Mathf.Sqrt(
                Vector2.Distance(t.Corners[i].position, t.Corners[i].controlPoint1) +
                Vector2.Distance(t.Corners[nextIndex].controlPoint0, t.Corners[i].controlPoint1) +
                Vector2.Distance(t.Corners[nextIndex].controlPoint0, t.Corners[nextIndex].position)
                ) * t.smootFactor);

                for (int j = 0; j < segCount; j++)
                {
                    float _t = (float)j / segCount;

                    v = t.GetPointBezier(t.Corners[i].position, t.Corners[nextIndex].position, t.Corners[i].controlPoint1, t.Corners[nextIndex].controlPoint0, _t);

                    v.x *= t.transform.localScale.x;
                    v.y *= t.transform.localScale.y;

                    lineVerts.Add(new BezierPoint(t.transform.position +  t.transform.rotation * v,i));
                }
            }
            else
            {
                v = t.Corners[i].position;

                v.x *= t.transform.localScale.x;
                v.y *= t.transform.localScale.y;

                lineVerts.Add(new BezierPoint(t.transform.position + t.transform.rotation * v, i));
            }

        }

        if (t.Corners.Count > 0)
        {
            v = t.Corners[0].position;

            v.x *= t.transform.localScale.x;
            v.y *= t.transform.localScale.y;

            lineVerts.Add(new BezierPoint(t.transform.position + t.transform.rotation * v, t.Corners.Count - 1));

            if (t.showOutline && !Event.current.shift)
            {
                Vector3[] lvs = new Vector3[lineVerts.Count];

                for (int i = 0; i < lineVerts.Count; i++)
                {
                    lvs[i] = lineVerts[i].position;
                }

                Handles.DrawAAPolyLine(te, 3, lvs);
            }

            if (t.showWireframe && !Event.current.shift)
            {
                EditorUtility.SetSelectedWireframeHidden(t.GetComponent<MeshRenderer>(), false);
            }
            else
            {
                EditorUtility.SetSelectedWireframeHidden(t.GetComponent<MeshRenderer>(), true);
            }



            float minPointerDist = handleScale;
            float minDist = minPointerDist + 1, minVertDist = float.MaxValue;
            int minDistIndex = 0, minVertIndex = 0;

            bool updateMesh = false;

            //Find closest corner
            for (int i = 0; i < t.Corners.Count; i++)
            {
                v = t.Corners[i].position;

                v.x *= t.transform.localScale.x;
                v.y *= t.transform.localScale.y;

                if (Vector3.Distance(mousePos, t.transform.rotation * v + t.transform.position) <= minVertDist)
                {
                    minVertDist = Vector3.Distance(mousePos, t.transform.rotation * v + t.transform.position);
                    minVertIndex = i;
                }

            }

            //Find closest point on outline
            for (int i = 0; i < lineVerts.Count - 1; i++)
            {
                int nextIndex = i + 1;
                if (nextIndex > lineVerts.Count - 1) nextIndex = 0;

                if ((GetClosestPointOnLineSegment(lineVerts[i].position, lineVerts[nextIndex].position, mousePos) - mousePos).magnitude <= minDist)
                {
                    minDist = (GetClosestPointOnLineSegment(lineVerts[i].position, lineVerts[nextIndex].position, mousePos) - mousePos).magnitude;
                    minDistIndex = i;
                }

            }

            //Check clicking
            if (minDist <= minPointerDist && minDist < minVertDist * 0.25f && minVertDist > 0.1f && t.showOutline && !Event.current.shift)
            {
                int nextIndex = minDistIndex + 1;
                if (nextIndex > lineVerts.Count - 1) nextIndex = 0;

                Handles.DotCap(113, GetClosestPointOnLineSegment(lineVerts[minDistIndex].position, lineVerts[nextIndex].position, mousePos), Quaternion.identity, 0.03f * t.handleScale * handleScale);


                if (Event.current.type == EventType.MouseDown)
                {
                    if (Event.current.button == 1)
                    {
                        Undo.RecordObject(target, "SMundo");

                        v = Quaternion.Inverse(t.transform.rotation) * (GetClosestPointOnLineSegment(lineVerts[minDistIndex].position, lineVerts[nextIndex].position, mousePos) - t.transform.position);

                        v.x /= t.transform.localScale.x;
                        v.y /= t.transform.localScale.y;

                        t.Corners.Insert(lineVerts[minDistIndex].cornerIndex + 1,
                            new Corner(
                                    v,
                                    Event.current.control
                                )
                            );

                        updateMesh = true;

                        nextIndex = lineVerts[minDistIndex].cornerIndex + 2;
                        if (nextIndex > t.Corners.Count - 1) nextIndex = 0;

                        if (Event.current.control)
                        {
                            float midDistance = 0;
                            midDistance += Vector3.Distance(t.Corners[lineVerts[minDistIndex].cornerIndex + 1].position, t.Corners[lineVerts[minDistIndex].cornerIndex].position);
                            midDistance += Vector3.Distance(t.Corners[lineVerts[minDistIndex].cornerIndex + 1].position, t.Corners[nextIndex].position);

                            midDistance /= 6;

                            Vector3 b = (t.Corners[lineVerts[minDistIndex].cornerIndex].position - t.Corners[lineVerts[minDistIndex].cornerIndex + 1].position).normalized + (t.Corners[nextIndex].position - t.Corners[lineVerts[minDistIndex].cornerIndex + 1].position).normalized;
                            b.Normalize();
                            b = Quaternion.AngleAxis(90, Vector3.forward) * b;

                            t.Corners[lineVerts[minDistIndex].cornerIndex + 1].controlPoint0 = t.Corners[lineVerts[minDistIndex].cornerIndex + 1].position + b * midDistance;
                            t.Corners[lineVerts[minDistIndex].cornerIndex + 1].controlPoint1 = t.Corners[lineVerts[minDistIndex].cornerIndex + 1].position - b * midDistance;
                        }
                    }

                }
            }
            else if (minVertDist < 0.1f * t.handleScale * handleScale)
            {

                if (Event.current.type == EventType.MouseDown)
                {

                    if (Event.current.button == 1 && Event.current.control)
                    {
                        t.Corners[minVertIndex].isBezier = !t.Corners[minVertIndex].isBezier;

                        if (t.Corners[minVertIndex].isBezier)
                        {
                            int nextIndex = minVertIndex + 1;
                            if (nextIndex > t.Corners.Count - 1) nextIndex = 0;

                            int lastIndex = minVertIndex - 1;
                            if (lastIndex < 0) lastIndex = t.Corners.Count - 1;


                            float midDistance = 0;
                            midDistance += Vector3.Distance(t.Corners[minVertIndex].position, t.Corners[lastIndex].position);
                            midDistance += Vector3.Distance(t.Corners[minVertIndex].position, t.Corners[nextIndex].position);

                            midDistance /= 6;

                            Vector3 b = (t.Corners[lastIndex].position - t.Corners[minVertIndex].position).normalized + (t.Corners[nextIndex].position - t.Corners[minVertIndex].position).normalized;
                            b.Normalize();
                            b = Quaternion.AngleAxis(90, Vector3.forward) * b;

                            t.Corners[minVertIndex].controlPoint0 = t.Corners[minVertIndex].position + b * midDistance;
                            t.Corners[minVertIndex].controlPoint1 = t.Corners[minVertIndex].position - b * midDistance;

                        }

                        updateMesh = true;
                    }
                    else if (Event.current.button == 1)
                    {
                        Undo.RecordObject(target, "SMundo");

                        t.Corners.RemoveAt(minVertIndex);

                        updateMesh = true;

                    }
                }
                else if ((t.showHandles && !Event.current.shift) && !(Event.current.control && Event.current.shift))
                {
                    Handles.color = myColor;

                    v = t.Corners[minVertIndex].position;

                    v.x *= t.transform.localScale.x;
                    v.y *= t.transform.localScale.y;

                    /*Handles.CircleCap(114, t.transform.position + t.transform.rotation * v, Quaternion.identity, 0.05f * t.handleScale * handleScale);
                    Handles.CircleCap(115, t.transform.position + t.transform.rotation * v, Quaternion.identity, 0.04f * t.handleScale * handleScale);
                    Handles.CircleCap(116, t.transform.position + t.transform.rotation * v, Quaternion.identity, 0.03f * t.handleScale * handleScale);
                    Handles.CircleCap(117, t.transform.position + t.transform.rotation * v, Quaternion.identity, 0.02f * t.handleScale * handleScale);
                    Handles.CircleCap(118, t.transform.position + t.transform.rotation * v, Quaternion.identity, 0.01f * t.handleScale * handleScale);*/
                    Handles.DotCap(117, t.transform.position + t.transform.rotation * v, Quaternion.identity, 0.08f * t.handleScale * handleScale);

                }
            }



            if ((t.showHandles && !Event.current.shift) && !(Event.current.control && Event.current.shift))
            {
                for (int i = 0; i < t.Corners.Count; i++)
                {
                    EditorGUI.BeginChangeCheck();

                    Handles.color = Color.grey;


                    Vector3 cp0 = Vector3.zero, cp1 = Vector3.zero;

                    int nextIndex, lastIndex;
                    if (i > 0) lastIndex = i - 1; else lastIndex = t.Corners.Count - 1;
                    if (i < t.Corners.Count - 1) nextIndex = i + 1; else nextIndex = 0;


                    if (t.Corners[i].isBezier)
                    {
                        Handles.color = Color.grey;

                        v = t.Corners[i].position;

                        v.x *= t.transform.localScale.x;
                        v.y *= t.transform.localScale.y;

                        Vector3 v0 = t.Corners[i].controlPoint0;

                        v0.x *= t.transform.localScale.x;
                        v0.y *= t.transform.localScale.y;

                        if (t.Corners[lastIndex].isBezier)
                        {
                            cp0 = Handles.FreeMoveHandle(t.transform.position + t.transform.rotation * v0, Quaternion.identity, .03f * t.handleScale * handleScale, new Vector3(.5f, .5f, .5f), Handles.DotCap);
                            Handles.DrawLine(cp0, t.transform.position + t.transform.rotation * v);
                        }
                        else
                        {
                            cp0 = t.transform.position + t.transform.rotation * v0;
                        }


                        Vector3 v1 = t.Corners[i].controlPoint1;

                        v1.x *= t.transform.localScale.x;
                        v1.y *= t.transform.localScale.y;

                        if (t.Corners[nextIndex].isBezier)
                        {
                            cp1 = Handles.FreeMoveHandle(t.transform.position + t.transform.rotation * v1, Quaternion.identity, .03f * t.handleScale * handleScale, new Vector3(.5f, .5f, .5f), Handles.DotCap);
                            Handles.DrawLine(cp1, t.transform.position + t.transform.rotation * v);
                        }
                        else
                        {
                            cp1 = t.transform.position + t.transform.rotation * v1;
                        }

                        Handles.color = Color.blue;
                    }

                    v = t.Corners[i].position;

                    v.x *= t.transform.localScale.x;
                    v.y *= t.transform.localScale.y;

                    //Vector3 pos = Handles.FreeMoveHandle(t.transform.position + t.transform.rotation * v, Quaternion.identity, .06f * t.handleScale * handleScale, new Vector3(.5f, .5f, .5f), Handles.CubeCap);
                    Vector3 pos = Handles.FreeMoveHandle(t.transform.position + t.transform.rotation * v, Quaternion.identity, .06f * t.handleScale * handleScale, new Vector3(.5f, .5f, .5f), Handles.DotCap);

                    int minVSymIndex = -1, minHSymIndex = -1;

                    Vector3 p = Quaternion.Inverse(t.transform.rotation) * (pos - t.transform.position);

                    p.x /= t.transform.localScale.x;
                    p.y /= t.transform.localScale.y;

                    if (t.snap > 0)
                    {
                        p.x = RoundFloat.RoundToFloat(p.x, t.snap);
                        p.y = RoundFloat.RoundToFloat(p.y, t.snap);
                        p.z = RoundFloat.RoundToFloat(p.z, t.snap);
                    }


                    Vector3 vSym = p;
                    vSym.x *= -1;

                    float minVSDist = float.MaxValue;

                    for (int j = 0; j < t.Corners.Count; j++)
                    {
                        if (i == j) continue;

                        float distance = Vector3.Distance(vSym, t.Corners[j].position);

                        if (distance < minVSDist)
                        {
                            minVSDist = distance;
                            minVSymIndex = j;
                        }
                    }

                    v = Quaternion.Inverse(t.transform.rotation) * (pos - t.transform.position);
                    v.x /= t.transform.localScale.x;
                    v.y /= t.transform.localScale.y;

                    if (t.verticalSymmetry != Event.current.alt && t.Corners[i].position != v)
                    {
                        if (minVSymIndex != -1)
                        {
                            if (minVSDist <= t.symmertryDistance * 10)
                            {
                                Vector3 vv = t.Corners[minVSymIndex].position;
                                vv.x *= -1;
                                vSymPos = vv;
                            }
                        }
                    }

                    Vector3 hSym = p;
                    hSym.y *= -1;

                    float minHSDist = float.MaxValue;

                    for (int j = 0; j < t.Corners.Count; j++)
                    {
                        if (i == j) continue;

                        float distance = Vector3.Distance(hSym, t.Corners[j].position);

                        if (distance < minHSDist)
                        {
                            minHSDist = distance;
                            minHSymIndex = j;
                        }
                    }

                    if (t.horizontalSymmetry != Event.current.alt && t.Corners[i].position != v)
                    {
                        if (minHSymIndex != -1)
                        {
                            if (minHSDist <= t.symmertryDistance * 10)
                            {
                                Vector3 vv = t.Corners[minHSymIndex].position;
                                vv.y *= -1;
                                hSymPos = vv;
                            }
                        }
                    }

                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(target, "SMundo");

                        if (t.Corners[i].isBezier)
                        {
                            v = t.Corners[i].position;

                            v.x *= t.transform.localScale.x;
                            v.y *= t.transform.localScale.y;

                            cp0 += pos - t.transform.position - t.transform.rotation * v;
                            cp1 += pos - t.transform.position - t.transform.rotation * v;

                            bool shiften = false;

                            v = Quaternion.Inverse(t.transform.rotation) * (cp0 - t.transform.position);
                            v.x /= t.transform.localScale.x;
                            v.y /= t.transform.localScale.y;
                            if (t.Corners[i].controlPoint0 != v)
                            {
                                p = Quaternion.Inverse(t.transform.rotation) * (cp0 - t.transform.position);

                                p.x /= t.transform.localScale.x;
                                p.y /= t.transform.localScale.y;

                                if (t.snap > 0)
                                {
                                    p.x = RoundFloat.RoundToFloat(p.x, t.snap);
                                    p.y = RoundFloat.RoundToFloat(p.y, t.snap);
                                    p.z = RoundFloat.RoundToFloat(p.z, t.snap);
                                }

                                if (Event.current.alt)
                                {
                                    if (t.Corners[lastIndex].isBezier && t.Corners[nextIndex].isBezier)
                                    {
                                        t.Corners[i].controlPoint1 = 2 * t.Corners[i].position - p;
                                        shiften = true;
                                    }
                                }

                                p.z = 0;
                                t.Corners[i].controlPoint0 = p;
                                updateMesh = true;
                            }
                            v = Quaternion.Inverse(t.transform.rotation) * (cp1 - t.transform.position);
                            v.x /= t.transform.localScale.x;
                            v.y /= t.transform.localScale.y;
                            if (t.Corners[i].controlPoint1 != v && !shiften)
                            {
                                p = Quaternion.Inverse(t.transform.rotation) * (cp1 - t.transform.position);

                                p.x /= t.transform.localScale.x;
                                p.y /= t.transform.localScale.y;

                                if (t.snap > 0)
                                {
                                    p.x = RoundFloat.RoundToFloat(p.x, t.snap);
                                    p.y = RoundFloat.RoundToFloat(p.y, t.snap);
                                    p.z = RoundFloat.RoundToFloat(p.z, t.snap);
                                }

                                if (Event.current.alt)
                                {
                                    if (t.Corners[lastIndex].isBezier && t.Corners[nextIndex].isBezier)
                                    {
                                        t.Corners[i].controlPoint0 = 2 * t.Corners[i].position - p;
                                    }
                                }

                                p.z = 0;
                                t.Corners[i].controlPoint1 = p;
                                updateMesh = true;
                            }
                        }

                        if (t.Corners[i].position != pos - t.transform.position)
                        {
                            hotID = i;

                            p = Quaternion.Inverse(t.transform.rotation) * (pos - t.transform.position);

                            p.x /= t.transform.localScale.x;
                            p.y /= t.transform.localScale.y;

                            if (t.snap > 0)
                            {
                                p.x = RoundFloat.RoundToFloat(p.x, t.snap);
                                p.y = RoundFloat.RoundToFloat(p.y, t.snap);
                                p.z = RoundFloat.RoundToFloat(p.z, t.snap);
                            }

                            if (t.verticalSymmetry != Event.current.alt)
                            {
                                if (minVSymIndex != -1)
                                {
                                    if (minVSDist <= t.symmertryDistance)
                                    {
                                        vSym = t.Corners[minVSymIndex].position;
                                        vSym.x *= -1;

                                        t.Corners[i].controlPoint0 -= p - vSym;
                                        t.Corners[i].controlPoint1 -= p - vSym;

                                        p = vSym;
                                    }
                                }
                            }



                            if (t.horizontalSymmetry != Event.current.alt)
                            {
                                if (minHSymIndex != -1)
                                {
                                    if (minHSDist <= t.symmertryDistance)
                                    {
                                        hSym = t.Corners[minHSymIndex].position;
                                        hSym.y *= -1;

                                        t.Corners[i].controlPoint0 -= p - hSym;
                                        t.Corners[i].controlPoint1 -= p - hSym;

                                        p = hSym;
                                    }
                                }

                            }
                            p.z = 0;
                            t.Corners[i].position = p;
                            updateMesh = true;
                        }
                    }
                }

                if (GUIUtility.hotControl == lastHotControl)
                {
                    if (hotID != -1)
                    {
                        if (t.verticalSymmetry != Event.current.alt && Vector3.Distance(vSymPos, t.Corners[hotID].position) <= t.symmertryDistance * 10)
                        {
                            v = vSymPos;
                            v.x *= t.transform.localScale.x;
                            v.y *= t.transform.localScale.y;

                            Handles.color = new Color(150f / 255f, 125f / 255f, 1);

                            Handles.CircleCap(222, t.transform.rotation * v + t.transform.position, Quaternion.identity, 0.047f * t.handleScale * handleScale);
                        }

                        if (t.horizontalSymmetry != Event.current.alt && Vector3.Distance(hSymPos, t.Corners[hotID].position) <= t.symmertryDistance * 10)
                        {
                            v = hSymPos;
                            v.x *= t.transform.localScale.x;
                            v.y *= t.transform.localScale.y;

                            Handles.color = new Color(232f / 255f, 226f / 255f, 90f / 255f);

                            Handles.CircleCap(222, t.transform.rotation * v + t.transform.position, Quaternion.identity, 0.047f * t.handleScale * handleScale);
                        }
                    }

                }
                else
                {
                    lastHotControl = GUIUtility.hotControl;
                    hotID = -1;
                }
            }

            if (EventType.KeyDown == e.type)
            {
                if (KeyCode.KeypadPlus == e.keyCode)
                {
                    t.snap += 0.05f;
                    t.snap = RoundFloat.RoundToFloat(t.snap,0.05f);
                }
                else if (KeyCode.KeypadMinus == e.keyCode)
                {
                    if (t.snap - 0.05f <= 0)
                    {
                        t.snap = 0;
                    }
                    else
                    {
                        t.snap -= 0.05f;
                        t.snap = RoundFloat.RoundToFloat(t.snap, 0.05f);
                    }
                }

            }

            if ((updateMesh && t.autoBuild) || (EventType.KeyDown == e.type && KeyCode.B == e.keyCode))
                t.BuildMesh();
        }

        EditorUtility.SetDirty(t);

    }
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        var t = (target as MeshBuilder);

        if (GUILayout.Button("Build"))
        {
            t.BuildMesh();
        }

        if (GUILayout.Button("Quick Start"))
        {
            t.QuickStart();
        }

        if (GUILayout.Button("Import SmartMesh"))
        {
            t.ImportSmartMesh();
        }

        if (GUILayout.Button("Save Mesh"))
        {
            t.SaveAsAsset();
        }

        if (GUILayout.Button("Save SmartMesh"))
        {
            t.SaveAsSmartMesh();
        }

        /*if (GUILayout.Button("Adopt Mesh"))
        {
            t.SaveAsAsset();
        }*/

    }

    Vector3 GetClosestPointOnLineSegment(Vector3 A, Vector3 B, Vector3 P)
    {
        Vector3 p = B +  Vector3.Project(P - B, A - B);

        if (Vector3.Dot((B - A).normalized, P - A) <= 0)
        {
            return A;
        }
        else if (Vector3.Dot((B - A).normalized, P - A) >= Vector3.Distance(A, B))
        {
            return B;
        }

        return p;


    }
}
#endif
class BezierPoint
{
    public Vector3 position;
    public int cornerIndex;

    public BezierPoint(Vector3 pos, int ind)
    {
        position = pos;
        cornerIndex = ind;
    }
}
