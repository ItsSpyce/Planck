# Resource Packaging

## How resource fetching works

All resources that match the URL https://planck.virtual/ will be handled by `CoreWebView2.WebResourceRequested` and transformed via:

```cs
CoreWebView2.WebResourceRequested += async (_, args) =>
{
  var resourceName = UrlUtilities.RemovePlanckDomain(args.Request.Uri);
  args.Response.Content = await _resourceService.GetResourceAsync(resourceName);
};
```

**RESOURCE PACKING SHOULD ONLY OCCUR WHEN BUILDING FOR RELEASE.**

## Packaged resources

Planck will package all resources into a compressed directory that will be distributed with the final binary. These will be fetched at runtime via ZIP stream redirecting.

## Embedded resources

Planck will embed all resources for the front-end inside of the application. The return stream is from the native resource fetching.

### Process steps

1. App: `planck-build [DIRECTORY] --embedded` (`DIRECTORY` should be the public directory)
2. Planck reads the project file from `DIRECTORY` and clones
3. Planck adds the following XML to the project
```xml
<ItemGroup>
  <!-- foreach FILE in DIRECTORY -->
  <None Remove="$(FILE)" />
  <!-- end foreach -->
</ItemGroup>
<ItemGroup>
  <!-- foreach FILE in DIRECTORY -->
  <EmbeddedResource Include="$(FILE)" />
  <!-- end foreach -->
</ItemGroup>
```
4: Planck calls `dotnet build $(TempProjectFile)`