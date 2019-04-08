Imports System.IO

Module Program
    Sub Main(args() As String)
        Dim app As New Application
        Dim win As MainWindow
        If args.Count > 0 Then
            Dim fileName As String = args(0)
            If File.Exists(fileName) Then
                win = New MainWindow(fileName)
            Else
                win = New MainWindow()
            End If
        Else
            win = New MainWindow()
        End If
        app.Run(win)
    End Sub
End Module
