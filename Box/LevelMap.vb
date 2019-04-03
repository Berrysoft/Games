Imports System.Runtime.CompilerServices
Imports GamesHelper

Enum SquareState
    Road = &H0
    Wall = &H1
    Box = &H2
    EndPoint = &H4
End Enum

Public Enum Direction
    None = 0
    Left = 1
    Top = 2
    Right = -1
    Bottom = -2
End Enum

Structure [Step]
    Public Dir As Direction
    Public MoveBox As Boolean
End Structure

Module ArrayHelper
    <Extension>
    Public Iterator Function AsEnumerable(Of T)(array As T(,)) As IEnumerable(Of T)
        For i = 0 To array.GetLength(0) - 1
            For j = 0 To array.GetLength(1) - 1
                Yield array(i, j)
            Next
        Next
    End Function
End Module

Class LevelMap
    Implements ICloneable

    Private steps As Stack(Of [Step])

    Public ReadOnly Property Body As IntPoint
    Public ReadOnly Property Map As SquareState(,)

    Public Sub New()
        Me.New(New IntPoint(), {})
    End Sub

    Public Sub New(body As IntPoint, map As SquareState(,))
        Me.Body = body
        Me.Map = map
    End Sub

    Public Function Clone() As Object Implements ICloneable.Clone
        Dim newlevel As LevelMap = MemberwiseClone()
        newlevel._Map = _Map.Clone()
        newlevel.steps = New Stack(Of [Step])()
        Return newlevel
    End Function

    Public Sub GoDir(dir As Direction)
        Dim r = GoDirIntrnal(dir)
        If r.HasValue Then
            steps.Push(New [Step] With {.Dir = dir, .MoveBox = r.Value})
        End If
    End Sub

    Private Function GoDirIntrnal(dir As Direction) As Boolean?
        Dim maxx = Map.GetLength(0)
        Dim maxy = Map.GetLength(1)
        Dim newb = GetNewPoint(Body, dir)
        If Not IsValid(newb, maxx, maxy) Then
            Return Nothing
        End If
        Dim newstate = Map(newb.X, newb.Y)
        If newstate = SquareState.Wall Then
            Return Nothing
        End If
        Dim result As Boolean = False
        If newstate And SquareState.Box Then
            result = True
            Dim bnewb = GetNewPoint(newb, dir)
            If Not IsValid(bnewb, maxx, maxy) Then
                Return Nothing
            End If
            Dim bnewstate = Map(bnewb.X, bnewb.Y)
            If bnewstate = SquareState.Wall OrElse ((bnewstate And SquareState.Box) <> 0) Then
                Return Nothing
            End If
            Map(newb.X, newb.Y) = newstate And (Not SquareState.Box)
            Map(bnewb.X, bnewb.Y) = bnewstate Or SquareState.Box
        End If
        _Body = newb
        Return result
    End Function

    Private Function GetNewPoint(p As IntPoint, dir As Direction) As IntPoint
        Select Case dir
            Case Direction.Left
                p.X -= 1
            Case Direction.Right
                p.X += 1
            Case Direction.Top
                p.Y -= 1
            Case Direction.Bottom
                p.Y += 1
        End Select
        Return p
    End Function

    Private Function IsValid(p As IntPoint, maxx As Integer, maxy As Integer) As Boolean
        Return Not (p.X < 0 OrElse p.X >= maxx OrElse p.Y < 0 OrElse p.Y >= maxy)
    End Function

    Public Sub Undo()
        If steps.Count > 0 Then
            Dim st As [Step] = steps.Pop()
            Dim oldb = Body
            GoDirIntrnal(-st.Dir)
            If st.MoveBox Then
                Dim boldb = GetNewPoint(oldb, st.Dir)
                Map(boldb.X, boldb.Y) = Map(boldb.X, boldb.Y) And (Not SquareState.Box)
                Map(oldb.X, oldb.Y) = Map(oldb.X, oldb.Y) Or SquareState.Box
            End If
        End If
    End Sub

    Public ReadOnly Property CanUndo As Boolean
        Get
            Return steps.Count > 0
        End Get
    End Property
End Class
