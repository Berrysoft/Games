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

    Private Sub Game_CanUndo(sender As Object, e As CanExecuteRoutedEventArgs)
        If MainView IsNot Nothing Then
            e.CanExecute = MainView.CanUndo
        End If
    End Sub

    Private Sub Game_CanGoPrev(sender As Object, e As CanExecuteRoutedEventArgs)
        If MainView IsNot Nothing Then
            e.CanExecute = MainView.Level > 0
        End If
    End Sub

    Private Sub Game_CanGoNext(sender As Object, e As CanExecuteRoutedEventArgs)
        If MainView IsNot Nothing Then
            e.CanExecute = MainView.Level < MainView.MaxLevel - 1
        End If
    End Sub

    Private Sub MainView_LevelPassed(sender As Object, e As Integer)
        MessageBox.Show("恭喜过关！", "推箱子")
    End Sub

    Private Sub MainView_Stepped(sender As Object, e As Integer)
        If e > 0 Then
            Me.Title = $"推箱子 - 第{MainView.Level + 1}关 - {e}步"
        Else
            Me.Title = $"推箱子 - 第{MainView.Level + 1}关"
        End If
    End Sub
End Class
