Imports System.Windows.Threading
Class MainWindow
#Region "Constants"
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
#End Region
#Region "Varients"
    Private times As Double
    Private realWidth As Double
    Private realHeight As Double
    Private cell As Vector
    Private WithEvents Timer As New DispatcherTimer()
    Private WithEvents TimerEnded As New DispatcherTimer()
    Private visual As New DrawingVisual
    Private mode As GameMode
    Private ReadOnly map As New Map()
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
    End Sub
    ''' <summary>
    ''' 初始化游戏
    ''' </summary>
    Private Sub InitGame()
        mode = GameMode.One
        'Me.Title = GetTitle()
        map.Reset(False, New IntPoint(13, 16))
    End Sub
    ''' <summary>
    ''' 初始化双人游戏
    ''' </summary>
    Private Sub InitGame2()
        mode = GameMode.Two
        map.Reset(False, New IntPoint(13, 12), New IntPoint(13, 20))
    End Sub
    Private Sub InitGame3()
        mode = GameMode.TwoCompete
        map.Reset(True, New IntPoint(13, 12), New IntPoint(13, 20))
    End Sub
#End Region
#Region "Game"
    ''' <summary>
    ''' 获取当前应该显示的标题
    ''' </summary>
    ''' <returns>当前应该显示的标题</returns>
    Private Function GetTitle() As String
        Dim s As Integer = map.Snake(0).Score
        If s = 0 Then
            Return CAPTION
        Else
            Return String.Format(CPTION_WITH_SCORE, s)
        End If
    End Function

    ''' <summary>
    ''' 获取双人模式应该显示的标题
    ''' </summary>
    ''' <returns>双人模式应该显示的标题</returns>
    Private Function GetTitle2() As String
        Return String.Format(CAPTION_TWO_SCORE, map.Snake(0).Score, map.Snake(1).Score)
    End Function
    ''' <summary>
    ''' 画蛇（头在前，为了使运动看起来更连贯）
    ''' </summary>
    ''' <param name="dc"></param>
    ''' <param name="snake"></param>
    ''' <param name="body"></param>
    ''' <param name="head"></param>
    Private Sub DrawSnake(dc As DrawingContext, snake As Snake, body As Brush, head As Brush)
        dc.DrawRectangle(head, NoBorderPen, New Rect(snake.Head.ToPoint(times), cell))
        For Each s In snake.Skip(1)
            dc.DrawRectangle(body, NoBorderPen, New Rect(s.ToPoint(times), cell))
        Next
    End Sub
    ''' <summary>
    ''' 画蛇（头在后，为了使头不消失）
    ''' </summary>
    ''' <param name="dc"></param>
    ''' <param name="snake"></param>
    ''' <param name="body"></param>
    ''' <param name="head"></param>
    Private Sub DrawSnakeWithHeadLast(dc As DrawingContext, snake As Snake, body As Brush, head As Brush)
        For Each s In snake
            dc.DrawRectangle(body, NoBorderPen, New Rect(s.ToPoint(times), cell))
        Next
        dc.DrawRectangle(head, NoBorderPen, New Rect(snake.Head.ToPoint(times), cell))
    End Sub
    ''' <summary>
    ''' 画食物
    ''' </summary>
    ''' <param name="dc"></param>
    ''' <param name="food"></param>
    ''' <param name="brush"></param>
    Private Sub DrawFood(dc As DrawingContext, food As IntPoint, brush As Brush)
        dc.DrawEllipse(brush, NoBorderPen, food.ToPoint(times) + cell / 2, times / 2, times / 2)
    End Sub
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
    Private Sub Timer_Tick(sender As Object, e As EventArgs) Handles Timer.Tick
        Try
            map.Move()
        Catch ex As SnakeEatenException
            Timer.Stop()
            TimerEnded.Start()
        End Try
        Using dc = visual.RenderOpen()
            DrawSnake(dc, map.Snake(0), Body, Head)
            If mode <> GameMode.One Then
                DrawSnake(dc, map.Snake(1), Body2, Head2)
            End If
            DrawFood(dc, map.Food(0), FoodBrush)
            If mode = GameMode.Two Then
                DrawFood(dc, map.Food(1), FoodBrush2)
            End If
        End Using
        If mode = GameMode.One Then
            Me.Title = GetTitle()
        Else
            Me.Title = GetTitle2()
        End If
    End Sub

    Private Sub Turn_Executed(sender As Object, e As ExecutedRoutedEventArgs)
        If Timer.IsEnabled Then
            Dim ndir = CType(e.Command, DirectionCommand).Direction
            map.Snake(0).Turn(ndir)
        End If
    End Sub
    Private Sub Turn_Executed2(sender As Object, e As ExecutedRoutedEventArgs)
        If Timer.IsEnabled Then
            Dim ndir = CType(e.Command, DirectionCommand).Direction
            If mode <> GameMode.One Then
                map.Snake(1).Turn(ndir)
            Else
                map.Snake(0).Turn(ndir)
            End If
        End If
    End Sub
    Private Sub Pause_Executed(sender As Object, e As ExecutedRoutedEventArgs)
        If dir <> Direction.None Then
            If Timer.IsEnabled = True Then
                Timer.Stop()
                Me.Title = String.Format(If(mode = GameMode.One, CAPTION_WITH_PAUSE, CAPTION_TWO_PAUSE), map.Snake(0).Score, map.Snake(1).Score)
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
        Using dc = visual.RenderOpen()
            If mode <> GameMode.One Then
                If map.Snake(0).Dir = Direction.None Then
                    If map.Snake(1).Dir = Direction.None Then
                        If shineNumber Mod 2 = 0 Then
                            DrawSnakeWithHeadLast(dc, map.Snake(0), Body, Head)
                            DrawSnakeWithHeadLast(dc, map.Snake(1), Body2, Head2)
                        End If
                    Else
                        If shineNumber Mod 2 = 0 Then
                            DrawSnakeWithHeadLast(dc, map.Snake(0), Body, Head)
                        End If
                        DrawSnakeWithHeadLast(dc, map.Snake(1), Body2, Head2)
                    End If
                Else
                    If shineNumber Mod 2 = 0 Then
                        DrawSnakeWithHeadLast(dc, map.Snake(1), Body2, Head2)
                    End If
                    DrawSnakeWithHeadLast(dc, map.Snake(0), Body, Head)
                End If
                DrawFood(dc, map.Food(0), FoodBrush)
                If mode = GameMode.Two Then
                    DrawFood(dc, map.Food(1), FoodBrush2)
                End If
            Else
                If shineNumber Mod 2 = 0 Then
                    DrawSnakeWithHeadLast(dc, map.Snake(0), Body, Head)
                End If
                DrawFood(dc, map.Food(0), FoodBrush)
            End If
        End Using
    End Sub

    Private Sub MainWindow_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        InitGame()
        Timer.Start()
    End Sub

    Private Sub MainWindow_SizeChanged(sender As Object, e As SizeChangedEventArgs) Handles Me.SizeChanged
        times = Math.Min(RenderSize.Width / Map.WIDTH, RenderSize.Height / Map.HEIGHT)
        realWidth = (Map.WIDTH - 1) * times
        realHeight = (Map.HEIGHT - 1) * times
        cell = New Vector(times, times)
    End Sub
#End Region
End Class
#Region "GameControl"
''' <summary>
''' 游戏模式
''' </summary>
Enum GameMode
    ''' <summary>
    ''' 单人游戏
    ''' </summary>
    One
    ''' <summary>
    ''' 双人游戏
    ''' </summary>
    Two
    ''' <summary>
    ''' 双人竞争游戏
    ''' </summary>
    TwoCompete
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
