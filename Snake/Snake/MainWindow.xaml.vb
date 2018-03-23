Imports System.Windows.Threading
Class MainWindow
    Enum GameMode
        One
        Two
        TwoCompete
    End Enum
#Region "Constants"
    Private Const TIMES As Integer = 10
    Private Const MAX_WIDTH As Integer = 380
    Private Const MAX_HEIGHT As Integer = 360
    Private Const SHINE As Integer = 3
    Private Const CAPTION As String = "贪吃蛇"
    Private Const CPTION_WITH_SCORE As String = "贪吃蛇 - 分数：{0}"
    Private Const CAPTION_WITH_PAUSE As String = "贪吃蛇 - 分数：{0} - 暂停"
    Private Const CAPTION_WITH_ENDED As String = "贪吃蛇 - 分数：{0} - 已结束"
    Private Const CAPTION_TWO_SCORE As String = "贪吃蛇 - 绿：{0} 蓝：{1}"
    Private Const CAPTION_TWO_PAUSE As String = "贪吃蛇 - 绿：{0} 蓝：{1} - 暂停"
    Private Const CAPTION_TWO_ENDED As String = "贪吃蛇 - 绿：{0} 蓝：{1} - 已结束"
    Private Shared ReadOnly NoBorderPen As New Pen(Brushes.Transparent, 0)
    Private Shared ReadOnly Body As Brush = Brushes.Green
    Private Shared ReadOnly Head As Brush = Brushes.LightGreen
    Private Shared ReadOnly FoodBrush As Brush = Brushes.Red
    Private Shared ReadOnly Body2 As Brush = Brushes.DeepSkyBlue
    Private Shared ReadOnly Head2 As Brush = Brushes.SkyBlue
    Private Shared ReadOnly FoodBrush2 As Brush = Brushes.Gold
    Private Shared ReadOnly cell As New Vector(TIMES, TIMES)
#End Region
#Region "Varients"
    Private snake As New Queue(Of Point)()
    Private snake2 As New Queue(Of Point)()
    Private WithEvents Timer As New DispatcherTimer()
    Private WithEvents TimerEnded As New DispatcherTimer()
    Private dir As Direction
    Private dir2 As Direction
    Private front As Point
    Private front2 As Point
    Private visual As New DrawingVisual
    Private directed As Boolean
    Private directed2 As Boolean
    Private food As Point
    Private food2 As Point
    Private rnd As New Random
    Private mode As GameMode
#End Region
#Region "Initialize"
    ''' <summary>
    ''' 创建<seealso cref="MainWindow"/>的新实例
    ''' </summary>
    Public Sub New()
        InitializeComponent()
        Me.AddVisualChild(visual)
        TimerEnded.Interval = TimeSpan.FromMilliseconds(400)
        Timer.Interval = TimeSpan.FromMilliseconds(40)
        AddHandler Timer.Tick, AddressOf Timer_Tick
        InitGame()
    End Sub
    ''' <summary>
    ''' 初始化游戏
    ''' </summary>
    Private Sub InitGame()
        mode = GameMode.One
        Me.Title = GetTitle()
        dir = Direction.Right
        front = New Point(10 * TIMES, 16 * TIMES)
        snake.Enqueue(front)
        front.X += TIMES
        snake.Enqueue(front)
        front.X += TIMES
        snake.Enqueue(front)
        CreateFood(snake, food)
        Timer.Start()
    End Sub
    ''' <summary>
    ''' 初始化双人游戏
    ''' </summary>
    Private Sub InitGame2()
        mode = GameMode.Two
        Timer.Stop()
        snake.Clear()
        snake2.Clear()
        RemoveHandler Timer.Tick, AddressOf Timer_Tick
        RemoveHandler Timer.Tick, AddressOf Timer_Tick2
        RemoveHandler Timer.Tick, AddressOf Timer_Tick3
        AddHandler Timer.Tick, AddressOf Timer_Tick2
        dir = Direction.Right
        dir2 = Direction.Right
        front = New Point(10 * TIMES, 12 * TIMES)
        snake.Enqueue(front)
        front.X += TIMES
        snake.Enqueue(front)
        front.X += TIMES
        snake.Enqueue(front)
        front2 = New Point(10 * TIMES, 20 * TIMES)
        snake2.Enqueue(front2)
        front2.X += TIMES
        snake2.Enqueue(front2)
        front2.X += TIMES
        snake2.Enqueue(front2)
        Me.Title = GetTitle2()
        CreateFood(snake, food)
        CreateFood(snake2, food2)
        Timer.Start()
    End Sub
    Private Sub InitGame3()
        mode = GameMode.TwoCompete
        Timer.Stop()
        snake.Clear()
        snake2.Clear()
        RemoveHandler Timer.Tick, AddressOf Timer_Tick
        RemoveHandler Timer.Tick, AddressOf Timer_Tick2
        RemoveHandler Timer.Tick, AddressOf Timer_Tick3
        AddHandler Timer.Tick, AddressOf Timer_Tick3
        dir = Direction.Right
        dir2 = Direction.Right
        front = New Point(10 * TIMES, 12 * TIMES)
        snake.Enqueue(front)
        front.X += TIMES
        snake.Enqueue(front)
        front.X += TIMES
        snake.Enqueue(front)
        front2 = New Point(10 * TIMES, 20 * TIMES)
        snake2.Enqueue(front2)
        front2.X += TIMES
        snake2.Enqueue(front2)
        front2.X += TIMES
        snake2.Enqueue(front2)
        Me.Title = GetTitle2()
        CreateFood3(snake, snake2, food)
        Timer.Start()
    End Sub
    ''' <summary>
    ''' 重新开始游戏
    ''' </summary>
    Private Sub Restart()
        Timer.Stop()
        snake.Clear()
        RemoveHandler Timer.Tick, AddressOf Timer_Tick2
        RemoveHandler Timer.Tick, AddressOf Timer_Tick3
        RemoveHandler Timer.Tick, AddressOf Timer_Tick
        AddHandler Timer.Tick, AddressOf Timer_Tick
        InitGame()
    End Sub
