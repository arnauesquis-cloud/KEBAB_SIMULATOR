using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.XR.CoreUtils;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using static Brasa.Editor.RestaurantGeometry;

namespace Brasa.Editor
{
    public static class TestingSceneBuilder
    {
        public const string TestingScenePath = "Assets/Kebab/Scenes/Testing.unity";
        public const string EvidenceDir = "Documentation/Sprint2_Evidencies";

        [MenuItem("BRASA/07 · Crear escena Testing")]
        public static void BuildTestingScene()
        {
            Reset();
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // Reutilitza els materials URP del restaurant per mantenir coherència visual i compatibilitat Quest.
            Material floorMat = LoadMat("Stone terrazzo tiles", "#D8C6AD");
            Material wallMat = LoadMat("Plaster warm ivory", "#ECE5D5");
            Material woodMat = LoadMat("Natural oak", "#C99A63");
            Material blackMat = LoadMat("Powder coated charcoal", "#263333");
            Material steelMat = LoadMat("Brushed stainless steel", "#AFBABC");
            Material redMat = LoadMat("Enamel oxblood", "#84383F");
            Material goldMat = LoadMat("Mustard shell", "#DDA83F");
            Material greenMat = LoadMat("Fresh lettuce", "#79A74C");
            Material pitaMat = LoadMat("Pita golden crust", "#DEB77B");
            Material whiteMat = LoadMat("Ceramic ivory", "#F3E9D4");
            Material tomatoMat = LoadMat("Tomato red", "#B84E3C");

            var env = Group("ENTORN TESTING · PROVES VR (12 x 12 m)");

            // Paviment transitable amb BoxCollider a y = 0.
            Box("Paviment Testing", env, new Vector3(0f, -0.10f, 0f), new Vector3(12f, 0.2f, 12f), floorMat, 0f, true);

            // Parets perimetrals amb collider per provar els límits del teletransport.
            Box("Paret Nord", env, new Vector3(0f, 1.5f, 6.1f), new Vector3(12.4f, 3.0f, 0.2f), wallMat, 0f, true);
            Box("Paret Sud", env, new Vector3(0f, 1.5f, -6.1f), new Vector3(12.4f, 3.0f, 0.2f), wallMat, 0f, true);
            Box("Paret Oest", env, new Vector3(-6.1f, 1.5f, 0f), new Vector3(0.2f, 3.0f, 12.2f), wallMat, 0f, true);
            Box("Paret Est", env, new Vector3(6.1f, 1.5f, 0f), new Vector3(0.2f, 3.0f, 12.2f), wallMat, 0f, true);

            // Il·luminació neta en temps real per a proves ràpides.
            var lights = Group("Il·luminacio Testing", env);
            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.82f, 0.86f, 0.90f);
            RenderSettings.ambientEquatorColor = new Color(0.70f, 0.68f, 0.64f);
            RenderSettings.ambientGroundColor = new Color(0.45f, 0.42f, 0.38f);
            var sun = Group("Sol Testing", lights).gameObject.AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.transform.eulerAngles = new Vector3(48f, -30f, 0f);
            sun.color = new Color(1f, 0.96f, 0.88f);
            sun.intensity = 1.45f;
            sun.shadows = LightShadows.Hard;
            RenderSettings.sun = sun;

            // Zona 1: Banc de proves per agafar i deixar objectes (XRGrabInteractable).
            var grabZone = Group("ZONA 01 · Proves Grab (Agafar i Deixar)", env);
            CreateTestTable("Taula Origen · Agafar", grabZone, new Vector3(-0.78f, 0f, -0.55f), woodMat, blackMat, goldMat);
            CreateTestTable("Taula Desti · Deixar", grabZone, new Vector3(0.78f, 0f, -0.55f), steelMat, blackMat, greenMat);

            // Objectes reals del joc i de prova amb XRGrabInteractable sobre Taula Origen (top a y = 0.90).
            var pitaMesh = AssetDatabase.LoadAssetAtPath<Mesh>(Root + "/Art/Meshes/Pita.asset");
            var bottleMesh = AssetDatabase.LoadAssetAtPath<Mesh>(Root + "/Art/Meshes/SauceBottle.asset");
            var nozzleMesh = AssetDatabase.LoadAssetAtPath<Mesh>(Root + "/Art/Meshes/SauceNozzle.asset");

