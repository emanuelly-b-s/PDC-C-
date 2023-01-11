using System;
using System.Linq;
using System.Drawing;
using System.Numerics;
using System.Windows.Forms;
using System.Drawing.Imaging;
using System.Collections.Generic;
using System.Runtime.InteropServices;

// (Bitmap bmp, float[] img) sobel((Bitmap bmp, float[] img) t)
// {

//     var _img = t.img;
//     var wid = t.bmp.Width;
//     var hei = t.bmp.Height;

//     float[] imgRed = new float[_img.Length];
//     float[] imgRed2 = new float[_img.Length];

//     for (int i = 1; i < wid - 1; i++)
//     {
//         for (int j = 1; j < hei - 1; j++)
//         {
//             int index = i + j * wid;
//             var op = _img[index] + 2 * _img[index + 1] + _img[index + 2];

//             imgRed2[i + j * wid] = op;

//         }
//     }

//     for (int j = 1; j < hei - 1; j++)
//     {
//         int index = 1 + j * wid;
//         float sum = imgRed2[index - 1] + imgRed2[index];
//         for (int i = 1; i < wid - 1; i++)
//         {
//             index = i + j * wid;
//             var newSum = imgRed2[index] + imgRed2[index + 1];
//             imgRed[index] = sum - newSum;
//             sum = newSum;

//             if (imgRed[index]  < 0)
//                 imgRed[index]  = 0f;
//             else if (imgRed[index]  > 1)
//                 imgRed[index]  = 1f;

//         }
//     }

//     var Imgbytes = discretGray(imgRed);
//     img(t.bmp, Imgbytes);

//     return (t.bmp, imgRed);
// }

(Bitmap bmp, float[] img) bilinear((Bitmap bmp, float[] img) t)
{
    float[] _img = t.img;
    float[] imgRed = new float[_img.Length];
    int wid = t.bmp.Width;
    int hei = t.bmp.Height; //altura

    for (int i = 0; i < hei; i++)
    {
        for (int j = 0; j < wid; j++)
        {
            int index = j + i * wid;
            if (_img[index] > 0f || 
                j == 0 || i == 0 || 
                j == wid - 1 || i == hei - 1)
            {
                imgRed[index] = _img[index];
                continue;
            }

            var newX = j - 1 + (i - 1) * wid;
            var newY = j + 1 + (i - 1) * wid;
            float newIndex = (_img[newX] + _img[newY]) / 2;

            var newX2 = j - 1 + (i + 1) * wid;
            var newY2 = j + 1 + (i + 1) * wid;
            float b = (_img[newX2] + _img[newY2]) / 2;

            imgRed[index] = (newIndex + b) / 2;
        }
    }

    var imgBytes = discretGray(imgRed);
    img(t.bmp, imgBytes);

    return (t.bmp, imgRed);
}

(Bitmap bmp, float[] img) interpolacao((Bitmap bmp, float[] img) t)
{
    float[] _img = t.img;
    float[] result = new float[_img.Length];
    int wid = t.bmp.Width;
    int hei = t.bmp.Height;

    for (int i = 0; i < hei; i++)
    {
        for (int j = 0; j < wid; j++)
        {
            int index = j + i * wid;
            if (_img[index] != 0)
                continue;

            int index_diag1 = (j-1) +( i-1) * wid;
            int index_diag2 = (j+1) +( i+1) * wid;
            while(_img[index_diag1] == 0 && _img[index_diag2] ==0)
            {
                
                
                
            }

            
            // if(_img[index_diag1] != 0 && _img[index_diag2] != 0)
                // {
                //     float a = _img[(j-1) +( i-1) * wid];
                //     float b = _img[(j+1) +( i-1) * wid];
                
                // }
                
            float xAB = x*b + (x-1)*a;

           float c = _img[(j-1) +( i+1) * wid];
                    float d = _img[(j+1) +( i+1) * wid];
            float xCD = x*d + (x-1)*a;


        }
    }

   var Imgbytes = discretGray(result);
    img(t.bmp, Imgbytes);

    return (t.bmp, result);
}