#End Region
#Region "Game"
    ''' <summary>
    ''' 随机生成食物
    ''' </summary>
    ''' <param name="food"></param>
    ''' <param name="snake"></param>
    Private Sub CreateFood(snake As Queue(Of Point), ByRef food As Point)
        Do
            food.X = rnd.Next(MAX_WIDTH / TIMES - 1) * TIMES
            food.Y = rnd.Next(MAX_HEIGHT / TIMES - 1) * TIMES
        Loop While snake.Contains(food)
    End Sub
    Private Sub CreateFood3(snake As Queue(Of Point), snake2 As Queue(Of Point), ByRef food As Point)
        Do
            food.X = rnd.Next(MAX_WIDTH / TIMES - 1) * TIMES
            food.Y = rnd.Next(MAX_HEIGHT / TIMES - 1) * TIMES
        Loop While snake.Contains(food) OrElse snake2.Contains(food)
    End Sub
    ''' <summary>
    ''' 获取当前应该显示的标题
    ''' </summary>
    ''' <returns>当前应该显示的标题</returns>
    Private Function GetTitle() As String
        If snake.Count <= 3 Then
            Return CAPTION
        Else
            Return String.Format(CPTION_WITH_SCORE, GetScore(snake))
        End If
    End Function
    ''' <summary>
    ''' 获取双人模式应该显示的标题
    ''' </summary>
    ''' <returns>双人模式应该显示的标题</returns>
    Private Function GetTitle2() As String
        Return String.Format(CAPTION_TWO_SCORE, GetScore(snake), GetScore(snake2))
    End Function
    ''' <summary>
    ''' 画蛇（头在前，为了使运动看起来更连贯）
    ''' </summary>
    ''' <param name="dc"></param>
    ''' <param name="snake"></param>
    ''' <param name="front"></param>
    ''' <param name="body"></param>
    ''' <param name="head"></param>
    Private Sub DrawSnake(dc As DrawingContext, snake As Queue(Of Point), front As Point, body As Brush, head As Brush)
        dc.DrawRectangle(head, NoBorderPen, New Rect(front, cell))
        For Each s In snake.Take(snake.Count - 1)
            dc.DrawRectangle(body, NoBorderPen, New Rect(s, cell))
        Next
    End Sub
    ''' <summary>
    ''' 画蛇（头在后，为了使头不消失）
    ''' </summary>
    ''' <param name="dc"></param>
    ''' <param name="snake"></param>
    ''' <param name="front"></param>
    ''' <param name="body"></param>
    ''' <param name="head"></param>
    Private Sub DrawSnakeWithHeadLast(dc As DrawingContext, snake As Queue(Of Point), front As Point, body As Brush, head As Brush)
        For Each s In snake
            dc.DrawRectangle(body, NoBorderPen, New Rect(s, cell))
        Next
        dc.DrawRectangle(head, NoBorderPen, New Rect(front, cell))
    End Sub
    ''' <summary>
    ''' 画食物
    ''' </summary>
    ''' <param name="dc"></param>
    ''' <param name="food"></param>
    ''' <param name="brush"></param>
    Private Sub DrawFood(dc As DrawingContext, food As Point, brush As Brush)
        dc.DrawEllipse(brush, NoBorderPen, food + cell / 2, TIMES / 2, TIMES / 2)
    End Sub
    ''' <summary>
    ''' 获取分数
    ''' </summary>
    ''' <param name="snake"></param>
    ''' <returns>分数</returns>
    Private Function GetScore(snake As Queue(Of Point)) As Integer
        Return snake.Count - 3
    End Function
