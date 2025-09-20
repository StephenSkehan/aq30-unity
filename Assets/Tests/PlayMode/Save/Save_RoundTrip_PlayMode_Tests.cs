using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace AQ.App.Save.Tests
{
    public class Save_RoundTrip_PlayMode_Tests
    {
        [UnityTest]
        public IEnumerator RoundTrip_Using_Project_SaveService_Works()
        {
            // JsonSaveService is static
            JsonSaveService.Clear();
            
            // Build a minimal SaveBlob
            var blob = new SaveBlob(); // Adjust to match your actual SaveBlob structure
            
            JsonSaveService.Save(blob);
            yield return null; // simulate frame
            
            Assert.IsTrue(JsonSaveService.HasSave(), "Save should exist");
            
            var loaded = JsonSaveService.Load();
            Assert.NotNull(loaded, "Load should succeed");
            
            yield return null;
        }
    }
}
