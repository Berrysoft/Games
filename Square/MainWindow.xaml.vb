Imports System.Windows.Threading
Imports GamesHelper

Class MainWindow
#Region "Fields"
    Private Const CELLSIZE As Integer = 10
    Private Const MAX_WIDTH As Integer = 200
    Private Const MAX_HEIGHT As Integer = 360
    Private Const CAPTION As String = "{0} - 俄罗斯方块"
    Private Shared ReadOnly NoBorderPen As New Pen(Brushes.Transparent, 0)
    Private Shared ReadOnly BackgroundBrush As New SolidColorBrush(Color.FromRgb(31, 31, 31))
    Private Shared ReadOnly Cell As New IntPoint(CELLSIZE, CELLSIZE)
    Private Shared ReadOnly GoDown As New IntPoint(0, CELLSIZE)
    Private Shared ReadOnly GoRight As New IntPoint(CELLSIZE, 0)
    Private Shared ReadOnly GoLeft As New IntPoint(-CELLSIZE, 0)
    Private Shared ReadOnly Center As New IntPoint(MAX_WIDTH \ 2, -2 * CELLSIZE)
    Private Shared ReadOnly Patterns() As Pattern
    Private Shared ReadOnly FillBrush As Brush = Brushes.Green
    Private Shared ReadOnly BorderPen As New Pen(Brushes.Black, 1)
    Private times As Double
    Private realWidth As Double
    Private realHeight As Double
    Private off As Vector
    Private cells(MAX_WIDTH \ CELLSIZE - 1, MAX_HEIGHT \ CELLSIZE - 1) As Boolean
    Private backVisual As New DrawingVisual()
    Private squareVisual As New DrawingVisual()
    Private WithEvents Timer As New DispatcherTimer()
    Private rnd As New Random()
    Private current As Pattern
    Private score As Integer
    Private playing As Boolean?
    Shared Sub New()
        '方块
        Dim s4 As New Pattern(New IntPoint(0, 0), New IntPoint(-CELLSIZE, 0), New IntPoint(-CELLSIZE, -CELLSIZE), New IntPoint(0, -CELLSIZE))
        s4.NextPattern = s4
        '直线
        Dim lineh As New Pattern(New IntPoint(-CELLSIZE, 0), New IntPoint(0, 0), New IntPoint(CELLSIZE, 0), New IntPoint(2 * CELLSIZE, 0))
        Dim linev As New Pattern(New IntPoint(0, CELLSIZE), New IntPoint(0, -2 * CELLSIZE), New IntPoint(0, -CELLSIZE), New IntPoint(0, 0))
        lineh.NextPattern = linev
        linev.NextPattern = lineh
        'L
        Dim l1 As New Pattern(New IntPoint(0, CELLSIZE), New IntPoint(-CELLSIZE, CELLSIZE), New IntPoint(-CELLSIZE, 0), New IntPoint(-CELLSIZE, -CELLSIZE))
        Dim l2 As New Pattern(New IntPoint(-2 * CELLSIZE, 0), New IntPoint(0, -CELLSIZE), New IntPoint(-CELLSIZE, -CELLSIZE), New IntPoint(-2 * CELLSIZE, -CELLSIZE))
        Dim l3 As New Pattern(New IntPoint(-CELLSIZE, -2 * CELLSIZE), New IntPoint(0, 0), New IntPoint(0, -CELLSIZE), New IntPoint(0, -2 * CELLSIZE))
        Dim l4 As New Pattern(New IntPoint(-CELLSIZE, 0), New IntPoint(0, 0), New IntPoint(CELLSIZE, 0), New IntPoint(CELLSIZE, -CELLSIZE))
        l1.NextPattern = l2
        l2.NextPattern = l3
        l3.NextPattern = l4
        l4.NextPattern = l1
        '反L
        Dim bl1 As New Pattern(New IntPoint(0, CELLSIZE), New IntPoint(-CELLSIZE, CELLSIZE), New IntPoint(0, 0), New IntPoint(0, -CELLSIZE))
        Dim bl2 As New Pattern(New IntPoint(0, 0), New IntPoint(-CELLSIZE, 0), New IntPoint(-2 * CELLSIZE, 0), New IntPoint(-2 * CELLSIZE, -CELLSIZE))
        Dim bl3 As New Pattern(New IntPoint(0, -2 * CELLSIZE), New IntPoint(-CELLSIZE, 0), New IntPoint(-CELLSIZE, -CELLSIZE), New IntPoint(-CELLSIZE, -2 * CELLSIZE))
        Dim bl4 As New Pattern(New IntPoint(CELLSIZE, 0), New IntPoint(-CELLSIZE, -CELLSIZE), New IntPoint(0, -CELLSIZE), New IntPoint(CELLSIZE, -CELLSIZE))
        bl1.NextPattern = bl2
        bl2.NextPattern = bl3
        bl3.NextPattern = bl4
        bl4.NextPattern = bl1
        'T
        Dim t1 As New Pattern(New IntPoint(0, 0), New IntPoint(-CELLSIZE, -CELLSIZE), New IntPoint(CELLSIZE, -CELLSIZE), New IntPoint(0, -CELLSIZE))
        Dim t2 As New Pattern(New IntPoint(0, 0), New IntPoint(CELLSIZE, CELLSIZE), New IntPoint(CELLSIZE, 0), New IntPoint(CELLSIZE, -CELLSIZE))
        Dim t3 As New Pattern(New IntPoint(-CELLSIZE, CELLSIZE), New IntPoint(0, CELLSIZE), New IntPoint(CELLSIZE, CELLSIZE), New IntPoint(0, 0))
        Dim t4 As New Pattern(New IntPoint(0, 0), New IntPoint(-CELLSIZE, CELLSIZE), New IntPoint(-CELLSIZE, 0), New IntPoint(-CELLSIZE, -CELLSIZE))
        t1.NextPattern = t2
        t2.NextPattern = t3
        t3.NextPattern = t4
        t4.NextPattern = t1
        'Z
        Dim zh As New Pattern(New IntPoint(0, 0), New IntPoint(-CELLSIZE, 0), New IntPoint(0, -CELLSIZE), New IntPoint(CELLSIZE, -CELLSIZE))
        Dim zv As New Pattern(New IntPoint(0, CELLSIZE), New IntPoint(-CELLSIZE, 0), New IntPoint(0, 0), New IntPoint(-CELLSIZE, -CELLSIZE))
        zh.NextPattern = zv
        zv.NextPattern = zh
        '反Z
        Dim bzh As New Pattern(New IntPoint(CELLSIZE, 0), New IntPoint(0, 0), New IntPoint(0, -CELLSIZE), New IntPoint(-CELLSIZE, -CELLSIZE))
        Dim bzv As New Pattern(New IntPoint(0, 0), New IntPoint(-CELLSIZE, CELLSIZE), New IntPoint(-CELLSIZE, 0), New IntPoint(0, -CELLSIZE))
        bzh.NextPattern = bzv
        bzv.NextPattern = bzh
        Patterns = {s4, lineh, linev, l1, l2, l3, l4, bl1, bl2, bl3, bl4, t1, t2, t3, t4, zh, zv, bzh, bzv}
    End Sub
