using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(StackExchange.Startup))]
namespace StackExchange
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
            System.Globalization.CultureInfo.DefaultThreadCurrentCulture = new System.Globalization.CultureInfo("en-US");
        }
    }
}