(Bitmap bmp, float[] img) resize((Bitmap bmp, float[] img) t, 
    float newWid, float newHei)
{
    int wid = t.bmp.Width;
    int hei = t.bmp.Height;
    Bitmap imgRedirecionada = new Bitmap(
        (int)(newWid * wid),
        (int)(newHei * hei)
    );
    int widImgNova = imgRedirecionada.Width;
    int hidImgNova = imgRedirecionada.Height;
    float[] newImg = t.img;
    float[] tamNewImg = new float[widImgNova * hidImgNova];
    for (int i = 0; i < wid; i++)
    {
        for (int j = 0; j < hei; j++)
        {
            tamNewImg[i + j * widImgNova]
                = newImg[i + j * wid];
        }
    }
    var imgCinza = discretGray(tamNewImg);
    var imgRed = img(imgRedirecionada, imgCinza) as Bitmap;
    var result = (imgRed, tamNewImg);

    result = 
        affine(result,
            scale(newWid, newHei));
    
    return result;
}

Matrix4x4 mat(params float[] arr)
{
    return new Matrix4x4(
        arr[0], arr[1], arr[2], 0,
        arr[3], arr[4], arr[5], 0,
        arr[6], arr[7], arr[8], 0,
             0,      0,      0, 1
    );
}

Matrix4x4 rotation(float degree)
{
    float radian = degree / 180 * MathF.PI;
    float cos = MathF.Cos(radian);
    float sin = MathF.Sin(radian);
    return mat(
        cos, -sin, 0,
        sin,  cos, 0,
          0,    0, 1
    );
}

Matrix4x4 translate(float dx, float dy)
{
    return mat(
        1, 0, dx,
        0, 1, dy,
        0, 0, 1
    );
}

Matrix4x4 translateFromSize(float dx, float dy,
    (Bitmap bmp, float[] img) t)
{
    return mat(
        1, 0, dx * t.bmp.Width,
        0, 1, dy * t.bmp.Height,
        0, 0, 1
    );
}

Matrix4x4 scale(float dx, float dy)
{
    return mat(
        dx, 0, 0,
        0, dy, 0,
        0, 0, 1
    );
}

Matrix4x4 shear(float cx, float cy)
{
    return mat(
        1, cx, 0,
        cy, 1, 0,
        0, 0, 1
    );
}

(Bitmap bmp, float[] img) affine((Bitmap bmp, float[] img) t,
    Matrix4x4 mat)
{
    float[] p = new float[]
    {
        mat.M11, mat.M12, mat.M13,
        mat.M21, mat.M22, mat.M23,
        mat.M31, mat.M32, mat.M33,
    };
    var _img = t.img;
    float[] nova = new float[_img.Length];
    int wid = t.bmp.Width;
    int hei = t.bmp.Height;
    int x = 0;
    int y = 0;
    int index = 0;

    for (int i = 0; i < wid; i++)
    {
        for (int j = 0; j < hei; j++)
        {
            x = (int)(p[0] * i + p[1] * j + p[2]);
            y = (int)(p[3] * i + p[4] * j + p[5]);

            if(x < 0 || x >= wid || y < 0 || y >= wid)
                continue;
            else
            {
                index = (int)(x + y * wid);
                nova[index] = _img[i+j * wid];
            }
        }
    }

    var Imgbytes = discretGray(nova);
    img(t.bmp, Imgbytes);

    return (t.bmp, nova);
}


(Bitmap bmp, float[] img) sobel((Bitmap bmp, float[] img) t,
    bool dir = true)
{
    var im = t.img;
    float[] tempo = new float[im.Length];
    float[] final = new float[im.Length];
    int wid = t.bmp.Width;
    int hei = t.bmp.Height;

    for (int i = 1; i < wid - 1; i++)
    {
        float sum =
            im[i + 0 * wid] +
            im[i + 1 * wid] +
            im[i + 2 * wid];
        for (int j = 1; j < hei - 1; j++)
        {
            int index = i + j * wid;
            tempo[index] = im[index] + sum;

            sum -= im[index - 1];
            sum += im[index + 1];
        }
    }

    for (int j = 1; j < hei - 1; j++)
    {
        float seq =
            im[0 + j * wid] +
            im[1 + j * wid];
        for (int i = 1; i < wid - 1; i++)
        {
            float nextSeq =
                im[i + j * wid] +
                im[i + 1 + j * wid];

            int index = i + j * wid;
            float value = dir ? seq - nextSeq : nextSeq - seq;
            if (value > 1f)
                value = 1f;
            else if (value < 0f)
                value = 0f;
            final[index] = value;

            seq = nextSeq;
        }
    }

    var Imgbytes = discretGray(final);
    img(t.bmp, Imgbytes);

    return (t.bmp, final);
}


