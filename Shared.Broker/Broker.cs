using Shard.Commons;
using Shared.Broker.Internal;
using System.Collections.Concurrent;
using System.Reflection;

namespace Shared.Broker
{
    public delegate object ResolveOne(Type messageType);
    public delegate IEnumerable<object> ResolveMany(Type messageType);

    public class Broker : IBroker
    {
        public Broker(ITypeScanner typeScanner, Action<ITypeMapper> provider = null)
        {
            var resolver = new HandlerResolver();
            resolver.LoadAll(typeScanner, mapper =>
            {
                mapper.Add<IBroker>(this); //Always resolve requests for IBroker to this instance
                provider?.Invoke(mapper); //Delegate anything else to the user provided method
            });
            m_singleResolver = resolver.SingleResolver;
            m_manyResolver = resolver.ManyResolver;
        }

        public Broker(HandlerResolver resolver)
        {
            if (resolver == null) throw new ArgumentNullException(nameof(resolver));

            m_singleResolver = resolver.SingleResolver;
            m_manyResolver = resolver.ManyResolver;
        }

        public Broker(ResolveOne singleResolver, ResolveMany multiResolver)
        {
            if (singleResolver == null) throw new ArgumentNullException(nameof(singleResolver));
            if (multiResolver == null) throw new ArgumentNullException(nameof(multiResolver));

            m_singleResolver = singleResolver;
            m_manyResolver = multiResolver;
        }

        ResolveOne m_singleResolver;
        ResolveMany m_manyResolver;

        //Cache's constructed caller proxies to handle returning calls
        readonly ConcurrentDictionary<Type, object> m_callerCache = new ConcurrentDictionary<Type, object>();

        /// <summary>
        /// When called, validates that every IBrokered/IBrokeredReturns (and async versions) have a corresponding handler registered.
        /// </summary>
        /// <param name="typeScanner">The type scanner to use to locate all questions.</param>
        public Result<List<Type>> ValidateHandlers(ITypeScanner typeScanner)
        {
            var ibrokered = typeof(IBrokered);
            var ibrokeredasync = typeof(IBrokeredAsync);
            var ibrokeredreturns = typeof(IBrokeredReturns<>);
            var ibrokeredasyncreturns = typeof(IBrokeredAsyncReturns<>);
            var ioptional = typeof(IBrokeredHandlerIsOptional);

            var questions = typeScanner.Search(type =>
            {
                if (type.IsGenericTypeDefinition) return false; //We can't check questions with generic arguments because we don't know what they'd be called with
                var faces = type.GetAllInterfaces();
                return faces.Any(i =>
                {
                    if (i == ibrokered || i == ibrokeredasync) return true;
                    if (!i.IsConstructedGenericType) return false;
                    var gen = i.GetGenericTypeDefinition();
                    return gen == ibrokeredreturns || gen == ibrokeredasyncreturns;
                })
                && !faces.Any(i => i == ioptional); //Ignore anything that's optional.
            });

            List<Type> noHandlers = new List<Type>();
            foreach (var type in questions.Select(v => v.AsType()))
            {
                //Some questions are handled by providers; so we need to check both types.
                if (m_singleResolver(type) == null && m_manyResolver(type) == null)
                    noHandlers.Add(type);
            }

            if (noHandlers.Count == 0) return true;
            else return Results.Failure(noHandlers);
        }

        /// <summary>
        /// Checks if a given input type will be handled (can be used to see if an optional handler is registered or not)
        /// </summary>
        /// <typeparam name="TInput">The input/request type to check for</typeparam>
        /// <returns>True if this type can be handled, false if not</returns>
        public bool HasHandler<TInput>()
        {
            if (m_singleResolver(typeof(TInput)) != null) return true; //Is handled by a single handler
            var handlers = m_manyResolver(typeof(TInput));
            return handlers != null && handlers.Any(); //Must have at least one handler registered to count if it's a multi
        }

        /// <summary>
        /// Locates the handler for a given message on a one-to-one basis and executes it
        /// </summary>
        /// <typeparam name="TInput">The type of message to handle</typeparam>
        /// <param name="with">The message content to handle</param>
        public void Do<TInput>(TInput with) where TInput : IBrokered
        {
            var handler = GetHandler(with);
            handler.Handle(with);
        }

