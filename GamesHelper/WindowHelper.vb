Imports System.ComponentModel
Imports System.Runtime.InteropServices
Imports System.Windows.Interop

Public Module WindowHelper
    Private Sub CheckWin32Bool(result As Integer)
        If result = 0 Then
            Throw New Win32Exception()
        End If
    End Sub

    Private Sub CheckHResult(result As Integer)
        If result <> 0 Then
#If NETCOREAPP Then
            Throw New Exception() With {.HResult = result}
#Else
            Throw New HResultException(result)
#End If
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

    Private Declare Unicode Function SendMessageW Lib "user32.dll" (hWnd As IntPtr, Msg As UInteger, wParam As IntPtr, lParam As IntPtr) As <MarshalAs(UnmanagedType.SysUInt)> IntPtr

    Private Declare Auto Sub RtlGetNtVersionNumbers Lib "ntdll.dll" (major As IntPtr, minor As IntPtr, ByRef build As UInteger)

    Private Declare Auto Function DwmSetWindowAttribute Lib "dwmapi.dll" (hWnd As IntPtr, dwAttribute As UInteger, <MarshalAs(UnmanagedType.Bool)> ByRef pvAttribute As Boolean, cbAttribute As UInteger) As Integer

    Private Declare Auto Function ShouldAppUseDarkMode Lib "Uxtheme.dll" Alias "#132" () As <MarshalAs(UnmanagedType.Bool)> Boolean
    Private Declare Auto Function AllowDarkModeForApp Lib "Uxtheme.dll" Alias "#135" (<MarshalAs(UnmanagedType.Bool)> value As Boolean) As <MarshalAs(UnmanagedType.Bool)> Boolean
    Private Declare Auto Function SetPreferredAppMode Lib "Uxtheme.dll" Alias "#135" (value As PREFERRED_APP_MODE) As PREFERRED_APP_MODE
    Private Declare Auto Sub FlushMenuThemes Lib "Uxtheme.dll" Alias "#136" ()
    Private Declare Auto Function ShouldSystemUseDarkMode Lib "Uxtheme.dll" Alias "#138" () As <MarshalAs(UnmanagedType.Bool)> Boolean
    Private Declare Auto Function IsDarkModeAllowedForApp Lib "Uxtheme.dll" Alias "#139" () As <MarshalAs(UnmanagedType.Bool)> Boolean

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
                AllowDarkModeForApp(mode = PREFERRED_APP_MODE.ALLOW_DARK OrElse mode = PREFERRED_APP_MODE.FORCE_DARK)
            Else
                SetPreferredAppMode(mode)
            End If
        End If
    End Sub

    Public Function IsDarkModeEnabledForApp() As Boolean
        Return IsDarkModeExists() AndAlso If(GetSystemBuild() < 18362UI, ShouldAppUseDarkMode(), ShouldSystemUseDarkMode()) AndAlso IsDarkModeAllowedForApp()
    End Function

    Public Sub SetWindowDarkMode(wnd As Window)
        If IsDarkModeExists() Then
            Dim helper As New WindowInteropHelper(wnd)
            CheckHResult(DwmSetWindowAttribute(helper.Handle, &H14UI, True, 4))
            SendMessageW(helper.Handle, &H31AUI, 0, 0)
            FlushMenuThemes()
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

Enum PREFERRED_APP_MODE As Integer
    [DEFAULT]
    ALLOW_DARK
    FORCE_DARK
    FORCE_LIGHT
End Enum

Public Enum PreferredAppMode
    DefaultMode
    AllowDark
    ForceDark
    ForceLight
End Enum

#If Not NETCOREAPP Then
Class HResultException
    Inherits Exception

    Public Sub New(value As Integer)
        HResult = value
    End Sub
End Class
#End If
