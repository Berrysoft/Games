Imports System.IO
Imports GamesHelper

Class BoxView
    Inherits FrameworkElement

    Private Shared ReadOnly NoBorderPen As New Pen(Brushes.Transparent, 0)
    Private Shared ReadOnly BorderPen As New Pen(Brushes.Black, 1)

    Private times As Double
    Private off As Vector

    Private mapVisual As New DrawingVisual
    Private bodyVisual As New DrawingVisual

    Private maps As New List(Of LevelMap)
    Private currentMap As LevelMap

    Public Sub New()
        AddVisualChild(mapVisual)
        AddVisualChild(bodyVisual)
        LoadLevel()
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
            dc.DrawEllipse(Brushes.DeepSkyBlue, BorderPen, currentMap.Body.ToPoint(times) + New Vector(times / 2, times / 2) + off, times / 2, times / 2)
        End Using
    End Sub

    Private Sub BoxView_SizeChanged() Handles Me.SizeChanged
        Dim maxx = currentMap.Map.GetLength(0)
        Dim maxy = currentMap.Map.GetLength(1)
        Dim tw As Double = Me.ActualWidth / maxx
        Dim th As Double = Me.ActualHeight / maxy
        times = Math.Min(tw, th)
        off = New Vector((Me.ActualWidth - maxx * times) / 2, (Me.ActualHeight - maxy * times) / 2)
        DrawAll()
    End Sub
End Class
