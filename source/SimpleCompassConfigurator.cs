using Bindito.Core;

namespace SimpleCompass
{
    [Context("Game")]
    [Context("MapEditor")]
    public class SimpleCompassConfigurator : Configurator
    {
        protected override void Configure()
        {
            Bind<CompassScaleSetting>().AsSingleton();
            Bind<SimpleCompassWidget>().AsSingleton();
            Bind<CompassSizeSettingsSlider>().AsSingleton();
        }
    }
}
