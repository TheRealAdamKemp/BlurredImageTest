using Xamarin.Forms;

namespace BlurredImageTest
{
    public class App : Application
    {
        public App()
        {
            // The root page of your application
            MainPage = new ContentPage {
                Content = new BlurredImage { Source = new FileImageSource { File = "dog_and_monkeys.png" } }
            };
        }
    }
}

