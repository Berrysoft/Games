Class SnakeEatenException
    Inherits Exception
End Class

Class Map
    Public Const WIDTH As Integer = 40
    Public Const HEIGHT As Integer = 40

    Private ReadOnly snakes As New List(Of Snake)
    Private ReadOnly foods As New List(Of IntPoint)
    Private ReadOnly rnd As New Random
    Private shareFood As Boolean

    Public ReadOnly Property Snake(index As Integer) As Snake
        Get
            Return snakes(index)
        End Get
    End Property

    Public ReadOnly Property Food(index As Integer) As IntPoint
        Get
            Return foods(index)
        End Get
    End Property

    Public Sub Move()
        Dim count As Integer = snakes.Count
        For i = 0 To count - 1
            snakes(i).Move()
        Next
        For i = 0 To count - 1
            For j = i + 1 To count - 1
                If snakes(i).Head = snakes(j).Head Then
                    Throw New SnakeEatenException()
                End If
            Next
        Next
        For i = 0 To count - 1
            For j = 0 To count - 1
                If snakes(j).Contains(snakes(i).Head) Then
                    If i <> j Then
                        snakes(i).HaveFood(snakes(j).Eaten(snakes(i).Head))
                    Else
                        Throw New SnakeEatenException()
                    End If
                End If
            Next
            Dim foodp = foods(If(shareFood, 0, i))
            If snakes(i).Head = foodp Then
                snakes(i).HaveFood()
                foods(If(shareFood, 0, i)) = NewFood(i)
            End If
        Next
    End Sub

    Private Function NewFood(index As Integer) As IntPoint
        Dim f As IntPoint
        If shareFood Then
            Do
                f.X = rnd.Next(WIDTH)
                f.Y = rnd.Next(HEIGHT)
            Loop Until snakes.TrueForAll(Function(s) Not s.Contains(f))
        Else
            Do
                f.X = rnd.Next(WIDTH)
                f.Y = rnd.Next(HEIGHT)
            Loop While snakes(index).Contains(f)
        End If
        Return f
    End Function

    Public Sub Reset(share As Boolean, ParamArray starts() As IntPoint)
        snakes.Clear()
        For Each p In starts
            snakes.Add(New Snake(Me, p))
        Next
        shareFood = share
        foods.Clear()
        If shareFood Then
            foods.Add(NewFood(0))
        Else
            For i = 0 To snakes.Count - 1
                foods.Add(NewFood(i))
            Next
        End If
    End Sub
End Class
