using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.TestTools;

public class AddressablesSmokeTests
{
    [UnityTest]
    public IEnumerator Ep01Assets_Load_From_Addressables()
    {
        AsyncOperationHandle<IList<Object>> handle =
            Addressables.LoadAssetsAsync<Object>("Ep01", null);
        yield return handle;

        Assert.That(handle.Status, Is.EqualTo(AsyncOperationStatus.Succeeded));
        Assert.That(handle.Result, Is.Not.Null);
        Assert.That(handle.Result.Count, Is.GreaterThan(0));
        Addressables.Release(handle);
    }
}
