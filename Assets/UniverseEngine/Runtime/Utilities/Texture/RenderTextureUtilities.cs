using UnityEngine;

namespace UniverseEngine
{
    public static class RenderTextureUtilities
    {
        public static bool EnsureRenderTarget(ref RenderTexture rt, 
                                              int width,
                                              int height,
                                              RenderTextureFormat format, 
                                              FilterMode filterMode,
                                              string name,
                                              int depthBits = 0,
                                              int antiAliasing = 1)
        {
            if (rt == null)
            {
                rt = RenderTexture.GetTemporary(width, height, depthBits, format, RenderTextureReadWrite.Default, antiAliasing);
                rt.name = name;
                rt.filterMode = filterMode;
                rt.wrapMode = TextureWrapMode.Repeat;
                return true;
            }
            
            bool exist = rt != null;
            bool sizeDontMatch = rt.width != width;
            bool heightDontMatch = rt.height != height;
            bool formatDontMatch = rt.format != format;
            bool modeDontMatch = rt.filterMode != filterMode;
            bool aliasingDontMatch = rt.antiAliasing != antiAliasing;
            
            if (exist && (sizeDontMatch || heightDontMatch || formatDontMatch || modeDontMatch || aliasingDontMatch))
            {
                RenderTexture.ReleaseTemporary(rt);
                rt = null;
                rt = RenderTexture.GetTemporary(width, height, depthBits, format, RenderTextureReadWrite.Default, antiAliasing);
                rt.name = name;
                rt.filterMode = filterMode;
                rt.wrapMode = TextureWrapMode.Repeat;
                
                //new target
                return true; 
            }
            
            return false; 
        }
    }
}