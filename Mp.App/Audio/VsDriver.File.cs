using System;
using System.IO;
using System.Threading;
using System.Runtime.CompilerServices;

using Microsoft.SPOT;

namespace Mp.App.Audio
{
    static partial class VsDriver
    {
        private static bool playFile(FileStream file)
        {
            if (status != VsStatus.Stopped) return false;
            status = VsStatus.Playing;
            if (VsStatusChanged != null) VsStatusChanged(status);

            vsThread = new Thread(() =>
            {
                bool noMoreDataInvoked = false;

                try
                {
                    byte inUse = 0;
                    int length = (int)file.Length;
                    int read = file.Read(buffer[inUse], 0, BUFFER_SIZE);

                    if (read == 0) return;

                    while (true)
                    {
                        vsStreamData.InvokeEx(buffer[inUse], read);

                        inUse ^= 1;
                        read = file.Read(buffer[inUse], 0, BUFFER_SIZE);

                        if (read == 0)
                        {
                            vsNoMoreData.Invoke();
                            noMoreDataInvoked = true;
                            break;
                        }

                        wait.WaitOne();
                    }

                }
                catch (ThreadAbortException)
                { }
                catch (Exception ex)
                {
                    RaiseException(ex.Message + "\n" + ex.StackTrace);
                }
                finally
                {
                    if (!noMoreDataInvoked)
                        vsNoMoreData.Invoke();

                    file.Close();

                    status = VsStatus.Stopped;
                    if (VsStatusChanged != null) VsStatusChanged(status);
                }
            }) { Priority = ThreadPriority.Highest };
            vsThread.Start();
            return true;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public static bool PlayFile(FileStream file)
        {
            if (!initialized)
            {
                RaiseException("VS Driver not initialized");
                return false;
            }
            return playFile(file);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public static bool PauseFile()
        {
            if (status != VsStatus.Playing) return false;
            vsThread.Suspend();
            status = VsStatus.Paused;
            if (VsStatusChanged != null) VsStatusChanged(status);
            return true;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public static bool ResumeFile()
        {
            if (status != VsStatus.Paused) return false;
            vsThread.Resume();
            status = VsStatus.Playing;
            if (VsStatusChanged != null) VsStatusChanged(status);
            return true;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public static bool StopFile()
        {
            if (status != VsStatus.Playing) return false;
            vsThread.Abort();
            vsThread.Join();
            return true;
        }
    }
}
