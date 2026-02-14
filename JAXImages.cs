/* =============================================================== *
 * JAXBase Image Registry
 * 
 * ===============================================================
 * 
 * Hisory
 * -------------
 *  2025-12-11 - JLW
 *      Start of definition.
 *      
 *      The image class and images in general have not yet
 *      defined, so I'm switching all image refernces to
 *      SixLabors imaging.
 *      
 * 
 * ===============================================================
 * 
 * TODO
 * -------------
 *      Add GetWebImage(string [,HTTP Object])
 *      
 * =============================================================== */

using Microsoft.VisualStudio.Services.ClientNotification;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Bmp;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Tiff;
using SixLabors.ImageSharp.Processing;
using System.Drawing.Drawing2D;

namespace JAXBase
{
    public class JAXImages
    {
        Dictionary<string, SixLabors.ImageSharp.Image> ImageLibrary = [];
        AppClass App;

        public JAXImages(AppClass app) { App = app; }

        /*
         * Returns 0 for success
         *      1 - File not found
         *    501 - Image already exists
         *      x - Not authorized
         *    599 - Internal error
         */
        /// <summary>
        /// Put an image from a file into the registry
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="imageName"></param>
        /// <returns>int</returns>
        public int RegisterImage(string fileName, string? imageName, out string imgName)
        {
            int result = 0;

            imageName = string.IsNullOrWhiteSpace(imageName) ? JAXLib.JustFName(fileName).ToLower() : imageName.Trim().ToLower();
            imgName = JAXLib.JustStem(imageName);

            // Remove all valid characters and see if anything is left
            string testName = JAXLib.ChrTran(imgName, "abcdefghijklmnopqrstuvwxyz0123456790._-", "");

            if (testName.Length > 0 || string.IsNullOrWhiteSpace(imageName) || JAXLib.Between(imageName[0], 'a', 'z') == false)
                result = 519;
            else if (ImageLibrary.ContainsKey(imageName))
                result = 501;
            else
            {
                if (File.Exists(fileName) == false)
                {
                    // Look for the image in search path using naming conventions
                    string filePath = AppHelper.FindPathForFile(App, fileName);
                    fileName = filePath + AppHelper.FixFileCase(string.Empty, JAXLib.JustFName(fileName), App.CurrentDS.JaxSettings.Naming, App.CurrentDS.JaxSettings.NamingAll);
                }

                string msg = string.Empty;

                if (File.Exists(fileName))
                {
                    // Load the file into the registry
                    try
                    {
                        SixLabors.ImageSharp.Image imageObject = SixLabors.ImageSharp.Image.Load(fileName);
                        ImageLibrary.Add(imgName, imageObject);
                    }
                    catch (ArgumentException ex) { result = 11; msg = ex.Message; }
                    catch (FileNotFoundException ex) { result = 1; msg = ex.Message; }
                    catch (OutOfMemoryException ex) { result = 499; msg = ex.Message; }
                    catch (Exception ex) { result = 599; msg = ex.Message; }

                    if (string.IsNullOrWhiteSpace(msg) == false)
                    {
                        App.DebugLog($"RegisterImage tossed an error {result} with exception: {msg}");
                    }
                }
                else
                    result = 1;
            }

            return result;
        }

        /*
         * Returns 0 for success
         *    502 - Image not found
         *    599 - Internal error
         */
        /// <summary>
        /// Remove an image from the registry
        /// </summary>
        /// <param name="imageName"></param>
        /// <returns>int</returns>
        public int UnRegisterImage(string imageName)
        {
            int result = 0;
            return result;
        }


        /// <summary>
        /// Returns true if the image exists in the library
        /// </summary>
        /// <param name="imagename"></param>
        /// <returns></returns>
        public bool HasImage(string imagename)
        {
            return ImageLibrary.ContainsKey(imagename.Trim().ToLower());
        }


