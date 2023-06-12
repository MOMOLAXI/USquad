using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace UniverseEngine
{
    public struct WidgetRegister
    {
        public int ID;
        public string ResourceKey;
        public ECanvasLayer Layer;
    }

    public class UISystem : EngineSystem
    {
        public static string UIPackageName = string.Empty;

        public static Canvas UICanvas => GetComponent<UIEnvironmentComponent>().UICanvas;
        public static Camera UICamera => GetComponent<UIEnvironmentComponent>().UICamera;

        public static Vector2 Resolution
        {
            get => UIEnvironmentComponent.Resolution;
            set => UIEnvironmentComponent.Resolution = value;
        }

        public override void OnInit()
        {

        }

        public static void Register(WidgetRegister register)
        {
            UIRegistryComponent registry = GetComponent<UIRegistryComponent>();
            UIMeta meta = UIMeta.Create(register.ID, register.ResourceKey, register.Layer);
            registry.Register(meta);
        }

        public static bool IsOpened(int id)
        {
            UIRegistryComponent registry = GetComponent<UIRegistryComponent>();
            UIMeta meta = registry.GetUIMeta(id);
            if (meta == null)
            {
                return false;
            }

            UIWidgetCollectionComponent collection = GetComponent<UIWidgetCollectionComponent>();
            return collection.FindInRunning(meta) != null;
        }

        public static int OpenBuiltInWidget(string location,
                                            ECanvasLayer targetLayer,
                                            Transform root = null,
                                            Run<UIWidget> onFinishOpen = null,
                                            Args args = default(Args))
        {
            if (string.IsNullOrEmpty(location))
            {
                Log.Error("BuiltIn widget resource location is invalid");
                return int.MinValue;
            }

            GameObject go = Resources.Load<GameObject>(location);
            if (go == null)
            {
                Log.Error($"BuiltIn widget : {location} not exist in resource folder");
                return int.MinValue;
            }

            UIWidget widget = Object.Instantiate(go).GetComponent<UIWidget>();
            if (widget == null)
            {
                Log.Error($"BuiltIn widget must attach component {nameof(UIWidget)}");
                return int.MinValue;
            }

            int uiid = widget.GetHashCode();
            //没有meta信息，注册进去
            UIRegistryComponent registry = GetComponent<UIRegistryComponent>();
            UIMeta meta = registry.GetUIMeta(uiid);
            if (meta == null)
            {
                meta = UIMeta.Create(uiid, string.Empty, targetLayer);
                registry.Register(meta);
            }

            //没有打开过进行创建流程
            WidgetInstance instance = FindInstance(meta);
            if (instance == null)
            {
                instance = WidgetInstance.Create(meta);
                instance.Widget = widget;
                OnWidgetCreate(instance, meta, root);
            }

            //Open
            OnWidgetOpen(instance, onFinishOpen, args);
            return uiid;
        }

        public static void OpenWidget(int typeID, Run<UIWidget> onFinishOpen = null, Args args = default(Args))
        {
            InternalOpenWidget(typeID, null, onFinishOpen, args);
        }

        public static void OpenSubWidget(int typeID,
                                         Transform attachRoot,
                                         Run<UISubWidget> onFinishOpen,
                                         Args args = default(Args))
        {
            InternalOpenWidget(typeID, attachRoot, onFinishOpen, args);
        }

        public static void Close(int typeID, bool destroy = false, Run onClose = null)
        {
            UIMeta meta = GetMeta(typeID);
            if (meta == null)
            {
                Log.Error($"UIWidget : Type id : [{typeID}] did not register in the UISystem");
                return;
            }

            WidgetInstance instance = FindInstance(meta);
            if (instance == null)
            {
                return;
            }

            instance.IsActive = false;
            UIWidgetCollectionComponent collection = GetComponent<UIWidgetCollectionComponent>();
            UIElement[] elements = instance.Widget.GetComponentsInChildren<UIElement>();
            foreach (UIElement element in elements)
            {
                WidgetInstance subInstance = FindInstance(element.Meta);
                if (subInstance == null)
                {
                    Log.Error($"UIWidget : Type id : [{typeID.ToString()}] did not open yet");
                    return;
                }

                Function.Run(element.OnCloseDelegate);
                collection.RemoveFromRunning(subInstance);
                if (destroy)
                {
                    Function.Run(subInstance.Widget.OnDisposeDelegate);
                    Object.Destroy(subInstance.Widget.gameObject);
                }
                else
                {
                    collection.AddToCache(subInstance);
                }
            }

            Function.Run(onClose);
        }

        public static void CloseAll(bool destroy = false)
        {
            UIWidgetCollectionComponent collection = GetComponent<UIWidgetCollectionComponent>();
            using (Collections.AllocAutoList(out List<int> runningWdigets))
            {
                collection.GetRunningWidgets(runningWdigets);
                for (int i = 0; i < runningWdigets.Count; i++)
                {
                    int wdiget = runningWdigets[i];
                    Close(wdiget, destroy);
                }
                Log.Info($"Close All UIWidgets, destroy ? {destroy.ToString()}");
            }
        }

        public static void AttachToLayer(Transform transform, ECanvasLayer layer)
        {
            GetComponent<UIEnvironmentComponent>().AttachToLayer(layer, transform);
        }

        public Transform GetLayer(ECanvasLayer layer)
        {
            return GetComponent<UIEnvironmentComponent>().GetLayer(layer);
        }

        public override void OnRegisterComponents()
        {
            RegisterComponent<UIRegistryComponent>();
            RegisterComponent<UIEnvironmentComponent>();
            RegisterComponent<UIWidgetCollectionComponent>();
        }

        static void InternalOpenWidget<T>(int typeID,
                                          Transform attachRoot,
                                          Run<T> onFinishOpen,
                                          Args args = default(Args)) where T : UIElement
        {
            if (string.IsNullOrEmpty(UIPackageName))
            {
                Log.Error("UIPackageName was not set, please set UIPackageName before open");
                return;
            }

            //Get Meta
            UIMeta meta = GetMeta(typeID);
            if (meta == null)
            {
                Log.Error($"UISubWidget : Type id : [{typeID.ToString()}] did not register in the UISystem");
                return;
            }

            //Get Or Create WidgetInstance
            WidgetInstance instance = FindInstance(meta);

            //没有显示也没有缓存则重新创建
            instance ??= WidgetInstance.Create(meta);

            //Open With Load
            if (instance.Widget == null)
            {
                LoadAndOpen(meta, instance, attachRoot, onFinishOpen, args).Forget();
            }

            //Open Directly
            else
            {
                OnWidgetOpen(instance, onFinishOpen, args);
            }
        }

        static UIMeta GetMeta(int typeID)
        {
            UIRegistryComponent registry = GetComponent<UIRegistryComponent>();
            return registry.GetUIMeta(typeID);
        }

        static WidgetInstance FindInstance(UIMeta meta)
        {
            UIWidgetCollectionComponent collection = GetComponent<UIWidgetCollectionComponent>();
            WidgetInstance instance = collection.FindInRunning(meta) ?? collection.FindInCache(meta);
            return instance;
        }

        async static UniTaskVoid LoadAndOpen<T>(UIMeta meta,
                                                WidgetInstance instance,
                                                Transform parent,
                                                Run<T> onFinishOpen = null,
                                                Args args = default(Args)) where T : UIElement
        {
            // using UProgress<float> progress = UProgress<float>.Create($"Load Widget {meta.ResourceKey}", s_ResourceProgress);
            GameObject gameObject = null;
            // GameObject gameObject = await Engine.InstantiateAsync(UIPackageName, meta.ResourceKey, progress);
            if (gameObject == null)
            {
                Log.Error($"Invalid UIWidget Instantiation : {meta.ResourceKey}");
                return;
            }

            //Set Layer
            gameObject.transform.SetLayerRecursively(LayerMask.NameToLayer("UI"));

            //Check Component
            T widget = gameObject.GetComponent<T>();
            if (widget == null)
            {
                Log.Error($"Widget [{meta.ResourceKey}] must attach Component {typeof(T).Name}");
                return;
            }

            //Set Widget
            instance.Widget = widget;

            //Create
            OnWidgetCreate(instance, meta, parent);

            //Open
            OnWidgetOpen(instance, onFinishOpen, args);
        }

        static void OnWidgetCreate(WidgetInstance instance, UIMeta meta, Transform parent)
        {
            //Invoke OnCreate
            UIElement widget = instance.Widget;
            widget.Meta = meta;
            UIElement[] elements = widget.GetComponentsInChildren<UIElement>(true);
            foreach (UIElement element in elements)
            {
                Function.Run(element.OnCreateDelegate);
            }

            if (parent != null)
            {
                //Attach to parent
                UIEnvironmentComponent.AttachTo(widget.Rect, parent);
            }
            else
            {
                //Attach to layer
                UIEnvironmentComponent env = GetComponent<UIEnvironmentComponent>();
                env.AttachToLayer(meta.CanvasLayer, widget.Rect);
            }
        }

        static void OnWidgetOpen<T>(WidgetInstance instance, Run<T> onFinishOpen, Args args = default(Args)) where T : UIElement
        {
            //Show
            T widget = instance.Widget as T;
            widget.gameObject.SetActive(true);
            widget.transform.SetAsLastSibling();

            //Invoke Open
            UIElement[] elements = widget.GetComponentsInChildren<UIElement>(true);
            foreach (UIElement element in elements)
            {
                Function.Run(element.OnOpenDelegate, args);
            }

            //Invoke callback
            Function.Run(onFinishOpen, widget);

            //Add to running
            UIWidgetCollectionComponent collection = GetComponent<UIWidgetCollectionComponent>();
            collection.AddToRunning(instance);

            //Remove from cache
            collection.RemoveFromCache(instance);
        }
    }
}
