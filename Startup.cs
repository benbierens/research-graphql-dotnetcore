using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HotChocolate;
using HotChocolate.Execution;
using HotChocolate.AspNetCore;
using HotChocolate.AspNetCore.Subscriptions;
using HotChocolate.Types;
using HotChocolate.Subscriptions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;

namespace graphql_web
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddInMemorySubscriptions();
            services.AddInMemorySubscriptionProvider();

            services
                .AddGraphQL(sp => SchemaBuilder.New()
                    .AddQueryType<Query>()
                    .AddMutationType<Mutation>()
                    .AddSubscriptionType<Subscription>()
                    .AddServices(sp)
                    .Create());
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseWebSockets();

            app.UseGraphQL();

            app.UseGraphQLSubscriptions();

            // app.UseRouting();

            // app.UseEndpoints(endpoints =>
            // {
            //     endpoints.MapGet("/", async context =>
            //     {
            //         await context.Response.WriteAsync("Hello World!");
            //     });
            // });
        }
    }
}