        /// <summary>
        /// Locates the handler for a given message on a one-to-one basis and then returns a Task representing it's asynchronous execution
        /// </summary>
        /// <typeparam name="TInput">The type of message to handle</typeparam>
        /// <param name="with">The message content to handle</param>
        /// <returns>A task representing the action</returns>
        public Task DoAsync<TInput>(TInput with) where TInput : IBrokeredAsync
        {
            return DoAsync(with, CancellationToken.None);
        }

        /// <summary>
        /// Locates the handler for a given message on a one-to-one basis and then returns a Task representing it's asynchronous execution
        /// </summary>
        /// <typeparam name="TInput">The type of message to handle</typeparam>
        /// <param name="with">The message content to handle</param>
        /// <param name="token">A cancellation token to pass to the handler</param>
        /// <returns>A task representing the action</returns>
        public Task DoAsync<TInput>(TInput with, CancellationToken token) where TInput : IBrokeredAsync
        {
            var handler = GetAsyncHandler(with);
            return handler.Handle(with, token);
        }

        /// <summary>
        /// Locates the handler for a given message on a one-to-one basis and executes it
        /// </summary>
        /// <typeparam name="TReturn">The type of value that will be returned from the handler</typeparam>
        /// <param name="with">The message content to handle</param>
        /// <returns>The result of the action</returns>
        public TReturn Request<TReturn>(IBrokeredReturns<TReturn> with)
        {
            var caller = GetCaller(with);
            var handler = GetReturningHandler(with);
            return caller.Handle(with, handler);
        }

        /// <summary>
        /// Locates the handler for a given message on a one-to-one basis and then returns a Task representing it's asynchronous execution
        /// </summary>
        /// <typeparam name="TReturn">The type of value that will be returned from the handler</typeparam>
        /// <param name="with">The message content to handle</param>
        /// <returns>A task representing the action</returns>
        public Task<TReturn> RequestAsync<TReturn>(IBrokeredAsyncReturns<TReturn> with)
        {
            return RequestAsync(with, CancellationToken.None);
        }

        /// <summary>
        /// Locates the handler for a given message on a one-to-one basis and then returns a Task representing it's asynchronous execution
        /// </summary>
        /// <typeparam name="TReturn"></typeparam>
        /// <param name="with">The message content to handle</param>
        /// <param name="token">A cancellation token to pass to the handler</param>
        /// <returns>A task representing the action</returns>
        public Task<TReturn> RequestAsync<TReturn>(IBrokeredAsyncReturns<TReturn> with, CancellationToken token)
        {
            var caller = GetCaller(with);
            var handler = GetReturningHandler(with);
            return caller.Handle(with, handler, token);
        }

        /// <summary>
        /// Dispatches a notification to zero or many handlers
        /// </summary>
        /// <typeparam name="TInput">The type of notification to handle</typeparam>
        /// <param name="notification">The notification to send</param>
        public void Dispatch<TInput>(TInput notification) where TInput : IBrokeredNotice
        {
            var handlers = GetHandlers(notification);
            foreach (var handler in handlers)
                handler.Receive(notification);
        }

        /// <summary>
        /// Dispatches a notification to zero or many handlers
        /// </summary>
        /// <typeparam name="TInput">The type of notification to handle</typeparam>
        /// <param name="notification">The notification to send</param>
        /// <returns>A task representing the completion of all handlers</returns>
        public Task DispatchAsync<TInput>(TInput notification) where TInput : IBrokeredNoticeAsync
        {
            return DispatchAsync(notification, CancellationToken.None);
        }

        /// <summary>
        /// Dispatches a notification to zero or many handlers
        /// </summary>
        /// <typeparam name="TInput">The type of notification to handle</typeparam>
        /// <param name="notification">The notification to send</param>
        /// <param name="token">A cancellation token to pass to the handlers</param>
        /// <returns>A task representing the completion of all handlers</returns>
        public Task DispatchAsync<TInput>(TInput notification, CancellationToken token) where TInput : IBrokeredNoticeAsync
        {
            var tasks = GetAsyncHandlers(notification).Select(v => v.Receive(notification, token));
            return Task.WhenAll(tasks);
        }

