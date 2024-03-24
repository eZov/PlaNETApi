Imports Microsoft.VisualBasic

Public Class ApiRole
    Public Property api_id As Integer
    Public Property api_controller As String
    Public Property api_method As String
    Public Property api_roles_allowed As String
End Class

Public Class ApiRoles
    Public Property api_roles As ApiRole()
End Class
