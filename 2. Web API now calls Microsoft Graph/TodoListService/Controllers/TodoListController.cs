/*
 The MIT License (MIT)

Copyright (c) 2018 Microsoft Corporation

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
 */
#define ENABLE_OBO
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Client;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using TodoListService.Models;
using Microsoft.Graph;
using TodoListService.Extensions;

namespace TodoListService.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    public class TodoListController : Controller
    {
        private readonly IGraphServiceClient _graphServiceClient;
        static readonly ConcurrentBag<TodoItem> TodoStore = new ConcurrentBag<TodoItem>();

        public TodoListController(IGraphServiceClient graphServiceClient)
        {
            _graphServiceClient = graphServiceClient;
        }

        // GET: api/values
        [HttpGet]
        public IEnumerable<TodoItem> Get()
        {
            string owner = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return TodoStore.Where(t => t.Owner == owner).ToList();
        }

        // POST api/values
        [HttpPost]
        public async void Post([FromBody]TodoItem todo)
        {
            string owner = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            string ownerName;
#if ENABLE_OBO
            // This is a synchronous call, so that the clients know, when they call Get, that the 
            // call to the downstream API (Microsoft Graph) has completed.
            try
            {
                // Call graph to get a user.
                User me = _graphServiceClient.Me.Request().WithUser(HttpContext).GetAsync().GetAwaiter().GetResult();
                ownerName = me.UserPrincipalName;

                string title = string.IsNullOrWhiteSpace(ownerName) ? todo.Title : $"{todo.Title} ({ownerName})";
                TodoStore.Add(new TodoItem { Owner = owner, Title = title });
            }
            catch (MsalException ex)
            {
                HttpContext.Response.ContentType = System.Net.Mime.MediaTypeNames.Text.Plain;
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                await HttpContext.Response.WriteAsync("An authentication error occurred while acquiring a token for downstream API\n" + ex.ErrorCode + "\n" + ex.Message);
            }
            catch (Exception ex)
            {
                HttpContext.Response.ContentType = System.Net.Mime.MediaTypeNames.Text.Plain; ;
                HttpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                await HttpContext.Response.WriteAsync("An error occurred while calling the downstream API\n" + ex.Message);
            }
#endif
        }
    }
}
