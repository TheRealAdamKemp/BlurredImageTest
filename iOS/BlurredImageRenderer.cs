using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

using BlurredImageTest;
using BlurredImageTest.iOS;

[assembly: ExportRenderer(typeof(BlurredImage), typeof(BlurredImageRenderer))]

namespace BlurredImageTest.iOS
{
    public class BlurredImageRenderer : ImageRenderer
    {
        private UIVisualEffectView _effectView;

        protected override void OnElementChanged(ElementChangedEventArgs<Image> e)
        {
            base.OnElementChanged(e);

            if (_effectView == null)
            {
                _effectView = new UIVisualEffectView(UIBlurEffect.FromStyle(UIBlurEffectStyle.Light));
                AddSubview(_effectView);
            }
        }

        public override void LayoutSubviews()
        {
            base.LayoutSubviews();

            _effectView.Frame = Bounds;
        }
    }
}

