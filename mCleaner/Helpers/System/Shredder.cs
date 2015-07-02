using System;
using System.IO;
using System.Security.Cryptography;

namespace mCleaner.Helpers
{
    public class Wipe
    {
        /// <summary>
        /// Deletes a file in a secure way by overwriting it with
        /// random garbage data n times.
        /// </summary>
        /// <param name="filename">Full path of the file to be deleted</param>
        /// <param name="timesToWrite">Specifies the number of times the file should be overwritten</param>
        public void WipeFile(string filename, int timesToWrite)
        {
            try
            {
                if (File.Exists(filename))
                {
                    // Set the files attributes to normal in case it's read-only.
                    File.SetAttributes(filename, FileAttributes.Normal);

                    // Calculate the total number of sectors in the file.
                    double sectors = Math.Ceiling(new FileInfo(filename).Length / 512.0);

                    // Create a dummy-buffer the size of a sector.
                    byte[] dummyBuffer = new byte[512];

                    // Create a cryptographic Random Number Generator.
                    // This is what I use to create the garbage data.
                    RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();

                    // Open a FileStream to the file.
                    FileStream inputStream = new FileStream(filename, FileMode.Open);
                    for (int currentPass = 0; currentPass < timesToWrite; currentPass++)
                    {
                        UpdatePassInfo(currentPass + 1, timesToWrite);

                        // Go to the beginning of the stream
                        inputStream.Position = 0;

                        // Loop all sectors
                        for (int sectorsWritten = 0; sectorsWritten < sectors; sectorsWritten++)
                        {
                            UpdateSectorInfo(sectorsWritten + 1, (int)sectors);

                            // Fill the dummy-buffer with random data
                            rng.GetBytes(dummyBuffer);
                            // Write it to the stream
                            inputStream.Write(dummyBuffer, 0, dummyBuffer.Length);
                        }
                    }
                    // Truncate the file to 0 bytes.
                    // This will hide the original file-length if you try to recover the file.
                    inputStream.SetLength(0);
                    // Close the stream.
                    inputStream.Close();

                    // As an extra precaution I change the dates of the file so the
                    // original dates are hidden if you try to recover the file.
                    DateTime dt = new DateTime(2037, 1, 1, 0, 0, 0);
                    File.SetCreationTime(filename, dt);
                    File.SetLastAccessTime(filename, dt);
                    File.SetLastWriteTime(filename, dt);

                    File.SetCreationTimeUtc(filename, dt);
                    File.SetLastAccessTimeUtc(filename, dt);
                    File.SetLastWriteTimeUtc(filename, dt);

                    // Finally, delete the file
                    File.Delete(filename);

                    WipeDone();
                }
            }
            catch (Exception e)
            {
                WipeError(e);
            }
        }

        # region Events
        public event PassInfoEventHandler PassInfoEvent;
        private void UpdatePassInfo(int currentPass, int totalPasses)
        {
            PassInfoEvent(new PassInfoEventArgs(currentPass, totalPasses));
        }

        public event SectorInfoEventHandler SectorInfoEvent;
        private void UpdateSectorInfo(int currentSector, int totalSectors)
        {
            SectorInfoEvent(new SectorInfoEventArgs(currentSector, totalSectors));
        }

        public event WipeDoneEventHandler WipeDoneEvent;
        private void WipeDone()
        {
            WipeDoneEvent(new WipeDoneEventArgs());
        }

        public event WipeErrorEventHandler WipeErrorEvent;
        private void WipeError(Exception e)
        {
            WipeErrorEvent(new WipeErrorEventArgs(e));
        }
        # endregion
    }

    # region Events
    # region PassInfo
    public delegate void PassInfoEventHandler(PassInfoEventArgs e);
    public class PassInfoEventArgs : EventArgs
    {
        private readonly int cPass;
        private readonly int tPass;

        public PassInfoEventArgs(int currentPass, int totalPasses)
        {
            cPass = currentPass;
            tPass = totalPasses;
        }

        /// <summary> Get the current pass </summary>
        public int CurrentPass { get { return cPass; } }
        /// <summary> Get the total number of passes to be run </summary> 
        public int TotalPasses { get { return tPass; } }
    }
    # endregion

    # region SectorInfo
    public delegate void SectorInfoEventHandler(SectorInfoEventArgs e);
    public class SectorInfoEventArgs : EventArgs
    {
        private readonly int cSector;
        private readonly int tSectors;

        public SectorInfoEventArgs(int currentSector, int totalSectors)
        {
            cSector = currentSector;
            tSectors = totalSectors;
        }

        /// <summary> Get the current sector </summary> 
        public int CurrentSector { get { return cSector; } }
        /// <summary> Get the total number of sectors to be run </summary> 
        public int TotalSectors { get { return tSectors; } }
    }
    # endregion

    # region WipeDone
    public delegate void WipeDoneEventHandler(WipeDoneEventArgs e);
    public class WipeDoneEventArgs : EventArgs
    {
    }
    # endregion

    # region WipeError
    public delegate void WipeErrorEventHandler(WipeErrorEventArgs e);
    public class WipeErrorEventArgs : EventArgs
    {
        private readonly Exception e;

        public WipeErrorEventArgs(Exception error)
        {
            e = error;
        }

        public Exception WipeError { get { return e; } }
    }
    # endregion
    # endregion
}