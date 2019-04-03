Class MainWindow
    Private Sub Game_Restart()
        MainView.Restart()
    End Sub

    Private Sub Game_Undo()
        MainView.Undo()
    End Sub

    Private Sub Game_GoPrev()
        MainView.GoPrev()
    End Sub

    Private Sub Game_GoNext()
        MainView.GoNext()
    End Sub

    Private Sub Game_GoLevel()
        Dim slwnd As New SelectLevelWindow
        For i = 1 To MainView.MaxLevel
            slwnd.LevelCombo.Items.Add(i)
        Next
        slwnd.Level = MainView.Level
        If slwnd.ShowDialog() Then
            MainView.GoLevel(slwnd.Level)
        End If
    End Sub

    Private Sub Game_GoDir(sender As Object, e As ExecutedRoutedEventArgs)
        MainView.GoDir(e.Parameter)
    End Sub

    Private Sub Game_GetHelp()
        Dim wnd As New HelpWindow
        wnd.ShowDialog()
    End Sub
End Class
