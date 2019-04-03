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
End Class
