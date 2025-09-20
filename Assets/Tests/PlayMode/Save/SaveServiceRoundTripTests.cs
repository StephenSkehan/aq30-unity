using NUnit.Framework;
using System;
using System.IO;
using AQ.App.Save;

public class SaveServiceRoundTripTests
{
    [Test]
    public void RoundTrip_Save_Then_Load_Works()
    {
        // JsonSaveService is static, so we test it directly
        var original = new SaveBlob { /* adjust fields to match your SaveBlob */ };
        
        // Clear any existing save
        JsonSaveService.Clear();
        
        // Save and verify it worked
        JsonSaveService.Save(original);
        Assert.IsTrue(JsonSaveService.HasSave(), "Save should exist");
        
        // Load and verify
        var loaded = JsonSaveService.Load();
        Assert.NotNull(loaded, "Load should succeed");
        
        // Add specific field comparisons based on your SaveBlob structure
    }
}
