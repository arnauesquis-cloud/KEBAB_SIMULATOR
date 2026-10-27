using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEditor.XR.Management;
using UnityEditor.XR.Management.Metadata;
using UnityEditor.XR.OpenXR;
using UnityEditor.XR.OpenXR.Features;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.XR.Management;
using UnityEngine.XR.OpenXR;
using UnityEngine.XR.OpenXR.Features;
using UnityEngine.XR.OpenXR.Features.Interactions;
using UnityEngine.XR.OpenXR.Features.MetaQuestSupport;
using Unity.XR.CoreUtils;
using static Brasa.Editor.RestaurantGeometry;

namespace Brasa.Editor
{
    public static class QuestProjectSetup
    {
        [MenuItem("BRASA/02 · Configurar Quest 3 i Link")]
        public static void Configure()
        {
            PlayerSettings.companyName="Arnau i Zoran";PlayerSettings.productName="BRASA - Kebab Restaurant";PlayerSettings.bundleVersion="0.1.0";
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android,"com.arnauzoran.brasakebab");
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android,ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures=AndroidArchitecture.ARM64;
            PlayerSettings.Android.minSdkVersion=AndroidSdkVersions.AndroidApiLevel29;
            PlayerSettings.Android.targetSdkVersion=AndroidSdkVersions.AndroidApiLevelAuto;
            PlayerSettings.Android.androidTVCompatibility=false;PlayerSettings.Android.minifyRelease=false;
            PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.Android,false);PlayerSettings.SetGraphicsAPIs(BuildTarget.Android,new[]{GraphicsDeviceType.Vulkan});
            PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.StandaloneWindows64,false);PlayerSettings.SetGraphicsAPIs(BuildTarget.StandaloneWindows64,new[]{GraphicsDeviceType.Direct3D11});
            PlayerSettings.defaultInterfaceOrientation=UIOrientation.LandscapeLeft;
            PlayerSettings.runInBackground=true;PlayerSettings.stereoRenderingPath=StereoRenderingPath.Instancing;
            PlayerSettings.SetManagedStrippingLevel(NamedBuildTarget.Android,ManagedStrippingLevel.Low);
            PlayerSettings.SetMobileMTRendering(NamedBuildTarget.Android,true);
            EditorUserBuildSettings.androidBuildSubtarget=MobileTextureSubtarget.ASTC;
            EditorUserBuildSettings.buildAppBundle=false;
            // Unity exposes activeInputHandler only as a serialized project setting.
            var player=new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/ProjectSettings.asset")[0]);
            player.FindProperty("activeInputHandler").intValue=1;player.ApplyModifiedPropertiesWithoutUndo();
            if(!EditorBuildSettings.TryGetConfigObject(XRGeneralSettings.k_SettingsKey,out XRGeneralSettingsPerBuildTarget all))
            {
                all=ScriptableObject.CreateInstance<XRGeneralSettingsPerBuildTarget>();
                AssetDatabase.CreateAsset(all,Root+"/Settings/XRGeneralSettings.asset");EditorBuildSettings.AddConfigObject(XRGeneralSettings.k_SettingsKey,all,true);
            }
            foreach(var target in new[]{BuildTargetGroup.Standalone,BuildTargetGroup.Android})
            {
                if(!all.HasSettingsForBuildTarget(target))all.CreateDefaultSettingsForBuildTarget(target);
                if(!all.HasManagerSettingsForBuildTarget(target))all.CreateDefaultManagerSettingsForBuildTarget(target);
                var general=all.SettingsForBuildTarget(target);general.InitManagerOnStart=true;
                var manager=general.Manager;manager.automaticLoading=true;manager.automaticRunning=true;
                if(!XRPackageMetadataStore.AssignLoader(manager,typeof(OpenXRLoader).FullName,target))throw new Exception("Cannot assign OpenXR loader to "+target);
                var settings=EnsureOpenXRSettings(target);FeatureHelpers.RefreshFeatures(target);
                settings.renderMode=OpenXRSettings.RenderMode.SinglePassInstanced;
                if(target==BuildTargetGroup.Android)settings.latencyOptimization=OpenXRSettings.LatencyOptimization.PrioritizeInputPolling;
                foreach(var f in settings.GetFeatures())
                {
                    f.enabled=f is OculusTouchControllerProfile||f is MetaQuestTouchPlusControllerProfile||(target==BuildTargetGroup.Android&&f is MetaQuestFeature);
                    EditorUtility.SetDirty(f);
                }
                EditorUtility.SetDirty(settings);EditorUtility.SetDirty(manager);EditorUtility.SetDirty(general);
            }
            EditorUtility.SetDirty(all);AssetDatabase.SaveAssets();Debug.Log("BRASA_XR_CONFIGURED: Android + Windows OpenXR, Quest 3 Touch Plus, ARM64/IL2CPP/Vulkan.");
        }
        static OpenXRSettings EnsureOpenXRSettings(BuildTargetGroup group)
        {
            // The pinned OpenXR 1.19 editor API hides the package container. Create it through its factory.
            // An Android configuration is saved even on computers without Android Build Support.
            var type=typeof(FeatureHelpers).Assembly.GetType("UnityEditor.XR.OpenXR.OpenXRPackageSettings",true);
            var package=(ScriptableObject)type.GetMethod("GetOrCreateInstance",BindingFlags.Static|BindingFlags.Public).Invoke(null,null);
            var settings=OpenXRSettings.GetSettingsForBuildTargetGroup(group);
            if(settings)return settings;
            var dictionary=(Dictionary<BuildTargetGroup,OpenXRSettings>)type.GetField("Settings",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(package);
            settings=ScriptableObject.CreateInstance<OpenXRSettings>();settings.name=group.ToString();dictionary[group]=settings;
            AssetDatabase.AddObjectToAsset(settings,package);EditorUtility.SetDirty(package);AssetDatabase.SaveAssets();return settings;
        }
        public static void CreateRig()
        {
            var rig=Group("XR Origin · Quest 3 · nomes tracking",null,new(0,0,-3.8f));
            var offset=Group("Camera Offset",rig);
            var cameraObject=Group("Main Camera",offset,new(0,1.65f,0)).gameObject;cameraObject.tag="MainCamera";
            var camera=cameraObject.AddComponent<Camera>();camera.nearClipPlane=.05f;camera.farClipPlane=60;camera.fieldOfView=78;
            camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=Hex("#A8B9BC");cameraObject.AddComponent<AudioListener>();camera.GetUniversalAdditionalCameraData().renderPostProcessing=false;
            Track(cameraObject,"<XRHMD>/centerEyePosition","<XRHMD>/centerEyeRotation","<XRHMD>/trackingState");
            var origin=rig.gameObject.AddComponent<XROrigin>();origin.Camera=camera;origin.CameraFloorOffsetObject=offset.gameObject;origin.CameraYOffset=1.65f;origin.RequestedTrackingOriginMode=XROrigin.TrackingOriginMode.Floor;
            foreach(var hand in new[]{"LeftHand","RightHand"})
            {
                var controller=Group(hand,offset);Track(controller.gameObject,$"<XRController>{{{hand}}}/devicePosition",$"<XRController>{{{hand}}}/deviceRotation",$"<XRController>{{{hand}}}/trackingState");
                var model=Group("Model controlador",controller);
                var shell=Mat("Controller pearl","#DAD9D2",.05f,.3f);var dark=Mat("Controller charcoal","#343A3C");
                Lathe("Empunyadura",model,new(0,-.035f,0),new[]{new Vector2(.018f,-.07f),new Vector2(.026f,-.01f),new Vector2(.028f,.025f)},shell,20,true,"ControllerGrip");
                Box("Capçal",model,new(0,.012f,.012f),new(.075f,.032f,.065f),shell,.014f);
                Cylinder("Joystick",model,new(-.013f,.035f,.012f),.008f,.012f,dark,12);
                foreach(var t in model.GetComponentsInChildren<Transform>())GameObjectUtility.SetStaticEditorFlags(t.gameObject,0);
                var visibility=controller.gameObject.AddComponent<Brasa.TrackedControllerVisual>();visibility.model=model.gameObject;visibility.node=hand=="LeftHand"?UnityEngine.XR.XRNode.LeftHand:UnityEngine.XR.XRNode.RightHand;model.gameObject.SetActive(false);
            }
            PrefabUtility.SaveAsPrefabAsset(rig.gameObject,Root+"/Prefabs/Quest3TrackingRig.prefab");
        }
        static void Track(GameObject go,string position,string rotation,string state)
        {
            var t=go.AddComponent<TrackedPoseDriver>();t.updateType=TrackedPoseDriver.UpdateType.UpdateAndBeforeRender;
            t.positionInput=new InputActionProperty(new InputAction("Position",InputActionType.Value,position,expectedControlType:"Vector3"));
            t.rotationInput=new InputActionProperty(new InputAction("Rotation",InputActionType.Value,rotation,expectedControlType:"Quaternion"));
            t.trackingStateInput=new InputActionProperty(new InputAction("Tracking State",InputActionType.Value,state,expectedControlType:"Integer"));
        }
        [MenuItem("BRASA/03 · Validar escena i XR")]
        public static void Validate()
        {
            EditorSceneManager.OpenScene(RestaurantBuilder.ScenePath);
            var lines=new List<string>();int errors=0;
            foreach(var group in new[]{BuildTargetGroup.Android,BuildTargetGroup.Standalone})
            {
                var settings=OpenXRSettings.GetSettingsForBuildTargetGroup(group);
                if(!settings){errors++;lines.Add("ERROR: missing OpenXR settings "+group);continue;}
                var xr=XRGeneralSettingsPerBuildTarget.XRGeneralSettingsForBuildTarget(group);
                bool loader=xr&&xr.Manager&&xr.Manager.activeLoaders.Any(x=>x is OpenXRLoader);
                if(!loader)errors++;
                lines.Add(group+" loader="+loader+", stereo="+settings.renderMode);
                lines.Add("Enabled: "+string.Join(", ",settings.GetFeatures().Where(f=>f.enabled).Select(f=>f.GetType().Name)));
                var issues=new List<OpenXRFeature.ValidationRule>();
                var previouslySelected=EditorUserBuildSettings.selectedBuildTargetGroup;
                try
                {
                    // Meta's validator reads the selected tab, even when a target is passed explicitly.
                    EditorUserBuildSettings.selectedBuildTargetGroup=group;
                    OpenXRProjectValidation.GetCurrentValidationIssues(issues,group);
                }
                finally { EditorUserBuildSettings.selectedBuildTargetGroup=previouslySelected; }
                foreach(var issue in issues){lines.Add((issue.error?"ERROR ":"ADVICE ")+group+": "+issue.message);if(issue.error)errors++;}
            }
            var all=UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsSortMode.None);
            int missing=all.Sum(t=>GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject));errors+=missing;lines.Add("Missing scripts="+missing);
            foreach(var r in UnityEngine.Object.FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None))if(r.sharedMaterials.Any(m=>!m||!m.shader)){errors++;lines.Add("ERROR material: "+r.name);}
            var origin=UnityEngine.Object.FindFirstObjectByType<XROrigin>();if(!origin||!origin.Camera){errors++;lines.Add("ERROR camera rig");}
            var camera=origin.Camera;Physics.SyncTransforms();
            var spawnHits=Physics.OverlapSphere(camera.transform.position,.22f);lines.Add("Spawn obstacles="+spawnHits.Length);if(spawnHits.Length>0)errors++;
            lines.Add("Android Build Support installed="+BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.Android,BuildTarget.Android));
            lines.Add("Hardware tracking and performance: NOT TESTED; headset required.");lines.Add("Validation errors="+errors);
            File.WriteAllLines("Documentation/XR-Validation.txt",lines);foreach(var l in lines)Debug.Log(l);
            if(errors>0)throw new Exception("BRASA validation failed: "+errors);Debug.Log("BRASA_VALIDATION_OK");
        }
        [MenuItem("BRASA/05 · Crear APK per a Quest 3")]
        public static void BuildQuestAPK()
        {
            if(!BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.Android,BuildTarget.Android))
                throw new BuildFailedException("Instal·la Android Build Support + Android SDK & NDK Tools + OpenJDK a Unity Hub > Installs > 6000.3.24f1 > Add modules. Després torna a executar aquest menú.");
            Configure();Validate();Directory.CreateDirectory("Builds/Quest3");
            var result=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{RestaurantBuilder.ScenePath},target=BuildTarget.Android,locationPathName="Builds/Quest3/BrasaRestaurant.apk",options=BuildOptions.None});
            if(result.summary.result!=BuildResult.Succeeded)throw new BuildFailedException(result.summary.result.ToString());
            Debug.Log("BRASA_APK_OK: Builds/Quest3/BrasaRestaurant.apk");
        }
    }
}
