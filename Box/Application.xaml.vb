Imports System.Windows.Markup

Class Application
    ' https://github.com/dotnet/wpf/issues/500
    Private Sub IComponentConnector_InitializeComponent() Implements IComponentConnector.InitializeComponent
        InitializeComponent()
    End Sub
End Class