(Bitmap bmp, float[] img) conv(
    (Bitmap bmp, float[] img) t, float[] kernel)
{
    var N = (int)Math.Sqrt(kernel.Length);
    var wid = t.bmp.Width;
    var hei = t.bmp.Height;
    var _img = t.img;
    float[] imgRed = new float[_img.Length];

    for (int j = N / 2; j < hei - N / 2; j++)
    {
        for (int i = N / 2; i < wid - N / 2; i++)
        {
            float sum = 0;

            for (int k = 0; k < N; k++)
            {
                for (int l = 0; l < N; l++)
                {
                    sum += _img[i + k - (N / 2) + (j + l - (N / 2)) * wid] *
                        kernel[k + l * N];
                }
            }

            if (sum > 1f)
                sum = 1f;

            else if (sum < 0f)
                sum = 0f;

            imgRed[i + j * wid] = sum;
        }
    }

    var Imgbytes = discretGray(imgRed);
    img(t.bmp, Imgbytes);

    return (t.bmp, imgRed);
}

(Bitmap bmp, float[] img) morfology((Bitmap bmp, float[] img) t, float[] kernel, bool erosion)
{
    bool match = false;
    int wid = t.bmp.Width;
    int hei = t.bmp.Height;

    float[] imgor = t.img;
    float[] newImg = new float[imgor.Length];
    var tamKernel = (int)Math.Sqrt(kernel.Length);

    for (int i = 0; i < imgor.Length; i++)
    {
        match = erosion;
        int x = i % wid,
            y = i / wid;

        for (int j = 0; j < kernel.Length; j++)
        {
            if (kernel[j] == 0f)
                continue;

            int kx = j % tamKernel,
                ky = j / tamKernel;

            int tx = x + kx - tamKernel / 2;
            int ty = y + ky - tamKernel / 2;

            if (tx < 0 || ty < 0 || tx >= wid || ty >= hei)
                continue;

            int index = tx + ty * wid;

            if (imgor[index] == 1f)
            {
                if (!erosion)
                {
                    match = true;
                    break;
                }
            }
            else
            {
                if (erosion)
                {
                    match = false;
                    break;
                }
            }
        }

        if (match)
            newImg[i] = 1f;
    }

    var Imgbytes = discretGray(newImg);
    img(t.bmp, Imgbytes);

    return (t.bmp, newImg);
}

List<Rectangle> segmentation((Bitmap bmp, float[] img) t)
{
    var rects = segmentationT(t, 0);
    var areas = rects.Select(r => r.Width * r.Height);
    var average = areas.Average();

    return rects
        .Where(r => r.Width * r.Height > average)
        .ToList();
}

List<Rectangle> segmentationT((Bitmap bmp, float[] img) t, int threshold)
{
    List<Rectangle> list = new List<Rectangle>();
    Stack<int> stack = new Stack<int>();

    float[] img = t.img;
    int wid = t.bmp.Width;
    float crr = 0.01f;

    int minx, maxx, miny, maxy;
    int count = 0;

    for (int i = 0; i < img.Length; i++)
    {
        if (img[i] > 0f)
            continue;

        minx = int.MaxValue;
        miny = int.MaxValue;
        maxx = int.MinValue;
        maxy = int.MinValue;
        count = 0;
        stack.Push(i);

        while (stack.Count > 0)
        {
            int j = stack.Pop();

            if (j < 0 || j >= img.Length)
                continue;

            if (img[j] > 0f)
                continue;

            int x = j % wid,
                y = j / wid;

            if (x < minx)
                minx = x;
            if (x > maxx)
                maxx = x;

            if (y < miny)
                miny = y;
            if (y > maxy)
                maxy = y;

            img[j] = crr;
            count++;

            stack.Push(j - 1);
            stack.Push(j + 1);
            stack.Push(j + wid);
            stack.Push(j - wid);
        }

        crr += 0.01f;
        if (count < threshold)
            continue;

        Rectangle rect = new Rectangle(
            minx, miny, maxx - minx, maxy - miny
        );
        list.Add(rect);
    }

    return list;
}

