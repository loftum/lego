using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace LCTP.Core.Routing
{
    public abstract class BaseController: IController
    {
        protected readonly ISet<Route> Routes = new HashSet<Route>();

        protected void Get(string route, Func<RequestMessage, Match, Task<ResponseMessage>> handler)
        {
            Add("get", route, handler);
        }

        protected void Set(string route, Func<RequestMessage, Match, Task<ResponseMessage>> handler)
        {
            Add("set", route, handler);
        }

        protected void Add(string method, string route, Func<RequestMessage, Match, Task<ResponseMessage>> handler)
        {
            Routes.Add(new Route(method, route, handler));
        }

        public Task<ResponseMessage> Execute(RequestMessage request)
        {
            var route = Routes.Select(r => r.GetHandler(request)).FirstOrDefault(h => h != null);
            return route == null
                ? NotFound()
                : route(request);
        }

        private static Task<ResponseMessage> NotFound()
        {
            return Task.FromResult(new ResponseMessage
            {
                StatusCode = 404
            });
        }

        public abstract void ConnectionClosed();
        public abstract void ConnectionOpened();
    }
}