#End Region
#Region "Drawing"
    Private Sub DrawBackground()
        Using dc = backVisual.RenderOpen()
            dc.DrawRectangle(BackgroundBrush, NoBorderPen, New Rect(off.X, off.Y, realWidth, realHeight))
        End Using
    End Sub
    Private Sub DrawPattern(dc As DrawingContext, current As Pattern)
        For Each p In current.Points
            dc.DrawRectangle(FillBrush, BorderPen, New Rect((current.Center + p).ToPoint(times) + off, Cell.ToSize(times)))
        Next
    End Sub
    Private Sub DrawGround(dc As DrawingContext, cells(,) As Boolean)
        For i = 0 To MAX_WIDTH \ CELLSIZE - 1
            For j = MAX_HEIGHT \ CELLSIZE - 1 To 0 Step -1
                If cells(i, j) Then
                    dc.DrawRectangle(FillBrush, BorderPen, New Rect((New IntPoint(i, j) * CELLSIZE).ToPoint(times) + off, Cell.ToSize(times)))
                End If
            Next
        Next
    End Sub
    Private Sub Redraw()
        Using dc = squareVisual.RenderOpen()
            DrawPattern(dc, current)
            DrawGround(dc, cells)
        End Using
    End Sub
    Protected Overrides ReadOnly Property VisualChildrenCount As Integer
        Get
            Return 2
        End Get
    End Property
    Protected Overrides Function GetVisualChild(index As Integer) As Visual
        Select Case index
            Case 0
                Return backVisual
            Case 1
                Return squareVisual
            Case Else
                Throw New IndexOutOfRangeException()
        End Select
    End Function
