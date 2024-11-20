using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Unity.Properties;
using UnityEngine;

namespace GoC
{
    public class Hud : IDisposable
    {
        private const int MAX_LOG_LINES = 11;

        private TaskCompletionSource<string> _executeTcs;

        public Hud()
        {
            Application.logMessageReceivedThreaded += OnLogMessageReceived;
        }

        public void Dispose()
        {
            Application.logMessageReceivedThreaded -= OnLogMessageReceived;
        }

        [CreateProperty]
        public string logText { get; private set; }

        [CreateProperty]
        public string script { private get; set; }

        public async IAsyncEnumerable<string> GetScriptsAsync([EnumeratorCancellation] CancellationToken ct)
        {
            using (ct.Register(() => StopGetScriptsAsync()))
                while (!ct.IsCancellationRequested)
                    yield return await (_executeTcs = new TaskCompletionSource<string>()).Task;
        }

        public void Execute()
        {
            // _executeTcs can be null if hud is currently not ready to accept new script for execution (i.e. it hasn't finished executing previous script yet)
            TaskCompletionSource<string> tempExecuteTcs = _executeTcs;
            _executeTcs = null;
            tempExecuteTcs?.SetResult(script);
        }

        private void StopGetScriptsAsync() => _executeTcs?.SetCanceled();

        private void OnLogMessageReceived(string condition, string stacktrace, LogType type)
        {
            // TODO: need threading protection
            logText += string.IsNullOrEmpty(logText) ? condition : "\n" + condition;
            int linesCount = CountLines(logText);
            if (linesCount > MAX_LOG_LINES)
                logText = TrimLeadingLines(logText, linesCount - MAX_LOG_LINES);
        }

        private static int CountLines(string text)
        {
            if (string.IsNullOrEmpty(text))
                return 0;

            int linesCount = 1;
            foreach (char c in text)
                if (c == '\n')
                    ++linesCount;

            return linesCount;
        }

        private static string TrimLeadingLines(string text, int trimCount) => string.Join('\n', text.Split('\n').Skip(trimCount));
    }
}
