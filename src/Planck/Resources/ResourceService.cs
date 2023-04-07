using Microsoft.Extensions.Options;
using Planck.Configuration;
using Planck.Controls;
using System.IO;

namespace Planck.Resources
{
  public interface IResourceService
  {
    public const string AppUrl = "http://app.planck/";

    Stream? GetResource(string name);
    void ConnectToPlanck(IPlanckWindow planck, string? root);
  }

  internal abstract class InternalResourceService : IResourceService
  {
    protected readonly PlanckConfiguration _configuration;

    private string _stdlib = "";

    protected InternalResourceService(PlanckConfiguration configuration)
    {
      _configuration = configuration;
    }

    protected void InitializeStartup(IPlanckWindow planck, string? root)
    {
      if (planck.CoreWebView2 == null)
      {
        throw new ArgumentNullException("CoreWebView2 is not initialized", nameof(planck.CoreWebView2));
      }
      if (string.IsNullOrEmpty(_stdlib))
      {
        // load stdlib
        var asm = GetType().Assembly;
        var resourceName = $"{asm.GetName().Name}.Scripts.core.js";
        using var streamReader = new StreamReader(asm.GetManifestResourceStream(resourceName)!);
        _stdlib = streamReader.ReadToEnd();
      }

      planck.CoreWebView2.NavigationCompleted += (_, args) =>
      {
        if (!planck.HasCompletedBootstrap)
        {
          planck.HasCompletedBootstrap = true;
        }
      };
      planck.CoreWebView2.AddScriptToExecuteOnDocumentCreatedAsync(_stdlib);
    }

    public abstract void ConnectToPlanck(IPlanckWindow planck, string? root);
    public abstract Stream? GetResource(string name);
  }
}
