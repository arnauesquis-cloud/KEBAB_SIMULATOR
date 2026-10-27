# BRASA · Kebab Restaurant VR

Projecte d'entorn 3D per a **Unity 6000.3.24f1**, amb **URP** i configuració **OpenXR per a Meta Quest 3**. Models originals creats per a Arnau i Zoran. Aquesta entrega és només el local: no conté clients, comandes, cuina interactiva, puntuació ni locomoció.

Ubicació de treball principal: **C:\Users\Arnau\Desktop\KebabRestaurantVR**. A partir d'ara, treballa sempre amb aquesta carpeta de l'escriptori. Les captures es desen a `Documentation/Previews`, dins del projecte.

## Obrir el restaurant

1. A Unity Hub, tria **Projects → Add → Add project from disk** i selecciona aquesta carpeta `KebabRestaurantVR`.
2. Obre'l amb **6000.3.24f1** i deixa acabar la importació.
3. Obre **Assets/Kebab/Scenes/Restaurant.unity**.
4. A la vista **Scene** pots recórrer el local amb els controls habituals de Unity (botó dret i WASD). **Play** activa la càmera i el seguiment XR; sense visor, mostra la vista des de l'entrada.

També pots executar `Obrir-Unity.ps1`, situat al costat d'aquest README.

## L'espai

- Planta interior de **12 × 10,5 m = 126 m²**; sostre a **3,4 m**. Una unitat de Unity equival a un metre.
- **6 taules i 24 cadires**, terra ceràmic, arrambador blau i làmpades vermelles.
- Taulell de **4,8 m**, TPV decoratiu, comanda i recollida senyalitzades.
- **Dos rostidors de döner** amb carn modelada per capes, resistències, espasa i safata.
- Fregidora doble, campana extractora, preparació amb sis cubetes, pica i nevera expositora.
- Zona de WC i magatzem, aparadors, entrada, cartelleria en català i mobiliari de servei.
- Colliders a l'arquitectura i al mobiliari principal. Les portes del servei estan tancades i són estàtiques.

La referència visual és [Kebab Restaurant Interior, de 3E 3D Everything](https://assetstore.unity.com/packages/3d/props/interior/kebab-restaurant-interior-118369). S'han pres com a orientació la paleta, les rajoles i el tipus de mobiliari. **Aquest projecte no inclou ni necessita comprar aquell asset**: els models, els patrons i els rètols inclosos són originals. No s'ha importat contingut comercial de la botiga.

## Quest 3

Configuració guardada per a Android i Windows/Quest Link: OpenXR, perfil Oculus Touch i Meta Quest Touch Plus, XR Origin amb alçada de terra, seguiment del visor i els dos comandaments. El model de cada comandament s'amaga quan no hi ha seguiment vàlid. No hi ha teletransport, gir artificial ni interaccions.

**El mòdul Android no està instal·lat en aquest ordinador.** Cal afegir-lo a Unity Hub abans de crear un APK. La instal·lació automàtica va ser bloquejada per la revisió de permisos (`blocked by policy`). El projecte i els paràmetres d'Android sí que estan preparats.

La guia pas a pas és a **Documentation/Quest3.md**. Les comprovacions efectuades amb Unity es guarden a **Documentation/Validation.txt** i **Documentation/XR-Validation.txt**. La prova física al visor i la mesura del rendiment queden pendents de disposar de les Quest connectades.

## Editar i reutilitzar

| Carpeta | Contingut |
| --- | --- |
| `Assets/Kebab/Scenes` | Escena del restaurant i il·luminació generada |
| `Assets/Kebab/Prefabs` | Taula, cadira, penjant, rostidor i rig Quest |
| `Assets/Kebab/Art/Meshes` | Malles editables i serialitzades; no es generen durant el joc |
| `Assets/Kebab/Art/Materials` | Materials URP |
| `Assets/Kebab/Art/Textures` | Patrons i gràfiques originals |
| `Assets/Kebab/Editor` | Eines de construcció, captures, configuració i validació |
| `Assets/Kebab/Scripts` | Visibilitat dels comandaments segons seguiment |
| `Assets/Kebab/Settings` | URP, XR i il·luminació |

Els objectes estan agrupats a la Hierarchy en arquitectura, menjador, cuina, detalls i il·luminació. El sostre es pot ocultar per editar la planta. Les càmeres `01_Interior` a `05_Planta` són punts de vista de revisió i estan desactivades durant l'execució.

El menú **BRASA** ofereix configuració XR, validació, exportació de captures i construcció d'APK. **Regenerar restaurant original substitueix l'escena original**: desa les teves modificacions en una escena diferent abans d'utilitzar-lo. Obrir el projecte no regenera ni modifica l'escena.

Després de moure parets, llums o mobiliari estàtic, recalcula la llum des de **Window → Rendering → Lighting → Generate Lighting**. S'utilitzen llums interiors precalculades, una llum direccional, materials compartits i MSAA 4×, sense postprocessat.

## Compartir el projecte

Comparteix `Assets`, `Packages`, `ProjectSettings`, `Documentation`, aquest README i el `.gitignore`. La carpeta `Library` és una memòria cau local i no cal compartir-la. `Packages/packages-lock.json` fixa les dependències resoltes.
