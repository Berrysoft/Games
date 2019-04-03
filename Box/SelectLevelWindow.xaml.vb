Public Class SelectLevelWindow
    Public Shared ReadOnly LevelProperty As DependencyProperty = DependencyProperty.Register(NameOf(Level), GetType(Integer), GetType(SelectLevelWindow))

    Friend Property Level As Integer
        Get
            Return GetValue(LevelProperty)
        End Get
        Set(value As Integer)
            SetValue(LevelProperty, value)
        End Set
    End Property

    Public Sub New()
        InitializeComponent()
        DataContext = Me
    End Sub

    Private Sub OK_Click(sender As Object, e As RoutedEventArgs)
        DialogResult = True
        Close()
    End Sub
End Class
