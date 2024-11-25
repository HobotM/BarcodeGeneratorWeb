using Microsoft.AspNetCore.Mvc;
using ZXing;
using ZXing.Common;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text.RegularExpressions;

namespace BarcodeGeneratorWeb.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        return View();
    }

    private string ValidateInput(string text, BarcodeFormat format)
    {
        switch (format)
        {
            case BarcodeFormat.EAN_13:
                if (!Regex.IsMatch(text, @"^\d{13}$"))
                    return "EAN-13 format requires exactly 13 digits.";
                break;
            case BarcodeFormat.UPC_A:
                if (!Regex.IsMatch(text, @"^\d{12}$"))
                    return "UPC-A format requires exactly 12 digits.";
                break;
            case BarcodeFormat.QR_CODE:
                // QR Codes can accept any input, but you might want to set a maximum length
                if (text.Length > 2953)
                    return "QR Code format supports up to 2953 characters.";
                break;
            case BarcodeFormat.CODE_39:
                if (!Regex.IsMatch(text, @"^[A-Z0-9\-\.\ \$\/\+\%]*$"))
                    return "Code 39 format supports uppercase letters, digits, and these characters: - . $ / + % space.";
                break;
            // Add more cases for other barcode formats as needed
            default:
                // For formats that don't require specific validation
                break;
        }

        return null; // Input is valid
    }


    [HttpPost]
    public IActionResult GenerateBarcode(BarcodeViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View("Index", model);
        }

        BarcodeFormat barcodeFormat;
        try
        {
            barcodeFormat = (BarcodeFormat)Enum.Parse(typeof(BarcodeFormat), model.Format);
        }
        catch
        {
            ModelState.AddModelError("Format", "Invalid barcode format selected.");
            return View("Index", model);
        }

        //Validator
         string validationError = ValidateInput(model.Text, barcodeFormat);
    if (validationError != null)
    {
        ModelState.AddModelError("Text", validationError);
        return View("Index", model);
    }
        try
        {
            var barcodeWriter = new BarcodeWriterPixelData
            {
                Format = barcodeFormat,
                Options = new EncodingOptions
                {
                    Height = 150,
                    Width = 300,
                    Margin = 10
                }
            };




            var pixelData = barcodeWriter.Write(model.Text);
            using (var bitmap = new Bitmap(pixelData.Width, pixelData.Height, PixelFormat.Format32bppRgb))
            {
                var bitmapData = bitmap.LockBits(
                    new Rectangle(0, 0, pixelData.Width, pixelData.Height),
                    ImageLockMode.WriteOnly,
                    bitmap.PixelFormat);

                try
                {
                    System.Runtime.InteropServices.Marshal.Copy(
                        pixelData.Pixels,
                        0,
                        bitmapData.Scan0,
                        pixelData.Pixels.Length);
                }
                finally
                {
                    bitmap.UnlockBits(bitmapData);
                }

                using (var stream = new MemoryStream())
                {
                    bitmap.Save(stream, ImageFormat.Png);
                    stream.Seek(0, SeekOrigin.Begin);
                    return File(stream.ToArray(), "image/png", "barcode.png");
                }
            }
        }
        catch (Exception ex)
        {
            ViewBag.Error = $"An error occurred while generating the barcode: {ex.Message}";
            return View("Index");
        }
        return View("Index");

    }


}
