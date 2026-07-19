using System;
using System.IO;
using System.Text;

namespace AQ.App.Persistence
{
    /// <summary>Checkpoints inside <see cref="AtomicSaveFile.Write"/> where a crash can land.</summary>
    public enum SaveCheckpoint
    {
        AfterTmpWrite,
        AfterPrevDelete,
        AfterLiveToPrev,
        AfterTmpToLive
    }

    /// <summary>Test seam: implementations throw at a checkpoint to simulate a crash mid-write.</summary>
    public interface IFaultInjector
    {
        void OnCheckpoint(SaveCheckpoint checkpoint);
    }

    /// <summary>
    /// The save-file write/read protocol used by BoardSaveSystem: write the whole
    /// aggregate to .tmp, rotate live → .prev, promote .tmp → live. A crash at any
    /// point leaves either the old aggregate (live or prev) or the new one readable —
    /// never a torn file. Extracted so EditMode tests can fault-inject each checkpoint.
    /// </summary>
    public static class AtomicSaveFile
    {
        public static void Write(string livePath, string prevPath, string tmpPath, string contents, IFaultInjector faults = null)
        {
            File.WriteAllText(tmpPath, contents, Encoding.UTF8);
            faults?.OnCheckpoint(SaveCheckpoint.AfterTmpWrite);

            if (File.Exists(prevPath))
                File.Delete(prevPath);
            faults?.OnCheckpoint(SaveCheckpoint.AfterPrevDelete);

            if (File.Exists(livePath))
                File.Move(livePath, prevPath);
            faults?.OnCheckpoint(SaveCheckpoint.AfterLiveToPrev);

            File.Move(tmpPath, livePath);
            faults?.OnCheckpoint(SaveCheckpoint.AfterTmpToLive);
        }

        /// <summary>
        /// Reads the newest consistent aggregate: live first, .prev fallback (a crash
        /// between the two moves in Write leaves only .prev on disk). The validator
        /// rejects torn/unparseable content and forces the fallback.
        /// </summary>
        public static bool TryRead(string livePath, string prevPath, Func<string, bool> validator, out string contents)
        {
            foreach (var path in new[] { livePath, prevPath })
            {
                if (!File.Exists(path)) continue;
                string text;
                try { text = File.ReadAllText(path, Encoding.UTF8); }
                catch { continue; }
                if (validator == null || validator(text))
                {
                    contents = text;
                    return true;
                }
            }
            contents = null;
            return false;
        }
    }
}
