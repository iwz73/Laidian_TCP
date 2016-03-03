using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThoughtWorks.QRCode.Codec;
using System.Drawing;
namespace ClS
{
    public class CreateQR
    {
        /// <summary>
        /// 生成二维码图
        /// </summary>
        /// <param name="temp"></param>
        /// <returns></returns>
        public static Bitmap creatQRCodeImage(string[] temp)
        {
            if (temp == null)
            {
                //msg.Text = "Data must not be empty.";
                return null;
            }

            QRCodeEncoder qrCodeEncoder = new QRCodeEncoder();
            //String encoding = @"http://www.laidiantech.com/?qrcode=" + temp[0];// +"&time=" + temp[1];
            String encoding = @"http://weixin.qq.com/r/kEz95XLEZS8arTaG9xmC?qrcode=" + temp[0];// +"&time=" + temp[1];
            if (encoding == "Byte")
            {
                qrCodeEncoder.QRCodeEncodeMode = QRCodeEncoder.ENCODE_MODE.BYTE;
            }
            else if (encoding == "AlphaNumeric")
            {
                qrCodeEncoder.QRCodeEncodeMode = QRCodeEncoder.ENCODE_MODE.ALPHA_NUMERIC;
            }
            else if (encoding == "Numeric")
            {
                qrCodeEncoder.QRCodeEncodeMode = QRCodeEncoder.ENCODE_MODE.NUMERIC;
            }
            try
            {
                int scale = Convert.ToInt16(6);
                qrCodeEncoder.QRCodeScale = scale;
            }
            catch (Exception exs)
            {
                // msg.Text = "Invalid size!" + ex.Message;
                return null;
            }
            try
            {
                int version = Convert.ToInt16(6);
                qrCodeEncoder.QRCodeVersion = version;
            }
            catch (Exception ex)
            {
                // msg.Text = "Invalid version !" + ex.Message;
            }

            string errorCorrect = "L";
            if (errorCorrect == "L")
                qrCodeEncoder.QRCodeErrorCorrect = QRCodeEncoder.ERROR_CORRECTION.L;
            else if (errorCorrect == "M")
                qrCodeEncoder.QRCodeErrorCorrect = QRCodeEncoder.ERROR_CORRECTION.M;
            else if (errorCorrect == "Q")
                qrCodeEncoder.QRCodeErrorCorrect = QRCodeEncoder.ERROR_CORRECTION.Q;
            else if (errorCorrect == "H")
                qrCodeEncoder.QRCodeErrorCorrect = QRCodeEncoder.ERROR_CORRECTION.H;
            try
            {
                // String ls_fileName = DateTime.Now.ToString("yyyyMMddhhmmss") + ".png";
                // String ls_savePath = Server.MapPath(".") + "/QRCodeImages/" + ls_fileName;
                //msg.Text = ls_savePath;

                return qrCodeEncoder.Encode(encoding);//.Save(ls_savePath);
                                                      // ImageButton2.ImageUrl = "QRCodeImages/" + ls_fileName;
            }
            catch (Exception ex)
            {
                return null;// msg.Text = "Invalid version !" + ex.Message;
            }
        }
    }
}
