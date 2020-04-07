Imports GamesHelper

Class Application
    Private Sub Application_Startup(sender As Object, e As StartupEventArgs) Handles Me.Startup
        SetApplicationPreferredMode(PreferredAppMode.AllowDark)
    End Sub
End Class
