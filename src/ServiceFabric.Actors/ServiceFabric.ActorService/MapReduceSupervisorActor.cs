using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceFabric.ActorService
{
    using Microsoft.ServiceFabric.Actors;
    using Microsoft.ServiceFabric.Actors.Runtime;
    using Microsoft.ServiceFabric.Data.Collections;
    using ServiceFabric.ActorService.Interfaces;
    using ServiceFabric.ActorService.Interfaces.Model;
    using System.IO;
    using System.Reactive.Concurrency;
    using System.Reactive.Linq;
    using System.Reactive.Threading.Tasks;
    using System.Threading;

    [ActorService(Name = "MapReduceSupervisorActorService")]
    [StatePersistence(StatePersistence.None)]
    public class MapReduceSupervisorActor : Actor, IMapReduceSupervisorActor
    {
        // * Recieve file path, stream file content.
        // * Push streamed content to Map actor(s)
        // * Take mapped result and push to reducer
        // * Return final result on completion

        public MapReduceSupervisorActor(ActorService actorService, ActorId actorId) 
            : base(actorService, actorId)
        {
        }


        public async Task<ReducedData> Process(string fileName)
        {
            var appName = ActorService.Context.CodePackageActivationContext.ApplicationName;
            var mapActor = ActorFactory.CreateActor<IMapActor>(appName, "MapActorService", "mapper", 1);
            var reduceActor = ActorFactory.CreateActor<IReduceActor>(appName, "ReduceActorService", "reducer", 1);

            await MapWithSingleActorAsync(fileName, mapActor, reduceActor);
            
            return await reduceActor.GetResultAsync();
        }

        private static async Task MapWithSingleActorAsync(string fileName, IMapActor mapActor, IReduceActor reduceActor)
        {
            var exception = (Exception)null;

            using (var fileObserver = new FileObserver(fileName))
            {
                var processCompleteSignal = new SemaphoreSlim(0, 1);

                var observer = fileObserver
                    .Observable
                    .SubscribeAsync(async (line) =>
                    {
                        var map = await mapActor.MapAsync(line);
                        await reduceActor.ReduceAsync(map);
                    },
                    (e) =>
                    {
                        exception = e;
                    },
                    () =>
                    {
                        processCompleteSignal.Release();
                    });

                await processCompleteSignal.WaitAsync();
            }

            if (exception != null)
                throw exception;
        }
    }

    public class FileObserver : IDisposable
    {
        private string _filePath;
        private FileStream _fileStream;
        private IObservable<string> _observable;

        public FileObserver(string filePath)
        {
            _filePath = filePath;
            _fileStream = File.OpenRead(filePath);
            _observable = ReadLines(_fileStream)
                .ToObservable(Scheduler.Default);
        }

        public IObservable<string> Observable
        {
            get { return _observable; }
        }

        private IEnumerable<string> ReadLines(Stream fileStream)
        {
            using (StreamReader reader = new StreamReader(fileStream))
            {
                while (!reader.EndOfStream)
                    yield return reader.ReadLine();
            }
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _fileStream.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~FileObserver() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        void IDisposable.Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }

    public static class ReactiveExtensions
    {
        public static IDisposable SubscribeAsync<T>(this IObservable<T> source, Func<T, Task> onNext, Action<Exception> onError, Action onCompleted)
        {
            return source.Select(e => Observable.Defer(() => onNext(e).ToObservable())).Concat()
                .Subscribe(
                e => { }, // empty
                onError,
                onCompleted);
        }
    }
}