#End Region
#Region "Initialize"
    Public Sub New()
        InitializeComponent()
        Me.AddVisualChild(backVisual)
        Me.AddVisualChild(squareVisual)
        Timer.Interval = TimeSpan.FromMilliseconds(100)
        InitGame()
    End Sub
    Private Sub InitGame()
        score = -1
        playing = True
        NewPattern()
        DrawBackground()
        Timer.Start()
    End Sub
    Private Sub NewPattern()
        score += 1
        Me.Title = String.Format(CAPTION, score)
        current = Patterns(rnd.Next(Patterns.Length - 1))
        current.Center = Center
    End Sub
    Private Sub Restart()
        For j = MAX_HEIGHT \ CELLSIZE - 1 To 0 Step -1
            Dim esc As Boolean = True
            For i = 0 To MAX_WIDTH \ CELLSIZE - 1
                If cells(i, j) Then
                    esc = False
                    cells(i, j) = False
                End If
            Next
            If esc Then
                Exit For
            End If
        Next
        InitGame()
    End Sub
#End Region
#Region "Game"
    Private Sub MoveRight()
        If playing Then
            current.MoveRight(cells)
            Redraw()
        End If
    End Sub
    Private Sub MoveLeft()
        If playing Then
            current.MoveLeft(cells)
            Redraw()
        End If
    End Sub
    Private Sub Roll()
        If playing Then
            current = current.Roll(cells)
            Redraw()
        End If
    End Sub
    Private Sub QuickDown()
        If playing Then
            Do Until current.IsConcat(cells)
                current.Center += GoDown
            Loop
            Redraw()
        End If
    End Sub
    Private Sub Pause()
        If playing.HasValue Then
            If playing Then
                Timer.Stop()
                playing = False
            Else
                Timer.Start()
                playing = True
            End If
        End If
    End Sub
    Private Sub ClearRow()
        For j = MAX_HEIGHT \ CELLSIZE - 1 To 0 Step -1
            Dim clear As Boolean = True
            Dim esc As Boolean = True
            For i = 0 To MAX_WIDTH \ CELLSIZE - 1
                If Not cells(i, j) Then
                    clear = False
                    Exit For
                Else
                    esc = False
                End If
            Next
            If clear Then
                For jj = j To 1 Step -1
                    Dim eesc As Boolean = True
                    For ii = 0 To MAX_WIDTH \ CELLSIZE - 1
                        cells(ii, jj) = cells(ii, jj - 1)
                        If cells(ii, jj) Then
                            eesc = False
                        End If
                    Next
                    If eesc Then
                        Exit For
                    End If
                Next
                For ii = 0 To MAX_WIDTH \ CELLSIZE - 1
                    cells(ii, 0) = False
                Next
                j += 1
            End If
            If esc Then
                Exit Sub
            End If
        Next
    End Sub
    Private Sub Timer_Tick(sender As Object, e As EventArgs) Handles Timer.Tick
        If current.IsConcat(cells) Then
            If Not current.Concat(cells) Then
                Timer.Stop()
                playing = Nothing
            End If
            NewPattern()
        Else
            current.Center += GoDown
        End If
        Redraw()
        ClearRow()
    End Sub

    Private Sub MainWindow_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        If IsDarkModeEnabledForApp() Then
            SetWindowDarkMode(Me)
        End If
    End Sub

    Private Sub MainWindow_SizeChanged(sender As Object, e As SizeChangedEventArgs) Handles Me.SizeChanged
        Dim s = GetClientSizeWithDpi(Me, squareVisual)
        times = Math.Min(s.Width / MAX_WIDTH, s.Height / MAX_HEIGHT)
        realWidth = MAX_WIDTH * times
        realHeight = MAX_HEIGHT * times
        off = New Vector((s.Width - realWidth) / 2, (s.Height - realHeight) / 2)
        Redraw()
        DrawBackground()
    End Sub
