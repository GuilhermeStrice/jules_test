using QRCoder;
using System;
using System.Drawing;
using System.IO;

namespace SAFT.Pdf
{
    public static class QrCodeHelper
    {
        /// <summary>
        /// Generates a QR code PNG image as a byte array for the given payload.
        /// </summary>
        public static byte[] GenerateQrCodePng(string payload, int pixelsPerModule = 5)
        {
            using var qrGenerator = new QRCodeGenerator();
            using var qrData = qrGenerator.CreateQrCode(payload, QRCodeGenerator.ECCLevel.Q);
            using var qrCode = new QRCode(qrData);
            using var bitmap = qrCode.GetGraphic(pixelsPerModule);
            using var ms = new MemoryStream();
            bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            return ms.ToArray();
        }
    }
}
