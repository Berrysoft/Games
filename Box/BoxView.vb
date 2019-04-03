Imports GamesHelper

Class BoxView
    Inherits FrameworkElement

    Private Shared ReadOnly NoBorderPen As New Pen(Brushes.Transparent, 0)
    Private Shared ReadOnly BorderPen As New Pen(Brushes.Black, 1)

    Private times As Double
    Private off As Vector

    Private mapVisual As New DrawingVisual
    Private bodyVisual As New DrawingVisual

    Private maps As LevelMap()
    Private currentMap As LevelMap

    Public Sub New()
        AddVisualChild(mapVisual)
        AddVisualChild(bodyVisual)
        ReDim maps(0)
        maps(0) = New LevelMap(New IntPoint(4, 4),
        {{1, 1, 1, 1, 1, 1, 1, 1},
          {1, 1, 1, 4, 1, 1, 1, 1},
          {1, 1, 1, 0, 1, 1, 1, 1},
          {1, 1, 1, 2, 0, 2, 4, 1},
          {1, 4, 0, 2, 0, 1, 1, 1},
          {1, 1, 1, 1, 2, 1, 1, 1},
          {1, 1, 1, 1, 4, 1, 1, 1},
          {1, 1, 1, 1, 1, 1, 1, 1}})
        GoLevel(0)
        DrawAll()
    End Sub

    Public Sub Restart()
        GoLevel(Level)
    End Sub

    Public Sub GoDir(dir As Direction)
        currentMap.GoDir(dir)
        DrawAll()
        If currentMap.Map.AsEnumerable().All(Function(state) state <> SquareState.EndPoint) Then
            MessageBox.Show("恭喜过关！")
            GoNext()
        End If
    End Sub

    Public Sub Undo()
        currentMap.Undo()
        DrawAll()
    End Sub

    Public ReadOnly Property Level As Integer

    Public ReadOnly Property MaxLevel As Integer
        Get
            Return maps.Length
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
        DrawAll()
    End Sub

    Protected Overrides ReadOnly Property VisualChildrenCount As Integer
        Get
            Return 2
        End Get
    End Property

    Protected Overrides Function GetVisualChild(index As Integer) As Visual
        Select Case index
            Case 0
                Return mapVisual
            Case 1
                Return bodyVisual
            Case Else
                Throw New IndexOutOfRangeException()
        End Select
    End Function

    Private Sub DrawAll()
        DrawMap()
        DrawBody()
    End Sub

    Private Sub DrawMap()
        Dim roundr = times / 8
        Dim rectsize As New Size(times, times)
        Dim maxx = currentMap.Map.GetLength(1)
        Dim maxy = currentMap.Map.GetLength(0)
        Using dc As DrawingContext = mapVisual.RenderOpen()
            dc.DrawRectangle(Brushes.Yellow, NoBorderPen, New Rect(CType(off, Point), New IntPoint(maxx, maxy).ToSize(times)))
            For i = 0 To maxy - 1
                For j = 0 To maxx - 1
                    Dim state = currentMap.Map(i, j)
                    Dim p = New IntPoint(j, i)
                    Select Case state
                        Case SquareState.Road
                        Case SquareState.Wall
                            dc.DrawRoundedRectangle(Brushes.Gray, BorderPen, New Rect(p.ToPoint(times) + off, rectsize), roundr, roundr)
                        Case Else
                            If state And SquareState.EndPoint Then
                                dc.DrawRoundedRectangle(Brushes.Red, BorderPen, New Rect(p.ToPoint(times) + off, rectsize), roundr, roundr)
                            End If
                            If state And SquareState.Box Then
                                dc.DrawRoundedRectangle(Brushes.Orange, BorderPen, New Rect(p.ToPoint(times) + off, rectsize), roundr, roundr)
                            End If
                    End Select
                Next
            Next
        End Using
    End Sub

    Private Sub DrawBody()
        Using dc As DrawingContext = bodyVisual.RenderOpen()
            dc.DrawEllipse(Brushes.Blue, BorderPen, New IntPoint(currentMap.Body.Y, currentMap.Body.X).ToPoint(times) + New Vector(times / 2, times / 2) + off, times / 2, times / 2)
        End Using
    End Sub

    Private Sub BoxView_SizeChanged(sender As Object, e As SizeChangedEventArgs) Handles Me.SizeChanged
        Dim maxx = currentMap.Map.GetLength(1)
        Dim maxy = currentMap.Map.GetLength(0)
        Dim tw As Double = Me.ActualWidth / maxx
        Dim th As Double = Me.ActualHeight / maxy
        times = Math.Min(tw, th)
        off = New Vector((Me.ActualWidth - maxx * times) / 2, (Me.ActualHeight - maxy * times) / 2)
        DrawAll()
    End Sub
End Class
