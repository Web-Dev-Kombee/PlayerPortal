using Shard.Commons;

namespace Shared.Broker.Internal
{
    public class IocBrokerWrapper : IBroker
    {
        public IocBrokerWrapper(ITypeScanner scanner, IServiceProvider iocContainer)
        {
            m_scanner = scanner;
            m_iocContainer = iocContainer;
        }

        ITypeScanner m_scanner;
        IServiceProvider m_iocContainer;
        IBroker m_broker;

        object m_init = new object();
        IBroker Broker
        {
            get
            {
                if (m_broker == null)
                    lock (m_init)
                        if (m_broker == null)
                            m_broker = new Broker(m_scanner, typeMap => typeMap.AddDynamic(m_iocContainer.GetService, ignoreEnumerables: true));

                return m_broker;
            }
        }

        public bool HasHandler<TInput>() => Broker.HasHandler<TInput>();
        public TReturn Request<TReturn>(IBrokeredReturns<TReturn> with) => Broker.Request(with);
        public Task<TReturn> RequestAsync<TReturn>(IBrokeredAsyncReturns<TReturn> with) => Broker.RequestAsync(with);
        public Task<TReturn> RequestAsync<TReturn>(IBrokeredAsyncReturns<TReturn> with, CancellationToken token) => Broker.RequestAsync(with, token);
        public void Dispatch<TInput>(TInput notification) where TInput : IBrokeredNotice => Broker.Dispatch(notification);
        public Task DispatchAsync<TInput>(TInput notification) where TInput : IBrokeredNoticeAsync => Broker.DispatchAsync(notification);
        public Task DispatchAsync<TInput>(TInput notification, CancellationToken token) where TInput : IBrokeredNoticeAsync => Broker.DispatchAsync(notification, token);
        public void Do<TInput>(TInput with) where TInput : IBrokered => Broker.Do(with);
        public Task DoAsync<TInput>(TInput with) where TInput : IBrokeredAsync => Broker.DoAsync(with);
        public Task DoAsync<TInput>(TInput with, CancellationToken token) where TInput : IBrokeredAsync => Broker.DoAsync(with, token);
        public void Run<TData>(TData pipeline) where TData : IBrokeredPipeline => Broker.Run(pipeline);
        public Task RunAsync<TData>(TData pipeline) where TData : IBrokeredPipelineAsync => Broker.RunAsync(pipeline);
        public Task RunAsync<TData>(TData pipeline, CancellationToken token) where TData : IBrokeredPipelineAsync => Broker.RunAsync(pipeline, token);
        public Result<List<Type>> ValidateHandlers(ITypeScanner typeScanner) => Broker.ValidateHandlers(typeScanner);
    }
}
