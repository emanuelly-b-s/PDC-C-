using System;
using System.Linq;
using System.Drawing;
using System.Windows.Forms;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

float[] equalization(float[] img)
{
    var min = img.Min();
    var max = img.Max();
    var r = 1 / (max - min);
    
    for (int i = 0; i < img.Length; i++)
        img[i] = (img[i] - min) * r;
    
    return img;
}

float[] binarizar(float[] img, float trashold)
{
    for (int i = 0; i < img.Length; i++)
    {
        img[i] = (img[i] > trashold) ? 1 : 0;
    }
    return img;
}

float[] equalizationThreshold(float[] img, float threshold = 0.05f, float db = 0.05f)
{
    int[] histogram = hist(img, db);

    int dropCount = (int)(img.Length * threshold);
    
    float min = 0;
    int droped = 0;
    for (int i = 0; i < histogram.Length; i++)
    {
        droped += histogram[i];
        if (droped > dropCount)
        {
            min = i * db;
            break;
        }
    }

    float max = 0;
    droped = 0;
    for (int i = histogram.Length - 1; i > -1; i--)
    {
        droped += histogram[i];
        if (droped > dropCount)
        {
            max = i * db;
            break;
        }
    }

    var r = 1 / (max - min);
    
    for (int i = 0; i < img.Length; i++)
    {
        float newValue = (img[i] - min) * r;
        if (newValue > 1f)
            newValue = 1f;
        else if (newValue < 0f)
            newValue = 0f;
        img[i] = newValue;
    }
    
    return img;
}

float[] inverse(float[] img)
{
    for (int i = 0; i < img.Length; i++)
        img[i] = 1f - img[i];
    return img;
}

void showHist(float[] img, float db = 0.05f)
{
    var histogram = hist(img, db);
    var histImg = drawHist(histogram);
    showBmp(histImg);
}

(Bitmap, float[]) open(string path)
{
    var bmp = Bitmap.FromFile(path) as Bitmap;
    var byteArray = bytes(bmp);
    var dataCont = continuous(byteArray);
    var gray = grayScale(dataCont);
    return (bmp, gray);
}

Image drawHist(int[] hist)
{
    var bmp = new Bitmap(512, 256);
    var g = Graphics.FromImage(bmp);
    float margin = 16;

    int max = hist.Max();
    float barlen = (bmp.Width - 2 * margin) / hist.Length;
    float r = (bmp.Height - 2 * margin) / max;

    for (int i = 0; i < hist.Length; i++)
    {
        float bar = hist[i] * r;
        g.FillRectangle(Brushes.Black, 
            margin + i * barlen,
            bmp.Height - margin - bar, 
            barlen,
            bar);
        g.DrawRectangle(Pens.DarkBlue, 
            margin + i * barlen,
            bmp.Height - margin - bar, 
            barlen,
            bar);
    }

    return bmp;
}

void show(Bitmap bmp, float[] gray)
{
    var bytes = discretGray(gray);
    var image = img(bmp, bytes);
    showBmp(bmp);
}

int[] hist(float[] img, float db = 0.05f)
{
    int histogramLen = (int)(1 / db) + 1;
    int[] histogram = new int[histogramLen];

    foreach (var pixel in img)
        histogram[(int)(pixel / db)]++;
    
    return histogram;
}

float[] grayScale(float[] img)
{
    float[] result = new float[img.Length / 3];
    
    for (int i = 0, j = 0; i < img.Length; i += 3, j++)
    {
        result[j] = 0.1f * img[i] + 
            0.59f * img[i + 1] +
            0.3f * img[i + 2];
    }

    return result;
}

float[] continuous(byte[] img)
{
    var result = new float[img.Length];
    
    for (int i = 0; i < img.Length; i++)
        result[i] = img[i] / 255f;

    return result;
}

byte[] discret(float[] img)
{
    var result = new byte[img.Length];
    
    for (int i = 0; i < img.Length; i++)
        result[i] = (byte)(255 * img[i]);

    return result;
}

byte[] discretGray(float[] img)
{
    var result = new byte[3 * img.Length];
    
    for (int i = 0; i < img.Length; i++)
    {
        var value = (byte)(255 * img[i]);
        result[3 * i] = value;
        result[3 * i + 1] = value;
        result[3 * i + 2] = value;
    }

    return result;
}

byte[] bytes(Image img)
{
    var bmp = img as Bitmap;
    var data = bmp.LockBits(
        new Rectangle(0, 0, img.Width, img.Height),
        ImageLockMode.ReadOnly,
        PixelFormat.Format24bppRgb);
    
    byte[] byteArray = new byte[3 * data.Width * data.Height];

    byte[] temp = new byte[data.Stride * data.Height];
    Marshal.Copy(data.Scan0, temp, 0, temp.Length);

    for (int j = 0; j < data.Height; j++)
    {
        for (int i = 0; i < 3 * data.Width; i++)
        {
            byteArray[i + j * 3 * data.Width] =
                temp[i + j * data.Stride];
        }
    }

    bmp.UnlockBits(data);

    return byteArray;
}

Image img(Image img, byte[] bytes)
{
    var bmp = img as Bitmap;
    var data = bmp.LockBits(
        new Rectangle(0, 0, img.Width, img.Height),
        ImageLockMode.ReadOnly,
        PixelFormat.Format24bppRgb);
    
    byte[] temp = new byte[data.Stride * data.Height];
    
    for (int j = 0; j < data.Height; j++)
    {
        for (int i = 0; i < 3 * data.Width; i++)
        {
            temp[i + j * data.Stride] =
                bytes[i + j * 3 * data.Width];
        }
    }
    
    Marshal.Copy(temp, 0, data.Scan0, temp.Length);

    bmp.UnlockBits(data);
    return img;
}

void showBmp(Image img)
{
    ApplicationConfiguration.Initialize();

    Form form = new Form();

    PictureBox pb = new PictureBox();
    pb.Dock = DockStyle.Fill;
    pb.SizeMode = PictureBoxSizeMode.Zoom;
    form.Controls.Add(pb);

    form.WindowState = FormWindowState.Maximized;
    form.FormBorderStyle = FormBorderStyle.None;

    form.Load += delegate
    {
        pb.Image = img;
    };

    form.KeyDown += (o, e) =>
    {
        if (e.KeyCode == Keys.Escape)
        {
            Application.Exit();
        }
    };

    Application.Run(form);
}

(Bitmap bmp, float[] image) = open("img/facil.jpeg");
binarizar(image, 0.7f);
// showHist(image);
show(bmp, image);