#End Region
#Region "Overrides"
    ''' <summary>
    ''' 重写<seealso cref="FrameworkElement.VisualChildrenCount"/>，并返回此元素内可视子元素的数目
    ''' </summary>
    ''' <returns>1</returns>
    Protected Overrides ReadOnly Property VisualChildrenCount As Integer
        Get
            Return 1
        End Get
    End Property
    ''' <summary>
    ''' 重写<seealso cref="FrameworkElement.GetVisualChild(Integer)"/>，并返回指定索引处的子从子元素的集合
    ''' </summary>
    ''' <param name="index">集合中请求的子元素的从零开始的索引</param>
    ''' <returns>请求的子元素。这不应返回 null；如果提供的索引超出范围，则引发<seealso cref="IndexOutOfRangeException"/>。</returns>
    ''' <exception cref="IndexOutOfRangeException"></exception>
    Protected Overrides Function GetVisualChild(index As Integer) As Visual
        If index = 0 Then
            Return visual
        Else
            Throw New IndexOutOfRangeException()
        End If
    End Function
#End Region
#Region "Execute"
    Private Sub Timer_Tick(sender As Object, e As EventArgs)
        If dir <> Direction.None Then
            Dim f = front
            GetFrontFromDir(dir, f)
            directed = True
            If snake.Contains(f) Then
                dir = Direction.None
                Exit Sub
            End If
            front = f
            snake.Dequeue()
            snake.Enqueue(front)
            Dim dc = visual.RenderOpen()
            DrawSnake(dc, snake, front, Body, Head)
            If front = food Then
                snake.Enqueue(front)
                CreateFood(snake, food)
                Me.Title = GetTitle()
            End If
            DrawFood(dc, food, FoodBrush)
            dc.Close()
        Else
            Timer.Stop()
            Me.Title = String.Format(CAPTION_WITH_ENDED, GetScore(snake))
            TimerEnded.Start()
        End If
    End Sub
    Private Sub Timer_Tick2(sender As Object, e As EventArgs)
        If dir <> Direction.None AndAlso dir2 <> Direction.None Then
            SwitchIfOneContainsAnother()
            Dim dc = visual.RenderOpen()
            DrawSnake(dc, snake, front, Body, Head)
            DrawSnake(dc, snake2, front2, Body2, Head2)
            If front = food Then
                snake.Enqueue(front)
                CreateFood(snake, food)
                Me.Title = GetTitle2()
            End If
            If front2 = food2 Then
                snake2.Enqueue(front2)
                CreateFood(snake2, food2)
                Me.Title = GetTitle2()
            End If
            DrawFood(dc, food, FoodBrush)
            DrawFood(dc, food2, FoodBrush2)
            dc.Close()
        Else
            Timer.Stop()
            Me.Title = String.Format(CAPTION_TWO_ENDED, GetScore(snake), GetScore(snake2))
            TimerEnded.Start()
        End If
    End Sub
    Private Sub Timer_Tick3(sender As Object, e As EventArgs)
        If dir <> Direction.None AndAlso dir2 <> Direction.None Then
            SwitchIfOneContainsAnother()
            Dim dc = visual.RenderOpen()
            DrawSnake(dc, snake, front, Body, Head)
            DrawSnake(dc, snake2, front2, Body2, Head2)
            If front = food Then
                snake.Enqueue(front)
                CreateFood3(snake, snake2, food)
                Me.Title = GetTitle2()
            ElseIf front2 = food Then
                snake2.Enqueue(front2)
                CreateFood3(snake, snake2, food)
                Me.Title = GetTitle2()
            End If
            DrawFood(dc, food, FoodBrush)
            dc.Close()
        Else
            Timer.Stop()
            Me.Title = String.Format(CAPTION_TWO_ENDED, GetScore(snake), GetScore(snake2))
            TimerEnded.Start()
        End If
    End Sub
    Private Sub GetFrontFromDir(dir As Direction, ByRef f As Point)
        Select Case dir
            Case Direction.Left
                f.X -= TIMES
                If f.X < 0 Then
                    f.X += MAX_WIDTH
                End If
            Case Direction.Top
                f.Y -= TIMES
                If f.Y < 0 Then
                    f.Y += MAX_HEIGHT
                End If
            Case Direction.Right
                f.X += TIMES
                If f.X >= MAX_WIDTH Then
                    f.X -= MAX_WIDTH
                End If
            Case Direction.Bottom
                f.Y += TIMES
                If f.Y >= MAX_HEIGHT Then
                    f.Y -= MAX_HEIGHT
                End If
        End Select
    End Sub
    Private Sub SwitchIfOneContainsAnother()
        Dim f = front
        Dim f2 = front2
        GetFrontFromDir(dir, f)
        directed = True
        GetFrontFromDir(dir2, f2)
        directed2 = True
        If snake.Contains(f) Then
            dir = Direction.None
            Exit Sub
        End If
        If snake2.Contains(f2) Then
            dir2 = Direction.None
            Exit Sub
        End If
        If f = f2 Then
            dir = Direction.None
            dir2 = Direction.None
        Else
            If snake.Contains(f2) Then
                Do Until snake.Peek() = f2
                    snake.Dequeue()
                Loop
                snake.Dequeue()
                snake2.Enqueue(f2)
            ElseIf snake2.Contains(f) Then
                Do Until snake2.Peek() = f
                    snake2.Dequeue()
                Loop
                snake2.Dequeue()
                snake.Enqueue(f)
            End If
        End If
        front = f
        front2 = f2
        snake.Dequeue()
        snake.Enqueue(front)
        snake2.Dequeue()
        snake2.Enqueue(front2)
    End Sub
    Private Sub Turn_Executed(sender As Object, e As ExecutedRoutedEventArgs)
        If Timer.IsEnabled Then
            Dim ndir = CType(e.Command, DirectionCommand).Direction
            If ndir <> -dir AndAlso directed Then
                dir = ndir
                directed = False
            End If
        End If
    End Sub
    Private Sub Turn_Executed2(sender As Object, e As ExecutedRoutedEventArgs)
        If Timer.IsEnabled Then
            Dim ndir = CType(e.Command, DirectionCommand).Direction
            If mode <> GameMode.One Then
                If ndir <> -dir2 AndAlso directed2 Then
                    dir2 = ndir
                    directed2 = False
                End If
            Else
                If ndir <> -dir AndAlso directed Then
                    dir = ndir
                    directed = False
                End If
            End If
        End If
    End Sub
    Private Sub Pause_Executed(sender As Object, e As ExecutedRoutedEventArgs)
        If dir <> Direction.None Then
            If Timer.IsEnabled = True Then
                Timer.Stop()
                Me.Title = String.Format(If(mode = GameMode.One, CAPTION_WITH_PAUSE, CAPTION_TWO_PAUSE), GetScore(snake), GetScore(snake2))
            Else
                Timer.Start()
                Me.Title = GetTitle()
            End If
        End If
    End Sub
    Private Sub TimerEnded_Tick(sender As Object, e As EventArgs) Handles TimerEnded.Tick
        Static shineNumber As Integer
        shineNumber += 1
        If shineNumber >= SHINE * 2 Then
            TimerEnded.Stop()
            shineNumber = 0
        End If
        Dim dc = visual.RenderOpen()
        If mode <> GameMode.One Then
            If dir = Direction.None Then
                If dir2 = Direction.None Then
                    If shineNumber Mod 2 = 0 Then
                        DrawSnakeWithHeadLast(dc, snake, front, Body, Head)
                        DrawSnakeWithHeadLast(dc, snake2, front2, Body2, Head2)
                    End If
                Else
                    If shineNumber Mod 2 = 0 Then
                        DrawSnakeWithHeadLast(dc, snake, front, Body, Head)
                    End If
                    DrawSnakeWithHeadLast(dc, snake2, front2, Body2, Head2)
                End If
            Else
                If shineNumber Mod 2 = 0 Then
                    DrawSnakeWithHeadLast(dc, snake2, front2, Body2, Head2)
                End If
                DrawSnakeWithHeadLast(dc, snake, front, Body, Head)
            End If
            DrawFood(dc, food, FoodBrush)
            If mode = GameMode.Two Then
                DrawFood(dc, food2, FoodBrush2)
            End If
        Else
            If shineNumber Mod 2 = 0 Then
                DrawSnakeWithHeadLast(dc, snake, front, Body, Head)
            End If
            DrawFood(dc, food, FoodBrush)
        End If
        dc.Close()
    End Sub
#End Region
End Class
#Region "Direction"
''' <summary>
''' 方向
''' </summary>
Enum Direction
    ''' <summary>
    ''' 停止或未定义
    ''' </summary>
    None = 0
    ''' <summary>
    ''' 向左
    ''' </summary>
    Left = 1
    ''' <summary>
    ''' 向上
    ''' </summary>
    Top = 2
    ''' <summary>
    ''' 向右
    ''' </summary>
    Right = -1
    ''' <summary>
    ''' 向下
    ''' </summary>
    Bottom = -2
End Enum
''' <summary>
''' 包含方向描述的命令
''' </summary>
Class DirectionCommand
    Inherits RoutedCommand
    ''' <summary>
    ''' 方向
    ''' </summary>
    ''' <returns></returns>
    Public Property Direction As Direction
End Class
#End Region
