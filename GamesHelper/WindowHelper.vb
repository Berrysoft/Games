Imports System.ComponentModel
Imports System.Runtime.InteropServices
Imports System.Windows.Interop

Public Module WindowHelper
    Private Declare Auto Function GetClientRect Lib "user32.dll" (hWnd As IntPtr, ByRef lpRECT As tagRECT) As Integer

    Private Function GetClientRect(wnd As Window) As tagRECT
        Dim r As tagRECT
        If GetClientRect(New WindowInteropHelper(wnd).Handle, r) = 0 Then
            Throw New Win32Exception
        End If
        Return r
    End Function

    Public Function GetClientSizeWithDpi(wnd As Window, visual As DrawingVisual) As Size
        Dim r = GetClientRect(wnd)
        Dim dpi = VisualTreeHelper.GetDpi(visual)
        Return New Size((r.Right - r.Left) / dpi.DpiScaleX, (r.Bottom - r.Top) / dpi.DpiScaleY)
    End Function
End Module

<StructLayout(LayoutKind.Sequential)>
Structure tagRECT
    Public Left As Integer
    Public Top As Integer
    Public Right As Integer
    Public Bottom As Integer
End Structure
