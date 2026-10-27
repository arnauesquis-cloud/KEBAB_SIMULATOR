using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using static Brasa.Editor.RestaurantGeometry;

namespace Brasa.Editor
{
    public static partial class RestaurantBuilder
    {
        public const string ScenePath="Assets/Kebab/Scenes/Restaurant.unity";
        static Material plaster,red,gold,black,steel,chrome,wood,floor,blueTile,whiteTile,ceiling,green,leaf,bread,meat,chicken,tomato,white,glow,heat;
        static Transform environment;

        [MenuItem("BRASA/01 · Regenerar restaurant original")]
        public static void Build()
        {
            // Interactive regeneration is explicit; never overwrite work just by opening the project.
            if(!Application.isBatchMode && !EditorUtility.DisplayDialog("Regenerar restaurant", "Això regenera l'escena original i els seus models. Desa les teves variants amb un altre nom abans de continuar.","Regenerar","Cancel·lar"))return;
            foreach(var dir in new[]{"Art/Materials","Art/Meshes","Art/Textures","Prefabs","Scenes","Settings","Input"})Directory.CreateDirectory(Root+"/"+dir);
            AssetDatabase.Refresh();Reset();SetupMaterials();ConfigureRenderPipeline();
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            environment=Group("RESTAURANT · BRASA KEBAB · 12 × 10,5 m");
            BuildArchitecture();BuildDining();BuildKitchen();BuildDetails();BuildLighting();
            QuestProjectSetup.Configure();QuestProjectSetup.CreateRig();
            CreateCameras();
            EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene(),ScenePath);
            EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene(ScenePath,true)};
            AssetDatabase.SaveAssets();WriteReport();
            Debug.Log("BRASA_BUILD_OK: Original restaurant scene, editable prefabs and Quest settings saved.");
        }
        static void SetupMaterials()
        {
            var floorTex=Texture("Terrazzo",512,(x,y)=>{
                int gx=x%256,gy=y%256;float noise=Mathf.PerlinNoise(x*.22f,y*.22f);
                Color c=((x/256+y/256)%2==0)?Hex("#D8C6AD"):Hex("#C7B39B");
                if(gx<2||gy<2)c=Hex("#A69680");else c*=.94f+noise*.12f;
                if(Mathf.PerlinNoise(x*.61f+11,y*.61f)>.78f)c*=.82f;return c;});
            var wallTex=Texture("Subway",512,(x,y)=>{
                int row=y/128;int xx=(x+(row%2)*128)%256,yy=y%128;
                return xx<5||yy<5?Hex("#BEBCAF"):Hex("#EFECE1")*(.96f+.04f*Mathf.PerlinNoise(x*.2f,y*.2f));});
            var tileTex=Texture("Mediterranean",512,(x,y)=>{
                float u=(x%256)/256f,v=(y%256)/256f,dx=u-.5f,dy=v-.5f;
                float r=Mathf.Sqrt(dx*dx+dy*dy),a=Mathf.Atan2(dy,dx);
                if(x%256<4||y%256<4)return Hex("#C5C3B4");
                bool ink=Mathf.Abs(r-(.23f+.075f*Mathf.Cos(8*a)))<.027f||Mathf.Abs(r-.43f)<.016f||Mathf.Abs(dx)+Mathf.Abs(dy)<.12f;
                return ink?Hex("#31536B"):Hex("#ECE7D8");});
            var woodTex=Texture("Oak",512,(x,y)=>{float grain=Mathf.PerlinNoise(x*.1f,y*.008f)*.7f+Mathf.PerlinNoise(x*.4f,y*.016f)*.3f;return Color.Lerp(Hex("#B9844C"),Hex("#E9C690"),grain);});
            var meatTex=Texture("RoastedMeat",512,(x,y)=>{float n=Mathf.PerlinNoise(x*.055f,y*.19f);float stripe=Mathf.Sin(y*.66f)*.08f;return Color.Lerp(Hex("#482519"),Hex("#C78B4E"),Mathf.Clamp01(n+stripe));});
            var chickenTex=Texture("RoastedChicken",512,(x,y)=>{float n=Mathf.PerlinNoise(x*.05f+12,y*.13f);return Color.Lerp(Hex("#704224"),Hex("#E4BB75"),Mathf.Clamp01(n+Mathf.Sin(y*.6f)*.1f));});
            plaster=Mat("Plaster warm ivory","#ECE5D5");red=Mat("Enamel oxblood","#84383F",.1f,.32f);gold=Mat("Mustard shell","#DDA83F",.05f,.32f);
            black=Mat("Powder coated charcoal","#263333",.25f,.3f);steel=Mat("Brushed stainless steel","#AFBABC",.7f,.36f);chrome=Mat("Polished steel","#DEE4E3",.85f,.62f);
            wood=Mat("Natural oak","#FFFFFF",0,.28f,woodTex);floor=Mat("Stone terrazzo tiles","#FFFFFF",0,.32f,floorTex);blueTile=Mat("Blue patterned ceramic","#FFFFFF",0,.35f,tileTex);whiteTile=Mat("Kitchen subway tiles","#FFFFFF",0,.28f,wallTex);
            ceiling=Mat("Ceiling chalk","#F3ECDE");green=Mat("Herb pot green","#627767");leaf=Mat("Fresh lettuce","#79A74C");bread=Mat("Pita golden crust","#DEB77B");meat=Mat("Roasted beef","#FFFFFF",0,.16f,meatTex);chicken=Mat("Roasted chicken","#FFFFFF",0,.2f,chickenTex);
            tomato=Mat("Tomato red","#B84E3C",0,.4f);white=Mat("Ceramic ivory","#F3E9D4",0,.4f);glow=Mat("Warm lamp diffuser","#FFF1C7",0,.2f,null,false,2);heat=Mat("Infrared warm element","#E66B31",0,.2f,null,false,1.4f);
        }
        static void ConfigureRenderPipeline()
        {
            string path=Root+"/Settings/Quest_URP.asset";
            var pipeline=AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(path);
            if(!pipeline)
            {
                var data=ScriptableObject.CreateInstance<UniversalRendererData>();data.name="Quest Forward Renderer";AssetDatabase.CreateAsset(data,Root+"/Settings/Quest_Renderer.asset");
                pipeline=UniversalRenderPipelineAsset.Create(data);AssetDatabase.CreateAsset(pipeline,path);
            }
            pipeline.msaaSampleCount=4;pipeline.renderScale=1;pipeline.supportsHDR=false;pipeline.supportsCameraDepthTexture=false;pipeline.supportsCameraOpaqueTexture=false;
            var serializedPipeline=new SerializedObject(pipeline);
            serializedPipeline.FindProperty("m_MainLightRenderingMode").intValue=(int)LightRenderingMode.PerPixel;
            serializedPipeline.FindProperty("m_AdditionalLightsRenderingMode").intValue=(int)LightRenderingMode.Disabled;
            serializedPipeline.FindProperty("m_MainLightShadowsSupported").boolValue=true;
            serializedPipeline.FindProperty("m_SoftShadowsSupported").boolValue=false;
            serializedPipeline.ApplyModifiedPropertiesWithoutUndo();
            pipeline.shadowDistance=22;pipeline.shadowCascadeCount=1;pipeline.mainLightShadowmapResolution=1024;
            GraphicsSettings.defaultRenderPipeline=pipeline;QualitySettings.renderPipeline=pipeline;QualitySettings.vSyncCount=0;
            QualitySettings.antiAliasing=4;QualitySettings.anisotropicFiltering=AnisotropicFiltering.ForceEnable;QualitySettings.shadowDistance=22;
            QualitySettings.realtimeReflectionProbes=false;PlayerSettings.colorSpace=ColorSpace.Linear;
            EditorUtility.SetDirty(pipeline);
        }
        static void BuildArchitecture()
        {
            var a=Group("01 · Arquitectura",environment);
            Box("Paviment · 126 m2",a,new(0,-.10f,0),new(12,.2f,10.5f),floor,0,true);
            Box("Paret esquerra",a,new(-6.1f,1.7f,0),new(.2f,3.4f,10.7f),plaster,0,true);
            Box("Paret dreta",a,new(6.1f,1.7f,0),new(.2f,3.4f,10.7f),plaster,0,true);
            Box("Paret posterior",a,new(0,1.7f,5.35f),new(12.4f,3.4f,.2f),plaster,0,true);
            Box("Arrambador blau esquerra",a,new(-5.986f,.61f,-.9f),new(.028f,1.12f,8.7f),blueTile,0);
            Box("Remat arrambador esquerra",a,new(-5.96f,1.19f,-.9f),new(.07f,.055f,8.7f),wood,.009f);
            Box("Arrambador blau dreta",a,new(5.986f,.61f,-2.05f),new(.028f,1.12f,6.4f),blueTile,0);
            Box("Remat arrambador dreta",a,new(5.96f,1.19f,-2.05f),new(.07f,.055f,6.4f),wood,.009f);
            Box("Enrajolat cuina posterior",a,new(2.8f,1.5f,5.235f),new(6.4f,2.9f,.03f),whiteTile,0);
            Box("Enrajolat cuina lateral",a,new(5.98f,1.5f,3.3f),new(.03f,2.9f,3.9f),whiteTile,0);
            foreach(float x in new[]{-5.94f,5.94f})Box("Socol lateral",a,new(x,.065f,0),new(.055f,.13f,10.5f),black,0);
            Box("Socol posterior",a,new(0,.065f,5.2f),new(12,.13f,.055f),black,0);
            var roof=Group("SOSTRE · ocultar per veure la planta",a);
            Box("Sostre",roof,new(0,3.5f,0),new(12.4f,.2f,10.8f),ceiling,0,true);
            foreach(float z in new[]{-3.5f,0f,3.1f})Box("Biga roure",roof,new(0,3.32f,z),new(12,.14f,.13f),wood,.01f);
            for(int i=0;i<2;i++)
            {
                var vent=Group("Retorn climatitzacio",roof,new(-2.6f+i*6,3.37f,.6f));
                Box("Reixeta",vent,Vector3.zero,new(.7f,.05f,.48f),steel,.01f);
                for(int j=0;j<8;j++)Box("Lama",vent,new(0,-.029f,-.19f+j*.053f),new(.62f,.01f,.018f),black,0);
            }
            var facade=Group("FACANA · vidres i entrada",a);
            Box("Llinda",facade,new(0,2.95f,-5.3f),new(12.2f,.9f,.22f),red,0,true);
            Box("Ampit esquerre",facade,new(-3.4f,.2f,-5.3f),new(5.2f,.4f,.22f),red,0,true);
            Box("Ampit dret",facade,new(3.4f,.2f,-5.3f),new(5.2f,.4f,.22f),red,0,true);
            foreach(float x in new[]{-6f,-3.45f,-.85f,.85f,3.45f,6f})Box("Muntant aparador",facade,new(x,1.45f,-5.25f),new(.075f,2.9f,.1f),black,.008f,true);
            foreach(float x in new[]{-3.4f,3.4f})
            {
                Box("Travesser inferior",facade,new(x,.45f,-5.25f),new(5.1f,.06f,.1f),black,.005f);
                // Clear glazing is represented by its physical frame; no stacked transparency in VR.
                var glass=Group("Vidre transparent / col·lisio",facade,new(x,1.48f,-5.25f));var col=glass.gameObject.AddComponent<BoxCollider>();col.size=new Vector3(5.1f,2.05f,.025f);
            }
            Sign("Rètol exterior",facade,new(0,2.99f,-5.43f),new(4.65f,.7f),"Brand",black);
            Sign("Horari entrada",facade,new(1.31f,1.4f,-5.33f),new(.33f,.50f),"Hours",red);
            Box("Estora entrada",a,new(0,.009f,-4.75f),new(1.65f,.018f,.8f),black,.01f);
            var outdoors=Group("EXTERIOR · vorera",environment);
            Box("Vorera",outdoors,new(0,-.13f,-7),new(16,.15f,3.5f),plaster,0);
            Box("Carrer",outdoors,new(0,-.25f,-12),new(35,.1f,7),black,0);
            // Service rooms: complete small rooms behind the closed, modelled doors.
            var service=Group("Zona de servei · WC i magatzem",a);
            Box("Envà serveis",service,new(-3.4f,1.7f,3.45f),new(5.2f,3.4f,.12f),plaster,0,true);
            Box("Envà central",service,new(-3.7f,1.7f,4.35f),new(.12f,3.4f,1.8f),plaster,0,true);
            Box("Tancament magatzem",service,new(-.8f,1.7f,4.35f),new(.12f,3.4f,1.8f),plaster,0,true);
            Door(service,new(-4.8f,0,3.35f),"WC");Door(service,new(-2.2f,0,3.35f),"Magatzem");
            Sign("Indicacio serveis",service,new(-3.4f,2.75f,3.36f),new(2.4f,.6f),"Wayfinding",wood);
        }
        static void Door(Transform parent,Vector3 p,string name)
        {
            var d=Group("Porta " +name,parent,p);
            Box("Fulla",d,new(0,1.1f,0),new(.94f,2.2f,.055f),wood,.015f,true);
            foreach(float x in new[]{-.5f,.5f})Box("Marc vertical",d,new(x,1.13f,0),new(.065f,2.26f,.08f),black,.006f);
            Box("Marc superior",d,new(0,2.24f,0),new(1.06f,.07f,.08f),black,.006f);
            Rod("Maneta",d,new(.29f,1.04f,-.10f),new(.42f,1.04f,-.1f),.018f,chrome);
            Cylinder("Roseta",d,new(.39f,1.04f,-.067f),.035f,.03f,chrome).transform.localEulerAngles=new(90,0,0);
        }
        static void CreateCameras()
        {
            var views=Group("VISTES · cameres de revisio (desactivades)");
            CameraView("01_Interior",views,new(-.2f,2.15f,-4.65f),new(.4f,1.35f,2),70);
            CameraView("02_Cuina",views,new(.35f,1.95f,3.25f),new(2.9f,1.55f,4.4f),80);
            CameraView("03_Menjador",views,new(4.9f,2.15f,-2.7f),new(-2.9f,1.1f,.3f),72);
            CameraView("04_Facana",views,new(-3.7f,2.8f,-12f),new(0,1.4f,-4.8f),62);
            var plan=CameraView("05_Planta",views,new(0,15,0),Vector3.zero,60);plan.orthographic=true;plan.orthographicSize=7.3f;plan.transform.rotation=Quaternion.Euler(90,0,0);
        }
        static Camera CameraView(string name,Transform p,Vector3 from,Vector3 to,float fov)
        {
            var g=Group(name,p,from);g.LookAt(to);var c=g.gameObject.AddComponent<Camera>();c.fieldOfView=fov;c.nearClipPlane=.05f;c.farClipPlane=60;c.enabled=false;c.clearFlags=CameraClearFlags.SolidColor;c.backgroundColor=Hex("#A8B9BC");
            var data=c.GetUniversalAdditionalCameraData();data.renderPostProcessing=false;data.antialiasing=AntialiasingMode.None;return c;
        }
        static void BuildLighting()
        {
            var lights=Group("05 · Il·luminacio",environment);
            RenderSettings.ambientMode=AmbientMode.Trilight;RenderSettings.ambientSkyColor=Hex("#C5CFD4")*.7f;RenderSettings.ambientEquatorColor=Hex("#B4AAA0")*.6f;RenderSettings.ambientGroundColor=Hex("#7B6C58")*.45f;
            RenderSettings.defaultReflectionResolution=128;RenderSettings.reflectionIntensity=.7f;
            var sun=Group("Sol · unica llum en temps real",lights).gameObject.AddComponent<Light>();sun.type=LightType.Directional;sun.transform.eulerAngles=new(48,-24,0);sun.color=Hex("#FFEDD0");sun.intensity=1.5f;sun.shadows=LightShadows.Hard;sun.lightmapBakeType=LightmapBakeType.Mixed;sun.shadowBias=.02f;sun.shadowNormalBias=.15f;RenderSettings.sun=sun;
            foreach(var pos in new[]{new Vector3(-3.2f,3.2f,-2.5f),new Vector3(-3.2f,3.2f,1.5f),new Vector3(3.2f,3.2f,-2.5f),new Vector3(3.2f,3.2f,3.4f)})
            {
                var l=Group("Llum sostre baked",lights,pos).gameObject.AddComponent<Light>();l.type=LightType.Rectangle;l.areaSize=new(3,2);l.transform.eulerAngles=new(90,0,0);l.intensity=2f;l.color=Hex("#FFF1E1");l.lightmapBakeType=LightmapBakeType.Baked;
            }
            var ls=new LightingSettings { name="Restaurant Baked Lighting", bakedGI=true,realtimeGI=false,lightmapper=LightingSettings.Lightmapper.ProgressiveGPU,lightmapResolution=12,lightmapMaxSize=2048,lightmapPadding=4,indirectSampleCount=128,directSampleCount=32,environmentSampleCount=64,maxBounces=3,ao=true,aoMaxDistance=.55f,aoExponentIndirect=1.2f,mixedBakeMode=MixedLightingMode.Subtractive };
            string path=Root+"/Settings/RestaurantLighting.asset";var old=AssetDatabase.LoadAssetAtPath<LightingSettings>(path);if(old){EditorUtility.CopySerialized(ls,old);UnityEngine.Object.DestroyImmediate(ls);ls=old;}else AssetDatabase.CreateAsset(ls,path);Lightmapping.lightingSettings=ls;
            var probe=Group("Reflexio interior",lights,new(0,1.6f,0)).gameObject.AddComponent<ReflectionProbe>();probe.mode=ReflectionProbeMode.Baked;probe.size=new(12,3.4f,10.5f);probe.resolution=128;probe.boxProjection=true;probe.clearFlags=ReflectionProbeClearFlags.SolidColor;probe.backgroundColor=Hex("#B5AB99");
            var probes=Group("Light probes",lights).gameObject.AddComponent<LightProbeGroup>();var points=new System.Collections.Generic.List<Vector3>();
            for(int x=-5;x<=5;x+=2)for(int z=-4;z<=4;z+=2)foreach(float y in new[]{.4f,1.6f,2.7f})points.Add(new(x,y,z));probes.probePositions=points.ToArray();
        }
        public static void BuildBakeAndValidate()
        {
            Build();BakeAndCapture();QuestProjectSetup.Validate();
        }
        public static void BakeAndCapture()
        {
            EditorSceneManager.OpenScene(ScenePath);Debug.Log("BRASA_BAKE_START");
            bool baked=Lightmapping.Bake();EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());AssetDatabase.SaveAssets();
            Debug.Log("BRASA_BAKE_RESULT="+baked);Capture();WriteReport();
        }
        [MenuItem("BRASA/04 · Exportar vistes PNG")]
        public static void Capture()
        {
            Directory.CreateDirectory("Documentation/Previews");
            foreach(var cam in UnityEngine.Object.FindObjectsByType<Camera>(FindObjectsSortMode.None).Where(c=>c.name.Length>2&&c.name[2]=='_'))
            {
                var roof=GameObject.Find("SOSTRE · ocultar per veure la planta");if(cam.name=="05_Planta")roof.SetActive(false);
                var rt=new RenderTexture(1920,1280,24,RenderTextureFormat.ARGB32){antiAliasing=4};cam.targetTexture=rt;cam.Render();
                RenderTexture.active=rt;var tex=new Texture2D(1920,1280,TextureFormat.RGB24,false);tex.ReadPixels(new Rect(0,0,1920,1280),0,0);tex.Apply();File.WriteAllBytes("Documentation/Previews/"+cam.name+".png",tex.EncodeToPNG());
                cam.targetTexture=null;RenderTexture.active=null;rt.Release();UnityEngine.Object.DestroyImmediate(rt);UnityEngine.Object.DestroyImmediate(tex);if(roof)roof.SetActive(true);
            }
            Debug.Log("BRASA_CAPTURE_OK");
        }
        static void WriteReport()
        {
            var filters=UnityEngine.Object.FindObjectsByType<MeshFilter>(FindObjectsSortMode.None);long triangles=filters.Sum(f=>f.sharedMesh?f.sharedMesh.triangles.Length/3:0);
            int missing=GameObject.FindObjectsByType<Transform>(FindObjectsSortMode.None).Sum(t=>GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject));
            string report=$"Scene: {ScenePath}\nUnity: {Application.unityVersion}\nFootprint: 12 x 10.5 m = 126 m2\nDining: 6 tables / 24 chairs\nMesh renderers: {filters.Length}\nScene triangles: {triangles}\nMissing scripts: {missing}\nLightmap textures: {LightmapSettings.lightmaps.Length}\nAndroid module: {BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.Android,BuildTarget.Android)}\nQuest headset test: PENDING (physical hardware required)\n";
            Directory.CreateDirectory("Documentation");File.WriteAllText("Documentation/Validation.txt",report);Debug.Log(report);
            if(missing!=0)throw new Exception("Missing scripts in restaurant scene");
        }
    }
}
