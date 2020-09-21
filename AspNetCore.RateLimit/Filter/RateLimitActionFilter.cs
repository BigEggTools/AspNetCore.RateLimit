using BigEgg.AspNetCore.RateLimit.Models;
using BigEgg.AspNetCore.RateLimit.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.HttpActionResultsExtension;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace BigEgg.AspNetCore.RateLimit.Filter
{
    public class RateLimitActionFilter : IAsyncActionFilter
    {
        private readonly IRateLimitService _rateLimitService;
        private readonly ILogger<RateLimitActionFilter> _logger;
        private readonly RateLimitArgument _argument;

        public RateLimitActionFilter(IRateLimitService rateLimitService, ILogger<RateLimitActionFilter> logger, RateLimitArgument argument)
        {
            if (argument.RateLimitType == RateLimitType.ViaParameter && string.IsNullOrWhiteSpace(argument.Metadata))
            {
                throw new ArgumentNullException(nameof(argument), "Argument metadata should not be empty when rate limit via query parameter.");
            }

            _rateLimitService = rateLimitService;
            _logger = logger;
            _argument = argument;
        }

        /// <summary>
        /// Called asynchronously before the action, after model binding is complete.
        /// </summary>
        /// <param name="context">The <see cref="ActionExecutingContext" />.</param>
        /// <param name="next">The <see cref="ActionExecutionDelegate" />. Invoked to execute the next action filter or the action itself.</param>
        /// <returns>
        /// A <see cref="Task" /> that on completion indicates the filter has executed.
        /// </returns>
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var identity = PrepareIdentity(_argument, context);

            if (!(await _rateLimitService.ProcessRequestAsync(identity, new RateLimitRule(_argument))))
            {
                context.Result = new TooManyRequestsResult(_argument.Limit, _argument.Period);
            }
            else
            {
                await next();
            }
        }

        private RequestIdentity PrepareIdentity(RateLimitArgument argument, ActionContext context)
        {
            var identity = string.Empty;

            if (argument.RateLimitType == RateLimitType.ViaIP)
            {
                _logger.LogInformation("PrepareIdentity - Get client's ip address as identity");
                identity = context.HttpContext.Connection.RemoteIpAddress.ToString();
            }
            else if (argument.RateLimitType == RateLimitType.ViaParameter)
            {
                _logger.LogInformation("PrepareIdentity - Get client's parameter values as identity");
                var names = argument.Metadata.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var name in names)
                {
                    var value = GetParameterValue(context, name);
                    if (string.IsNullOrWhiteSpace(value))
                    {
                        _logger.LogError($"Paramter name {name} should exist in query or route when rate limit via query parameter.");
                        throw new ArgumentNullException(nameof(argument), $"Paramter name {name} should exist in query or route when rate limit via query parameter.");
                    }

                    identity += value;
                }
            }

            return new RequestIdentity()
            {
                Identity = identity,
                Path = context.HttpContext.Request.Path.ToString().ToLowerInvariant(),
                HttpVerb = context.HttpContext.Request.Method.ToLowerInvariant(),
            };
        }

        private string GetParameterValue(ActionContext context, string name)
        {
            if (context.HttpContext.Request.Query.ContainsKey(name))
            {
                return context.HttpContext.Request.Query[name];
            }

            if (context.RouteData.Values.ContainsKey(name))
            {
                return context.RouteData.Values[name].ToString().ToLowerInvariant();
            }

            return string.Empty;
        }
    }
}
