using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;
using System.CommandLine;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

public class Program
{
    private static string logFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "error.log");

    [STAThread]
    static void Main(string[] args)
    {
        // Configuración de la aplicación Windows Forms
        Application.SetHighDpiMode(HighDpiMode.SystemAware);
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);

        // Definición de comandos y opciones
        var comandoPrincipal = new RootCommand("Convertidor de HTML a PDF");

        var opcionArchivoEntrada = new Option<string>(
            name: "--entrada",
            description: "Ruta del archivo HTML de entrada")
        { IsRequired = true };

        var opcionArchivoSalida = new Option<string>(
            name: "--salida",
            description: "Ruta del archivo PDF de salida")
        { IsRequired = false }; 

        comandoPrincipal.AddOption(opcionArchivoEntrada);
        comandoPrincipal.AddOption(opcionArchivoSalida);

        // Manejo del comando principal
        comandoPrincipal.SetHandler((string rutaEntrada, string rutaSalida) =>
        {
            if (string.IsNullOrWhiteSpace(rutaSalida))
            {
                rutaSalida = Path.ChangeExtension(rutaEntrada, ".pdf");
            }
            ConvertirHtmlAPdf(rutaEntrada, rutaSalida);
        }, opcionArchivoEntrada, opcionArchivoSalida);

        comandoPrincipal.Invoke(args);
    }

    static void ConvertirHtmlAPdf(string rutaEntrada, string rutaSalida)
    {
        // Configuración del formulario invisible
        var formulario = new Form
        {
            Width = 1200,
            Height = 800,
            FormBorderStyle = FormBorderStyle.FixedToolWindow,
            ShowInTaskbar = false,
            StartPosition = FormStartPosition.Manual,
            Location = new System.Drawing.Point(-2000, -2000) // Ubicación fuera de la pantalla
        };

        formulario.Load += async (sender, e) =>
        {
            try
            {
                await DoConversion(formulario, rutaEntrada, rutaSalida);
            }
            catch (Exception ex)
            {
                LogError(ex);
            }
            finally
            {
                formulario.Close();
            }
        };

        Application.Run(formulario);
    }

    static async Task DoConversion(Form formulario, string rutaEntrada, string rutaSalida)
    {
        // Verificación del archivo de entrada
        if (!File.Exists(rutaEntrada))
        {
            throw new FileNotFoundException("No se encontró el archivo HTML de entrada", rutaEntrada);
        }

        var contenidoHtml = await File.ReadAllTextAsync(rutaEntrada);

        if (string.IsNullOrWhiteSpace(contenidoHtml))
        {
            throw new ArgumentException("El contenido HTML está vacío o no es válido.");
        }

        // Configuración del control WebView2
        var navegadorWeb = new WebView2
        {
            Dock = DockStyle.Fill
        };
        formulario.Controls.Add(navegadorWeb);

        try
        {
            string runtimePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "WebView2");
            string userDataFolder = Path.Combine(Path.GetTempPath(), "WebView2Cache");

            Directory.CreateDirectory(userDataFolder);

            var webView2Environment = await CoreWebView2Environment.CreateAsync(
                browserExecutableFolder: Directory.Exists(runtimePath) ? runtimePath : null,
                userDataFolder: userDataFolder);

            await navegadorWeb.EnsureCoreWebView2Async(webView2Environment);
        }
        catch (Exception ex)
        {
            throw new Exception("No se pudo encontrar o inicializar WebView2 Runtime.", ex);
        }

        // Navegación al contenido HTML
        navegadorWeb.NavigateToString(contenidoHtml);

        var tareaNavegacion = new TaskCompletionSource<bool>();
        navegadorWeb.NavigationCompleted += (s, e) =>
        {
            tareaNavegacion.SetResult(true);
        };
        await tareaNavegacion.Task;

        // Espera adicional para asegurar que el contenido esté completamente cargado
        await Task.Delay(1000);

        try
        {
            var rutaCompletaSalida = Path.GetFullPath(rutaSalida);
            var directorioSalida = Path.GetDirectoryName(rutaCompletaSalida);

            if (string.IsNullOrEmpty(directorioSalida))
            {
                throw new ArgumentException("No se pudo determinar el directorio de salida.");
            }

            if (!Directory.Exists(directorioSalida))
            {
                Directory.CreateDirectory(directorioSalida);
            }

            if (Path.GetExtension(rutaCompletaSalida).ToLower() != ".pdf")
            {
                throw new ArgumentException("El archivo de salida debe tener la extensión .pdf.");
            }

            var printSettings = navegadorWeb.CoreWebView2.Environment.CreatePrintSettings();
            printSettings.ShouldPrintBackgrounds = true;
            printSettings.ShouldPrintSelectionOnly = false;
            printSettings.ShouldPrintHeaderAndFooter = false;

            var success = await navegadorWeb.CoreWebView2.PrintToPdfAsync(rutaCompletaSalida, printSettings);

            if (!success)
            {
                throw new Exception("La generación del PDF falló sin error específico.");
            }
        }
        catch (Exception ex)
        {
            throw new Exception("Error al generar el PDF.", ex);
        }
    }

    static void LogError(Exception ex)
    {
        var message = $"{DateTime.Now}: {ex.Message}\n{ex.StackTrace}\n";
        if (ex.InnerException != null)
        {
            message += $"Causa interna: {ex.InnerException.Message}\n{ex.InnerException.StackTrace}\n";
        }

        File.AppendAllText(logFilePath, message);
    }
}
