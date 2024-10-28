using System;
using System.Globalization;
using System.Threading.Tasks;
using Jint;
using UnityEngine;

namespace GoC
{
    public static class EngineFactory
    {
        public static Engine Create()
        {
            Engine engine = new Engine(Configure);

            engine.SetValue("log", new Action<object>(Debug.Log));
            engine.SetValue("setTimeout", new Action<Action, int>(SetTimeout));
            engine.SetValue("delay", new Func<int, Task>(Delay));
            
            return engine;
        }
        
        private static void Configure(Options options)
        {
            options.Culture = CultureInfo.InvariantCulture;
        }
        
        private static async void SetTimeout(Action callback, int milliseconds)
        {
            await Task.Delay(milliseconds);
            callback();
        }

        private static async Task Delay(int milliseconds)
        {
            await Task.Delay(milliseconds).ConfigureAwait(false);
        }
    }
}
