using ScreenTemperature;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

MyLogger mylogger = new MyLogger();
mylogger.SetConsole();
var logger = mylogger.Logger;

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
    logger.Info("Image captured, saving");
    bmp.Save("test.png", ImageFormat.Png);
    return bmp;
}

string proc = "mspaint";
logger.Info($"Getting proccess by name: \"{proc}\"");

var processess = Process.GetProcessesByName(proc);
logger.Debug($"Found {processess.Length}");
if (processess.Length == 0)
{
    logger.Error("Process not found");
    return;
}

var image = CaptureApplication(proc);

logger.Debug("Triming image");
image = image.Clone(new Rectangle(100, 250, 100, 50), image.PixelFormat);
image.Save("test2.png");


/*
 * 1. Получаем картинку окна
 * 2. Обрезаем нужный фрагмент
 * 3. На фрагменте найти текст
 * 4. Сбор данных в заббикс. Либо вручную отправлять в заббикс, либо агентом собирать нужный файл
 */


