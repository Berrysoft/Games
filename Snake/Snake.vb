Imports System.Runtime.CompilerServices
Imports GamesHelper

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

Module IntPointHelper
    <Extension>
    Public Function AddDirection(p As IntPoint, dir As Direction) As IntPoint
        Dim result As IntPoint = p
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
End Module

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
