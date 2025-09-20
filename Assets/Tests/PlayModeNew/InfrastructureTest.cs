// Assets/Tests/PlayMode/InfrastructureTest.cs
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class PlayModeInfrastructureTest
{
    [UnityTest]
    public IEnumerator SimplePlayModeTest()
    {
        // Create a simple GameObject
        var go = new GameObject("TestObject");
        Assert.IsNotNull(go);
        
        yield return null; // Wait one frame
        
        // Clean up (this should work in PlayMode)
        Object.Destroy(go);
        
        yield return null; // Wait for destruction
        
        Assert.IsTrue(true, "PlayMode test infrastructure works");
    }
}