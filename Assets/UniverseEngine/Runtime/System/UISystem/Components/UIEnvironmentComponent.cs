using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace UniverseEngine
{
    public enum ECanvasLayer
    {
        BOTTOM_LAYER = 0,
        AUTO_LAYER = 10000,
        TOP_LAYER = 20000,
    }

    public class UIEnvironmentComponent : SystemComponent
    {
        GameObject m_Root;
        public Camera UICamera;
        public Canvas UICanvas;

        public static Vector2 Resolution = UniverseConstant.DefaultResolution;

        readonly Dictionary<ECanvasLayer, Canvas> m_LayerRoot = new();

        protected override void OnInit()
        {
            //创建根节点
            m_Root = UniverseEngine.CreateGlobalGameObject(nameof(UISystem));

            //Unity输入模块
            UniverseEngine.GetOrAddGlobalComponent("UnityEventSystem", out EventSystem system);
            UniverseEngine.GetOrAddComponent<StandaloneInputModule>(system);
            UniverseEngine.Attach(system, m_Root);

            //UI相机
            UniverseEngine.GetOrAddGlobalComponent("UICamera", out UICamera);
            UniverseEngine.Attach(UICamera, m_Root);
            UICamera.clearFlags = CameraClearFlags.Color;
            UICamera.backgroundColor = Color.black;

            //顶层Canvas
            UniverseEngine.GetOrAddGlobalComponent("RootCanvas", out UICanvas);
            UniverseEngine.Attach(UICanvas, m_Root);
            UICanvas.renderMode = RenderMode.ScreenSpaceCamera;
            UICanvas.worldCamera = UICamera;
            UICanvas.vertexColorAlwaysGammaSpace = true;

            //Canvas缩放
            CanvasScaler scaler = UniverseEngine.GetOrAddComponent<CanvasScaler>(UICanvas);
            scaler.scaleFactor = 1;
            scaler.referencePixelsPerUnit = 100;
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = Resolution;
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0;

            //创建子Canvas层
            CreateRoot(ECanvasLayer.TOP_LAYER);
            CreateRoot(ECanvasLayer.AUTO_LAYER);
            CreateRoot(ECanvasLayer.BOTTOM_LAYER);
        }

        public Transform GetLayer(ECanvasLayer layer)
        {
            if (m_LayerRoot.TryGetValue(layer, out Canvas canvas))
            {
                return canvas.transform;
            }

            return null;
        }

        /// <summary>
        /// 添加到Canvas层
        /// </summary>
        /// <param name="layer"></param>
        /// <param name="transform"></param>
        public void AttachToLayer(ECanvasLayer layer, Transform transform)
        {
            if (transform == null)
            {
                return;
            }

            if (m_LayerRoot.TryGetValue(layer, out Canvas canvas))
            {
                transform.SetParent(canvas.transform);
            }
        }

        /// <summary>
        /// 添加到Canvas层
        /// </summary>
        /// <param name="layer"></param>
        /// <param name="rectTransform"></param>
        public void AttachToLayer(ECanvasLayer layer, RectTransform rectTransform)
        {
            if (rectTransform == null)
            {
                return;
            }

            if (m_LayerRoot.TryGetValue(layer, out Canvas canvas))
            {
                rectTransform.SetParent(canvas.transform);
                rectTransform.StretchHorizontalAndVerticle();
            }
        }

        public static void AttachTo(RectTransform rectTransform, Transform target)
        {
            rectTransform.SetParent(target);
            rectTransform.StretchHorizontalAndVerticle();
        }

        void CreateRoot(ECanvasLayer layer)
        {
            if (m_LayerRoot.TryGetValue(layer, out Canvas canvas))
            {
                Object.Destroy(canvas.gameObject);
            }

            int order = (int)layer;

            //创建Canvas
            GameObject root = UniverseEngine.CreateGlobalGameObject($"{layer.ToString()}");
            root.transform.SetParent(UICanvas.transform);
            canvas = UniverseEngine.GetOrAddComponent<Canvas>(root);
            canvas.pixelPerfect = false;
            canvas.overrideSorting = true;
            canvas.sortingOrder = order;
            canvas.additionalShaderChannels = AdditionalCanvasShaderChannels.None;

            //对齐方式
            RectTransform rect = canvas.GetComponent<RectTransform>();
            rect.StretchHorizontalAndVerticle();

            //输入检测
            GraphicRaycaster raycaster = UniverseEngine.GetOrAddComponent<GraphicRaycaster>(root);
            raycaster.ignoreReversedGraphics = true;
            raycaster.blockingObjects = GraphicRaycaster.BlockingObjects.None;
            raycaster.blockingMask = -1;

            m_LayerRoot[layer] = canvas;
        }
    }
}
