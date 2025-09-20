using System.Collections;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace AQ30.Tests.PlayMode
{
    public class PlayBootSmoke
    {
        [UnityTest]
        public IEnumerator FrameAdvances()
        {
            yield return null;
            Assert.Pass("PlayMode smoke test ran.");
        }
    }
}