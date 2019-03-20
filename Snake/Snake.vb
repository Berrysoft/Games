Class Snake
    Implements IEnumerable(Of IntPoint)

    Private ReadOnly body As New LinkedList(Of IntPoint)

    Public ReadOnly Property Head As IntPoint
        Get
            Return body.First.Value
        End Get
    End Property

    Public Property Dir As Direction

    Private directed As Boolean

    Private ReadOnly map As Map

    Public Property Score As Integer

    Public Sub New(map As Map, head As IntPoint)
        Me.map = map
        body.AddLast(head)
        head.X -= 1
        body.AddLast(head)
        head.X -= 1
        body.AddLast(head)
        Dir = Direction.Right
    End Sub

    Public Sub HaveFood(Optional num As Integer = 1)
        For i = 0 To num - 1
            body.AddFirst(body.First.Value)
        Next
        Score += num
    End Sub

    Public Function Eaten(p As IntPoint) As Integer
        Dim result As Integer = 0
        Dim node = body.Find(p)
        Do While node IsNot Nothing
            Dim t = node.Next
            body.Remove(node)
            node = t
            result += 1
        Loop
        Return result
    End Function

    Public Sub Move()
        body.AddFirst(body.First.Value.AddDirection(Dir))
        body.RemoveLast()
        directed = False
    End Sub

    Public Sub Turn(dir As Direction)
        If Not directed AndAlso dir <> -Me.Dir Then
            Me.Dir = dir
            directed = True
        End If
    End Sub

    Public Function Contains(value As IntPoint) As Boolean
        If value = Head Then
            Dim i As Integer = 0
            For Each b In body
                If value = b Then
                    i += 1
                Else
                    Exit For
                End If
            Next
            If i >= body.Count Then
                Return False
            End If
            Return body.Skip(i).Contains(value)
        End If
        Return body.Contains(value)
    End Function

    Public Function GetEnumerator() As IEnumerator(Of IntPoint) Implements IEnumerable(Of IntPoint).GetEnumerator
        Return body.GetEnumerator()
    End Function

    Private Function IEnumerable_GetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
        Return CType(body, IEnumerable).GetEnumerator()
    End Function
End Class

Structure IntPoint
    Public X As Integer
    Public Y As Integer

    Public Sub New(x As Integer, y As Integer)
        Me.X = x
        Me.Y = y
    End Sub

    Public Overrides Function Equals(obj As Object) As Boolean
        If TypeOf obj Is IntPoint Then
            Return Me = CType(obj, IntPoint)
        End If
        Return False
    End Function

    Public Shared Operator =(p1 As IntPoint, p2 As IntPoint) As Boolean
        Return p1.X = p2.X AndAlso p1.Y = p2.Y
    End Operator

    Public Shared Operator <>(p1 As IntPoint, p2 As IntPoint) As Boolean
        Return Not p1 = p2
    End Operator

    Public Function ToPoint(times As Double) As Point
        Return New Point(X * times, Y * times)
    End Function

    Public Function AddDirection(dir As Direction) As IntPoint
        Dim result As IntPoint = Me
        Select Case dir
            Case Direction.Top
                result.Y -= 1
            Case Direction.Bottom
                result.Y += 1
            Case Direction.Left
                result.X -= 1
            Case Direction.Right
                result.X += 1
        End Select
        If result.Y < 0 Then
            result.Y = Map.WIDTH - 1
        ElseIf result.Y >= Map.WIDTH Then
            result.Y = 0
        End If
        If result.X < 0 Then
            result.X = Map.HEIGHT - 1
        ElseIf result.X >= Map.HEIGHT Then
            result.X = 0
        End If
        Return result
    End Function
End Structure

''' <summary>
''' 方向
''' </summary>
Enum Direction
    ''' <summary>
    ''' 未定义
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
