Imports Microsoft.VisualBasic

Public Class NgUserOrgRoles
    Public Property Email As String
    Public Property EmailDef As String
    Public Property EmployeeID As Integer
    Public Property FirstName As String
    Public Property LastName As String
    Public Property OrgJedNaziv As String
    Public Property OrgJedNaziv2 As String
    Public Property OrgJedSifra As String
    Public Property OrgID As String         'Name kolona sys_usr_role_json
    Public Property PGID As String          'NamePG kolona sys_usr_role_json
    Public Property Enabled As Boolean      'definiše enable/disable
    Public Property Form As String          'definiše na koga se odnose org.role, za putne naloge je: APIputnal
End Class
