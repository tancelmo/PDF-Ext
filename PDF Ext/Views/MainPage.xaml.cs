using System.Text;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml.Controls;

using PDF_Ext.ViewModels;
using Windows.Graphics;
using Windows.Storage.Pickers;
using Windows.Storage;
using WinRT.Interop;
using Microsoft.UI.Xaml;
using System.Runtime.InteropServices;
using Microsoft.UI.Xaml.Media.Animation;

namespace PDF_Ext.Views;

public sealed partial class MainPage : Page
{
    public MainViewModel ViewModel
    {
        get;
    }
    public static AppWindow m_AppWindow;
    public MainPage()
    {
        ViewModel = App.GetService<MainViewModel>();
        InitializeComponent();

        m_AppWindow = GetAppWindowForCurrentWindow();
        m_AppWindow.Title = "PDF Ext";
        //m_AppWindow.Resize(new SizeInt32(300, 200));
        

        if (AppWindowTitleBar.IsCustomizationSupported())
        {
            var titleBar = m_AppWindow.TitleBar;
            titleBar.ExtendsContentIntoTitleBar = true;
            AppTitleBar.Loaded += AppTitleBar_Loaded;
            AppTitleBar.SizeChanged += AppTitleBar_SizeChanged;
            titleBar.ButtonBackgroundColor = Colors.Transparent;
            titleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
            IntPtr hWnd = WindowNative.GetWindowHandle(App.MainWindow);
            WindowId wndId = Win32Interop.GetWindowIdFromWindow(hWnd);
            var presenter = AppWindow.GetFromWindowId(wndId).Presenter as OverlappedPresenter;
            presenter.IsResizable = false;
            presenter.IsMaximizable = false;
            m_AppWindow.SetIcon("Assets/WindowIco.ico");
        }
        else
        {
            // In the case that title bar customization is not supported, hide the custom title bar
            // element.
            AppTitleBar.Visibility = Visibility.Collapsed;

            // Show alternative UI for any functionality in
            // the title bar, such as search.
        }
    }

    #region TitleBar
    private void AppTitleBar_Loaded(object sender, RoutedEventArgs e)
    {
        // Check to see if customization is supported.
        // The method returns true on Windows 10 since Windows App SDK 1.2, and on all versions of
        // Windows App SDK on Windows 11.
        if (AppWindowTitleBar.IsCustomizationSupported())
        {
            SetDragRegionForCustomTitleBar(m_AppWindow);
        }
    }

