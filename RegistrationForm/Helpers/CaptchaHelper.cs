using System.Drawing.Imaging;
using System.Drawing;

namespace RegistrationForm.Helpers
{
    public class CaptchaHelper
    {
        public static string GenerateCaptchaText(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public static byte[] GenerateCaptchaImage(string captchaText)
        {
            int width = 200;
            int height = 60;
            using (Bitmap bitmap = new Bitmap(width, height))
            using (Graphics graphics = Graphics.FromImage(bitmap))
            {
                graphics.Clear(Color.LightGray);

                Random random = new Random();
                Pen pen = new Pen(Color.Gray, 0);
                for (int i = 0; i < 10; i++) 
                {
                    int x1 = random.Next(0, width);
                    int y1 = random.Next(0, height);
                    int x2 = random.Next(0, width);
                    int y2 = random.Next(0, height);
                    graphics.DrawLine(pen, x1, y1, x2, y2);
                }

                for (int i = 0; i < 30; i++) 
                {
                    int x = random.Next(0, width);
                    int y = random.Next(0, height);
                    int size = random.Next(1, 4);
                    graphics.FillEllipse(Brushes.Gray, x, y, size, size);
                }

                Font font = new Font("Arial", 26, FontStyle.Bold);
                Brush brush = new SolidBrush(Color.Black);
                PointF point = new PointF(10, 10);

                for (int i = 0; i < captchaText.Length; i++)
                {
                    float angle = random.Next(-15, 15); 
                    graphics.TranslateTransform(point.X, point.Y);
                    graphics.RotateTransform(angle);

                    graphics.DrawString(captchaText[i].ToString(), font, brush, 0, 0);

                    graphics.RotateTransform(-angle);
                    graphics.TranslateTransform(-point.X, -point.Y);

                    point.X += 30;
                }

                using (MemoryStream ms = new MemoryStream())
                {
                    bitmap.Save(ms, ImageFormat.Png);
                    return ms.ToArray();
                }
            }
        }
    }
}
