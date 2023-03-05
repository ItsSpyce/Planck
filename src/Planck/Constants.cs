using System.Text;

namespace Planck
{
  internal static class Constants
  {
    public const string StartPageContent = """
      <!DOCTYPE html>
      <html>
      </html>
      """;

    public static readonly string StartPageContentAsBase64 =
      $"data:text/html;charset=utf-8;base64,{Convert.ToBase64String(Encoding.UTF8.GetBytes(StartPageContent))}";
  }
}
