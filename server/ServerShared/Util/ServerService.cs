using System;
using LightInject;
using ServerShared.Service;
using ServerShared.NetworkHandler;

namespace ServerShared.Util
{
    public static class ServerService
    {
        private static ServiceContainer container;

        public class SingletoneLifetime : ILifetime
        {
            object instances;

            public object GetInstance(Func<object> instanceFactory, Scope currentScope)
            {
                if (instances == null)
                {
                    instances = instanceFactory();
                }
                return instances;
            }
        }

        public static bool Register()
        {
            container = new ServiceContainer();
            container.EnableAnnotatedPropertyInjection();

            // 이렇게 등록되어있는 Service 끼리만 Inject Annotation이 먹힌다. 
            // Service로 사용할 인스턴스라면, new 대신 GetInstance로 생성하거나, Inject를 사용해야 함에 주의하라.
            container.Register<ChannelService>(new SingletoneLifetime());
            container.Register<SessionService>(new SingletoneLifetime());
            container.Register<ServerDispatcher>(new SingletoneLifetime());
            container.Register<ServerHandler>(new SingletoneLifetime());

            PostConstruct();
            return true;
        }

        public static void PostConstruct()
        {
            var serverDispatcher = GetInstance<ServerDispatcher>();
            var channelService = GetInstance<ChannelService>();
            channelService.MessageWorker = serverDispatcher.MessageWorker;
        }

        public static TService GetInstance<TService>()
        {
            return container.GetInstance<TService>();
        }
    }
}