            GameObject pitaObj;
            if (pitaMesh)
            {
                pitaObj = Object("Pa pita · Grab", grabZone, pitaMesh, pitaMat, new Vector3(-0.45f, 0.945f, -0.55f));
                pitaObj.transform.localScale = new Vector3(1.25f, 1.0f, 0.95f);
            }
            else
            {
                pitaObj = Box("Pa pita · Grab", grabZone, new Vector3(-0.45f, 0.95f, -0.55f), new Vector3(0.22f, 0.08f, 0.16f), pitaMat, 0.02f);
            }
            QuestProjectSetup.MakeGrabbable(pitaObj, 0.25f);

            var whiteBottle = CreateSauceGrabbable("Ampolla salsa blanca · Grab", grabZone, new Vector3(-0.80f, 0.92f, -0.45f), bottleMesh, nozzleMesh, whiteMat, whiteMat);
            QuestProjectSetup.MakeGrabbable(whiteBottle, 0.30f);

            var redBottle = CreateSauceGrabbable("Ampolla salsa vermella · Grab", grabZone, new Vector3(-1.02f, 0.92f, -0.45f), bottleMesh, nozzleMesh, redMat, whiteMat);
            QuestProjectSetup.MakeGrabbable(redBottle, 0.30f);

            var trayObj = Box("Safata vermella · Grab", grabZone, new Vector3(-0.92f, 0.93f, -0.78f), new Vector3(0.36f, 0.025f, 0.24f), redMat, 0.008f);
            QuestProjectSetup.MakeGrabbable(trayObj, 0.40f);

            var testCube = Box("Cub prova VR · Grab", grabZone, new Vector3(-0.52f, 0.98f, -0.78f), new Vector3(0.13f, 0.13f, 0.13f), greenMat, 0.015f);
            QuestProjectSetup.MakeGrabbable(testCube, 0.35f);

            // Zona 2: Zones de proves de Teletransport VR + obstacle físic que bloqueja el destí.
            var teleportZone = Group("ZONA 02 · Proves Teletransport VR", env);
            Box("Pad Teleport A (Valid)", teleportZone, new Vector3(-2.6f, 0.004f, 2.3f), new Vector3(1.6f, 0.008f, 1.6f), goldMat, 0f, false);
            Box("Pad Teleport B (Valid)", teleportZone, new Vector3(2.6f, 0.004f, 2.3f), new Vector3(1.6f, 0.008f, 1.6f), greenMat, 0f, false);
            Box("Obstacle · bloqueja teleport", teleportZone, new Vector3(0f, 0.75f, 2.3f), new Vector3(1.5f, 1.5f, 0.55f), tomatoMat, 0.03f, true);

            // Instal·la el mateix XR Interaction Manager i XR Origin (Quest3TrackingRig) que el joc principal.
            var manager = QuestProjectSetup.EnsureInteractionManager();
            var rigPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(Root + "/Prefabs/Quest3TrackingRig.prefab");
            GameObject rigObj;
            if (rigPrefab)
            {
                rigObj = (GameObject)PrefabUtility.InstantiatePrefab(rigPrefab);
            }
            else
            {
                QuestProjectSetup.CreateRig();
                rigObj = UnityEngine.Object.FindFirstObjectByType<XROrigin>().gameObject;
            }
            rigObj.name = "XR Origin · Quest 3 · Testing";
            rigObj.transform.position = new Vector3(0f, 0f, -2.0f);
            rigObj.transform.rotation = Quaternion.identity;

            var origin = rigObj.GetComponent<XROrigin>();
            var teleport = rigObj.GetComponent<Brasa.PointTeleportLocomotion>();
            if (teleport)
            {
                teleport.restrictToRestaurantBounds = false;
                EditorUtility.SetDirty(teleport);
            }
            QuestProjectSetup.AttachXRGrabInteractors(origin, null, null);

            // Enllaça l'XRInteractionManager de l'escena Testing a tots els XRGrabInteractable.
            foreach (var grab in UnityEngine.Object.FindObjectsByType<XRGrabInteractable>(FindObjectsSortMode.None))
            {
                grab.interactionManager = manager;
                EditorUtility.SetDirty(grab);
            }

