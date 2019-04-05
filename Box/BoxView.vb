Imports System.ComponentModel
Imports System.IO
Imports GamesHelper

Class BoxView
    Inherits FrameworkElement

    Private Shared ReadOnly NoBorderPen As New Pen(Brushes.Transparent, 0)
    Private Shared ReadOnly BorderPen As New Pen(Brushes.Black, 1)

    Private times As Double
    Private off As Vector

    Private mapVisual As New DrawingVisual
    Private boxVisual As New DrawingVisual
    Private bodyVisual As New DrawingVisual

    Private maps As New List(Of LevelMap)
    Private currentMap As LevelMap

    Public Sub New()
        AddVisualChild(mapVisual)
        AddVisualChild(boxVisual)
        AddVisualChild(bodyVisual)
    End Sub

    Private Sub BoxView_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        If DesignerProperties.GetIsInDesignMode(Me) Then
            maps.Add(New LevelMap())
        Else
            LoadLevel()
        End If
        GoLevel(0)
        DrawAll()
    End Sub

    Private Sub LoadLevel()
        Using stream As New StreamReader("levels.txt")
            Do
                Dim first = stream.ReadLine()
                If String.IsNullOrWhiteSpace(first) Then
                    Exit Do
                End If
                Dim nums = first.Split(" "c).Select(Function(s) CInt(s)).ToArray()
                Dim map(nums(0) - 1, nums(1) - 1) As SquareState
                Dim body As New IntPoint(nums(2), nums(3))
                For j = 0 To nums(1) - 1
                    Dim line = stream.ReadLine()
                    Dim states = line.Split(" "c).Select(Function(s) CInt(s)).ToArray()
                    For i = 0 To nums(0) - 1
                        map(i, j) = states(i)
                    Next
                Next
                maps.Add(New LevelMap(body, map))
            Loop
        End Using
    End Sub

    Public Sub Restart()
        GoLevel(Level)
    End Sub

    Public Event LevelPassed As EventHandler(Of Integer)
    Private Sub RaiseLevelPassed()
        RaiseEvent LevelPassed(Me, Level)
    End Sub

    Public Event Stepped As EventHandler(Of Integer)
    Private Sub RaiseStepped()
        RaiseEvent Stepped(Me, currentMap.StepsCount)
    End Sub

    Public Sub GoDir(dir As Direction)
        currentMap.GoDir(dir)
        DrawAllWithoutMap()
        RaiseStepped()
        If currentMap.Map.AsEnumerable().All(Function(state) state <> SquareState.EndPoint) Then
            RaiseLevelPassed()
            GoNext()
        End If
    End Sub

    Public Sub Undo()
        currentMap.Undo()
        DrawAllWithoutMap()
        RaiseStepped()
    End Sub

    Public ReadOnly Property Level As Integer

    Public ReadOnly Property MaxLevel As Integer
        Get
            Return maps.Count
        End Get
    End Property

    Public Sub GoPrev()
        GoLevel(Level - 1)
    End Sub

    Public Sub GoNext()
        GoLevel(Level + 1)
    End Sub

    Public Sub GoLevel(index As Integer)
        If index < 0 OrElse index >= MaxLevel Then Return
        currentMap = maps(index).Clone()
        _Level = index
        BoxView_SizeChanged()
        DrawAll()
        RaiseStepped()
    End Sub

    Public ReadOnly Property CanUndo As Boolean
        Get
            Return currentMap.StepsCount > 0
        End Get
    End Property

    Protected Overrides ReadOnly Property VisualChildrenCount As Integer
        Get
            Return 3
        End Get
    End Property

    Protected Overrides Function GetVisualChild(index As Integer) As Visual
        Select Case index
            Case 0
                Return mapVisual
            Case 1
                Return boxVisual
            Case 2
                Return bodyVisual
            Case Else
                Throw New IndexOutOfRangeException()
        End Select
    End Function

    Private Sub DrawAll()
        DrawMap()
        DrawBox()
        DrawBody()
    End Sub

    Private Sub DrawAllWithoutMap()
        DrawBox()
        DrawBody()
    End Sub

    Private Sub DrawMap()
        Dim roundr = times / 8
        Dim rectsize As New Size(times, times)
        Dim maxx = currentMap.Map.GetLength(0)
        Dim maxy = currentMap.Map.GetLength(1)
        Using dc As DrawingContext = mapVisual.RenderOpen()
            dc.DrawRectangle(Brushes.Yellow, NoBorderPen, New Rect(CType(off, Point), New IntPoint(maxx, maxy).ToSize(times)))
            For i = 0 To maxx - 1
                For j = 0 To maxy - 1
                    Dim state = currentMap.Map(i, j)
                    Dim p = New IntPoint(i, j)
                    Select Case state
                        Case SquareState.Road
                        Case SquareState.Wall
                            dc.DrawRoundedRectangle(Brushes.Gray, BorderPen, New Rect(p.ToPoint(times) + off, rectsize), roundr, roundr)
                        Case Else
                            If state And SquareState.EndPoint Then
                                dc.DrawRoundedRectangle(Brushes.Red, BorderPen, New Rect(p.ToPoint(times) + off, rectsize), roundr, roundr)
                            End If
                    End Select
                Next
            Next
        End Using
    End Sub

    Private Sub DrawBox()
        Dim roundr = times / 8
        Dim rectsize As New Size(times, times)
        Dim maxx = currentMap.Map.GetLength(0)
        Dim maxy = currentMap.Map.GetLength(1)
        Using dc As DrawingContext = boxVisual.RenderOpen()
            For i = 0 To maxx - 1
                For j = 0 To maxy - 1
                    If currentMap.Map(i, j) And SquareState.Box Then
                        dc.DrawRoundedRectangle(Brushes.Orange, BorderPen, New Rect(New IntPoint(i, j).ToPoint(times) + off, rectsize), roundr, roundr)
                    End If
                Next
            Next
        End Using
    End Sub

    Private Sub DrawBody()
        Dim r = times / 2
        Using dc As DrawingContext = bodyVisual.RenderOpen()
            dc.DrawEllipse(Brushes.DeepSkyBlue, BorderPen, currentMap.Body.ToPoint(times) + New Vector(r, r) + off, r, r)
        End Using
    End Sub

    Private Sub BoxView_SizeChanged() Handles Me.SizeChanged
        If currentMap IsNot Nothing Then
            Dim maxx = currentMap.Map.GetLength(0)
            Dim maxy = currentMap.Map.GetLength(1)
            Dim tw As Double = Me.ActualWidth / maxx
            Dim th As Double = Me.ActualHeight / maxy
            times = Math.Min(tw, th)
            off = New Vector((Me.ActualWidth - maxx * times) / 2, (Me.ActualHeight - maxy * times) / 2)
            DrawAll()
        End If
    End Sub

    Private Sub BoxView_MouseUp(sender As Object, e As MouseButtonEventArgs) Handles Me.MouseUp
        If e.LeftButton = MouseButtonState.Released Then
            Dim p = e.GetPosition(Me) - off
            Dim acp As New IntPoint(p.X \ times, p.Y \ times)
            If acp.X = currentMap.Body.X OrElse acp.Y = currentMap.Body.Y Then
                If acp.X - currentMap.Body.X = 1 Then
                    GoDir(Direction.Right)
                ElseIf acp.X - currentMap.Body.X = -1 Then
                    GoDir(Direction.Left)
                ElseIf acp.Y - currentMap.Body.Y = 1 Then
                    GoDir(Direction.Bottom)
                ElseIf acp.Y - currentMap.Body.Y = -1 Then
                    GoDir(Direction.Top)
                End If
            End If
        End If
    End Sub
End Class