        /*
         * Mr Google and Grok for the win!
         */
        /// <summary>
        /// Convert a SixLabors image to a System Drawing Image
        /// </summary>
        /// <param name="sixLaborsImage"></param>
        /// <returns>System.Drawing.Image?</returns>
        public System.Drawing.Image? ConvertToSDImage(SixLabors.ImageSharp.Image sixLaborsImage)
        {
            System.Drawing.Image? netImage = null;

            try
            {
                // Fist we need the image format
                IImageFormat? format = sixLaborsImage.Metadata.DecodedImageFormat;

                if (format is not null)
                {
                    // Get the correct encoder for the image
                    IImageEncoder encoder = SixLabors.ImageSharp.Configuration.Default.ImageFormatsManager.GetEncoder(format);

                    using (MemoryStream ms = new MemoryStream())
                    {
                        // Save to the stream in the original format
                        sixLaborsImage.Save(ms, encoder);
                        ms.Position = 0; // Reset stream position to the beginning

                        // Create a System.Drawing.Bitmap from the stream
                        // Note: System.Drawing.Image/Bitmap implements IDisposable, so use 'using'
                        using (Bitmap systemDrawingBitmap = new Bitmap(ms))
                        {
                            netImage = (System.Drawing.Image)systemDrawingBitmap.Clone();
                        }
                    }
                }
                else
                    netImage = null;  // Invalid format detected (should not be a common problme)
            }
            catch (Exception ex)
            {
                App.DebugLog($"ConvertToSDImage tossed an exception: {ex.Message}");

                // All exceptions just create a null result
                netImage = null;
            }

            return netImage;
        }

        /*
         * Mr Google and Grok for the win!
         */

        /// <summary>
        /// Convert SixLabors image to System Image - 0=png, 1=jpg, 2=bmp, 3=gif, 4=tif
        /// </summary>
        /// <param name="sixLaborsImage"></param>
        /// <param name="encoderType"></param>
        /// <returns>System.Drawing.Image?</returns>
        public System.Drawing.Image? ConvertToSDImage(SixLabors.ImageSharp.Image sixLaborsImage, int encoderType)
        {
            System.Drawing.Image? netImage = null;

            try
            {
                // Fist we need the image format
                IImageFormat format = encoderType switch
                {
                    1 => JpegFormat.Instance,
                    2 => BmpFormat.Instance,
                    3 => GifFormat.Instance,
                    4 => TiffFormat.Instance,
                    _ => PngFormat.Instance,

                };


                if (format is not null)
                {
                    // Get the correct encoder for the image
                    //IImageEncoder enc = SixLabors.ImageSharp.Configuration.Default.ImageFormatsManager.GetEncoder(format)
                    IImageEncoder encoder = SixLabors.ImageSharp.Configuration.Default.ImageFormatsManager.GetEncoder(format);

                    using (MemoryStream ms = new MemoryStream())
                    {
                        // Save to the stream in the original format
                        sixLaborsImage.Save(ms, encoder);
                        ms.Position = 0; // Reset stream position to the beginning

                        // Create a System.Drawing.Bitmap from the stream
                        // Note: System.Drawing.Image/Bitmap implements IDisposable, so use 'using'
                        using (Bitmap systemDrawingBitmap = new Bitmap(ms))
                        {
                            netImage = (System.Drawing.Image)systemDrawingBitmap.Clone();
                        }
                    }
                }
                else
                    netImage = null;  // Invalid format detected (should not be a common problme)
            }
            catch (Exception ex)
            {
                App.DebugLog($"ConvertToSDImage tossed an exception: {ex.Message}");

                // All exceptions just create a null result
                netImage = null;
            }

            return netImage;
        }


        /// <summary>
        /// Turn a registered image into an icon of nSize
        /// </summary>
        /// <param name="imageName"></param>
        /// <param name="nSize"></param>
        /// <returns>System.Drawing.Icon?</returns>
        public Icon? ConvertImageToIcon(string imageName, int nSize)
        {
            SixLabors.ImageSharp.Image? temp = GetImage(imageName, out _);
            Icon? result = null;

            if (temp is not null)
            {
                System.Drawing.Image? sdTemp = ConvertToSDImage(temp);

                if (sdTemp is not null)
                {
                    result = ConvertImageToIcon(sdTemp, nSize);
                }
            }

            return result;
        }

