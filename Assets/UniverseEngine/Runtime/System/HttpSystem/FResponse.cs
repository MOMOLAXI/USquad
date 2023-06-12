using UnityEngine.Networking;

namespace UniverseEngine
{
    public struct FResponse
    {
        public string Text;
        public byte[] Data;
        public string Message;
        public long StatusCode;
        public UnityWebRequest.Result Result;

        public static FResponse InvalidURL()
        {
            return new()
            {
                Message = "Invalid url, null or empty",
                StatusCode = 400,
                Result = UnityWebRequest.Result.ConnectionError
            };
        }

        public static FResponse Custom(string message)
        {
            return new()
            {
                Message = $"{message}",
                StatusCode = 400,
                Result = UnityWebRequest.Result.ConnectionError
            };
        }
    }

    public static class ResponseExtension
    {
        public static FResponse Success(this UnityWebRequest request)
        {
            return new()
            {
                Text = request?.downloadHandler?.text,
                Data = request?.downloadHandler?.data,
                StatusCode = request?.responseCode ?? 200,
                Result = UnityWebRequest.Result.Success,
                Message = $"Request : [{request.url}] success"
            };
        }

        public static FResponse Error(this UnityWebRequest request)
        {
            return new()
            {
                Message = $"URL : [{request.url}], Message : [{request.error}]",
                StatusCode = request.responseCode,
                Result = UnityWebRequest.Result.ConnectionError,
            };
        }
    }
}
