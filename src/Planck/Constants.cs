using System.Text;

namespace Planck
{
  internal static class Constants
  {
    public const string StartPageContent = """
      <!DOCTYPE html>
      <html>
        <head>
          <script src="https://code.jquery.com/jquery-3.6.3.min.js"></script>
          <style>
            html,
            body {
              margin: 0;
              padding: 0;
            }

            #embedded-content {
              position: absolute;
              top: 0;
              left: 0;
              width: 100vw;
              height: 100vh;
              outline: none;
              border: none;
              margin: 0;
              padding: 0;
            }
          </style>
        </head>
        <body>
          <iframe id="embedded-content"></iframe>
        </body>
      </html>
      """;

    public static readonly string StartPageContentAsBase64 =
      $"data:text/html;charset=utf-8;base64,{Convert.ToBase64String(Encoding.UTF8.GetBytes(StartPageContent))}";
  }
}
