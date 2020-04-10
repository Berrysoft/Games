Imports System.ComponentModel
Imports System.Runtime.InteropServices
Imports System.Windows.Interop

Public Module WindowHelper
    Private Sub CheckWin32Bool(result As Integer)
        If result = 0 Then
            Throw New Win32Exception()
        End If
    End Sub

    Private Declare Auto Function GetClientRect Lib "user32.dll" (hWnd As IntPtr, ByRef lpRECT As RECT) As Integer

    Private Function GetClientRect(wnd As Window) As RECT
        Dim r As RECT
        CheckWin32Bool(GetClientRect(New WindowInteropHelper(wnd).Handle, r))
        Return r
    End Function

    Public Function GetClientSizeWithDpi(wnd As Window, visual As DrawingVisual) As Size
        Dim r = GetClientRect(wnd)
        Dim dpi = VisualTreeHelper.GetDpi(visual)
        Return New Size((r.Right - r.Left) / dpi.DpiScaleX, (r.Bottom - r.Top) / dpi.DpiScaleY)
    End Function

    Private Declare Auto Sub RtlGetNtVersionNumbers Lib "ntdll.dll" (major As IntPtr, minor As IntPtr, ByRef build As UInteger)

    Private Declare Auto Function DwmSetWindowAttribute Lib "dwmapi.dll" (hWnd As IntPtr, dwAttribute As UInteger, <MarshalAs(UnmanagedType.Bool)> ByRef pvAttribute As Boolean, cbAttribute As UInteger) As Integer

    Private Declare Auto Function ShouldAppsUseDarkMode Lib "Uxtheme.dll" Alias "#132" () As <MarshalAs(UnmanagedType.I1)> Boolean
    Private Declare Auto Function AllowDarkModeForApp Lib "Uxtheme.dll" Alias "#135" (<MarshalAs(UnmanagedType.I1)> value As Boolean) As <MarshalAs(UnmanagedType.I1)> Boolean
    Private Declare Auto Function SetPreferredAppMode Lib "Uxtheme.dll" Alias "#135" (value As PreferredAppMode) As PreferredAppMode

    Private Function GetSystemBuild() As UInteger
        Dim build As UInteger
        RtlGetNtVersionNumbers(0, 0, build)
        Return build And (Not &HF0000000UI)
    End Function

    Private Function IsDarkModeExists() As Boolean
        Return GetSystemBuild() >= 17763UI
    End Function

    Public Sub SetApplicationPreferredMode(mode As PreferredAppMode)
        If IsDarkModeExists() Then
            If GetSystemBuild() < 18362UI Then
                AllowDarkModeForApp(mode = PreferredAppMode.AllowDark OrElse mode = PreferredAppMode.ForceDark)
            Else
                SetPreferredAppMode(mode)
            End If
        End If
    End Sub

    Public Function IsDarkModeEnabledForApp() As Boolean
        Return IsDarkModeExists() AndAlso ShouldAppsUseDarkMode()
    End Function

    Public Sub SetWindowDarkMode(wnd As Window)
        If IsDarkModeExists() Then
            Dim helper As New WindowInteropHelper(wnd)
            If DwmSetWindowAttribute(helper.Handle, &H14UI, IsDarkModeEnabledForApp(), 4) <> 0 Then
                DwmSetWindowAttribute(helper.Handle, &H13UI, IsDarkModeEnabledForApp(), 4)
            End If
            wnd.Height += 1 'A trick to refresh window for dark mode
        End If
    End Sub
End Module

<StructLayout(LayoutKind.Sequential)>
Structure RECT
    Public Left As Integer
    Public Top As Integer
    Public Right As Integer
    Public Bottom As Integer
End Structure

Public Enum PreferredAppMode As Integer
    DefaultMode
    AllowDark
    ForceDark
    ForceLight
End Enum