        public System.Drawing.Image? FixIconSize(SixLabors.ImageSharp.Image original, int buttonSize)
        {
            System.Drawing.Image? result = null;

            try
            {
                result = ConvertToSDImage(original);
                if (result is not null)
                {
                    result = FixIconSize(result, buttonSize);
                }
            }
            catch (Exception ex)
            {
                App.DebugLog($"FixIconSize tossed an exception: {ex.Message}");
                result = null;
            }

            return result;

        }
        public System.Drawing.Image? FixIconSize(System.Drawing.Image original, int buttonSize)
        {
            // Force exactly 40×40 (or whatever size you want) with transparent padding if needed
            System.Drawing.Image? resizedImage;
            Bitmap? result = null;

            try
            {
                resizedImage = ResizeImage(original, buttonSize, buttonSize);

                if (resizedImage is not null)
                {
                    result = new Bitmap(buttonSize, buttonSize);
                    using (Graphics g = Graphics.FromImage(result))
                    {
                        g.Clear(System.Drawing.Color.Transparent);
                        g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                        g.DrawImage(resizedImage,
                            (buttonSize - resizedImage.Width) / 2f,   // center horizontally
                            (buttonSize - resizedImage.Height) / 2f,  // center vertically
                            resizedImage.Width, resizedImage.Height);
                    }
                }
            }
            catch (Exception ex)
            {
                App.DebugLog($"FixIconSize tossed an exception: {ex.Message}");
                result = null;
            }

            return result;
        }

        public System.Drawing.Image? ResizeImage(System.Drawing.Image originalImage, int newWidth, int newHeight)
        {

            // Create a new Bitmap with the desired dimensions
            Bitmap? result;

            try
            {
                result = new Bitmap(newWidth, newHeight);

                // Set the resolution of the new image to match the original
                result.SetResolution(originalImage.HorizontalResolution, originalImage.VerticalResolution);

                using (Graphics graphics = Graphics.FromImage(result))
                {
                    // Configure for high-quality resizing
                    graphics.CompositingQuality = CompositingQuality.HighQuality;
                    graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    graphics.SmoothingMode = SmoothingMode.HighQuality;
                    graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                    // Draw the original image onto the new bitmap, scaling it to the new dimensions
                    graphics.DrawImage(originalImage, new System.Drawing.Rectangle(0, 0, newWidth, newHeight),
                                       new System.Drawing.Rectangle(0, 0, originalImage.Width, originalImage.Height),
                                       GraphicsUnit.Pixel);
                }
            }
            catch (Exception ex)
            {
                App.DebugLog($"FixIconSize tossed an exception: {ex.Message}");
                result = null;
            }

            return result;
        }


        /// <summary>
        /// Turn a SD image into an icon of nSize
        /// </summary>
        /// <param name="originalImage"></param>
        /// <param name="nSize"></param>
        /// <returns>System.Drawing.Icon?</returns>
        public Icon? ConvertImageToIcon(System.Drawing.Image originalImage, int nSize)
        {
            Icon? result = null;

            try
            {
                // Create a new 32x32 bitmap with high-quality resizing
                using (Bitmap bitmap = new Bitmap(nSize, nSize, System.Drawing.Imaging.PixelFormat.Format32bppArgb))
                {
                    using (Graphics g = Graphics.FromImage(bitmap))
                    {
                        g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                        g.DrawImage(originalImage, new System.Drawing.Rectangle(0, 0, nSize, nSize));
                    }

                    // Create an icon from the resized bitmap
                    IntPtr hIcon = bitmap.GetHicon();
                    Icon icon = Icon.FromHandle(hIcon);

                    // Important: Clone the icon to take ownership (prevents resource leaks)
                    result = (Icon)icon.Clone();
                }
            }
            catch (Exception ex)
            {
                App.DebugLog($"ConvertImageToIcon tossed an exception: {ex.Message}");
                result = null;
            }

            return result;
        }


        /// <summary>
        /// Convert a SDImage to a SixLabors png image
        /// </summary>
        /// <param name="drawingImage"></param>
        /// <returns>SixLabors.ImageSharp.Image?</returns>
        public SixLabors.ImageSharp.Image? ConvertImage(System.Drawing.Image drawingImage)
        {
            byte[] imageBytes;
            using (MemoryStream ms = new MemoryStream())
            {
                // Save the System.Drawing.Image to a MemoryStream in a specific format (e.g., Png)
                drawingImage.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                imageBytes = ms.ToArray();
            }

            // Load the bytes into a SixLabors.ImageSharp.Image
            return SixLabors.ImageSharp.Image.Load(imageBytes);
        }


        /// <summary>
        /// Returns a SixLabors image if it exists, otherwise null if not found 
        /// </summary>
        /// <param name="imageName"></param>
        /// <returns></returns>
        public SixLabors.ImageSharp.Image? GetImage(string imageName, out string imgName)
        {
            imgName = JAXLib.JustStem(imageName);
            if (ImageLibrary.ContainsKey(imageName.ToLower()))
                return ImageLibrary[imageName.ToLower()];
            else
            {
                if (RegisterImage(imageName, imageName, out imgName) == 0)
                    return ImageLibrary[imageName.ToLower()];
                else
                    return null;
            }
        }


