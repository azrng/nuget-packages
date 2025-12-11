using Azrng.SettingConfig.Service;
using Microsoft.AspNetCore.Http;
using System.Net.Http.Headers;
using System.Text;

namespace Azrng.SettingConfig.BasicAuthorization
{
    /// <summary>
    /// Represents Hangfire authorization filter for basic authentication.
    /// </summary>
    /// <remarks>If you are using this together with OWIN security, configure Hangfire BEFORE OWIN security configuration.</remarks>
    public class BasicAuthAuthorizationFilter : IDashboardAuthorizationFilter
    {
        private readonly BasicAuthAuthorizationFilterOptions _options;

        public BasicAuthAuthorizationFilter()
            : this(new BasicAuthAuthorizationFilterOptions())
        {
        }

        public BasicAuthAuthorizationFilter(BasicAuthAuthorizationFilterOptions options)
        {
            _options = options;
        }

        private bool Challenge(HttpContext context)
        {
            context.Response.StatusCode = 401;
            context.Response.Headers.Append("WWW-Authenticate", "Basic realm=\"Hangfire Dashboard\"");
            return false;
        }

        public bool Authorize(DashboardContext _context)
        {
            var context = _context.GetHttpContext();
            if (_options.SslRedirect && (context.Request.Scheme != "https"))
            {
                var redirectUri = new UriBuilder("https", context.Request.Host.ToString(), 443, context.Request.Path)
                    .ToString();

                context.Response.StatusCode = 301;
                context.Response.Redirect(redirectUri);
                return false;
            }

            if ((_options.RequireSsl == true) && (context.Request.IsHttps == false))
            {
                return false;
            }

            string header = context.Request.Headers["Authorization"];

            if (string.IsNullOrWhiteSpace(header) == false)
            {
                var authValues = AuthenticationHeaderValue.Parse(header);

                if ("Basic".Equals(authValues.Scheme, StringComparison.OrdinalIgnoreCase))
                {
                    string parameter = Encoding.UTF8.GetString(Convert.FromBase64String(authValues.Parameter));
                    var parts = parameter.Split(':');

                    if (parts.Length > 1)
                    {
                        var login = parts[0];
                        var password = parts[1];

                        if (string.IsNullOrWhiteSpace(login) == false &&
                            string.IsNullOrWhiteSpace(password) == false)
                        {
                            return _options
                                       .Users
                                       .Any(user => user.Validate(login, password, _options.LoginCaseSensitive))
                                   || Challenge(context);
                        }
                    }
                }
            }

            return Challenge(context);
        }
    }
}