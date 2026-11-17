using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace VolanteStudio {
    // Keep the system caption, hit testing, Snap Layouts and monitor work-area handling.
    static class NativeWindowTheme {
        [DllImport("dwmapi.dll")]
        static extern int DwmSetWindowAttribute(IntPtr hwnd, int attribute, ref int value, int size);

        public static void Apply(Window window) {
            window.SourceInitialized += (sender, args) => {
                try {
                    var handle = new WindowInteropHelper(window).Handle;
                    int dark = 1;
                    if (DwmSetWindowAttribute(handle, 20, ref dark, sizeof(int)) != 0)
                        DwmSetWindowAttribute(handle, 19, ref dark, sizeof(int));
                    int caption = 0x000F0B09; // COLORREF: matches the app background.
                    DwmSetWindowAttribute(handle, 35, ref caption, sizeof(int));
                } catch (DllNotFoundException) {
                    // Older systems retain their standard system caption.
                } catch (EntryPointNotFoundException) { }
            };
        }
    }
}
