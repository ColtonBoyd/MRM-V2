using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(MRMV2.Startup))]
namespace MRMV2
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
