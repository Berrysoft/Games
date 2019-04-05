Imports System.IO
Imports System.Runtime.Serialization.Formatters.Binary
Imports System.Windows.Threading
Imports Berrysoft.Data
Imports GamesHelper
Imports Microsoft.Win32

Class MainWindow
    Private Const MAX_WIDTH As Integer = 30
    Private Const MAX_HEIGHT As Integer = 30
    Private times, realWidth, realHeight As Double
    Private off As Vector
    Private visual As New DrawingVisual
    Private bodyVisual As New DrawingVisual
    Private pathVisual As New DrawingVisual
    Private state As TipState
    Private WithEvents Timer As New DispatcherTimer(TimeSpan.FromMilliseconds(100), DispatcherPriority.Background, AddressOf DrawTips, Me.Dispatcher)

    Public Sub New()
        InitializeComponent()
        Me.AddVisualChild(visual)
        Me.AddVisualChild(bodyVisual)
        Me.AddVisualChild(pathVisual)
    End Sub

    Public Sub New(fileName As String)
        Me.New()
        OpenMaze(fileName)
    End Sub

    Protected Overrides ReadOnly Property VisualChildrenCount As Integer
        Get
            Return 3
        End Get
    End Property

    Protected Overrides Function GetVisualChild(index As Integer) As Visual
        Select Case index
            Case 0
                Return visual
            Case 1
                Return bodyVisual
            Case 2
                Return pathVisual
            Case Else
                Throw New IndexOutOfRangeException
        End Select
    End Function

    Private maze As Maze
    Private Async Sub CreateMaze()
        Timer.Stop()
        Await Me.Dispatcher.InvokeAsync(AddressOf DrawCreating)
        maze = Await Task.Run(Function() Maze.Create(MAX_WIDTH, MAX_HEIGHT))
        Await Me.Dispatcher.InvokeAsync(
            Sub()
                DrawMaze()
                current = New IntPoint(0, 0)
                state = TipState.NoTips
                DrawCurrent()
            End Sub)
    End Sub

    Private ReadOnly CreatingText As New FormattedText("正在生成...", Threading.Thread.CurrentThread.CurrentCulture, FlowDirection.LeftToRight, New Typeface("Microsoft Tahei UI"), 40, Brushes.Yellow, 1)
    Private Sub DrawCreating()
        Using dc As DrawingContext = visual.RenderOpen
            dc.DrawText(CreatingText, New Point((realWidth - CreatingText.Width) / 2, (realHeight - CreatingText.Height) / 2) + off)
        End Using
        bodyVisual.RenderOpen().Close()
        pathVisual.RenderOpen().Close()
    End Sub

    Private Const Offset As Integer = 20
    Private Shared ReadOnly WallPen As New Pen(Brushes.WhiteSmoke, 2)
    Private Sub DrawMaze()
        Using dc As DrawingContext = visual.RenderOpen
            dc.DrawLine(WallPen, off, New IntPoint(maze.Width, 0).ToPoint(times) + off)
            dc.DrawLine(WallPen, New IntPoint(0, 1).ToPoint(times) + off, New IntPoint(0, maze.Height).ToPoint(times) + off)
            For x = 0 To maze.Width - 1
                For y = 0 To maze.Height - 1
                    Select Case maze(x, y)
                        Case WallState.Both
                            If x <> maze.Width - 1 OrElse y <> maze.Height - 1 Then
                                DrawWall(dc, x, y, True)
                            End If
                            DrawWall(dc, x, y, False)
                        Case WallState.Right
                            If x <> maze.Width - 1 OrElse y <> maze.Height - 1 Then
                                DrawWall(dc, x, y, True)
                            End If
                        Case WallState.Bottom
                            DrawWall(dc, x, y, False)
                    End Select
                Next
            Next
        End Using
    End Sub

    Private Sub DrawWall(dc As DrawingContext, x As Integer, y As Integer, right As Boolean)
        If right Then
            dc.DrawLine(WallPen, New IntPoint(x + 1, y).ToPoint(times) + off, New IntPoint(x + 1, y + 1).ToPoint(times) + off)
        Else
            dc.DrawLine(WallPen, New IntPoint(x, y + 1).ToPoint(times) + off, New IntPoint(x + 1, y + 1).ToPoint(times) + off)
        End If
    End Sub

    Private Sub MainWindow_SizeChanged(sender As Object, e As SizeChangedEventArgs) Handles Me.SizeChanged
        Dim s = GetClientSizeWithDpi(Me, bodyVisual)
        times = Math.Min((s.Width - 2 * Offset) / MAX_WIDTH, (s.Height - 2 * Offset) / MAX_HEIGHT)
        realWidth = MAX_WIDTH * times
        realHeight = MAX_HEIGHT * times
        off = New Vector((s.Width - realWidth) / 2, (s.Height - realHeight) / 2)
        Body.SetFontSize(times * 4 / 5)
        If maze IsNot Nothing Then
            DrawMaze()
            DrawCurrent()
        End If
    End Sub

    Private Sub DrawPath()
        SetState(TipState.Path)
    End Sub

    Private path As IReadOnlyCollection(Of ITreeBase(Of IntPoint))
    Private Sub DrawPath(generate As Boolean)
        If generate Then
            GeneratePath()
        End If
        If path IsNot Nothing Then
            Using dc As DrawingContext = pathVisual.RenderOpen
                For i = path.Count - 1 To 0 Step -1
                    If path(i).Value.X = current.X AndAlso path(i).Value.Y = current.Y Then
                        Exit For
                    End If
                    Dim point = path(i).Value
                    DrawTip(dc, point)
                Next
            End Using
        End If
    End Sub

    Private Sub DrawNext()
        SetState(TipState.Crossing)
    End Sub

    Private Sub DrawNext(generate As Boolean)
        Using dc As DrawingContext = pathVisual.RenderOpen
            Dim point = GetNextPoint(generate)
            If point.HasValue Then
                DrawTip(dc, point.Value)
            End If
        End Using
    End Sub

    Private Function GetNextPoint(generate As Boolean) As IntPoint?
        If generate Then
            GeneratePath()
        End If
        If AtEnd(current) Then Return Nothing
        Dim i As Integer = 0
        For j = 0 To path.Count - 1
            If path(j).Value.X = current.X AndAlso path(j).Value.Y = current.Y Then
                i = j
                Exit For
            End If
        Next
        If i = 0 Then
            If path(i).Count > 2 Then
                Return path(i + 1).Value
            Else
                i += 1
            End If
        End If
        Do While i < path.Count
            If path(i).Count > 1 Then
                Return path(i + 1).Value
            End If
            i += 1
        Loop
        Return path(path.Count - 1).Value
    End Function

    Private Sub DrawTip(dc As DrawingContext, point As IntPoint)
        dc.DrawEllipse(Brushes.PaleVioletRed, Nothing, point.ToPoint(times) + New Vector(times / 2, times / 2) + off, times / 6, times / 6)
    End Sub

    Private drawTipsIndex As Integer
    Private Sub DrawTips()
        If drawTipsIndex >= path.Count Then
            Timer.Stop()
            drawTipsIndex = 0
            Return
        End If
        If path IsNot Nothing Then
            Dim point = path(drawTipsIndex).Value
            current = point
            DrawCurrent(False)
            drawTipsIndex += 1
        End If
    End Sub

    Private ReadOnly Body As New FormattedText("膜", Threading.Thread.CurrentThread.CurrentCulture, FlowDirection.LeftToRight, New Typeface("Microsoft Tahei UI"), 10, Brushes.Yellow, 1)
    Private Sub DrawBody(dc As DrawingContext, point As IntPoint)
        dc.DrawText(Body, point.ToPoint(times) + New Vector(times / 10, times / 10) + off)
    End Sub

    Private current As IntPoint
    Private Sub GeneratePath()
        Dim graph = maze.ToGraph
        Dim tree = graph.ToDFSTree(current)
        For Each item In tree.AsDFSWithPath
            Dim node = item.Node
            If node.Value.X = maze.Width - 1 AndAlso node.Value.Y = maze.Height - 1 Then
                path = item.Path
                Exit For
            End If
        Next
    End Sub

    Private Sub StartDrawTips()
        If Timer.IsEnabled Then
            Timer.Stop()
        Else
            If AtEnd(current) Then
                current = New IntPoint(0, 0)
                GeneratePath()
            End If
            GeneratePath()
            drawTipsIndex = 0
            Timer.Start()
        End If
    End Sub

    Private Function AtEnd(point As IntPoint) As Boolean
        Return point.X = maze.Width - 1 AndAlso point.Y = maze.Height - 1
    End Function

    Private Sub DrawCurrent(Optional generate As Boolean = True)
        Using dc As DrawingContext = bodyVisual.RenderOpen
            DrawBody(dc, current)
        End Using
        Select Case state
            Case TipState.NoTips
                pathVisual.RenderOpen.Close()
            Case TipState.Crossing
                DrawNext(generate)
            Case TipState.Path
                DrawPath(generate)
        End Select
    End Sub

    Private Sub SetState(state As TipState)
        Select Case Me.state
            Case state
                Me.state = TipState.NoTips
            Case Else
                Me.state = state
        End Select
        DrawCurrent()
    End Sub

    Private Sub GoUp()
        If Timer.IsEnabled Then Exit Sub
        If current.Y > 0 AndAlso maze(current.X, current.Y - 1).HasFlag(WallState.Right) Then
            current.Y -= 1
        End If
        DrawCurrent()
    End Sub

    Private Sub GoDown()
        If Timer.IsEnabled Then Exit Sub
        If current.Y < maze.Height - 1 AndAlso maze(current.X, current.Y).HasFlag(WallState.Right) Then
            current.Y += 1
        End If
        DrawCurrent()
    End Sub

    Private Sub GoLeft()
        If Timer.IsEnabled Then Exit Sub
        If current.X > 0 AndAlso maze(current.X - 1, current.Y).HasFlag(WallState.Bottom) Then
            current.X -= 1
        End If
        DrawCurrent()
    End Sub

    Private Sub GoRight()
        If Timer.IsEnabled Then Exit Sub
        If current.X < maze.Width - 1 AndAlso maze(current.X, current.Y).HasFlag(WallState.Bottom) Then
            current.X += 1
        End If
        DrawCurrent()
    End Sub

    Private Sub GoSUp()
        If Timer.IsEnabled Then Exit Sub
        Do While current.Y > 0 AndAlso maze(current.X, current.Y - 1).HasFlag(WallState.Right)
            current.Y -= 1
        Loop
        DrawCurrent()
    End Sub

    Private Sub GoSDown()
        If Timer.IsEnabled Then Exit Sub
        Do While current.Y < maze.Height - 1 AndAlso maze(current.X, current.Y).HasFlag(WallState.Right)
            current.Y += 1
        Loop
        DrawCurrent()
    End Sub

    Private Sub GoSLeft()
        If Timer.IsEnabled Then Exit Sub
        Do While current.X > 0 AndAlso maze(current.X - 1, current.Y).HasFlag(WallState.Bottom)
            current.X -= 1
        Loop
        DrawCurrent()
    End Sub

    Private Sub GoSRight()
        If Timer.IsEnabled Then Exit Sub
        Do While current.X < maze.Width - 1 AndAlso maze(current.X, current.Y).HasFlag(WallState.Bottom)
            current.X += 1
        Loop
        DrawCurrent()
    End Sub

    Private saveDialog As New SaveFileDialog() With {.Title = "保存迷宫", .Filter = "迷宫文件|*.maze"}
    Private Sub SaveMaze()
        If saveDialog.ShowDialog Then
            Using stream As New FileStream(saveDialog.FileName, FileMode.Create)
                Dim format As New BinaryFormatter
                format.Serialize(stream, current)
                format.Serialize(stream, maze)
            End Using
        End If
    End Sub

    Private openDialog As New OpenFileDialog() With {.Title = "打开迷宫", .Filter = "迷宫文件|*.maze"}
    Private Sub OpenMaze()
        If openDialog.ShowDialog Then
            OpenMaze(openDialog.FileName)
        End If
    End Sub

    Private Sub OpenMaze(fileName As String)
        Timer.Stop()
        Using stream As New FileStream(fileName, FileMode.Open)
            Dim format As New BinaryFormatter
            current = format.Deserialize(stream)
            maze = format.Deserialize(stream)
        End Using
        DrawMaze()
        DrawCurrent()
    End Sub

    Private Sub MainWindow_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        If maze Is Nothing Then
            CreateMaze()
        End If
    End Sub
End Class

Enum TipState
    NoTips
    Crossing
    Path
End Enum