        /// <summary>
        /// Returns a SixLabors image if it exists, otherwise null if not found 
        /// </summary>
        /// <param name="imageName"></param>
        /// <returns></returns>
        public System.Drawing.Image? GetSDImage(string imageName, out string imgName)
        {
            imgName=JAXLib.JustStem(imageName);

            if (ImageLibrary.ContainsKey(imageName.ToLower()))
                return ConvertToSDImage(ImageLibrary[imageName.ToLower()]);
            else
                if (RegisterImage(imageName, imageName, out imgName) == 0)
                    return ConvertToSDImage(ImageLibrary[imgName]);
                else
                    return null;
        }


        /// <summary>
        /// Returns a SixLabors Image from the registry if it exists, otherwise null if not found - if nwidth=0, no resizing. if nheight=0, resizes to nwidth x nwidth.
        /// </summary>
        /// <param name="imageName"></param>
        /// <param name="nSize"></param>
        /// <returns>SixLabors.ImageSharp.Image?</returns>
        public SixLabors.ImageSharp.Image? GetImage(string imageName, int nwidth, int nheight = 0)
        {
            SixLabors.ImageSharp.Image? temp = null;
            nwidth = nwidth < 1 ? 0 : nwidth;
            nheight = nheight < 1 ? nwidth : nheight;
            if (ImageLibrary.ContainsKey(imageName.ToLower()))
            {
                try
                {
                    temp = ImageLibrary[imageName.ToLower()];

                    if (nwidth > 0)
                    {
                        if (nheight == 0)
                            temp.Mutate(ctx => ctx.Resize(nwidth, nwidth, true));
                        else
                            temp.Mutate(ctx => ctx.Resize(nwidth, nheight, true));
                    }
                }
                catch (Exception ex)
                {
                    App.DebugLog($"GetImage tossed an exception: {ex.Message}");
                    temp = null;
                }
            }

            return temp;
        }


        /*
         * Returns 0 for success
         *    501 - Image already exists
         *    599 - Internal error
         */
        public int AddImage(string imageName, System.Drawing.Image image)
        {
            int result = 0;
            return result;
        }

        /*
         * Returns 0 for success
         *    501 - Image already exists
         *    502 - Invalid image name
         *    599 - Internal error
         */
        public int AddImage(string imageName, SixLabors.ImageSharp.Image image)
        {
            int result = 0;
            imageName = imageName.ToLower().Trim();
            string testName = JAXLib.ChrTran(imageName, "abcdefghijklmnopqrstuvwxyz0123456790._-", "");

            if (testName.Length > 0 || string.IsNullOrWhiteSpace(imageName) || JAXLib.Between(imageName[0], 'a', 'z') == false)
                result = 519;
            else if (ImageLibrary.ContainsKey(imageName))
                result = 501;
            else
            {
                ImageLibrary.Add(imageName, image);
            }
            return result;
        }


        /*
         * Returns 0 for success
         *    502 - Image not found
         *    599 - Internal error
         */
        public int UpdateImage(string imageName, SixLabors.ImageSharp.Image image)
        {
            int result = 0;

            imageName = imageName.ToLower().Trim();
            if (ImageLibrary.ContainsKey(imageName))
                ImageLibrary[imageName] = image;
            else
                result = 502;

            return result;
        }

        /*
         * Returns 0 for success
         *      x - File already exists
         *    502 - Image does not exist
         *      x - No access or not authorized
         *      x - Media error
         *    599 - Internal error
         */
        public int SaveImage(string imageName, string fileName, bool overwrite)
        {
            int result = 0;
            string msg = string.Empty;

            imageName = imageName.ToLower().Trim();
            if (ImageLibrary.ContainsKey(imageName))
            {
                try
                {
                    ImageLibrary[imageName].Save(fileName);
                }
                catch (NotAuthorizedException ex) { result = 2222; msg = ex.Message; }
                catch (UnsupportedMediaTypeException ex) { result = 2225; msg = ex.Message; }
                catch (AccessViolationException ex) { result = 2226; msg = ex.Message; }
                catch (Exception ex) { result = 599; msg = ex.Message; }

                if (string.IsNullOrWhiteSpace(msg) == false)
                    App.DebugLog($"Failed to save image with error {result}: {msg}");
            }
            else
                result = 502;

            return result;
        }
    }
}
