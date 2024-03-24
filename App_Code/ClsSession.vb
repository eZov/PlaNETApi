Imports Microsoft.VisualBasic
Imports System.Web
Imports System.Data
Imports System.Web.Security
Imports MySql.Data
Imports System.Web.Configuration
Imports System.IO
Imports System.Xml

<Serializable()> Public Class ClsSessionSink
    'Private objSession As System.Web.HttpContext

    Private myInit As Boolean
    Private myEmployeeID As Integer         ' id usera koji je prijavljen
    Private myCurrentEmployeeId As Integer  ' id uposlenika koga je izabrao user
    Private myCurrentOrgSfr As String
    Private myCurrentOrgId As Integer

    Private myEmployeeName As String
    Private myCurrentZahtjev As Object
    Private myUserName As String
    Private myVrstaPlacanja As Object
    Private myWritableZahtjev As Object
    Private myStatusNaloga As Object
    Private myIDNaloga As Integer
    Private myGOUnos As Object
    Private myIDNalogaRpt As Integer
    Private myIDNalogaLikvidatura As Integer

    Private myCurrentUser As String
    Private myUserRole As String
    Private myRoles As New DataTable
    Private myRolesDDL As New DataTable

    Private myRedirect As New Boolean


    Public Sub New()
        myInit = True

        myEmployeeID = -1
        myCurrentEmployeeId = -1

        myCurrentOrgId = -1

    End Sub

    Public Property EmployeeID() As Integer
        Get
            Return myEmployeeID
        End Get
        Set(ByVal value As Integer)
            myEmployeeID = value
        End Set
    End Property
    Public Property CurrentEmployee() As Integer
        Get
            Return myCurrentEmployeeId
        End Get
        Set(ByVal value As Integer)
            myCurrentEmployeeId = value
        End Set
    End Property
    Public Property CurrentOrgSfr() As String
        Get
            Return myCurrentOrgSfr
        End Get
        Set(ByVal value As String)
            myCurrentOrgSfr = value
        End Set
    End Property
    Public Property CurrentOrgId() As Integer
        Get
            Return myCurrentOrgId
        End Get
        Set(ByVal value As Integer)
            myCurrentOrgId = value
        End Set
    End Property
    Public Property EmployeeName() As String
        Get
            Return myEmployeeName
        End Get
        Set(ByVal value As String)
            myEmployeeName = value
        End Set
    End Property
    Public Property CurrentZahtjev() As Object
        Get
            Return myCurrentZahtjev
        End Get
        Set(ByVal value As Object)
            myCurrentZahtjev = value
        End Set
    End Property
    Public Property UserName() As String
        Get
            Return myUserName
        End Get
        Set(ByVal value As String)
            myUserName = value
        End Set
    End Property
    Public Property VrstaPlacanja() As Object
        Get
            Return myVrstaPlacanja
        End Get
        Set(ByVal value As Object)
            myVrstaPlacanja = value
        End Set
    End Property
    Public Property WritableZahtjev() As Object
        Get
            Return myWritableZahtjev
        End Get
        Set(ByVal value As Object)
            myWritableZahtjev = value
        End Set
    End Property
    Public Property StatusNaloga() As Object
        Get
            Return myStatusNaloga
        End Get
        Set(ByVal value As Object)
            myStatusNaloga = value
        End Set
    End Property
    Public Property IDNaloga() As Integer
        Get
            Return myIDNaloga
        End Get
        Set(ByVal value As Integer)
            myIDNaloga = value
        End Set
    End Property
    Public Property GOUnos() As Object
        Get
            Return myGOUnos
        End Get
        Set(ByVal value As Object)
            myGOUnos = value
        End Set
    End Property

    Public Property IDNalogaRpt() As Integer
        Get
            Return myIDNalogaRpt
        End Get
        Set(ByVal value As Integer)
            myIDNalogaRpt = value
        End Set
    End Property

    Public Property IDNalogaLikvidatura() As Integer
        Get
            Return myIDNalogaLikvidatura
        End Get
        Set(ByVal value As Integer)
            myIDNalogaLikvidatura = value
        End Set
    End Property

    Public Property CurrentUser() As String
        Get
            Return myCurrentUser
        End Get
        Set(ByVal value As String)
            myCurrentUser = value
        End Set
    End Property
    Public Property UserRole() As String
        Get
            Return myUserRole
        End Get
        Set(ByVal value As String)
            myUserRole = value
        End Set
    End Property
    Public ReadOnly Property AllRoles() As DataTable
        Get
            Return myRoles
        End Get
    End Property

    Public ReadOnly Property RolesDDL() As DataTable
        Get
            Return myRolesDDL
        End Get
    End Property

    Public Property Redirect() As Boolean
        Get
            Return (myRedirect)
        End Get
        Set(ByVal value As Boolean)
            myRedirect = value
        End Set
    End Property

    Public Sub PullAllRoles()

        myRoles.Columns.Add(New DataColumn("id"))
        myRoles.Columns.Add(New DataColumn("name"))

        'Dim myUserRoles() As String = Roles.GetRolesForUser()

        'For i As Integer = 0 To myUserRoles.Length - 1
        '    Dim newRoleRow As DataRow = myRoles.NewRow()
        '    newRoleRow("id") = i
        '    newRoleRow("name") = myUserRoles(i)
        '    myRoles.Rows.Add(newRoleRow)
        'Next

        GetRolesDDL()

    End Sub

    Public Sub GetRolesDDL()

        Dim ConnectionString As String = WebConfigurationManager.ConnectionStrings("MySQLConnection").ConnectionString
        Dim strSQL As String


        strSQL = <![CDATA[ 
         SELECT r.`name` FROM `my_aspnet_usersinroles` ur INNER JOIN `my_aspnet_roles` r ON ur.`roleId`=r.`id`
INNER JOIN `my_aspnet_menusbyrole` m ON r.`name`=m.`role`
WHERE ur.`userId`=(SELECT u.`id` FROM `my_aspnet_users` u
WHERE u.`applicationId`=1 AND u.`name`=@username);
    ]]>.Value


        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            Dim dbRead As MySqlClient.MySqlDataReader
            Dim mycmd As New MySql.Data.MySqlClient.MySqlCommand

            mycmd.Connection = myconnection

            mycmd.CommandText = strSQL

            mycmd.Parameters.AddWithValue("@username", myUserName)
            mycmd.Prepare()

            myRoles.Rows.Clear()
            Dim idRow As Integer = 0

            dbRead = mycmd.ExecuteReader
            Do While dbRead.Read
                Dim newRoleRow As DataRow = myRoles.NewRow()
                newRoleRow("id") = idRow
                newRoleRow("name") = dbRead.GetString("name")
                myRoles.Rows.Add(newRoleRow)
                idRow += 1
            Loop
            dbRead.Close()

        End Using


    End Sub

    'Public Sub GetMenu(ByVal pRole As String, ByRef pMenu As Syncfusion.JavaScript.Web.Menu)

    '    Dim ConnectionString As String = WebConfigurationManager.ConnectionStrings("MySQLConnection").ConnectionString
    '    Dim strSQL As String

    '    Dim _mnuItemsJson As String = ""
    '    Dim _mnuList As New List(Of ClsMenu)
    '    Dim _mnuListFilter As New List(Of ClsMenu)

    '    pMenu.Items.Clear()

    '    strSQL = <![CDATA[ 
    '        SELECT m.* FROM my_aspnet_menusbyrole m WHERE m.role=@rolename ;
    ']]>.Value


    '    Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
    '        myconnection.Open()

    '        Dim dbRead As MySqlClient.MySqlDataReader
    '        Dim mycmd As New MySql.Data.MySqlClient.MySqlCommand

    '        mycmd.Connection = myconnection

    '        mycmd.CommandText = strSQL
    '        mycmd.Prepare()
    '        ''
    '        ' GO tekuće godine
    '        ''
    '        mycmd.Parameters.AddWithValue("@rolename", pRole)

    '        dbRead = mycmd.ExecuteReader
    '        Do While dbRead.Read
    '            _mnuItemsJson = dbRead.GetString("json")
    '        Loop
    '        dbRead.Close()

    '    End Using

    '    pMenu.Items.Clear()

    '    _mnuList = ClsJSON.getItemList(_mnuItemsJson)

    '    Dim ClsPlaNET As New ClsPlaNET
    '    Dim idxMnuLst As New List(Of Integer)


    '    For Each el As ClsMenu In _mnuList
    '        'If Not ((ClsPlaNET.XmlReport And el.Link = "plcxmlizvjestaji") OrElse (ClsPlaNET.GodOdm And el.Link = "hrgododm")) Then
    '        '    _mnuListFilter.Add(el)
    '        'End If
    '        If el.Link = "plcxmlizvjestaji" And ClsPlaNET.XmlReport = False Then

    '        ElseIf el.Link = "hrgododm" And ClsPlaNET.GodOdm = False Then

    '        ElseIf el.Link = "evdzbirnalistaobustave" And ClsPlaNET.Obustave = False Then

    '        Else
    '            _mnuListFilter.Add(el)
    '        End If
    '    Next

    '    pMenu.DataSource = _mnuListFilter
    '    pMenu.DataIdField = "Id"
    '    pMenu.DataTextField = "Text"
    '    pMenu.DataParentIdField = "ParentId"
    '    pMenu.DataUrlField = "Link"
    'End Sub
End Class

