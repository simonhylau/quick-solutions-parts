using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CefSharp.Handler;
using CefSharp;
using System.IO;
using System.Collections.Concurrent;

namespace HyWebSpider.Lib.CefBrowser
{
    public class RequestHandler : DefaultRequestHandler
    {
        public string CachePath { get; set; }
        public ConcurrentDictionary<string, string> fileDict { get; set; } = new ConcurrentDictionary<string, string>();
        private Dictionary<ulong, MemoryStreamResponseFilter> responseDictionary = new Dictionary<ulong, MemoryStreamResponseFilter>();
        public override IResponseFilter GetResourceResponseFilter(IWebBrowser browserControl, IBrowser browser, IFrame frame, IRequest request, IResponse response)
        {
            switch (response.MimeType)
            {
                case "text/html":
                case "image/png":
                case "image/gif":
                case "image/jpg":
                case "image/jpeg":
                case "image/webp":
                    break;
                default:
                    return base.GetResourceResponseFilter( browserControl,  browser,  frame,  request,  response);
            }
            var dataFilter = new MemoryStreamResponseFilter();
            
            responseDictionary.Add(request.Identifier, dataFilter);
            return dataFilter;
        }

        public override CefReturnValue OnBeforeResourceLoad(IWebBrowser browserControl, IBrowser browser, IFrame frame, IRequest request, IRequestCallback callback)
        {
            request.Headers["User - Agent"] = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/66.0.3359.139 Safari/537.36";
            //if (!request.Url.Contains("wnacg")) return CefReturnValue.Cancel;
            if (request.Url.Contains("juicyads") || request.Url.Contains("adxadserv") || request.Url.Contains("google-analytics") || request.Url.Contains("tyrantdb")) return CefReturnValue.Cancel;
            return base.OnBeforeResourceLoad(browserControl, browser, frame, request, callback);
        }

        public override void OnResourceLoadComplete(IWebBrowser browserControl, IBrowser browser, IFrame frame, IRequest request, IResponse response, UrlRequestStatus status, long receivedContentLength)
        {
            
            MemoryStreamResponseFilter filter;
            if (responseDictionary.TryGetValue(request.Identifier, out filter))
            {
                string tempFile = Path.GetTempPath() + "\\" + Guid.NewGuid() + ".tmp";
                while (File.Exists(tempFile))
                {
                    tempFile = Path.GetTempPath() + "\\" + Guid.NewGuid() + ".tmp";
                }
                FileInfo fileInfo = new FileInfo(tempFile);
                string filename = "";
                string extension = "";
                var data = filter.Data; //This returns a byte[]
                switch (response.MimeType)
                {
                    case "text/html":
                        filename = "main";
                        filename += ".html";
                        break;
                    case "image/png":
                    case "image/gif":
                    case "image/jpg":
                    case "image/jpeg":
                    case "image/webp":
                        string memeType = (response.MimeType == "image/jpeg" || response.MimeType == "image/webp") ? "image/jpg" : response.MimeType;
                        extension = "." + memeType.Substring(memeType.LastIndexOf("/")+1);

                        filename = request.Url.Substring(request.Url.LastIndexOf("/")+1);
                        if (filename.LastIndexOf(extension) > 0)
                        {
                            filename = filename.Substring(0, filename.LastIndexOf(extension));
                        }
                        if (!filename.EndsWith(extension))
                        {
                            filename += extension;
                        }
                        
                        break;
                    case "text/css":
                        break;
                    case "image/svg+xml":
                        break;
                    case "application/javascript":
                        break;
                    case "application/x-javascript":
                        break;
                    case "text/javascript":
                        break;
                    case "application/json":
                        break;
                    case "font/ttf":
                        break;
                    default:
                        break;
                }
                if (filename != "")
                {
                    try
                    {
                        if (File.Exists(CachePath + "\\" + filename))
                        {
                            File.Delete(CachePath + "\\" + filename);
                        }

                        File.WriteAllBytes(CachePath + "\\" + filename, data);
                    }
                    catch (Exception)
                    {
                        filename = fileInfo.Name + filename.Substring(filename.Length - (filename.Length - filename.LastIndexOf(".")));
                        File.WriteAllBytes(CachePath + "\\" + filename, data);
                    }
                    if (!fileDict.ContainsKey(request.Url))
                        fileDict.TryAdd(request.Url, filename);
                }
            }
        }
    }
}
