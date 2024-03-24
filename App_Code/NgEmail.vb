Imports Microsoft.VisualBasic

Public Class NgEmailSmtp
    Property id As Integer
    Property host As String
    Property port As Integer
    Property ssl As Boolean
    Property username As String
    Property password As String
End Class

Public Class NgEmailsSmtpConfigs
    Property emailSmtpConfigs As NgEmailSmtp()
End Class

Public Class ngEmail
    Property employeeID As Integer
    Property membershipEmail As String
    Property email As String
    Property name As String
    Property attachPath As String
End Class

Public Class NgEmaillAddress

    Public Property email As String
    Public Property name As String

    Public Sub New(ByVal pEmail As String, ByVal pName As String)
        Me.email = pEmail
        Me.name = pName
    End Sub
End Class

Public Class NgEmailTemplate

    Public Property id As String
    Public Property template_name As String

    Public Sub New(ByVal pId As String, ByVal pTemplateName As String)
        id = pId
        template_name = pTemplateName
    End Sub
End Class