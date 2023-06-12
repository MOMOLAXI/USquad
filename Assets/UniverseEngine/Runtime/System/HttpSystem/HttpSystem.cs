using System;
using System.Collections.Generic;
using UnityEngine.Networking;

namespace UniverseEngine
{
    public class HttpSystem : EngineSystem
    {
        /// <summary>
        /// 自定义的证书认证实例
        /// </summary>
        public static CertificateHandler CertificateHandlerInstance;

        /// <summary>
        /// 启用断点续传功能文件的最小字节数
        /// </summary>
        public static int BreakpointResumeFileSize { set; get; } = int.MaxValue;

        /// <summary>
        /// 下载失败后清理文件的HTTP错误码
        /// </summary>
        static readonly Dictionary<long, Action<long, string>> s_FileDownloadErrorEvent = new();

        public static void RegisterDownloadFailedEvent(long errorCode, Action<long, string> callback)
        {
            if (s_FileDownloadErrorEvent.ContainsKey(errorCode))
            {
                Log.Error($"Already contains bundle download error event code : {errorCode}");
                return;
            }

            s_FileDownloadErrorEvent[errorCode] = callback;
        }

        public static bool InvokeDownloadError(long code, string message)
        {
            if (s_FileDownloadErrorEvent.TryGetValue(code, out Action<long, string> callback))
            {
                callback?.Invoke(code, message);
                return true;
            }

            return false;
        }

        public static async UniTask<byte[]> RequestBytes(string url, int timeout = 0)
        {
            FResponse response = await InternalRequestBinaries(url, timeout);
            if (response.Result == UnityWebRequest.Result.Success)
            {
                Log<HttpSystem>.Info(response.Message);
                return response.Data;
            }

            Log<HttpSystem>.Error($"Request bytes failed... Status Code : {response.StatusCode.ToString()}, {response.Message}");
            return null;
        }

        public static async UniTask<string> RequestText(string url, int timeout = 0)
        {
            FResponse response = await InternalRequestText(url, timeout);
            if (response.Result == UnityWebRequest.Result.Success)
            {
                Log<HttpSystem>.Info(response.Message);
                return response.Text;
            }

            Log<HttpSystem>.Error($"Request text failed...Status Code : {response.StatusCode.ToString()}, {response.Message}");
            return string.Empty;
        }

        public static async UniTask<string> DownloadFileText(string url, string savePath, int timeout = 0)
        {
            FResponse response = await InternalDownloadFile(url, savePath, timeout);
            if (response.Result == UnityWebRequest.Result.Success)
            {
                Log<HttpSystem>.Info(response.Message);
                return response.Text;
            }

            Log<HttpSystem>.Error($"Download file failed... Status Code : {response.StatusCode.ToString()}, {response.Message}");
            return string.Empty;
        }
        
        public static async UniTask<byte[]> DownloadFileBytes(string url, string savePath, int timeout = 0)
        {
            FResponse response = await InternalDownloadFile(url, savePath, timeout);
            if (response.Result == UnityWebRequest.Result.Success)
            {
                Log<HttpSystem>.Info(response.Message);
                return response.Data;
            }

            Log<HttpSystem>.Error($"Download file failed...Status Code : {response.StatusCode.ToString()}, {response.Message}");
            return null;
        }

        static async UniTask<FResponse> InternalRequestBinaries(string url, int timeout = 0)
        {
            if (string.IsNullOrEmpty(url))
            {
                return FResponse.InvalidURL();
            }

            UnityWebRequest response = null;
            try
            {
                DownloadHandlerBuffer handler = new();
                response = await StartHttpRequest(url, UnityWebRequest.kHttpVerbGET, handler, timeout);
            }
            catch (Exception e)
            {
                Log<HttpSystem>.Exception(e);
                Log<HttpSystem>.Error($"URL : [{url}] response error");
                return response.Error();
            }

            FResponse result = response.result == UnityWebRequest.Result.Success ? response.Success() : response.Error();
            response.Dispose();
            return result;
        }

        static async UniTask<FResponse> InternalRequestText(string url, int timeout = 0)
        {
            if (string.IsNullOrEmpty(url))
            {
                return FResponse.InvalidURL();
            }

            UnityWebRequest response = null;
            try
            {
                DownloadHandlerBuffer handler = new();
                response = await StartHttpRequest(url, UnityWebRequest.kHttpVerbGET, handler, timeout);
            }
            catch (Exception e)
            {
                Log<HttpSystem>.Exception(e);
                Log<HttpSystem>.Error($"URL : [{url}] response error");
                return response.Error();
            }

            FResponse result = response.result == UnityWebRequest.Result.Success ? response.Success() : response.Error();
            response.Dispose();
            return result;
        }

        public static async UniTask<FResponse> InternalDownloadFile(string url, string savePath, int timeout = 0)
        {
            if (string.IsNullOrEmpty(savePath))
            {
                return FResponse.Custom("File Save path is invalid (null or empty)");
            }

            if (string.IsNullOrEmpty(url))
            {
                return FResponse.InvalidURL();
            }

            UnityWebRequest response = null;
            try
            {
                DownloadHandlerFile handler = new(savePath);
                handler.removeFileOnAbort = true;
                response = await StartHttpRequest(url, UnityWebRequest.kHttpVerbGET, handler, timeout);
            }
            catch (Exception e)
            {
                Log<HttpSystem>.Error($"URL : [{url}] response error");
                Log<HttpSystem>.Exception(e);
                return response.Error();
            }

            FResponse result = response.result == UnityWebRequest.Result.Success ? response.Success() : response.Error();
            response.Dispose();
            return result;
        }

        static async UniTask<UnityWebRequest> StartHttpRequest(string url,
                                                               string method,
                                                               DownloadHandler downloadHandler,
                                                               int timeout = 0)
        {
            UnityWebRequest request = new(url, method);
            request.downloadHandler = downloadHandler;
            request.disposeDownloadHandlerOnDispose = true;
            request.timeout = timeout;
            await request.SendWebRequest();
            return request;
        }
    }
}