        /// <summary>
        /// Runs data sequentially through a pipeline, where each step can alter the content
        /// </summary>
        /// <typeparam name="TData">The type of pipeline data to run</typeparam>
        /// <param name="pipeline">The data to feed the pipeline</param>
        public void Run<TData>(TData pipeline) where TData : IBrokeredPipeline
        {
            var handlers = GetPipeline(pipeline);
            foreach (var handler in handlers.OrderBy(v => v.OrderPreference).ThenBy(v => v.GetType().Name))
                if (handler.Build(pipeline) == PipelineExecution.Abort) break;
        }

        /// <summary>
        /// Runs data sequentially through a pipeline, where each step can alter the content.
        /// Each step will be run sequentially as an asynchronous task
        /// </summary>
        /// <typeparam name="TData">The type of pipeline data to run</typeparam>
        /// <param name="pipeline">The data to feed the pipeline</param>
        /// <returns>A Task representing the execution of the pipeline</returns>
        public Task RunAsync<TData>(TData pipeline) where TData : IBrokeredPipelineAsync
        {
            return RunAsync(pipeline, CancellationToken.None);
        }

        /// <summary>
        /// Runs data sequentially through a pipeline, where each step can alter the content.
        /// Each step will be run sequentially as an asynchronous task
        /// </summary>
        /// <typeparam name="TData">The type of pipeline data to run</typeparam>
        /// <param name="pipeline">The data to feed the pipeline</param>
        /// <param name="token">A cancellation token to pass to the handlers</param>
        /// <returns>A Task representing the execution of the pipeline</returns>
        public async Task RunAsync<TData>(TData pipeline, CancellationToken token) where TData : IBrokeredPipelineAsync
        {
            var handlers = GetAsyncPipeline(pipeline);
            foreach (var handler in handlers.OrderBy(v => v.OrderPreference).ThenBy(v => v.GetType().Name))
                if (await handler.Build(pipeline, token).ConfigureAwait(false) == PipelineExecution.Abort) break;
        }

        ////////////////////////////////

        private IBrokeredHandler<TInput> GetHandler<TInput>(TInput with) where TInput : IBrokered
        {
            var handler = Locate(typeof(TInput), typeof(IBrokeredHandler<TInput>)) as IBrokeredHandler<TInput>;
            if (handler == null && !(with is IBrokeredHandlerIsOptional)) throw new NullReferenceException($"Failed to locate a one-to-one handler for {typeof(TInput).Name}.");
            return handler;
        }

        private IBrokeredAsyncHandler<TInput> GetAsyncHandler<TInput>(TInput with) where TInput : IBrokeredAsync
        {
            var handler = Locate(typeof(TInput), typeof(IBrokeredAsyncHandler<TInput>)) as IBrokeredAsyncHandler<TInput>;
            if (handler == null && !(with is IBrokeredHandlerIsOptional)) throw new NullReferenceException($"Failed to locate a one-to-one handler for {typeof(TInput).Name}.");
            return handler;
        }
     

        ////////////////////////////////


        private ICaller<TReturn> GetCaller<TReturn>(IBrokeredReturns<TReturn> with)
        {
            var inType = with.GetType();
            var caller = m_callerCache.GetOrAdd(inType, (t) => Activator.CreateInstance(typeof(Caller<,>).MakeGenericType(inType, typeof(TReturn))));
            return caller as ICaller<TReturn>;
        }

        private object GetReturningHandler<TReturn>(IBrokeredReturns<TReturn> with)
        {
            var inType = with.GetType();
            var callType = typeof(IBrokeredHandler<,>).MakeGenericType(inType, typeof(TReturn));
            var handler = Locate(inType, callType);
            if (handler != null) return handler;