void otsu((Bitmap bmp, float[] img) t, float db = 0.05f)
{
    var histogram = hist(t.img, db);
    int threshold = 0;

    float Ex0 = 0;
    float Ex1 = t.img.Average();
    float Dx0 = 0;
    float Dx1 = t.img.Sum(x => x * x);
    int N0 = 0;
    int N1 = t.img.Length;

    float minstddev = float.PositiveInfinity;

    for (int i = 0; i < histogram.Length; i++)
    {
        float value = db * (2 * i + 1) / 2;
        float s = histogram[i] * value;

        if (N0 == 0 && histogram[i] == 0)
            continue;

        Ex0 = (Ex0 * N0 + s) / (N0 + histogram[i]);
        Ex1 = (Ex1 * N1 - s) / (N1 - histogram[i]);

        N0 += histogram[i];
        N1 -= histogram[i];

        Dx0 += value * value * histogram[i];
        Dx1 -= value * value * histogram[i];

        float stddev =
            Dx0 - N0 * Ex0 * Ex0 +
            Dx1 - N1 * Ex1 * Ex1;

        if (float.IsInfinity(stddev) ||
            float.IsNaN(stddev))
            continue;

        if (stddev < minstddev)
        {
            minstddev = stddev;
            threshold = i;
        }
    }
    float bestTreshold = db * (2 * threshold + 1) / 2;

    tresh(t, bestTreshold);
}

void tresh((Bitmap bmp, float[] img) t,
    float threshold = 0.5f)
{
    for (int i = 0; i < t.img.Length; i++)
        t.img[i] = t.img[i] > threshold ? 1f : 0f;
}

float[] equalization(
    (Bitmap bmp, float[] img) t,
    float threshold = 0f,
    float db = 0.05f)
{
    int[] histogram = hist(t.img, db);

    int dropCount = (int)(t.img.Length * threshold);

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

    for (int i = 0; i < t.img.Length; i++)
    {
        float newValue = (t.img[i] - min) * r;
        if (newValue > 1f)
            newValue = 1f;
        else if (newValue < 0f)
            newValue = 0f;
        t.img[i] = newValue;
    }

    return t.img;
}

void showHist((Bitmap bmp, float[] img) t, float db = 0.05f)
{
    var histogram = hist(t.img, db);
    var histImg = drawHist(histogram);
    showBmp(histImg);
}

(Bitmap bmp, float[] img) open(string path)
{
    var bmp = Bitmap.FromFile(path) as Bitmap;
    var byteArray = bytes(bmp);
    var dataCont = continuous(byteArray);
    var gray = grayScale(dataCont);
    return (bmp, gray);
}

float[] inverse(float[] img)
{
    for (int i = 0; i < img.Length; i++)
        img[i] = 1f - img[i];
    return img;
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

void show((Bitmap bmp, float[] gray) t)
{
    var bytes = discretGray(t.gray);
    var image = img(t.bmp, bytes);
    showBmp(image);
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
    float[] imgRed = new float[img.Length / 3];

    for (int i = 0, j = 0; i < img.Length; i += 3, j++)
    {
        imgRed[j] = 0.1f * img[i] +
            0.59f * img[i + 1] +
            0.3f * img[i + 2];
    }

    return imgRed;
}

float[] continuous(byte[] img)
{
    var imgRed = new float[img.Length];

    for (int i = 0; i < img.Length; i++)
        imgRed[i] = img[i] / 255f;

    return imgRed;
}

byte[] discret(float[] img)
{
    var imgRed = new byte[img.Length];

    for (int i = 0; i < img.Length; i++)
        imgRed[i] = (byte)(255 * img[i]);

    return imgRed;
}

byte[] discretGray(float[] img)
{
    var imgRed = new byte[3 * img.Length];

    for (int i = 0; i < img.Length; i++)
    {
        var value = (byte)(255 * img[i]);
        imgRed[3 * i] = value;
        imgRed[3 * i + 1] = value;
        imgRed[3 * i + 2] = value;
    }

    return imgRed;
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
        ImageLockMode.ReadWrite,
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

void showRects((Bitmap bmp, float[] img) t, List<Rectangle> list)
{
    var g = Graphics.FromImage(t.bmp);

    foreach (var rect in list)
        g.DrawRectangle(Pens.Red, rect);

    showBmp(t.bmp);
}


var image = open("img/shuregui.png");
image = resize(image, 1.25f, 1f);
image = bilinear(image);
image = resize(image, 1f, 1.25f);
image = bilinear(image);
show(image);

// otsu(image);
// var rects = segmentation(image);

// float[] kernel = new float[9]{0,1,0,0,0,0,0,0,0};

// morfology(image, kernel, true);

// showRects(image, rects);