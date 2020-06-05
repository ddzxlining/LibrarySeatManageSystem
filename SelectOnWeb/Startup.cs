using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(SelectOnWeb.Startup))]
namespace SelectOnWeb
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
