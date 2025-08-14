using System;
using System.IO;
using System.Net;

public static class DownloadHelper
{
    /// <summary>
    /// Download a file. Optionally connect to a specific IP (override DNS) while keeping the original Host header.
    /// SSL certificate errors are ignored for the duration of the call (testing only).
    /// </summary>
    /// <param name="url">Original URL (e.g., https://www.google.com/...)</param>
    /// <param name="outputPath">Local path to save to.</param>
    /// <param name="overrideIp">Optional IP to connect to instead of DNS (e.g., "127.0.0.1").</param>
    public static void DownloadFile(string url, string outputPath, string overrideIp = null)
    {
        if (url == null) throw new ArgumentNullException("url");
        if (outputPath == null) throw new ArgumentNullException("outputPath");

        var uri = new Uri(url);
        var originalHost = uri.Host;

        // Build the actual request URL (swap host to IP if override is provided)
        string requestUrl = url;
        if (!string.IsNullOrEmpty(overrideIp))
        {
            var ub = new UriBuilder(uri);
            ub.Host = overrideIp;
            requestUrl = ub.Uri.ToString();
        }

        // Save current global settings, then (temporarily) disable TLS validation
        var previousValidator = ServicePointManager.ServerCertificateValidationCallback;
        var previousProtocols = ServicePointManager.SecurityProtocol;

        ServicePointManager.ServerCertificateValidationCallback =
            (sender, certificate, chain, errors) => true; // ⚠️ testing only

        // Ensure modern TLS on older frameworks
        ServicePointManager.SecurityProtocol =
            SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;

        try
        {
            var req = (HttpWebRequest)WebRequest.Create(requestUrl);
            req.Method = "GET";
            req.AllowAutoRedirect = true;
            req.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

            // If we changed the URL host to an IP, preserve the original Host header
            if (!string.IsNullOrEmpty(overrideIp))
            {
                req.Host = originalHost;
            }

            using (var resp = (HttpWebResponse)req.GetResponse())
            using (var respStream = resp.GetResponseStream())
            using (var fs = new FileStream(outputPath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                respStream.CopyTo(fs);
            }
        }
        finally
        {
            // Restore global settings
            ServicePointManager.ServerCertificateValidationCallback = previousValidator;
            ServicePointManager.SecurityProtocol = previousProtocols;
        }
    }
}
