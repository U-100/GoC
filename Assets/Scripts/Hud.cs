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

        private TaskCompletionSource<bool> _executeTcs;

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
            while (!ct.IsCancellationRequested)
            {
                _executeTcs = new TaskCompletionSource<bool>();
                using (ct.Register(StopGetScriptsAsync))
                {
                    if (await _executeTcs.Task)
                        yield return script;
                    else
                        break;
                }
            }
        }

        public void Execute()
        {
            // _executeTcs can be null if hud is currently not ready to accept new script for execution
            TaskCompletionSource<bool> tempExecuteTcs = _executeTcs;
            _executeTcs = null;
            tempExecuteTcs?.SetResult(true);
        }

        private void StopGetScriptsAsync()
        {
            TaskCompletionSource<bool> tempExecuteTcs = _executeTcs;
            _executeTcs = null;
            tempExecuteTcs?.SetResult(false);
        }

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
