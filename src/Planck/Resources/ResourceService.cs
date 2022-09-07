using Microsoft.Web.WebView2.Core;
using Planck.Controls;
using Planck.Controls.Wpf;
using Planck.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace Planck.Resources
{
  public interface IResourceService
  {
    Stream? GetResource(string name);
    void ConnectToPlanck(IPlanckWindow planck, string? root);
  }

  internal class PackagedResourceService : IResourceService, IDisposable
  {
    private const string _planckPackageExtension = ".ppx";

    private ZipArchive? _packageArchive;
    private bool _disposed;

    public void ConnectToPlanck(IPlanckWindow planck, string? root)
    {
      if (string.IsNullOrEmpty(root))
      {
        throw new ArgumentNullException(nameof(root));
      }
      if (planck.CoreWebView2 == null)
      {
        throw new ArgumentNullException("CoreWebView2 is not initialized", nameof(planck.CoreWebView2));
      }
      var fileInfo = new FileInfo(root);
      if ((fileInfo.Attributes & FileAttributes.Directory) != 0)
      {
        //planck.CoreWebView2.SetVirtualHostNameToFolderMapping(
        //  Constants.PlanckUrl,
        //  root,
        //  CoreWebView2HostResourceAccessKind.DenyCors);
      }
      else
      {
        if (fileInfo.Extension != _planckPackageExtension)
        {
          throw new ArgumentException("Invalid package file", nameof(root));
        }
        _packageArchive = ZipFile.OpenRead(root);

        //planck.CoreWebView2.WebResourceRequested += (_, args) =>
        //{
        //  if (_disposed)
        //  {
        //    return;
        //  }
        //  if (!UrlUtilities.IsPlanckUrl(args.Request.Uri))
        //  {
        //    return;
        //  }
        //  var resourceName = UrlUtilities.RemovePlanckDomain(args.Request.Uri);
        //  var packagedResource = GetResource(resourceName);
        //  if (packagedResource == null)
        //  {
        //    return;
        //  }
        //  args.Response.Content = packagedResource;
        //};
      }
    }

    public void Dispose()
    {
      if (_disposed)
      {
        return;
      }
      _disposed = true;
      _packageArchive?.Dispose();
    }

    public Stream? GetResource(string name)
    {
      var entry = _packageArchive?.GetEntry(name);
      if (entry == null)
      {
        return null;
      }
      return entry.Open();
    }
  }

  /// <summary>
  ///   Informs Planck that resources will be embedded inside of the final application
  /// </summary>
  /// <remarks>
  ///   Process goes:
  ///   1. app calls "planck-build [DIRECTORY] --embedded"
  ///   2. planck creates a clone of the csproj that adds two lines:
  ///     - 
  /// </remarks>
  internal class EmbeddedResourceService : IResourceService
  {
    public void ConnectToPlanck(IPlanckWindow planck, string? root)
    {
      throw new NotImplementedException();
    }

    public Stream? GetResource(string name)
    {
      throw new NotImplementedException();
    }
  }
}
