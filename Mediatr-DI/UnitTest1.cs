using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Mediatr_DI
{
    public class UnitTest1
    {
        [Fact]
        public async Task Test1()
        {
            var services = new ServiceCollection();
            services.AddMediatR(GetType().Assembly);
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(AlwaysTrueBehaviour<,>));

            var mediatr = services.BuildServiceProvider().GetRequiredService<IMediator>();

            var result = await mediatr.Send(new ShouldBeTrueRequest());
            Assert.True(result.Prop);

            var result2 = await mediatr.Send(new ShouldBeFalseRequest());
            Assert.False(result2.Prop);
        }

        public interface IAlwaysTrueRequest<out T> : IRequest<T>
        {
            bool Prop { get; set; }
        }

        public class ShouldBeTrueRequest : IAlwaysTrueRequest<Response>
        {
            public bool Prop { get; set; }
        }

        public class ShouldBeFalseRequest : IRequest<Response>
        {
            public bool Prop { get; set; }
        }

        public class Response
        {
            public bool Prop { get; set; }
        }

        public class TrueHandler : IRequestHandler<ShouldBeTrueRequest, Response>
        {
            public Task<Response> Handle(ShouldBeTrueRequest shouldBeTrueRequest, CancellationToken cancellationToken)
            {
                return Task.FromResult(new Response
                {
                    Prop = shouldBeTrueRequest.Prop
                });
            }
        }

        public class FalseHandler : IRequestHandler<ShouldBeFalseRequest, Response>
        {
            public Task<Response> Handle(ShouldBeFalseRequest request, CancellationToken cancellationToken)
            {
                return Task.FromResult(new Response
                {
                    Prop = request.Prop
                });
            }
        }

        public class AlwaysTrueBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
          where TRequest : IAlwaysTrueRequest<TResponse>
        {
            public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<TResponse> next)
            {
                request.Prop = true;

                return await next();
            }
        }
    }
}
