
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net;
using ZXing;
using ZXing.Common;
using ZXing.QrCode;
using ZXing.QrCode.Internal;

namespace Mall.Core.Helper
{
    public class QRCodeHelper
    {
        /// <summary>
        /// 生成图形码
        /// </summary>
        /// <param name="content"></param>
        /// <param name="format"></param>
        /// <returns></returns>
        public static Bitmap Create(string content, zxingBarcodeFormat format = zxingBarcodeFormat.QR_CODE)
        {
            return Create(content, 300, 300, format: format);
        }

        /// <summary>
        /// 生成二维码，并显示logo
        /// </summary>
        /// <param name="content"></param>
        /// <param name="imagePath"></param>
        /// <returns></returns>
        public static Bitmap Create(string content, string imagePath)
        {
            if (imagePath.Contains("http://") || imagePath.Contains("https://"))
            {
                Uri myUri = new Uri(imagePath);
                WebRequest webRequest = WebRequest.Create(myUri);
                WebResponse webResponse = webRequest.GetResponse();
                var image = new Bitmap(webResponse.GetResponseStream());
                Bitmap qrcode = Create(content, 300, 300, image, zxingBarcodeFormat.QR_CODE);
                image.Dispose();
                return qrcode;
            }
            else
            {
                if (!imagePath.Contains(":"))
                    imagePath = Core.Helper.IOHelper.GetMapPath(imagePath);
                var image = Image.FromFile(imagePath);

                Bitmap qrcode = Create(content, 300, 300, image, zxingBarcodeFormat.QR_CODE);
                image.Dispose();
                return qrcode;
            }

        }
        /// <summary>
        /// 生成图片
        /// </summary>
        /// <param name="content"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="centralImage"></param>
        /// <param name="format"></param>
        /// <returns></returns>
        public static Bitmap Create(string content, int width, int height, Image centralImage = null, zxingBarcodeFormat format = zxingBarcodeFormat.QR_CODE)
        {
            BarcodeFormat codeformat = BarcodeFormat.QR_CODE;
            switch (format)
            {
                case zxingBarcodeFormat.QR_CODE:
                    codeformat = BarcodeFormat.QR_CODE;
                    break;
                case zxingBarcodeFormat.CODE_128:
                    codeformat = BarcodeFormat.CODE_128;
                    break;
            }
            //构造二维码写码器
            MultiFormatWriter mutiWriter = new MultiFormatWriter();
            IDictionary<EncodeHintType,object> hint =   new Dictionary<EncodeHintType, object>(); ;
            hint.Add(EncodeHintType.CHARACTER_SET, "UTF-8");
            hint.Add(EncodeHintType.ERROR_CORRECTION, ErrorCorrectionLevel.H);
            //生成二维码
            BitMatrix bm = mutiWriter.encode(content, codeformat, width, height, hint);

            BarcodeWriterPixelData bw = new BarcodeWriterPixelData
            {
                Format = BarcodeFormat.QR_CODE,
                Options = new QrCodeEncodingOptions
                {
                    Height = width,
                    Width = height,
                    Margin = 0
                }

            };

            var piexlData = bw.Write(bm);

              Bitmap img = new Bitmap(piexlData.Width, piexlData.Height, System.Drawing.Imaging.PixelFormat.Format32bppRgb);


            var ms = new MemoryStream();

            var bitmapData = img.LockBits(new System.Drawing.Rectangle(0, 0, piexlData.Width, piexlData.Height),
            System.Drawing.Imaging.ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format32bppRgb);
            try
            {
                // we assume that the row stride of the bitmap is aligned to 4 byte multiplied by the width of the image
                System.Runtime.InteropServices.Marshal.Copy(piexlData.Pixels, 0, bitmapData.Scan0,
                piexlData.Pixels.Length);
            }
            finally
            {
                img.UnlockBits(bitmapData);
            }


            Bitmap result = img;
            if (centralImage != null)
            {
                //要插入到二维码中的图片
                Image middlImg = centralImage;
                //获取二维码实际尺寸（去掉二维码两边空白后的实际尺寸）
                //  System.Drawing.Size realSize = mutiWriter.GetEncodeSize(content, com.google.zxing.BarcodeFormat.QR_CODE, 300, 300);
                var realSize = mutiWriter.encode(content, codeformat, width, height);
                //计算插入图片的大小和位置
                int middleImgW = Math.Min((int)(realSize.Width / 3.5), middlImg.Width);
                int middleImgH = Math.Min((int)(realSize.Height / 3.5), middlImg.Height);
                int middleImgL = (img.Width - middleImgW) / 2;
                int middleImgT = (img.Height - middleImgH) / 2;

                //将img转换成bmp格式，否则后面无法创建 Graphics对象
                result = new Bitmap(img.Width, img.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                using (Graphics g = Graphics.FromImage(result))
                {
                    g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                    g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                    g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                    g.DrawImage(img, 0, 0);
                }

                //在二维码中插入图片
                System.Drawing.Graphics MyGraphic = System.Drawing.Graphics.FromImage(result);
                //白底
                MyGraphic.FillRectangle(Brushes.White, middleImgL, middleImgT, middleImgW, middleImgH);
                MyGraphic.DrawImage(middlImg, middleImgL, middleImgT, middleImgW, middleImgH);
            }

            return result;
        }
    }
    /// <summary>
    /// 图片码类型
    /// </summary>
    public enum zxingBarcodeFormat
    {
        /// <summary>
        /// 二维码
        /// </summary>
        QR_CODE = 0,
        /// <summary>
        /// 条形码
        /// </summary>
        CODE_128 = 1,
        /*
        DATAMATRIX = 2,
        UPC_E = 3,
        UPC_A = 4,
        EAN_8 = 5,
        EAN_13 = 6,
        CODE_39 = 7,
        ITF = 8,
        PDF417 = 9,
        */
    }
}
