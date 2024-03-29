﻿using Microsoft.Web.WebView2.Core;
using Newtonsoft.Json.Linq;
using Planck.IO;
using Planck.Resources;
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

    public static Task<string> ExecuteScriptAsync(this CoreWebView2 coreWebView2, string filename, Assembly assembly)
    {
      var @namespace = assembly.GetName().Name!;
      return ExecuteScriptAsync(coreWebView2, filename, assembly, @namespace);
    }

    public static async Task<string> ExecuteScriptAsync(
      this CoreWebView2 coreWebView2,
      string filename,
      Assembly assembly,
      string @namespace)
    {
      var script = await ReadEmbeddedScriptAsync($"{@namespace}.{filename}", assembly);
      return await coreWebView2.ExecuteScriptAsync(script);
    }

    public static Task<string> AddScriptToExecuteOnDocumentCreatedAsync(this CoreWebView2 coreWebView2, string filename, Assembly assembly)
    {
      var @namespace = assembly.GetName().Name!;
      return AddScriptToExecuteOnDocumentCreatedAsync(coreWebView2, filename, assembly, @namespace);
    }

    public static async Task<string> AddScriptToExecuteOnDocumentCreatedAsync(
      this CoreWebView2 coreWebView2,
      string filename,
      Assembly assembly,
      string @namespace)
    {
      var script = await ReadEmbeddedScriptAsync($"{@namespace}.{filename}", assembly);
      return await coreWebView2.AddScriptToExecuteOnDocumentCreatedAsync(script);
    }

    public static void PostWebMessage(this CoreWebView2 coreWebView2, string command, object? body)
    {
      var json = JObject.FromObject(new { command, body });
      coreWebView2.PostWebMessageAsJson(json.ToString());
    }

    public static void PostWebMessage(this CoreWebView2 coreWebView2, int operationId, object? body)
    {
      var json = JObject.FromObject(new { operationId, body });
      coreWebView2.PostWebMessageAsJson(json.ToString());
    }

    public static CoreWebView2WebResourceResponse CreateResourceResponse(
      this CoreWebView2 coreWebView2,
      Stream stream) => CreateResourceResponse(coreWebView2, stream, HttpStatusCode.OK);

    public static CoreWebView2WebResourceResponse CreateResourceResponse(
      this CoreWebView2 coreWebView2,
      Stream stream,
      HttpStatusCode statusCode) => CreateResourceResponse(coreWebView2, stream, statusCode, new());

    public static CoreWebView2WebResourceResponse CreateResourceResponse(
      this CoreWebView2 coreWebView2,
      Stream stream,
      HttpStatusCode statusCode,
      Dictionary<string, string> headers)
    {
      var response = coreWebView2.Environment.CreateWebResourceResponse(
        new ManagedStream(stream), (int)statusCode, "OK", $"{_defaultHeaders}{Environment.NewLine}{GetHeadersFromDictionary(headers)}");
      // TODO: change message based on code
      return response;
    }

    static string GetHeadersFromDictionary(Dictionary<string, string> headers) => string.Join(
      Environment.NewLine,
      headers.Select((kvp) => $"{kvp.Key}: {kvp.Value}"));

    static async Task<string> ReadEmbeddedScriptAsync(string path, Assembly assembly)
    {
      var resx = assembly.GetManifestResourceStream(path);
      if (resx == null)
      {
        throw new FileNotFoundException("Could not locate script. If you're calling without the namespace parameter, ensure that the project namespace is the same as the assembly name", path);
      }
      using var streamReader = new StreamReader(resx);
      var script = await streamReader.ReadToEndAsync();
      return script;
    }
  }
}
