using UnityEditor;
using UnityEngine;
using static Brasa.Editor.RestaurantGeometry;

namespace Brasa.Editor
{
    public static partial class RestaurantBuilder
    {
        static void BuildDining()
        {
            var dining=Group("02 · Menjador · 24 places",environment);
            var table=MakeTable();var chair=MakeChair();var pendant=MakePendant();
            for(int row=0;row<3;row++)for(int col=0;col<2;col++)
            {
                float x=-4.5f+col*2.65f,z=-3.25f+row*2.35f;
                var set=Group("Taula "+(row*2+col+1)+" · 4 places",dining,new(x,0,z));
                Instance(table,set,Vector3.zero);
                foreach(float zz in new[]{-.72f,.72f})foreach(float xx in new[]{-.32f,.32f})Instance(chair,set,new(xx,0,zz),zz<0?0:180);
                Instance(pendant,set,new(0,2.56f,0));
                TableProps(set);
            }
            Sign("Cartell foc lent",dining,new(-5.91f,2.12f,-.1f),new(.91f,1.36f),"Poster",wood,-90);
            Sign("Cartell menjador",dining,new(5.91f,2.12f,-1.75f),new(.91f,1.36f),"Poster",wood,90);
            Plant(dining,new(-5.35f,0,-4.6f),1);Plant(dining,new(5.25f,0,-4.5f),2);
        }
        static GameObject MakeChair()
        {
            var g=Group("Cadira · mostassa i acer");
            foreach(float x in new[]{-.205f,.205f})foreach(float z in new[]{-.20f,.20f})
            {
                Rod("Pota acer",g,new(x*1.13f,.035f,z*1.13f),new(x,.47f,z),.014f,black);
                Cylinder("Protector goma",g,new(x*1.13f,.018f,z*1.13f),.018f,.03f,black,12);
            }
            Box("Seient arrodonit",g,new(0,.465f,0),new(.48f,.055f,.46f),gold,.024f);
            var back=Box("Respatller",g,new(0,.71f,-.213f),new(.47f,.43f,.048f),gold,.022f);back.transform.localEulerAngles=new(-8,0,0);
            Rod("Travesser",g,new(-.2f,.27f,-.19f),new(.2f,.27f,-.19f),.009f,black);
            var collider=g.gameObject.AddComponent<BoxCollider>();collider.center=new(0,.46f,0);collider.size=new(.49f,.92f,.51f);
            return Prefab(g,"MustardChair");
        }
        static GameObject MakeTable()
        {
            var g=Group("Taula · roure 120 x 75");
            Box("Sobre de roure",g,new(0,.755f,0),new(1.2f,.055f,.75f),wood,.022f);
            Box("Cantell negre",g,new(0,.718f,0),new(1.14f,.025f,.69f),black,.011f);
            Cylinder("Peu central",g,new(0,.36f,0),.047f,.7f,black);
            Box("Base estable",g,new(0,.027f,0),new(.73f,.05f,.49f),black,.02f);
            var c=g.gameObject.AddComponent<BoxCollider>();c.center=new(0,.4f,0);c.size=new(1.2f,.8f,.75f);
            return Prefab(g,"OakTable");
        }
        static GameObject MakePendant()
        {
            var g=Group("Penjant · esmalt vermell");
            Lathe("Pantalla",g,Vector3.zero,new[]{new Vector2(.27f,0),new Vector2(.28f,.022f),new Vector2(.25f,.045f),new Vector2(.19f,.13f),new Vector2(.09f,.21f),new Vector2(.035f,.23f)},red,32,false,"PendantShade");
            Cylinder("Difusor",g,new(0,.018f,0),.244f,.008f,glow,32);
            Cylinder("Cable",g,new(0,.5f,0),.007f,.55f,black,8);Cylinder("Rosassa",g,new(0,.81f,0),.065f,.03f,black);
            return Prefab(g,"RedPendant");
        }
        static void TableProps(Transform t)
        {
            Box("Safata",t,new(.05f,.796f,.0f),new(.41f,.02f,.28f),red,.018f);
            Box("Tovallo",t,new(.04f,.809f,.0f),new(.32f,.003f,.22f),white,0);
            var pita=Lathe("Pa pita",t,new(.05f,.835f,0),new[]{new Vector2(.09f,-.015f),new Vector2(.125f,.01f),new Vector2(.11f,.045f),new Vector2(.065f,.065f)},bread,20,true,"Pita");pita.transform.localScale=new(1,.8f,.7f);
            for(int i=0;i<5;i++)
            {
                var l=Lathe("Fulla amanida",t,new(-.04f+i*.04f,.886f,.03f),new[]{new Vector2(.025f,0),new Vector2(.035f,.012f),new Vector2(.009f,.026f)},leaf,8,true,"LettucePiece");l.transform.localEulerAngles=new(0,i*65,15);
            }
            Cylinder("Tomàquet",t,new(.05f,.892f,0),.038f,.012f,tomato,16);
            Box("Tovallonera",t,new(-.41f,.85f,.18f),new(.13f,.14f,.09f),steel,.008f);
            Box("Tovallons",t,new(-.41f,.921f,.18f),new(.105f,.018f,.065f),white,.002f);
            Sauce(t,new(.40f,.79f,.17f),white,.65f);Sauce(t,new(.5f,.79f,.17f),red,.65f);
        }
        static void Sauce(Transform p,Vector3 at,Material mat,float scale=1)
        {
            var g=Group("Ampolla salsa",p,at);g.localScale=Vector3.one*scale;
            Lathe("Ampolla",g,Vector3.zero,new[]{new Vector2(.035f,0),new Vector2(.038f,.025f),new Vector2(.036f,.16f),new Vector2(.025f,.19f)},mat,16,true,"SauceBottle");
            Lathe("Broquet",g,new(0,.19f,0),new[]{new Vector2(.024f,0),new Vector2(.022f,.016f),new Vector2(.005f,.065f)},white,12,true,"SauceNozzle");
        }
        static void Plant(Transform parent,Vector3 at,int seed)
        {
            var p=Group("Planta en test",parent,at);
            Lathe("Test ceràmic",p,Vector3.zero,new[]{new Vector2(.19f,.02f),new Vector2(.25f,.44f),new Vector2(.265f,.45f),new Vector2(.265f,.49f),new Vector2(.24f,.49f)},green,24,true,"PlantPot");
            Cylinder("Terra",p,new(0,.465f,0),.235f,.01f,meat,20);
            for(int i=0;i<10;i++)
            {
                float a=i*137.5f*Mathf.Deg2Rad;var end=new Vector3(Mathf.Cos(a)*.32f,.85f+(i%3)*.16f,Mathf.Sin(a)*.32f);
                Rod("Tija",p,new(0,.46f,0),end,.008f,green);
                var l=Lathe("Fulla",p,end,new[]{new Vector2(0,-.18f),new Vector2(.07f,-.07f),new Vector2(.08f,.05f),new Vector2(0,.2f)},leaf,8,true,"PlantLeaf");l.transform.localScale=new(1,1,.25f);l.transform.localRotation=Quaternion.Euler(28,i*137.5f,35);
            }
        }
        static void BuildKitchen()
        {
            var k=Group("03 · Cuina i taulell",environment);
            // 4.8 m service counter; staff passage stays open on its left.
            var counter=Group("Taulell · comanda i recollida",k,new(3.25f,0,1.45f));
            Box("Cos vermell",counter,new(0,.52f,0),new(4.8f,.97f,.88f),red,.035f,true);
            Box("Socol",counter,new(0,.07f,0),new(4.7f,.14f,.82f),black,.015f);
            Box("Sobre pedra",counter,new(0,1.04f,0),new(4.98f,.08f,1.04f),black,.025f,true);
            Box("Franja llautó",counter,new(0,.95f,-.452f),new(4.69f,.022f,.012f),gold,.003f);
            for(int i=0;i<32;i++)Box("Llistó frontal",counter,new(-2.29f+i*.147f,.51f,-.455f),new(.025f,.77f,.015f),red,.004f);
            Sign("Marca frontal taulell",counter,new(0,.56f,-.48f),new(2.15f,.6f),"Brand",gold);
            Register(counter,new(-1.65f,1.09f,0));
            Sign("Indicador comanda",counter,new(-1.68f,1.34f,-.43f),new(.75f,.19f),"Order",black);
            Sign("Indicador recollida",counter,new(1.6f,1.34f,-.43f),new(.75f,.19f),"Pickup",black);
            for(int i=0;i<5;i++)Sauce(counter,new(.92f+i*.14f,1.09f,.12f),i%2==0?white:red);
            for(int i=0;i<5;i++)Box("Safates apilades",counter,new(1.88f,1.1f+i*.018f,0),new(.43f,.016f,.31f),red,.015f);
            // Separate return doubles as a preparation surface; no objects in the walking lane.
            Workbench(k,new(5.35f,0,3.15f),new(.95f,.9f,2.1f),"Preparacio lateral");
            Workbench(k,new(2.6f,0,4.55f),new(4.6f,.88f,1.0f),"Linia calenta");
            var spit=MakeSpit();Instance(spit,k,new(1.18f,.93f,4.58f));Instance(spit,k,new(2.43f,.93f,4.58f));
            // Distinguish the second meat cone with a lighter chicken material.
            var spits=k.GetComponentsInChildren<MeshRenderer>();bool first=true;
            foreach(var r in spits)if(r.name=="Carn laminada / doner"){if(!first)r.sharedMaterial=chicken;first=false;}
            Hood(k,new(2.3f,2.68f,4.55f));
            Fryer(k,new(3.92f,.94f,4.53f));
            for(int i=0;i<6;i++)IngredientTray(k,new(5.34f,1.0f,2.38f+i*.30f),i);
            Box("Taula tall",k,new(5.26f,.972f,3.93f),new(.62f,.035f,.38f),white,.01f);
            Box("Ganivet · fulla",k,new(5.2f,1.0f,3.92f),new(.20f,.012f,.038f),chrome,.004f);
            Box("Ganivet · mànec",k,new(5.37f,1.0f,3.92f),new(.13f,.026f,.04f),black,.009f);
            Sink(k,new(-.05f,0,4.65f));
            Fridge(k,new(4.95f,0,-.15f));
            Sign("Carta gran",k,new(2.78f,2.96f,2.88f),new(4.8f,.75f),"Menu",black);
            // Menu suspended from the ceiling with two slim ties.
            foreach(float x in new[]{.78f,4.78f})Rod("Suport carta",k,new(x,3.31f,2.90f),new(x,3.43f,2.90f),.017f,black);
        }
        static void Workbench(Transform p,Vector3 at,Vector3 size,string name)
        {
            var g=Group(name,p,at);
            Box("Sobre inoxidable",g,new(0,size.y,0),new(size.x,.065f,size.z),steel,.018f,true);
            Box("Prestatge baix",g,new(0,.23f,0),new(size.x-.1f,.04f,size.z-.1f),steel,.009f);
            foreach(float x in new[]{-(size.x/2-.09f),size.x/2-.09f})foreach(float z in new[]{-(size.z/2-.09f),size.z/2-.09f})Cylinder("Pota",g,new(x,size.y*.5f,z),.029f,size.y,steel);
            Box("Peto",g,new(0,size.y+.11f,size.z*.5f-.015f),new(size.x,.18f,.035f),steel,.005f);
        }
        static GameObject MakeSpit()
        {
            var s=Group("Rostidor vertical · doner");
            Box("Base",s,new(0,.045f,0),new(.89f,.09f,.69f),steel,.025f);
            Box("Panell posterior",s,new(0,.78f,.25f),new(.86f,1.5f,.11f),steel,.035f);
            Box("Cambra resistencies",s,new(0,.83f,.183f),new(.64f,1.24f,.045f),black,.005f);
            for(int i=0;i<4;i++)
            {
                Box("Resistencia radiant",s,new(0,.37f+i*.29f,.151f),new(.56f,.23f,.027f),heat,.008f);
                for(int j=0;j<5;j++)Rod("Proteccio",s,new(-.23f+j*.115f,.25f+i*.29f,.12f),new(-.23f+j*.115f,.48f+i*.29f,.12f),.004f,steel);
            }
            Cylinder("Eix acer",s,new(0,.80f,-.12f),.013f,1.48f,chrome,12);
            Doner(s,new(0,.20f,-.12f),meat,42);
            Cylinder("Plat recollida",s,new(0,.13f,-.12f),.34f,.035f,steel,32);
            Box("Motor superior",s,new(0,1.57f,.12f),new(.23f,.15f,.26f),black,.025f);
            Rod("Brac superior",s,new(0,1.53f,-.12f),new(0,1.53f,.23f),.025f,steel);
            foreach(float y in new[]{.45f,.82f,1.18f}){var dial=Cylinder("Control temperatura",s,new(.375f,y,.17f),.035f,.035f,black);dial.transform.localEulerAngles=new(90,0,0);}
            Box("Banda vermella",s,new(0,.05f,-.352f),new(.78f,.05f,.015f),red,.005f);
            return Prefab(s,"DonerRotisserie");
        }
        static void Hood(Transform p,Vector3 at)
        {
            var h=Group("Campana extractora",p,at);
            Box("Campana",h,Vector3.zero,new(3.64f,.36f,1.17f),steel,.045f);
            Box("Filtre inferior",h,new(0,-.19f,0),new(3.37f,.035f,.94f),black,.005f);
            for(int i=0;i<28;i++)Box("Lames filtre",h,new(-1.58f+i*.117f,-.212f,0),new(.038f,.018f,.81f),steel,.003f);
            Box("Conducte",h,new(0,.44f,.23f),new(.83f,.59f,.55f),steel,.025f);
            foreach(float x in new[]{-1.45f,1.45f})Box("Llum campana",h,new(x,-.213f,-.39f),new(.15f,.014f,.11f),glow,.01f);
        }
        static void Fryer(Transform p,Vector3 at)
        {
            var f=Group("Fregidora doble",p,at);
            Box("Cos",f,new(0,.17f,0),new(.9f,.34f,.65f),steel,.025f);
            Box("Oli",f,new(0,.345f,0),new(.76f,.009f,.47f),black,0);
            for(int b=0;b<2;b++)
            {
                float x=-.21f+b*.42f;
                Box("Cistell",f,new(x,.355f,0),new(.32f,.018f,.40f),gold,.005f);
                for(int j=0;j<7;j++)Rod("Malla cistell",f,new(x-.15f+j*.05f,.37f,-.2f),new(x-.15f+j*.05f,.37f,.2f),.003f,steel);
                Rod("Mànec cistell",f,new(x,.37f,-.19f),new(x,.47f,-.47f),.012f,steel);
                Box("Empunyadura",f,new(x,.47f,-.49f),new(.09f,.04f,.16f),black,.015f);
            }
            Box("Panell control",f,new(0,.30f,.27f),new(.9f,.35f,.12f),steel,.014f);
            foreach(float x in new[]{-.23f,.23f})Cylinder("Dial",f,new(x,.38f,.198f),.034f,.03f,black).transform.localEulerAngles=new(90,0,0);
        }
        static void IngredientTray(Transform p,Vector3 at,int index)
        {
            var t=Group("Cubeta ingredient "+index,p,at);
            Box("Cubeta",t,Vector3.zero,new(.64f,.11f,.26f),steel,.016f);
            Box("Interior",t,new(0,.057f,0),new(.55f,.008f,.19f),index%3==0?leaf:index%3==1?tomato:white,.01f);
            for(int j=0;j<6;j++)
            {
                var bit=Box("Ingredient",t,new(-.22f+j*.085f,.07f,Mathf.Sin(j*3.1f)*.04f),new(.06f,.028f,.08f),index%3==0?leaf:index%3==1?tomato:white,.012f);bit.transform.localEulerAngles=new(0,j*29,8);
            }
        }
        static void Register(Transform p,Vector3 at)
        {
            var r=Group("TPV decoratiu",p,at);
            Box("Peu",r,new(0,.025f,0),new(.36f,.05f,.28f),black,.018f);
            Box("Columna",r,new(0,.17f,.03f),new(.06f,.3f,.07f),steel,.015f);
            var monitor=Sign("Pantalla",r,new(0,.32f,0),new(.37f,.28f),"Register",black);monitor.transform.localEulerAngles=new(-12,0,0);
            Box("Impressora tiquets",r,new(.36f,.075f,0),new(.17f,.15f,.21f),black,.02f);
            Box("Paper",r,new(.36f,.15f,-.05f),new(.09f,.01f,.11f),white,0);
        }
        static void Sink(Transform p,Vector3 at)
        {
            var s=Group("Rentat · pica inoxidable",p,at);
            Workbench(s,Vector3.zero,new(1.05f,.90f,.82f),"Moble pica");
            Box("Cubeta fosca",s,new(0,.94f,0),new(.7f,.015f,.48f),black,.045f);
            foreach(float x in new[]{-.37f,.37f})Box("Vora pica",s,new(x,.952f,0),new(.035f,.035f,.55f),chrome,.01f);
            foreach(float z in new[]{-.255f,.255f})Box("Vora pica",s,new(0,.952f,z),new(.77f,.035f,.035f),chrome,.01f);
            Rod("Aixeta vertical",s,new(.22f,.95f,.29f),new(.22f,1.29f,.29f),.018f,chrome);
            Rod("Aixeta bec",s,new(.22f,1.29f,.29f),new(.22f,1.29f,.04f),.018f,chrome);
            Rod("Sortida aixeta",s,new(.22f,1.29f,.04f),new(.22f,1.23f,.04f),.018f,chrome);
            Sauce(s,new(-.35f,.94f,.27f),green,.7f);
        }
        static void Fridge(Transform p,Vector3 at)
        {
            var f=Group("Nevera expositora",p,at);
            Box("Cos exterior",f,new(0,1.05f,0),new(1.05f,2.1f,.75f),black,.025f,true);
            Box("Interior il·luminat",f,new(0,1.12f,-.387f),new(.92f,1.62f,.035f),white,.01f);
            foreach(float x in new[]{-.46f,.46f})Box("Marc porta",f,new(x,1.12f,-.54f),new(.045f,1.68f,.09f),steel,.009f);
            foreach(float y in new[]{.29f,1.94f})Box("Marc horitzontal",f,new(0,y,-.54f),new(.94f,.045f,.09f),steel,.009f);
            for(int row=0;row<4;row++)
            {
                float y=.42f+row*.36f;Box("Prestatge",f,new(0,y,-.47f),new(.88f,.018f,.18f),steel,.002f);
                for(int i=0;i<7;i++)
                {
                    var b=Group("Ampolla beguda",f,new(-.36f+i*.12f,y+.015f,-.46f));
                    Lathe("Ampolla",b,Vector3.zero,new[]{new Vector2(.038f,0),new Vector2(.04f,.14f),new Vector2(.019f,.19f),new Vector2(.019f,.24f)},i%3==0?green:i%3==1?red:gold,12,true,"DrinkBottle");
                    Cylinder("Etiqueta",b,new(0,.09f,0),.041f,.065f,white,12);Cylinder("Tap",b,new(0,.245f,0),.021f,.02f,black,12);
                }
            }
            Rod("Tirador",f,new(.33f,.85f,-.62f),new(.33f,1.38f,-.62f),.018f,chrome);
            Sign("Franja nevera",f,new(0,2.02f,-.394f),new(.95f,.20f),"Fridge",red);
        }
        static void BuildDetails()
        {
            var details=Group("04 · Detalls i instal·lacions",environment);
            // Wall shelf, crockery, bread and utility details provide scale without interaction logic.
            Box("Prestatge fons",details,new(-2.0f,1.75f,5.0f),new(2.4f,.045f,.38f),steel,.008f);
            for(int i=0;i<3;i++)for(int j=0;j<5;j++)Cylinder("Plat apilat",details,new(-2.8f+i*.73f,1.79f+j*.018f,4.96f),.19f,.016f,white,24);
            for(int i=0;i<4;i++)Box("Caixa magatzem",details,new(-2.8f+(i%2)*1.05f,.33f+(i/2)*.58f,4.6f),new(.82f,.55f,.6f),wood,.012f,true);
            var bin=Group("Paperera reciclatge",details,new(.1f,0,2.65f));
            Box("Cos",bin,new(0,.38f,0),new(.42f,.74f,.42f),black,.03f,true);Box("Tapa",bin,new(0,.77f,0),new(.45f,.06f,.45f),steel,.02f);
            Box("Obertura",bin,new(0,.805f,0),new(.25f,.008f,.18f),black,.01f);
            var extinguisher=Group("Extintor",details,new(5.82f,.65f,-3.65f));
            Cylinder("Ampolla vermella",extinguisher,new(0,.25f,0),.09f,.5f,red);Cylinder("Coll",extinguisher,new(0,.52f,0),.035f,.08f,black);
            Rod("Maneta",extinguisher,new(-.06f,.58f,0),new(.08f,.58f,0),.015f,black);
            Rod("Manega",extinguisher,new(.06f,.54f,0),new(.13f,.1f,0),.012f,black);
            // A nonfunctional WC bowl behind the closed door, modelled only as environment.
            var wc=Group("Sanitari",details,new(-5.3f,0,4.8f));
            Lathe("Peu sanitari",wc,Vector3.zero,new[]{new Vector2(.19f,0),new Vector2(.17f,.24f),new Vector2(.26f,.4f),new Vector2(.27f,.44f)},white,24,true,"ToiletBowl");
            Box("Cisterna",wc,new(0,.62f,.22f),new(.47f,.4f,.17f),white,.05f);
        }
        static GameObject Prefab(Transform g,string name)
        {
            var result=PrefabUtility.SaveAsPrefabAsset(g.gameObject,Root+"/Prefabs/"+name+".prefab");UnityEngine.Object.DestroyImmediate(g.gameObject);return result;
        }
        static GameObject Instance(GameObject prefab,Transform parent,Vector3 position,float yaw=0)
        {
            var g=(GameObject)PrefabUtility.InstantiatePrefab(prefab,parent);g.transform.localPosition=position;g.transform.localEulerAngles=new(0,yaw,0);return g;
        }
    }
}
