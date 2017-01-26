using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(OnlineVoting.Startup))]
namespace OnlineVoting
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
