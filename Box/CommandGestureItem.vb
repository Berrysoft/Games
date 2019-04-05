Class CommandGestureItem
    Inherits DependencyObject

    Public Shared ReadOnly IconProperty As DependencyProperty = DependencyProperty.Register(NameOf(Icon), GetType(ImageSource), GetType(CommandGestureItem))
    Public Property Icon As ImageSource
        Get
            Return GetValue(IconProperty)
        End Get
        Set(value As ImageSource)
            SetValue(IconProperty, value)
        End Set
    End Property

    Public Shared ReadOnly TextProperty As DependencyProperty = DependencyProperty.Register(NameOf(Text), GetType(String), GetType(CommandGestureItem))
    Public Property Text As String
        Get
            Return GetValue(TextProperty)
        End Get
        Set(value As String)
            SetValue(TextProperty, value)
        End Set
    End Property

    Public Shared ReadOnly GestureTextProperty As DependencyProperty = DependencyProperty.Register(NameOf(GestureText), GetType(String), GetType(CommandGestureItem))
    Public Property GestureText As String
        Get
            Return GetValue(GestureTextProperty)
        End Get
        Set(value As String)
            SetValue(GestureTextProperty, value)
        End Set
    End Property

End Class