            callType = typeof(IBrokeredProvider<,>).MakeGenericType(inType, typeof(TReturn));
            var handlers = LocateAll(inType, callType).ToList();
            if (handlers.Count == 0 && !(with is IBrokeredHandlerIsOptional)) throw new NullReferenceException($"Failed to locate a suitable handler for {inType.Name}.");
            return handlers;
        }

        private IAsyncCaller<TReturn> GetCaller<TReturn>(IBrokeredAsyncReturns<TReturn> with)
        {
            var inType = with.GetType();
            var caller = m_callerCache.GetOrAdd(inType, (t) => Activator.CreateInstance(typeof(AsyncCaller<,>).MakeGenericType(inType, typeof(TReturn))));
            return caller as IAsyncCaller<TReturn>;
        }

        private object GetReturningHandler<TReturn>(IBrokeredAsyncReturns<TReturn> with)
        {
            var inType = with.GetType();
            var callType = typeof(IBrokeredAsyncHandler<,>).MakeGenericType(inType, typeof(TReturn));
            var handler = Locate(inType, callType);
            if (handler != null) return handler;

            callType = typeof(IBrokeredAsyncProvider<,>).MakeGenericType(inType, typeof(TReturn));
            var handlers = LocateAll(inType, callType).ToList();
            if (handlers.Count == 0 && !(with is IBrokeredHandlerIsOptional)) throw new NullReferenceException($"Failed to locate a suitable handler for {inType.Name}.");
            return handlers;
        }

        ////////////////////////////////

        private IEnumerable<IBrokeredNoticeReceiver<TInput>> GetHandlers<TInput>(TInput notification) where TInput : IBrokeredNotice
        {
            return LocateAll(typeof(TInput), typeof(IBrokeredNoticeReceiver<TInput>)).Cast<IBrokeredNoticeReceiver<TInput>>();
        }

        private IEnumerable<IBrokeredNoticeAsyncReceiver<TInput>> GetAsyncHandlers<TInput>(TInput notification) where TInput : IBrokeredNoticeAsync
        {
            return LocateAll(typeof(TInput), typeof(IBrokeredNoticeAsyncReceiver<TInput>)).Cast<IBrokeredNoticeAsyncReceiver<TInput>>();
        }

        ////////////////////////////////

        private IEnumerable<IBrokeredPipelineBuilder<TData>> GetPipeline<TData>(TData notification) where TData : IBrokeredPipeline
        {
            return LocateAll(typeof(TData), typeof(IBrokeredPipelineBuilder<TData>)).Cast<IBrokeredPipelineBuilder<TData>>();
        }

        private IEnumerable<IBrokeredPipelineAsyncBuilder<TData>> GetAsyncPipeline<TData>(TData notification) where TData : IBrokeredPipelineAsync
        {
            return LocateAll(typeof(TData), typeof(IBrokeredPipelineAsyncBuilder<TData>)).Cast<IBrokeredPipelineAsyncBuilder<TData>>();
        }

        ////////////////////////////////

        private object Locate(Type dataType, Type callType)
        {
            //Finds a handler for a specified input type. The registered handler must match the callType.
            //A missing handler, or if the handler uses the wrong type, an exception will throw.
            var handler = m_singleResolver(dataType);
            if (handler == null) return null;
            if (!callType.GetTypeInfo().IsAssignableFrom(handler.GetType().GetTypeInfo()))
                throw new NotSupportedException($"The handler {handler.GetType().Name} doesn't implement {callType.Name} for {dataType.Name}.");
            return handler;
        }

        private IEnumerable<object> LocateAll(Type dataType, Type callType)
        {
            //Finds many handlers for a specified input type. The registered handlers must match the callType.
            //Registered handlers that don't match will throw an error; no exception will be generated if there are no items.
            var handlers = m_manyResolver(dataType);
            if (handlers == null) yield break;
            var callTypeInfo = callType.GetTypeInfo();
            foreach (var handler in handlers)
            {
                if (handler == null) continue;
                if (!callTypeInfo.IsAssignableFrom(handler.GetType().GetTypeInfo()))
                    throw new NotSupportedException($"The handler {handler.GetType().Name} doesn't implement {callType.Name} for {dataType.Name}.");
                yield return handler;
            }
        }

    }
}
