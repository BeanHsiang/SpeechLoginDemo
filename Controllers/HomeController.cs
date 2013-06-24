using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using Microsoft.Speech.Recognition;

namespace SpeechLoginDemo.Controllers
{
    public class HomeController : Controller
    {
        private const string strValidateCodeBound = "abcdefghijkmnpqrstuvwxyzABCDEFGHIJKLMNPQRSTUVWXYZ23456789";
        private static string[] Fonts = new string[] {  "Helvetica",
                                                 "Geneva", 
                                                 "sans-serif",
                                                 "Verdana",
                                                 "Times New Roman",
                                                 "Courier New",
                                                 "Arial"
                                             };

        //
        // GET: /Home/
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Welcome()
        {
            if (Request.ContentType.Equals("audio/x-mpeg"))
            {
                using (BinaryReader br = new System.IO.BinaryReader(Request.InputStream))
                {
                    int length = 0;
                    byte[] byt = new byte[1000];
                    using (FileStream fs = System.IO.File.Create("d:\\output.mp3"))
                    {
                        while ((length = br.Read(byt, 0, byt.Length)) > 0)
                        {
                            fs.Write(byt, 0, length);
                        }
                    }
                }
            }
            var txt = Recognize();
            return Content(Session["ValidateCode"].ToString() == txt ? "验证通过" : "验证不通过");
        }

        public ActionResult GetCaptcha()
        {
            var str_ValidateCode = "请说出  天王盖地虎  的下句";
            Session["ValidateCode"] = "宝塔镇河妖";
            return File(CreateImage(str_ValidateCode), "image/png");
        }

        /// <summary>
        /// Generate random string
        /// </summary>
        private string GetRandomString(int int_NumberLength)
        {
            string valString = string.Empty;
            Random theRandomNumber = new Random((int)DateTime.Now.Ticks);

            for (int int_index = 0; int_index < int_NumberLength; int_index++)
                valString += strValidateCodeBound[theRandomNumber.Next(strValidateCodeBound.Length - 1)].ToString();

            return valString;
        }

        /// <summary>
        /// Generate random Color
        /// </summary>
        private Color GetRandomColor()
        {
            Random RandomNum_First = new Random((int)DateTime.Now.Ticks);

            System.Threading.Thread.Sleep(RandomNum_First.Next(50));
            Random RandomNum_Sencond = new Random((int)DateTime.Now.Ticks);


            int int_Red = RandomNum_First.Next(256);
            int int_Green = RandomNum_Sencond.Next(256);
            int int_Blue = (int_Red + int_Green > 400) ? 0 : 400 - int_Red - int_Green;
            int_Blue = (int_Blue > 255) ? 255 : int_Blue;

            return Color.FromArgb(int_Red, int_Green, int_Blue);
        }

        /// <summary>
        /// Create Validation Code Image
        /// </summary>
        private byte[] CreateImage(string str_ValidateCode)
        {
            int int_ImageWidth = str_ValidateCode.Length * 22;
            Random newRandom = new Random();

            Bitmap theBitmap = new Bitmap(int_ImageWidth + 6, 38);
            Graphics theGraphics = Graphics.FromImage(theBitmap);

            theGraphics.Clear(Color.White);

            drawLine(theGraphics, theBitmap, newRandom);
            theGraphics.DrawRectangle(new Pen(Color.LightGray, 1), 0, 0, theBitmap.Width - 1, theBitmap.Height - 1);

            for (int int_index = 0; int_index < str_ValidateCode.Length; int_index++)
            {
                Matrix X = new Matrix();
                X.Shear((float)newRandom.Next(0, 300) / 1000 - 0.25f, (float)newRandom.Next(0, 100) / 1000 - 0.05f);
                theGraphics.Transform = X;
                string str_char = str_ValidateCode.Substring(int_index, 1);
                System.Drawing.Drawing2D.LinearGradientBrush newBrush = new System.Drawing.Drawing2D.LinearGradientBrush(new Rectangle(0, 0, theBitmap.Width, theBitmap.Height), Color.Blue, Color.DarkRed, 1.2f, true);
                Point thePos = new Point(int_index * 21 + 1 + newRandom.Next(3), 1 + newRandom.Next(13));

                Font theFont = new Font(Fonts[newRandom.Next(Fonts.Length - 1)], newRandom.Next(14, 18), FontStyle.Bold);

                theGraphics.DrawString(str_char, theFont, newBrush, thePos);
            }

            drawPoint(theBitmap, newRandom);

            MemoryStream ms = new MemoryStream();
            theBitmap.Save(ms, ImageFormat.Png);

            var ret = ms.ToArray();
            theGraphics.Dispose();
            theBitmap.Dispose();
            ms.Close();
            return ret;
        }

        /// <summary>
        /// Draw Line for noise
        /// </summary>
        private void drawLine(Graphics gfc, Bitmap img, Random ran)
        {
            for (int i = 0; i < 10; i++)
            {
                int x1 = ran.Next(img.Width);
                int y1 = ran.Next(img.Height);
                int x2 = ran.Next(img.Width);
                int y2 = ran.Next(img.Height);
                gfc.DrawLine(new Pen(Color.Silver), x1, y1, x2, y2);
            }
        }

        /// <summary>
        /// Draw Point for noise
        /// </summary>
        private void drawPoint(Bitmap img, Random ran)
        {
            for (int i = 0; i < 30; i++)
            {
                int x = ran.Next(img.Width);
                int y = ran.Next(img.Height);
                img.SetPixel(x, y, Color.FromArgb(ran.Next()));
            }

        }

        private string Recognize()
        {
            using (SpeechRecognitionEngine sre = new SpeechRecognitionEngine(new CultureInfo("zh-CN")))
            {
                Choices colors = new Choices(new string[] { "宝塔镇河妖" });
                GrammarBuilder gb = new GrammarBuilder();
                gb.Append(colors);

                // Create the Grammar instance.
                Grammar g = new Grammar(gb);
                sre.LoadGrammar(g);
                // Configure the input to the recognizer.
                sre.SetInputToWaveFile("d:\\output.mp3");
                RecognitionResult result = sre.Recognize();

                return result == null ? string.Empty : result.Text;
            }
        }
    }
}