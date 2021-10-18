using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(DisplayMonkey.Startup))]
namespace DisplayMonkey
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            
            ConfigureAuth(app);
        }
    }
}
