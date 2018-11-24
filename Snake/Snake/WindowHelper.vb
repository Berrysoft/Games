Imports System.Runtime.InteropServices
Imports System.Windows.Interop

Module WindowHelper
    Private Declare Auto Function GetClientRect Lib "user32.dll" (hWnd As IntPtr, ByRef lpRECT As tagRECT) As Integer

    Public Function GetClientRect(wnd As Window) As tagRECT
        Dim r As tagRECT
        GetClientRect(New WindowInteropHelper(wnd).Handle, r)
        Return r
    End Function
End Module

<StructLayout(LayoutKind.Sequential)>
Structure tagRECT
    Public Left As Integer
    Public Top As Integer
    Public Right As Integer
    Public Bottom As Integer
End Structure
