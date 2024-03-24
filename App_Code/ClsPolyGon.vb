Imports Microsoft.VisualBasic
Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Text

Namespace RouteGeoFence
    Class Point
        Public X As Double
        Public Y As Double
    End Class

    Class ClsPolyGon
        Public myPts As List(Of Point) = New List(Of Point)()

        Public Sub New()
        End Sub

        Public Sub New(ByVal points As List(Of Point))
            For Each p As Point In points
                Me.myPts.Add(p)
            Next
        End Sub

        Public Sub Add(ByVal p As Point)
            Me.myPts.Add(p)
        End Sub

        Public Function Count() As Integer
            Return myPts.Count
        End Function

        Public Function FindPoint(ByVal X As Double, ByVal Y As Double) As Boolean
            Dim sides As Integer = Me.Count() - 1
            Dim j As Integer = sides - 1
            Dim pointStatus As Boolean = False

            For i As Integer = 0 To sides - 1

                If myPts(i).Y < Y AndAlso myPts(j).Y >= Y OrElse myPts(j).Y < Y AndAlso myPts(i).Y >= Y Then

                    If myPts(i).X + (Y - myPts(i).Y) / (myPts(j).Y - myPts(i).Y) * (myPts(j).X - myPts(i).X) < X Then
                        pointStatus = Not pointStatus
                        'Return pointStatus
                    End If
                End If

                j = i
            Next

            Return pointStatus
        End Function
    End Class
End Namespace