    private void AppTitleBar_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        // Check to see if customization is supported.
        // The method returns true on Windows 10 since Windows App SDK 1.2, and on all versions of
        // Windows App SDK on Windows 11.
        if (AppWindowTitleBar.IsCustomizationSupported()
            && m_AppWindow.TitleBar.ExtendsContentIntoTitleBar)
        {
            // Update drag region if the size of the title bar changes.
            SetDragRegionForCustomTitleBar(m_AppWindow);
        }
    }
    private AppWindow GetAppWindowForCurrentWindow()
    {
        IntPtr hWnd = WindowNative.GetWindowHandle(App.MainWindow);
        WindowId wndId = Win32Interop.GetWindowIdFromWindow(hWnd);


        return AppWindow.GetFromWindowId(wndId);
    }
    [DllImport("Shcore.dll", SetLastError = true)]
    internal static extern int GetDpiForMonitor(IntPtr hmonitor, Monitor_DPI_Type dpiType, out uint dpiX, out uint dpiY);

    internal enum Monitor_DPI_Type : int
    {
        MDT_Effective_DPI = 0,
        MDT_Angular_DPI = 1,
        MDT_Raw_DPI = 2,
        MDT_Default = MDT_Effective_DPI
    }

    private double GetScaleAdjustment()
    {
        IntPtr hWnd = WindowNative.GetWindowHandle(App.MainWindow);
        WindowId wndId = Win32Interop.GetWindowIdFromWindow(hWnd);
        DisplayArea displayArea = DisplayArea.GetFromWindowId(wndId, DisplayAreaFallback.Primary);
        IntPtr hMonitor = Win32Interop.GetMonitorFromDisplayId(displayArea.DisplayId);

        // Get DPI.
        int result = GetDpiForMonitor(hMonitor, Monitor_DPI_Type.MDT_Default, out uint dpiX, out uint _);
        if (result != 0)
        {
            throw new Exception("Could not get DPI for monitor.");
        }

        uint scaleFactorPercent = (uint)(((long)dpiX * 100 + (96 >> 1)) / 96);
        return scaleFactorPercent / 100.0;
    }

    private void SetDragRegionForCustomTitleBar(AppWindow appWindow)
    {
        // Check to see if customization is supported.
        // The method returns true on Windows 10 since Windows App SDK 1.2, and on all versions of
        // Windows App SDK on Windows 11.
        if (AppWindowTitleBar.IsCustomizationSupported()
            && appWindow.TitleBar.ExtendsContentIntoTitleBar)
        {
            double scaleAdjustment = GetScaleAdjustment();

            RightPaddingColumn.Width = new GridLength(appWindow.TitleBar.RightInset / scaleAdjustment);
            LeftPaddingColumn.Width = new GridLength(appWindow.TitleBar.LeftInset / scaleAdjustment);

            List<Windows.Graphics.RectInt32> dragRectsList = new();

            Windows.Graphics.RectInt32 dragRectL;
            dragRectL.X = (int)((LeftPaddingColumn.ActualWidth) * scaleAdjustment);
            dragRectL.Y = 0;
            dragRectL.Height = (int)(AppTitleBar.ActualHeight * scaleAdjustment);
            dragRectL.Width = (int)((IconColumn.ActualWidth
                                    + TitleColumn.ActualWidth
                                    + LeftDragColumn.ActualWidth) * scaleAdjustment);
            dragRectsList.Add(dragRectL);

            Windows.Graphics.RectInt32 dragRectR;
            dragRectR.X = (int)((LeftPaddingColumn.ActualWidth
                                + IconColumn.ActualWidth
                                + TitleTextBlock.ActualWidth
                                + LeftDragColumn.ActualWidth
                                + SearchColumn.ActualWidth
                                - 32) * scaleAdjustment);
            dragRectR.Y = 0;
            dragRectR.Height = (int)(AppTitleBar.ActualHeight * scaleAdjustment);
            dragRectR.Width = (int)(RightDragColumn.ActualWidth * scaleAdjustment);
            dragRectsList.Add(dragRectR);

            Windows.Graphics.RectInt32[] dragRects = dragRectsList.ToArray();

            appWindow.TitleBar.SetDragRectangles(dragRects);
        }
    }

    #endregion
    private async void btnSelect_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        // Clear previous returned file name, if it exists, between iterations of this scenario
        PickFilesOutputTextBlock.Text = "";

        // Create a file picker
        var openPicker = new Windows.Storage.Pickers.FileOpenPicker();

        // Retrieve the window handle (HWND) of the current WinUI 3 window.

        var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow);

        // Initialize the file picker with the window handle (HWND).
        WinRT.Interop.InitializeWithWindow.Initialize(openPicker, hWnd);

        // Set options for your file picker
        openPicker.ViewMode = PickerViewMode.List;
        openPicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
        openPicker.FileTypeFilter.Add(".pdf");

        // Open the picker for the user to pick a file
        IReadOnlyList<StorageFile> files = await openPicker.PickMultipleFilesAsync();
        if (files.Count > 0)
        {
            int i = 0;
            StringBuilder output = new StringBuilder("Picked " + files.Count + " file(s): \n\n");
            foreach (StorageFile file in files)
            {
                i++;
                output.Append(file.Name + "\n");
                listFiles.Items.Add("File " + i + " - " + file.Name);
            } 
            // PickFilesOutputTextBlock.Text = output.ToString();
            PickFilesOutputTextBlock.Text = "Picked " + files.Count + " file(s): \n\n";
        }
        else
        {
            PickFilesOutputTextBlock.Text = "Operation cancelled.";
        }

    }

    private void btnSettins_Click(object sender, RoutedEventArgs e)
    {
        this.Frame.Navigate(typeof(SettingsPage), null, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight });

    }
}
