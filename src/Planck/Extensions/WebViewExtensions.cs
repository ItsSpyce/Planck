using Microsoft.Web.WebView2.Core;
using Planck.Commands;
using Planck.Resources;
using Planck.Utilities;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.Json;

namespace Planck.Extensions
{
  public static class WebViewExtensions
  {
    static readonly JsonSerializerOptions _serializerOptions = new()
    {
      PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    const string _defaultHeaders = $"""
      Origin: {IResourceService.AppUrl}
      """;

    public static void PostWebMessage<T>(this CoreWebView2 coreWebView2, T command) where T : struct
    {
      var commandAttr = command.GetType().GetCustomAttribute<CommandAttribute>();
      if (commandAttr == null)
      {
        throw new InvalidCommandException(typeof(T));
      }
      var message = new { command = commandAttr.Name, body = command };
      var asJsonBytes = JsonSerializer.SerializeToUtf8Bytes(message, _serializerOptions);
      var asJsonStr = Encoding.UTF8.GetString(asJsonBytes);
      coreWebView2.PostWebMessageAsJson(asJsonStr);
    }

    public static CoreWebView2WebResourceResponse CreateResourceResponse(this CoreWebView2 coreWebView2, Stream stream, HttpStatusCode code = HttpStatusCode.OK)
    {
      var response = coreWebView2.Environment.CreateWebResourceResponse(
        new ManagedStream(stream), (int)code, "OK", _defaultHeaders);
      // TODO: change message based on code
      return response;
    }
  }
}
