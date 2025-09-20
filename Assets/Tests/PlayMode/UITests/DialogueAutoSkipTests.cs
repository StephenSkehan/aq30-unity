using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace AQ.App.Tests
{
    public class DialogueAutoSkipTests
    {
        [UnityTest, Ignore("Auto/Skip behavior is gated by WK2-3 Step C narrative decisions. Will unignore when gates finalize.")]
        public System.Collections.IEnumerator Auto_and_skip_advances_all_lines_and_logs_recent()
        {
            yield return null;
        }
    }
}

