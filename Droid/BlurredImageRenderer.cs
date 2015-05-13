using System;
using System.IO;
using System.Threading.Tasks;
using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Renderscripts;
using Android.Widget;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

using BlurredImageTest;
using BlurredImageTest.Droid;

[assembly: ExportRenderer(typeof(BlurredImage), typeof(BlurredImageRenderer))]

namespace BlurredImageTest.Droid
{
    public class BlurredImageRenderer : ViewRenderer<BlurredImage, ImageView>
    {
        private bool _isDisposed;

        protected override void OnElementChanged(ElementChangedEventArgs<BlurredImage> e)
        {
            base.OnElementChanged(e);

            if (Control == null)
            {
                var imageView = new ImageView(Context);
                SetNativeControl(imageView);
            }

            UpdateBitmap(e.OldElement);
            UpdateAspect();
        }

        protected override void OnElementPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);

            if (e.PropertyName == Image.SourceProperty.PropertyName)
            {
                UpdateBitmap(null);
                return;
            }
            if (e.PropertyName == Image.AspectProperty.PropertyName)
            {
                UpdateAspect();
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (_isDisposed)
            {
                return;
            }
            _isDisposed = true;
            BitmapDrawable bitmapDrawable;
            if (disposing && Control != null && (bitmapDrawable = (Control.Drawable as BitmapDrawable)) != null)
            {
                Bitmap bitmap = bitmapDrawable.Bitmap;
                if (bitmap != null)
                {
                    bitmap.Recycle();
                    bitmap.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        private void UpdateAspect()
        {
            using (ImageView.ScaleType scaleType = ToScaleType(Element.Aspect))
            {
                Control.SetScaleType(scaleType);
            }
        }

        private static ImageView.ScaleType ToScaleType(Aspect aspect)
        {
            switch (aspect)
            {
                case Aspect.AspectFill:
                    return ImageView.ScaleType.CenterCrop;
                case Aspect.Fill:
                    return ImageView.ScaleType.FitXy;
            }
            return ImageView.ScaleType.FitCenter;
        }

        private async void UpdateBitmap(Image previous = null)
        {
            Bitmap bitmap = null;
            ImageSource source = Element.Source;
            if (previous == null || !object.Equals(previous.Source, Element.Source))
            {
//                ((IElementController)base.Element).SetValueFromRenderer(Image.IsLoadingPropertyKey, true);
//                ((FormsImageView)base.Control).SkipInvalidate();
                Control.SetImageResource(17170445);
                if (source != null)
                {
                    try
                    {
                        bitmap = await GetImageFromImageSource(source, Context);
                    }
                    catch (TaskCanceledException)
                    {
                    }
                    catch (IOException)
                    {
                    }
                    catch (NotImplementedException)
                    {
                    }
                }
                if (Element != null && object.Equals(Element.Source, source))
                {
                    if (!_isDisposed)
                    {
                        Control.SetImageBitmap(bitmap);
                        if (bitmap != null)
                        {
                            bitmap.Dispose();
                        }
//                        ((IElementController)base.Element).SetValueFromRenderer(Image.IsLoadingPropertyKey, false);
                        ((IVisualElementController)base.Element).NativeSizeChanged();
                    }
                }
            }
        }

        private async Task<Bitmap> GetImageFromImageSource(ImageSource imageSource, Context context)
        {
            IImageSourceHandler handler;

            if (imageSource is FileImageSource)
            {
                handler = new FileImageSourceHandler();
            }
            else if (imageSource is StreamImageSource)
            {
                handler = new StreamImagesourceHandler(); // sic
            }
            else if (imageSource is UriImageSource)
            {
                handler = new ImageLoaderSourceHandler(); // sic
            }
            else
            {
                throw new NotImplementedException();
            }

            var originalBitmap = await handler.LoadImageAsync(imageSource, context);

            var blurredBitmap = await Task.Run(() => CreateBlurredImage(originalBitmap, 25));

            return blurredBitmap;
        }

        private Bitmap CreateBlurredImage(Bitmap originalBitmap, int radius)
        {
            // Create another bitmap that will hold the results of the filter.
            Bitmap blurredBitmap;
            blurredBitmap = Bitmap.CreateBitmap(originalBitmap);

            // Create the Renderscript instance that will do the work.
            RenderScript rs = RenderScript.Create(Context);

            // Allocate memory for Renderscript to work with
            Allocation input = Allocation.CreateFromBitmap(rs, originalBitmap, Allocation.MipmapControl.MipmapFull, AllocationUsage.Script);
            Allocation output = Allocation.CreateTyped(rs, input.Type);

            // Load up an instance of the specific script that we want to use.
            ScriptIntrinsicBlur script = ScriptIntrinsicBlur.Create(rs, Android.Renderscripts.Element.U8_4(rs));
            script.SetInput(input);

            // Set the blur radius
            script.SetRadius(radius);

            // Start Renderscript working.
            script.ForEach(output);

            // Copy the output to the blurred bitmap
            output.CopyTo(blurredBitmap);

            return blurredBitmap;
        }
    }
}

