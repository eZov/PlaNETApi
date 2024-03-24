Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Net.Http
Imports System.Web
Imports System.Web.Security.Membership
Imports System.Runtime.InteropServices


Public Class NgApiLoginPassword

    Protected Function ChangePassword(ByVal user As String, ByVal oldpassword As String, ByVal newpassword As String) As Boolean

        Return Membership.Provider.ChangePassword(user, oldpassword, newpassword)

    End Function




    Protected Function TryCreatePrincipal(ByVal user As String, ByVal password As String) As Boolean

        If Not Membership.Provider.ValidateUser(user, password) Then Return False
        Dim roles As String() = System.Web.Security.Roles.Provider.GetRolesForUser(user)

        Return True
    End Function



End Class
