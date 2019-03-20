Imports GamesHelper

<Flags>
Public Enum WallState
    Both = &B0
    Bottom = &B1
    Right = &B10
End Enum

<Serializable>
Public Class Maze
    Private vertexes(,) As WallState
    Private Sub New(width As Integer, height As Integer)
        vertexes = New WallState(width - 1, height - 1) {}
    End Sub
    Public ReadOnly Property Width As Integer
        Get
            Return vertexes.GetLength(0)
        End Get
    End Property
    Public ReadOnly Property Height As Integer
        Get
            Return vertexes.GetLength(1)
        End Get
    End Property
    Default Public Property Item(x As Integer, y As Integer) As WallState
        Get
            Return vertexes(x, y)
        End Get
        Set(value As WallState)
            vertexes(x, y) = value
        End Set
    End Property

    Public Function ToGraph() As Berrysoft.Data.Graph(Of IntPoint)
        Dim w = Width
        Dim h = Height
        Dim result As New Berrysoft.Data.Graph(Of IntPoint)()
        For x = 0 To w - 1
            For y = 0 To h - 1
                Dim current As New IntPoint(x, y)
                result.Add(current)
                If vertexes(x, y).HasFlag(WallState.Bottom) Then
                    Dim np As New IntPoint(x + 1, y)
                    result.Add(np)
                    result.AddEdge(current, np)
                End If
                If vertexes(x, y).HasFlag(WallState.Right) Then
                    Dim np As New IntPoint(x, y + 1)
                    result.Add(np)
                    result.AddEdge(current, np)
                End If
            Next
        Next
        Return result
    End Function

    Private Shared ReadOnly rnd As New Random()
    Public Shared Function Create(width As Integer, height As Integer) As Maze
        Dim result As New Maze(width, height)
        Dim visited As New List(Of IntPoint)
        Dim surround As New List(Of IntPoint)(4)
        Dim current As New IntPoint(0, 0)
        Dim hori As New IntPoint(1, 0)
        Dim vert As New IntPoint(0, 1)
        visited.Add(current)
        Do Until visited.Count >= width * height
            surround.Clear()
            If current.X > 0 AndAlso Not visited.Contains(current - hori) Then
                surround.Add(current - hori)
            End If
            If current.Y > 0 AndAlso Not visited.Contains(current - vert) Then
                surround.Add(current - vert)
            End If
            If current.X < width - 1 AndAlso Not visited.Contains(current + hori) Then
                surround.Add(current + hori)
            End If
            If current.Y < height - 1 AndAlso Not visited.Contains(current + vert) Then
                surround.Add(current + vert)
            End If
            If surround.Count > 0 Then
                Dim index = rnd.Next(surround.Count)
                Dim cell = surround(index)
                Select Case cell.X - current.X
                    Case 0
                        Select Case cell.Y - current.Y
                            Case 1
                                result.vertexes(current.X, current.Y) = result.vertexes(current.X, current.Y) Or WallState.Right
                            Case -1
                                result.vertexes(cell.X, cell.Y) = result.vertexes(cell.X, cell.Y) Or WallState.Right
                        End Select
                    Case 1
                        result.vertexes(current.X, current.Y) = result.vertexes(current.X, current.Y) Or WallState.Bottom
                    Case -1
                        result.vertexes(cell.X, cell.Y) = result.vertexes(cell.X, cell.Y) Or WallState.Bottom
                End Select
                visited.Add(cell)
                current = cell
            Else
                Dim index = rnd.Next(visited.Count)
                current = visited(index)
            End If
        Loop
        Return result
    End Function
End Class
