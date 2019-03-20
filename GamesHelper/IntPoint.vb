Imports System.Windows

<Serializable>
Public Structure IntPoint
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

    Public Shared Operator +(p1 As IntPoint, p2 As IntPoint) As IntPoint
        Return New IntPoint(p1.X + p2.X, p1.Y + p2.Y)
    End Operator

    Public Shared Operator -(p1 As IntPoint, p2 As IntPoint) As IntPoint
        Return New IntPoint(p1.X - p2.X, p1.Y - p2.Y)
    End Operator

    Public Shared Operator *(p As IntPoint, time As Integer) As IntPoint
        Return New IntPoint(p.X * time, p.Y * time)
    End Operator

    Public Function ToPoint(times As Double) As Point
        Return New Point(X * times, Y * times)
    End Function

    Public Function ToSize(times As Double) As Size
        Return New Size(X * times, Y * times)
    End Function
End Structure
