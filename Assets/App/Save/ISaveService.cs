namespace AQ.App.Save
{
    public interface ISaveService
    {
        bool TrySave(SaveState state);
        bool TryLoad(out SaveState state);
        string SavePath { get; }
    }
}