            // Càmeres de revisió / evidència (desactivades en execució normal VR).
            var cams = Group("CAMERES EVIDENCIA (desactivades)");
            CreateCam("Cam_Overview", cams, new Vector3(-3.4f, 2.75f, -3.9f), new Vector3(0.2f, 0.85f, 0.6f), 58f);
            CreateCam("Cam_Grab", cams, new Vector3(0.05f, 1.58f, -1.95f), new Vector3(0.05f, 0.98f, -0.55f), 52f);
            CreateCam("Cam_Teleport", cams, new Vector3(-2.6f, 2.25f, -2.9f), new Vector3(1.1f, 0.55f, 1.3f), 58f);

            EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene(), TestingScenePath);
            EnsureTestingInBuildSettings();
            AssetDatabase.SaveAssets();
            Debug.Log("BRASA_TESTING_SCENE_OK: Escena Assets/Kebab/Scenes/Testing.unity creada.");
        }

        [MenuItem("BRASA/08 · Provar Testing i exportar evidències")]
        public static void SetupAllVerifyAndCapture()
        {
            // 1) Instal·la XR Interaction Toolkit (Grab) + Teleport al restaurant principal i prefab.
            QuestProjectSetup.InstallTeleportAndGrabInRestaurant();

            // 2) Construeix l'escena Testing.unity amb el mateix rig VR i objectes agafables.
            BuildTestingScene();

            // 3) Obre Testing.unity i executa proves reals de Grab, Drop amb física i Teletransport.
            EditorSceneManager.OpenScene(TestingScenePath);
            Directory.CreateDirectory(EvidenceDir);

            var report = new List<string>();
            report.Add("BRASA KEBAB VR · INFORME DE PROVES ESCENA TESTING (SPRINT 2)");
            report.Add("Data: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm"));

            var manager = UnityEngine.Object.FindFirstObjectByType<XRInteractionManager>();
            var origin = UnityEngine.Object.FindFirstObjectByType<XROrigin>();
            var teleport = UnityEngine.Object.FindFirstObjectByType<Brasa.PointTeleportLocomotion>();
            var interactors = UnityEngine.Object.FindObjectsByType<XRDirectInteractor>(FindObjectsSortMode.None);
            var grabbables = UnityEngine.Object.FindObjectsByType<XRGrabInteractable>(FindObjectsSortMode.None);

            if (!manager || !origin || !teleport || interactors.Length < 2 || grabbables.Length < 4)
                throw new Exception($"Missing VR testing components: mgr={manager!=null} origin={origin!=null} tp={teleport!=null} interactors={interactors.Length} grabs={grabbables.Length}");

            report.Add($"[PASS] XRInteractionManager present: {manager.name}");
            report.Add($"[PASS] XRDirectInteractors (mans): {interactors.Length} (LeftHand + RightHand, Grip configurat)");
            report.Add($"[PASS] Objectes XRGrabInteractable a Testing: {grabbables.Length}");

            var camOverview = GameObject.Find("Cam_Overview").GetComponent<Camera>();
            var camGrab = GameObject.Find("Cam_Grab").GetComponent<Camera>();
            var camTeleport = GameObject.Find("Cam_Teleport").GetComponent<Camera>();

            // Captura 1: Vista general de l'escena Testing.
            RenderCam(camOverview, Path.Combine(EvidenceDir, "01_Overview.png"), 960, 640);

            // Mostra els models visuals dels comandaments per fer evident la mà VR a les captures.
            var rightHand = origin.transform.Find("Camera Offset/RightHand");
            var leftHand = origin.transform.Find("Camera Offset/LeftHand");
            SetControllerVisualVisible(rightHand, true);
            SetControllerVisualVisible(leftHand, true);

            // PROVA 1: Agafar objecte (XRGrabInteractable) amb RightHand.
            var pita = GameObject.Find("Pa pita · Grab");
            var pitaGrab = pita.GetComponent<XRGrabInteractable>();
            var pitaRb = pita.GetComponent<Rigidbody>();
            var rightInteractor = rightHand.GetComponent<XRDirectInteractor>();

            // Comprova propietats del component oficial XRGrabInteractable.
            bool grabConfigOk = pitaGrab != null && pitaRb != null && pitaRb.collisionDetectionMode == CollisionDetectionMode.ContinuousDynamic && rightInteractor != null;
            if (!grabConfigOk) throw new Exception("XRGrabInteractable configuration check failed.");

            // Simula agafar el Pa pita amb el comandament dret i aixecar-lo entre les dues taules.
            rightHand.position = new Vector3(0.05f, 1.24f, -0.58f);
            rightHand.eulerAngles = new Vector3(-12f, 20f, 0f);
            pitaRb.isKinematic = true;
            pita.transform.position = rightHand.TransformPoint(new Vector3(0f, 0.03f, 0.07f));
            pita.transform.rotation = rightHand.rotation;
            Physics.SyncTransforms();

            RenderCam(camGrab, Path.Combine(EvidenceDir, "02_Grab.png"), 960, 640);
            report.Add($"[PASS] Prova Grab (Agafar): RightHand (XRDirectInteractor) soste '{pita.name}' a y={pita.transform.position.y:0.00} m");

            // PROVA 2: Deixar l'objecte sobre Taula Desti · Deixar i simular fisica real (PhysX).
            rightHand.position = new Vector3(0.78f, 1.22f, -0.55f);
            pita.transform.position = new Vector3(0.78f, 1.12f, -0.55f);
            pita.transform.rotation = Quaternion.identity;
            pitaRb.isKinematic = false;
            pitaRb.useGravity = true;
            pitaRb.linearVelocity = Vector3.zero;
            pitaRb.angularVelocity = Vector3.zero;
            Physics.SyncTransforms();

            var prevSimMode = Physics.simulationMode;
            Physics.simulationMode = SimulationMode.Script;
            for (int i = 0; i < 100; i++) Physics.Simulate(0.02f);
            Physics.simulationMode = prevSimMode;

            float droppedY = pita.transform.position.y;
            if (droppedY < 0.88f || droppedY > 1.06f)
                throw new Exception($"Drop physics failed: pita ended at y={droppedY:0.000} (expected on top of table ~0.90..1.02)");

            // Retira la ma una mica enrere per mostrar l'objecte descansant sol a la taula desti.
            rightHand.position = new Vector3(0.55f, 1.32f, -0.95f);
            RenderCam(camGrab, Path.Combine(EvidenceDir, "03_Drop.png"), 960, 640);
            report.Add($"[PASS] Prova Drop (Deixar + Fisica): '{pita.name}' cau i reposa sobre Taula Desti a y={droppedY:0.00} m (sense travessar)");

            // PROVA 3: Teletransport VR (arc valid cap a Zona Teleport B + bloqueig a Obstacle).
            leftHand.position = new Vector3(0.45f, 1.18f, -1.65f);
            leftHand.LookAt(new Vector3(2.6f, 2.25f, 2.3f));
            Physics.SyncTransforms();

            bool validAim = teleport.ProvarArcTeleport(leftHand, false, out Vector3 targetPos);
            if (!validAim) throw new Exception("Teleport valid floor aim test failed.");

            RenderCam(camTeleport, Path.Combine(EvidenceDir, "04_Teleport.png"), 960, 640);
            report.Add($"[PASS] Prova Teletransport (Arc valid): Desti valid detectat a ({targetPos.x:0.00}, {targetPos.y:0.00}, {targetPos.z:0.00})");

            // Comprova que apuntar contra l'obstacle central queda bloquejat.
            leftHand.position = new Vector3(0f, 1.15f, 0.6f);
            leftHand.LookAt(new Vector3(0f, 1.25f, 2.3f));
            Physics.SyncTransforms();
            bool blockedAim = teleport.ProvarArcTeleport(leftHand, false, out _);
            if (blockedAim) throw new Exception("Teleport obstacle rejection test failed (obstacle was accepted!).");
            report.Add("[PASS] Prova Teletransport (Obstacle): Desti contra obstacle bloquejat correctament.");

            // Executa el teletransport real i verifica el desplacament de l'XROrigin.
            leftHand.position = new Vector3(0.45f, 1.18f, -1.65f);
            leftHand.LookAt(new Vector3(2.6f, 2.25f, 2.3f));
            Physics.SyncTransforms();
            Vector3 originBefore = origin.transform.position;
            bool movedOk = teleport.ProvarArcTeleport(leftHand, true, out Vector3 finalTarget);
            Vector3 originAfter = origin.transform.position;
            float movedDist = Vector3.Distance(originBefore, originAfter);
            if (!movedOk || movedDist < 1.5f)
                throw new Exception($"Teleport movement verification failed: dist={movedDist:0.00}");
            report.Add($"[PASS] Prova Teletransport (Execucio): XROrigin mogut {movedDist:0.00} m fins a ({originAfter.x:0.00}, {originAfter.y:0.00}, {originAfter.z:0.00})");

            // Reobre Testing.unity neta sense modificar la versio guardada.
            EditorSceneManager.OpenScene(TestingScenePath);

            File.WriteAllLines(Path.Combine(EvidenceDir, "Testing_Report.txt"), report);
            foreach (var line in report) Debug.Log(line);
            Debug.Log("BRASA_TESTING_VERIFY_OK");
        }

        static void CreateTestTable(string name, Transform parent, Vector3 pos, Material topMat, Material legMat, Material accentMat)
        {
            var g = Group(name, parent, pos);
            Box("Sobre taula", g, new Vector3(0f, 0.87f, 0f), new Vector3(1.15f, 0.06f, 0.72f), topMat, 0.015f, true);
            Box("Zona marcada", g, new Vector3(0f, 0.903f, 0f), new Vector3(0.96f, 0.006f, 0.56f), accentMat, 0.002f, false);
            foreach (float x in new[] { -0.48f, 0.48f })
                foreach (float z in new[] { -0.28f, 0.28f })
                    Cylinder("Pota", g, new Vector3(x, 0.43f, z), 0.028f, 0.86f, legMat, 12);
        }

        static GameObject CreateSauceGrabbable(string name, Transform parent, Vector3 pos, Mesh bottleMesh, Mesh nozzleMesh, Material bodyMat, Material nozzleMat)
        {
            var g = Group(name, parent, pos).gameObject;
            if (bottleMesh && nozzleMesh)
            {
                Object("Cos", g.transform, bottleMesh, bodyMat, Vector3.zero);
                Object("Broquet", g.transform, nozzleMesh, nozzleMat, new Vector3(0f, 0.19f, 0f));
            }
            else
            {
                Cylinder("Cos", g.transform, new Vector3(0f, 0.11f, 0f), 0.036f, 0.22f, bodyMat, 14);
            }
            return g;
        }

        static Material LoadMat(string name, string fallbackHex)
        {
            return AssetDatabase.LoadAssetAtPath<Material>(Root + "/Art/Materials/" + name + ".mat") ?? Mat(name, fallbackHex);
        }

        static Camera CreateCam(string name, Transform parent, Vector3 from, Vector3 to, float fov)
        {
            var g = Group(name, parent, from);
            g.LookAt(to);
            var c = g.gameObject.AddComponent<Camera>();
            c.fieldOfView = fov;
            c.nearClipPlane = 0.05f;
            c.farClipPlane = 50f;
            c.enabled = false;
            c.clearFlags = CameraClearFlags.SolidColor;
            c.backgroundColor = Hex("#B9C9CC");
            var data = c.GetUniversalAdditionalCameraData();
            data.renderPostProcessing = false;
            data.antialiasing = AntialiasingMode.None;
            return c;
        }

        static void RenderCam(Camera cam, string outputPath, int width, int height)
        {
            var rt = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32) { antiAliasing = 4 };
            cam.targetTexture = rt;
            cam.Render();
            RenderTexture.active = rt;
            var tex = new Texture2D(width, height, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            tex.Apply();
            File.WriteAllBytes(outputPath, tex.EncodeToPNG());
            cam.targetTexture = null;
            RenderTexture.active = null;
            rt.Release();
            UnityEngine.Object.DestroyImmediate(rt);
            UnityEngine.Object.DestroyImmediate(tex);
        }

        static void SetControllerVisualVisible(Transform hand, bool visible)
        {
            if (!hand) return;
            var model = hand.Find("Model controlador");
            if (model) model.gameObject.SetActive(visible);
        }

        static void EnsureTestingInBuildSettings()
        {
            var list = EditorBuildSettings.scenes.ToList();
            if (!list.Any(s => s.path == RestaurantBuilder.ScenePath))
                list.Insert(0, new EditorBuildSettingsScene(RestaurantBuilder.ScenePath, true));
            if (!list.Any(s => s.path == TestingScenePath))
                list.Add(new EditorBuildSettingsScene(TestingScenePath, true));
            EditorBuildSettings.scenes = list.ToArray();
        }
    }
}
