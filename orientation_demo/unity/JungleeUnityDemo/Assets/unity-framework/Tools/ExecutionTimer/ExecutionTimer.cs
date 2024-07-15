using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace XcelerateGames.RuntimeTools
{
    public class ExecutionTimer : IDisposable
    {
        #region Data
        //Private
        private static readonly Dictionary<string, ExecutionTimerData> _tests =
            new Dictionary<string, ExecutionTimerData>();
        private static string _lastStaticTest = string.Empty;
        private readonly string _disposableTest;
        #endregion//============================================================[ Data ]

        #region Public
        public ExecutionTimer(string title, bool useMilliseconds = false)
        {
            _disposableTest = title;
            _tests[_disposableTest] = new ExecutionTimerData(title, useMilliseconds);
        }

        public void Dispose()
        {
            _tests[_disposableTest].EndTest();
            _tests.Remove(_disposableTest);
        }

        public static void Start(string title, bool useMilliseconds = false)
        {
            if (_tests.ContainsKey(title))
            {
                _tests[title].Timer.Start();
            }
            else
            {
                _lastStaticTest = title;
                _tests[_lastStaticTest] = new ExecutionTimerData(title, useMilliseconds);
            }
        }

        public static void Pause()
        {
            if (!_tests.ContainsKey(_lastStaticTest))
            {
                UnityEngine.Debug.Log("Execution Timer : Stoped - It was never started");
                return;
            }

            _tests[_lastStaticTest].Timer.Stop();
        }

        public static void Pause(string title)
        {
            if (!_tests.ContainsKey(title))
            {
                UnityEngine.Debug.Log("Execution Timer : Paused - " + title);
                return;
            }

            _tests[title].Timer.Stop();
        }

        public static void End()
        {
            if (!_tests.ContainsKey(_lastStaticTest))
            {
                UnityEngine.Debug.Log("Execution Timer : Ended - It wasn't Started");
                return;
            }

            _tests[_lastStaticTest].EndTest();
            _tests.Remove(_lastStaticTest);
        }

        public static void End(string title)
        {
            if (!_tests.ContainsKey(title))
            {
                UnityEngine.Debug.Log("Execution Timer : Test not found - " + title);
                return;
            }

            _tests[title].EndTest();
            _tests.Remove(title);
            _lastStaticTest = string.Empty;
        }
        #endregion//============================================================[ Public ]

        #region Private
        private struct ExecutionTimerData
        {
            private readonly string _testTitle;
            private readonly bool _precise;
            public readonly Stopwatch Timer;
            private static readonly StringBuilder StringBuilder = new StringBuilder();

            public ExecutionTimerData(string testTitle, bool precise)
            {
                _testTitle = testTitle;
                _precise = precise;
                Timer = Stopwatch.StartNew();
            }

            public void EndTest()
            {
                var ms = Timer.ElapsedMilliseconds;
                var elapsedVal = _precise ? ms : ms / 1000f;
                var valMark = _precise ? "ms" : "s";
                UnityEngine.Debug.Log("Execution Timer : " + _testTitle + " - (" + elapsedVal + valMark + ")");
            }
        }
        #endregion//============================================================[ Private ]
    }
}