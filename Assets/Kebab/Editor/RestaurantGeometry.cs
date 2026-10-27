using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Brasa.Editor
{
    // Original, editable mesh construction. All dimensions are in metres.
    internal static class RestaurantGeometry
    {
        internal const string Root = "Assets/Kebab";
        static readonly Dictionary<string, Mesh> meshes = new();
        static readonly Dictionary<string, Material> materials = new();
        internal static void Reset() { meshes.Clear(); materials.Clear(); }
        internal static Color Hex(string hex) { ColorUtility.TryParseHtmlString(hex, out var c); return c; }
        internal static Transform Group(string name, Transform parent = null, Vector3 position = default)
        {
            var g = new GameObject(name); g.transform.SetParent(parent, false); g.transform.localPosition = position; return g.transform;
        }
        internal static Material Mat(string name, string color, float metallic = 0, float smooth = .25f, Texture texture = null, bool unlit = false, float emission = 0)
        {
            if (materials.TryGetValue(name, out var cached)) return cached;
            string path = Root + "/Art/Materials/" + name + ".mat";
            var m = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (!m) { m = new Material(Shader.Find(unlit ? "Universal Render Pipeline/Unlit" : "Universal Render Pipeline/Lit")); AssetDatabase.CreateAsset(m, path); }
            m.SetColor("_BaseColor", Hex(color)); m.SetFloat("_Metallic", metallic); m.SetFloat("_Smoothness", smooth);
            m.SetTexture("_BaseMap", texture);
            if (emission > 0) { m.EnableKeyword("_EMISSION"); m.SetColor("_EmissionColor", Hex(color) * emission); m.globalIlluminationFlags = MaterialGlobalIlluminationFlags.BakedEmissive; }
            m.enableInstancing = true; EditorUtility.SetDirty(m); materials[name] = m; return m;
        }
        internal static Texture2D Texture(string name, int size, Func<int,int,Color> paint)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGB24, true); var pixels = new Color[size*size];
            for(int y=0;y<size;y++) for(int x=0;x<size;x++) pixels[y*size+x]=paint(x,y);
            tex.SetPixels(pixels); tex.Apply(); string path=Root+"/Art/Textures/"+name+".png";
            System.IO.File.WriteAllBytes(path,tex.EncodeToPNG()); UnityEngine.Object.DestroyImmediate(tex);
            AssetDatabase.ImportAsset(path);
            var imp=(TextureImporter)AssetImporter.GetAtPath(path); imp.wrapMode=TextureWrapMode.Repeat; imp.anisoLevel=4; imp.maxTextureSize=size; imp.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }
        static Mesh SaveMesh(string key, List<Vector3> v, List<int> tri, List<Vector2> uv, List<Vector3> normals = null)
        {
            var mesh = new Mesh { name=key, indexFormat=v.Count>65535?IndexFormat.UInt32:IndexFormat.UInt16 };
            mesh.SetVertices(v); mesh.SetTriangles(tri,0); mesh.SetUVs(0,uv);
            if(normals!=null)mesh.SetNormals(normals);else mesh.RecalculateNormals();
            mesh.RecalculateTangents(); mesh.RecalculateBounds(); Unwrapping.GenerateSecondaryUVSet(mesh);
            string path=Root+"/Art/Meshes/"+key+".asset";
            var old=AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if(old) { EditorUtility.CopySerialized(mesh, old); UnityEngine.Object.DestroyImmediate(mesh); mesh=old; }
            else AssetDatabase.CreateAsset(mesh,path);
            meshes[key]=mesh; return mesh;
        }
        internal static GameObject Object(string name, Transform parent, Mesh mesh, Material mat, Vector3 p, bool collider=false)
        {
            var g=Group(name,parent,p).gameObject;
            g.AddComponent<MeshFilter>().sharedMesh=mesh; g.AddComponent<MeshRenderer>().sharedMaterial=mat;
            GameObjectUtility.SetStaticEditorFlags(g,StaticEditorFlags.BatchingStatic|StaticEditorFlags.ContributeGI|StaticEditorFlags.OccludeeStatic|StaticEditorFlags.OccluderStatic|StaticEditorFlags.ReflectionProbeStatic);
            if(collider){var c=g.AddComponent<BoxCollider>();c.center=mesh.bounds.center;c.size=mesh.bounds.size;}
            return g;
        }
        internal static GameObject Box(string name,Transform parent,Vector3 p,Vector3 size,Material mat,float bevel=.015f,bool collider=false)
        {
            string key=FormattableString.Invariant($"Box_{size.x:0.###}_{size.y:0.###}_{size.z:0.###}_{bevel:0.###}");
            if(!meshes.TryGetValue(key,out var mesh))
            {
                var v=new List<Vector3>();var n=new List<Vector3>();var uv=new List<Vector2>();var t=new List<int>();
                var half=size*.5f; float b=Mathf.Min(bevel,Mathf.Min(size.x,Mathf.Min(size.y,size.z))*.45f);
                Vector3[] normals={Vector3.right,Vector3.left,Vector3.up,Vector3.down,Vector3.forward,Vector3.back};
                foreach(var normal in normals)
                {
                    Vector3 u=Mathf.Abs(normal.y)>.5f?Vector3.right:Vector3.Cross(Vector3.up,normal); Vector3 vv=Vector3.Cross(normal,u);
                    float hu=Vector3.Dot(half,Abs(u)),hv=Vector3.Dot(half,Abs(vv));
                    float[] us={-hu,-hu+b,hu-b,hu},vs={-hv,-hv+b,hv-b,hv}; int start=v.Count;
                    int steps=b>0?4:2;
                    if(steps==2){us=new[]{-hu,hu};vs=new[]{-hv,hv};}
                    for(int j=0;j<steps;j++)for(int i=0;i<steps;i++)
                    {
                        var point=Vector3.Scale(normal,half)+u*us[i]+vv*vs[j];
                        var inner=new Vector3(Mathf.Clamp(point.x,-half.x+b,half.x-b),Mathf.Clamp(point.y,-half.y+b,half.y-b),Mathf.Clamp(point.z,-half.z+b,half.z-b));
                        var norm=b>0?(point-inner).normalized:normal;
                        v.Add(b>0?inner+norm*b:point);n.Add(norm);uv.Add(new Vector2(us[i]+hu,vs[j]+hv));
                    }
                    for(int j=0;j<steps-1;j++)for(int i=0;i<steps-1;i++){int a=start+j*steps+i;t.AddRange(new[]{a,a+1,a+steps+1,a,a+steps+1,a+steps});}
                }
                mesh=SaveMesh(key,v,t,uv,n);
            }
            return Object(name,parent,mesh,mat,p,collider);
        }
        static Vector3 Abs(Vector3 v)=>new(Mathf.Abs(v.x),Mathf.Abs(v.y),Mathf.Abs(v.z));
        internal static GameObject Lathe(string name,Transform parent,Vector3 p,Vector2[] profile,Material mat,int segments=24,bool cap=true,string meshKey=null)
        {
            string key=meshKey??name.Replace(" ","_")+"_"+segments;
            if(!meshes.TryGetValue(key,out var mesh))
            {
                var v=new List<Vector3>();var t=new List<int>();var uv=new List<Vector2>();
                for(int j=0;j<profile.Length;j++)for(int i=0;i<=segments;i++)
                {
                    float a=i*Mathf.PI*2/segments;v.Add(new Vector3(Mathf.Cos(a)*profile[j].x,profile[j].y,Mathf.Sin(a)*profile[j].x));
                    uv.Add(new Vector2(i/(float)segments,j/(float)(profile.Length-1)));
                    if(j>0&&i>0){int c=j*(segments+1)+i;t.AddRange(new[]{c,c-segments-2,c-1,c,c-segments-1,c-segments-2});}
                }
                if(cap)
                {
                    int bottom=v.Count;v.Add(new Vector3(0,profile[0].y,0));uv.Add(new Vector2(.5f,.5f));
                    int top=v.Count;v.Add(new Vector3(0,profile[^1].y,0));uv.Add(new Vector2(.5f,.5f));
                    for(int i=0;i<segments;i++){t.AddRange(new[]{bottom,i,i+1,top,(profile.Length-1)*(segments+1)+i+1,(profile.Length-1)*(segments+1)+i});}
                }
                mesh=SaveMesh(key,v,t,uv);
            }
            return Object(name,parent,mesh,mat,p);
        }
        internal static GameObject Cylinder(string name,Transform parent,Vector3 p,float radius,float height,Material mat,int sides=16)
        {
            string key=FormattableString.Invariant($"Cylinder_{radius:0.###}_{height:0.###}_{sides}");
            return Lathe(name,parent,p,new[]{new Vector2(radius,-height*.5f),new Vector2(radius,height*.5f)},mat,sides,true,key);
        }
        internal static GameObject Rod(string name,Transform parent,Vector3 a,Vector3 b,float radius,Material mat)
        {
            var g=Cylinder(name,parent,(a+b)*.5f,radius,Vector3.Distance(a,b),mat,12);g.transform.localRotation=Quaternion.FromToRotation(Vector3.up,b-a);return g;
        }
        internal static GameObject Doner(Transform parent,Vector3 p,Material mat,int seed)
        {
            string key="Doner_HandLayered_"+seed;
            if(!meshes.TryGetValue(key,out var mesh))
            {
                var rng=new System.Random(seed);var v=new List<Vector3>();var uv=new List<Vector2>();var tri=new List<int>();
                int sides=40,rings=58;
                for(int j=0;j<=rings;j++)
                {
                    float h=j/(float)rings;float r=Mathf.Lerp(.17f,.32f,Mathf.Sin(h*Mathf.PI*.77f));float edge=(j%2==0?.006f:-.004f);
                    for(int i=0;i<=sides;i++)
                    {
                        float a=2*Mathf.PI*i/sides;float rough=Mathf.Sin(i*2.8f+j*5.6f)*.004f;
                        v.Add(new Vector3(Mathf.Cos(a)*(r+edge+rough),h*1.16f,Mathf.Sin(a)*(r+edge+rough)));
                        uv.Add(new Vector2(i/(float)sides,h));
                        if(i>0&&j>0){int c=j*(sides+1)+i;tri.AddRange(new[]{c,c-sides-2,c-1,c,c-sides-1,c-sides-2});}
                    }
                }
                int bot=v.Count;v.Add(Vector3.zero);uv.Add(Vector2.zero);int top=v.Count;v.Add(Vector3.up*1.16f);uv.Add(Vector2.one);
                for(int i=0;i<sides;i++)tri.AddRange(new[]{bot,i,i+1,top,rings*(sides+1)+i+1,rings*(sides+1)+i});
                mesh=SaveMesh(key,v,tri,uv);
            }
            return Object("Carn laminada / doner",parent,mesh,mat,p);
        }
        // Front is local -Z. Quad UVs use the complete texture independently of dimensions.
        internal static GameObject Sign(string name,Transform parent,Vector3 p,Vector2 size,string texture,Material frame,float yaw=0)
        {
            var g=Group(name,parent,p);g.localEulerAngles=new Vector3(0,yaw,0);
            Box("Marc",g,Vector3.zero,new Vector3(size.x+.055f,size.y+.055f,.045f),frame,.01f);
            string key=FormattableString.Invariant($"Panel_{size.x:0.###}_{size.y:0.###}");
            if(!meshes.TryGetValue(key,out var mesh))mesh=SaveMesh(key,new(){new(-size.x/2,-size.y/2,0),new(-size.x/2,size.y/2,0),new(size.x/2,size.y/2,0),new(size.x/2,-size.y/2,0)},new(){0,1,2,0,2,3},new(){new(0,0),new(0,1),new(1,1),new(1,0)});
            var tex=AssetDatabase.LoadAssetAtPath<Texture2D>(Root+"/Art/Textures/"+texture+".png");
            Object("Grafica",g,mesh,Mat("Graphic_"+texture,"#FFFFFF",0,.25f,tex,true),new Vector3(0,0,-.026f));return g.gameObject;
        }
    }
}
