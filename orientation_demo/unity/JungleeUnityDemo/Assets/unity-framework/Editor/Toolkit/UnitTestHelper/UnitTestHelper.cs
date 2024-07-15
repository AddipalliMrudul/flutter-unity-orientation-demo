using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine;

namespace XcelerateGames.Editor.UnitTest
{
    public class UnitTestHelper : MonoBehaviour
    {
        public static void RunUnitTests()
        {
            var testRunnerApi = ScriptableObject.CreateInstance<TestRunnerApi>();
            var filter = new Filter()
            {
                testMode = TestMode.EditMode
            };
            Debug.Log("<<<executing unit test");
            testRunnerApi.Execute(new ExecutionSettings(filter));
            Debug.Log("<<<registering unit test callback");
            testRunnerApi.RegisterCallbacks(new UnitTestCallbacks());
        }
    }
    public class UnitTestCallbacks : ICallbacks
    {
        private bool hasAnyTestFailed = false;

        public void RunStarted(ITestAdaptor testsToRun)
        {
            Debug.Log("run started");
        }

        public void RunFinished(ITestResultAdaptor result)
        {
            Debug.Log("run finished");
            if (hasAnyTestFailed)
            {
                Debug.Log("<<<test failed exiting unity with code 1");
                EditorApplication.Exit((int)ExitCode.Unstable);
            }
            else
            {
                Debug.Log("<<<all tests passed exiting unity with exit code 0");
                EditorApplication.Exit((int)ExitCode.Success);
            }
        }

        public void TestStarted(ITestAdaptor test)
        {

        }

        public void TestFinished(ITestResultAdaptor result)
        {
            if (result.HasChildren && result.ResultState == "Passed")
            {
                Debug.Log(result.Test.Name + " : passed");
            }
            else if (!result.HasChildren && result.ResultState != "Passed")
            {
                Debug.Log(string.Format(">>>Test {0} {1}", result.Test.Name, result.ResultState));
                hasAnyTestFailed = true;
            }
        }

        public enum ExitCode
        {
            None = -1,
            Success = 0,
            Unstable = 1,
        }
    }
}
