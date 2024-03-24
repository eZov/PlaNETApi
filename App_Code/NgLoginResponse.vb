Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Net.Http
Imports System.Web

Public Class NgLoginResponse
    Public Sub New()
        Me.Token = ""
        Me.responseMsg = New HttpResponseMessage() With {
                .StatusCode = System.Net.HttpStatusCode.Unauthorized
            }
    End Sub

    Public Property Token As String
    Public Property responseMsg As HttpResponseMessage
End Class


