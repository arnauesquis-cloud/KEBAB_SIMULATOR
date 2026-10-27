# Connectar Meta Quest 3

## Opció A · Quest Link des de l'ordinador

1. Obre Meta Quest Link al PC i connecta el visor per USB o Air Link. Completa la connexió dins del visor.
2. A la configuració de Meta Quest Link, estableix **Meta Quest Link com a runtime OpenXR actiu**. Durant la preparació d'aquest projecte, el runtime actiu del PC era SteamVR; no s'ha canviat aquesta preferència del sistema.
3. Obre `Assets/Kebab/Scenes/Restaurant.unity` a Unity i prem **Play**.
4. Comprova que el cap mou la càmera i que els dos comandaments apareixen quan estan seguits. La càmera arrenca al passadís d'entrada, a escala real.

El projecte ja té el loader OpenXR activat per a Windows. Quest Link no necessita el mòdul Android. Per moure el punt inicial abans de Play, canvia la posició de `XR Origin · Quest 3 · nomes tracking` a la Hierarchy. No s'han afegit controls de locomoció.

## Opció B · APK autònom al visor

1. A **Unity Hub → Installs → 6000.3.24f1 → Manage / Add modules**, instal·la:
   - **Android Build Support**
   - **Android SDK & NDK Tools**
   - **OpenJDK**
2. Reinicia Unity després d'afegir els mòduls.
3. Activa el mode desenvolupador del visor amb el teu compte de desenvolupador Meta. Connecta les Quest per USB i accepta **Allow USB debugging** dins del visor.
4. Obre **File → Build Profiles** i selecciona **Meta Quest → Switch Platform**. Si aquesta opció no apareix, selecciona **Android**.
5. Comprova **Edit → Project Settings → XR Plug-in Management → OpenXR → Android**. Hi han de ser OpenXR, Meta Quest Support, Oculus Touch Controller Profile i Meta Quest Touch Plus Controller Profile.
6. Executa **BRASA → 03 · Validar escena i XR**. Revisa també **Project Validation** de Unity després de canviar la plataforma.
7. Executa **BRASA → 05 · Crear APK per a Quest 3**. El resultat es desa a `Builds/Quest3/BrasaRestaurant.apk`. Alternativament, fes **Build And Run** des de Build Profiles amb el visor com a Run Device.

El menú de creació d'APK prepara els paràmetres Android, valida l'escena i falla amb un missatge explicatiu si falten els mòduls. No instal·la programes ni canvia configuracions del compte Meta.

## Paràmetres preparats

| Paràmetre | Valor |
| --- | --- |
| Unity | 6000.3.24f1 |
| Render pipeline | URP 17.3.0, Forward |
| OpenXR | 1.19.0 |
| XR Management | 4.6.1 |
| Input System | 1.20.0 |
| Backend Android | IL2CPP |
| Arquitectura | ARM64 |
| Graphics API Android | Vulkan |
| Graphics API Windows | Direct3D 11 |
| Android mínim | API 29 |
| Android objectiu | Més alt instal·lat compatible amb Unity |
| Textures Android | ASTC |
| Stereo | Single Pass Instanced |
| Antialiasing | MSAA 4× |
| HDR / postprocessat | Desactivats |
| Nom del paquet | com.arnauzoran.brasakebab |
| Seguiment | Terra / Floor, HMD i dos comandaments |

No és una configuració de publicació a la botiga. No conté credencials, App ID de Meta ni signatura de producció.

## Validació física pendent

- El restaurant apareix als dos ulls, a la mida esperada.
- L'alçada del terra és correcta després d'establir el límit de l'espai del visor.
- El cap i els comandaments segueixen el moviment.
- El punt d'entrada està lliure de mobiliari.
- La il·luminació es veu correctament i no hi ha materials magenta.
- El rendiment es mesura al visor amb Unity Profiler; no s'afirma cap taxa de fotogrames sense aquesta prova.

Per veure zones llunyanes durant aquesta fase de modelatge, canvia el punt inicial del rig a Unity. El recorregut complet amb locomoció s'ha deixat per a la fase de joc.

## Fonts tècniques

- [Unity 6.3: Meta Quest build platform and build profile](https://docs.unity.com/en-us/engine/6000.3/manual/xr/configuring-project-for/meta-quest-develop/build-profile)
- [Meta: Manual Project Setup](https://developers.meta.com/vr/documentation/unity/unity-tutorial-hello-world-manual-steps/)
- La configuració utilitza les API i els fitxers font dels paquets instal·lats i fixats a `packages-lock.json`.