#End Region
#Region "Pattern"
    Class Pattern
        Public Sub New(ParamArray points() As IntPoint)
            Me.Points = points
        End Sub
        Public Property NextPattern As Pattern
        Public Property Center As IntPoint
        Public ReadOnly Property Points As IntPoint()
        Public Function IsConcat(cells(,) As Boolean) As Boolean
            For Each p In Points
                Dim rp As IntPoint = Center + p
                Dim x As Integer = rp.X \ CELLSIZE
                Dim y As Integer = rp.Y \ CELLSIZE
                If y + 1 >= MAX_HEIGHT \ CELLSIZE Then
                    Return True
                ElseIf y >= 0 AndAlso cells(x, y + 1) Then
                    Return True
                End If
            Next
            Return False
        End Function
        Public Function Concat(ByRef cells(,) As Boolean) As Boolean
            Dim result As Boolean = True
            For Each p In Points
                Dim rp As IntPoint = Center + p
                Dim x As Integer = rp.X \ CELLSIZE
                Dim y As Integer = rp.Y \ CELLSIZE
                If y < 0 Then
                    result = False
                    Continue For
                End If
                cells(x, y) = True
            Next
            Return result
        End Function
        Public Function MoveRight(cells(,) As Boolean) As Boolean
            Dim c = Center + GoRight
            For Each p In Points
                Dim rp As IntPoint = c + p
                Dim x As Integer = rp.X \ CELLSIZE
                Dim y As Integer = rp.Y \ CELLSIZE
                If x >= MAX_WIDTH \ CELLSIZE OrElse (y > 0 AndAlso cells(x, y)) Then
                    Return False
                End If
            Next
            Center = c
            Return True
        End Function
        Public Function MoveLeft(cells(,) As Boolean, Optional time As Integer = 1) As Boolean
            Dim c = Center + GoLeft * time
            For Each p In Points
                Dim rp As IntPoint = c + p
                Dim x As Integer = rp.X \ CELLSIZE
                Dim y As Integer = rp.Y \ CELLSIZE
                If x < 0 OrElse x >= MAX_WIDTH \ CELLSIZE OrElse (y > 0 AndAlso cells(x, y)) Then
                    Return False
                End If
            Next
            Center = c
            Return True
        End Function
        Public Function Roll(cells(,) As Boolean) As Pattern
            Dim pp = NextPattern
            pp.Center = Center
            For Each p In pp.Points
                Dim rp As IntPoint = pp.Center + p
                Dim x As Integer = rp.X \ CELLSIZE
                Dim y As Integer = rp.Y \ CELLSIZE
                If x < 0 Then
                    If pp.MoveRight(cells) Then
                        Return pp
                    Else
                        Return Me
                    End If
                ElseIf x >= MAX_WIDTH \ CELLSIZE Then
                    If pp.MoveLeft(cells) OrElse pp.MoveLeft(cells, 2) Then
                        Return pp
                    Else
                        Return Me
                    End If
                ElseIf y > 0 AndAlso cells(x, y) Then
                    Return Me
                End If
            Next
            Return pp
        End Function
    End Class
#End Region
End Class
