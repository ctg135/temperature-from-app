using ScreenTemperature;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using Tesseract;
using System.Text.RegularExpressions;
using NLog;

var logger = LogManager.GetCurrentClassLogger();

// Function for capturing application image
Bitmap CaptureApplication(string procName)
{
    var proc = Process.GetProcessesByName(procName)[0];
    var rect = new User32.Rect();
    User32.GetWindowRect(proc.MainWindowHandle, ref rect);

    int width = rect.right - rect.left;
    int height = rect.bottom - rect.top;

    var bmp = new Bitmap(width, height, PixelFormat.Format32bppArgb);
    using (Graphics graphics = Graphics.FromImage(bmp))
    {
        graphics.CopyFromScreen(rect.left, rect.top, 0, 0, new Size(width, height), CopyPixelOperation.SourceCopy);
    }
    return bmp;
}

logger.Info("Program started!");
logger.Info("initialization data...");

string proc = "HD-player";
string imgFile = "temp_image.png";
Rectangle trim = new Rectangle() { X = 100, Y = 150, Width = 250, Height = 100 };
Regex findTempReg = new Regex(@"-?[0-4]?\d\.\d"); // -?[0-4]?\d\.\d  -?\d{1,2}\.\d
string zabbixServer = "10.100.1.8";
string zabbixHostname = "\"Server_room_temperature_26k_402\"";
string zabbixKey = "temperature_c";

logger.Debug($"Process to find: {proc}");
logger.Debug($"Tempfile for image: {imgFile}");
logger.Debug($"Trim image bounds: {trim.ToString()}");
logger.Debug($"Regexp to find temperature in text: \"{findTempReg}\"");
logger.Debug($"Zabbix server - {zabbixServer}");
logger.Debug($"Zabbix host - {zabbixHostname}");
logger.Debug($"Zabbix key - {zabbixKey}");

try
{
    while (true)
    {
        logger.Info($"Getting proccess by name: \"{proc}\"");
        // Getting processess list
        var processess = Process.GetProcessesByName(proc);
        logger.Debug($"Found {processess.Length}");
        if (processess.Length == 0)
        {
            logger.Error("Process not found");
            await Task.Delay(60000);
            continue;
        }
        // taking image
        var image = CaptureApplication(proc);

        logger.Info("Image captured");
        logger.Debug("Triming image");
        // triming image
        image = image.Clone(trim, image.PixelFormat);
        image.Save(imgFile);
        // search text on image
        var engine = new TesseractEngine(@"./tessdata", "eng", EngineMode.Default);
        string res = engine.Process(Pix.LoadFromFile(imgFile)).GetText();

        logger.Debug($"Detected text on image: {res.Replace('\n', ' ')}");

        var matches = findTempReg.Matches(res);

        logger.Debug($"Matched {matches.Count} regexp entries");

        if (matches.Count == 0)
        {
            logger.Error("Text not found");
            await Task.Delay(60000);
            continue;
        }
        res = matches[0].Value;
        // send data to zabbix server
        logger.Info($"Detected text: {res}");
        logger.Debug("Sending data to Zabbix");
        Process.Start(@"C:\Program Files\Zabbix Agent\zabbix_sender", $" -z {zabbixServer} -s {zabbixHostname} -k {zabbixKey} -o {res}");
        logger.Debug("Waiting one minute...");

        await Task.Delay(60000);
    }
}
catch (Exception e)
{
    logger.Fatal($"Exception: {e.Message}");
    logger.Fatal($"Source: {e.Source}");
    logger.Fatal($"Stack trace: {e.StackTrace}");
}