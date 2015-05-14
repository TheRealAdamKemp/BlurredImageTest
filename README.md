BlurredImage for Xamarin.Forms
==============================

This is a demonstration of writing a custom renderer for a blurred image for Xamarin.Forms on iOS and Android. I didn't implement Windows Phone because I don't know how, but pull requests are welcome!

Screenshots
===========

![iOS Screenshot](./iOSScreenshot.png)
![Android Screenshot](./AndroidScreenshot.png)

iOS Implementation
==================

iOS uses a gaussian blur from CoreImage. Since this can take a bit of time I do the blur on a background thread. I was able to just create a subclass of `UIImageView` and override the `Image` property setter to do this automatically, and therefore I was able to subclass and reuse most of the base `ImageRenderer` class.

Android Implementation
======================

The core of the Android blurring implementation came from a [Xamarin recipe](http://developer.xamarin.com/recipes/android/other_ux/drawing/blur_an_image_with_renderscript/). The rest of the renderer is basically a copy/paste of the stock `ImageRenderer` because unfortunately the default image renderer is not designed well for subclassing. It uses a custom, internal `ImageView` subclass, which makes it difficult to use the same trick I used on iOS. It also doesn't provide any hooks for replacing the image in-flight. Therefore I made a whole new renderer that just did everything that the base renderer does. It relies on a bit of reflection to handle the setting of the `IsLoading` property on the `Image` element, which sucks, but that's the best I could come up with.

Obviously this is brittle, and I'd like to see the base renderer become more extensible in the future.
