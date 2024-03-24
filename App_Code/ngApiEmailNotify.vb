Imports System.Data
Imports System.Web.Configuration
Imports Microsoft.VisualBasic
Imports MySql.Data
Imports MySql.Data.MySqlClient
Imports Newtonsoft.Json

Public Class ngApiEmailNotify
    Public Property userSession As NgApiSession

    Public Property clientEmail As String
    Public Property clientEmpId As Integer
    Public Property clientEmailAttPath As String

    Public Property clientNotifyRoles As String
    Public Property clientNotifyEmails As New List(Of String)
    Public Property clientNotifySendEmails As New Dictionary(Of String, ngEmail)

    Public Property clientNotifyForbidden As New List(Of String)

    Public Property clientTemplateName As String
    Public Property clientTemplateSubject As String
    Public Property clientTemplateBody As String

    Sub New(ByRef pUserSession As NgApiSession)

        userSession = pUserSession

    End Sub


    Public Sub getNotifyEmails(ByVal pSfrOrgjed As String)

        getNotifyEmails(pSfrOrgjed, clientNotifyRoles)

    End Sub

    Public Sub getNotifyEmailsPG(ByVal pEmployeeId As Integer)

        ' Uzmi sve PG u kojima se nalazi pEmployeeId
        Dim _listPG As List(Of String) = getListPG(pEmployeeId)

        ' Za svaku PG identifikuj email 
        For Each _sfrPG In _listPG
            getNotifyEmailsPGSql(_sfrPG, clientNotifyRoles)
        Next

    End Sub

    Private Function getListPG(ByVal pEmployeeId As Integer) As List(Of String)

        Dim ConnectionString As String = WebConfigurationManager.ConnectionStrings("MySQLConnection").ConnectionString
        Dim strSQL As String
        Dim _listPG As New List(Of String)

        '
        ' Provjerava ima li red u tabeli
        strSQL = <![CDATA[
SELECT pg.`Sifra`
FROM `sfr_poslovnegrupe` pg
WHERE pg.`Opis` REGEXP '[[:<:]]%__EmployeeId__%[[:>:]]';
    ]]>.Value


        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            Dim dbRead As MySqlDataReader
            Dim mycmd As New MySqlCommand

            mycmd.Connection = myconnection
            strSQL = strSQL.Replace("%__EmployeeId__%", pEmployeeId.ToString())


            mycmd.CommandText = strSQL
            dbRead = mycmd.ExecuteReader()

            If dbRead.HasRows Then
                While dbRead.Read()
                    _listPG.Add(dbRead.GetString("Sifra"))
                End While
            End If

            dbRead.Close()
        End Using


        Return _listPG
    End Function

    Public Sub getNotifyEmails(ByVal pSfrOrgjed As String, ByVal pRoles As String)

        Dim _empSfrOrgJed = pSfrOrgjed

        Do While _empSfrOrgJed.Length >= 1
            getNotifyEmailsSql(_empSfrOrgJed, pRoles)
            If _empSfrOrgJed.Length > 2 Then
                _empSfrOrgJed = _empSfrOrgJed.Substring(0, _empSfrOrgJed.Length - 2)
            Else
                _empSfrOrgJed = ""
            End If
        Loop

        'clientNotifyEmails = clientNotifyEmails.Distinct().ToList

    End Sub

    Private Function getNotifyEmailsSql(ByVal pSfrOrgjed As String, ByVal pRoles As String) As List(Of String)

        Dim ConnectionString As String = WebConfigurationManager.ConnectionStrings("MySQLConnection").ConnectionString
        Dim strSQL As String

        '
        ' Provjerava ima li red u tabeli
        strSQL = <![CDATA[
SELECT org.`Type` AS email, u.`id`, r.roleId, rl.`name`
FROM `sys_usr_role_json` org
INNER JOIN `my_aspnet_users` u ON org.`Type` = CONVERT(u.name USING cp1250) AND u.`applicationId` = 1
INNER JOIN  `my_aspnet_usersinroles` r ON  u.id = r.userId 
INNER JOIN `my_aspnet_roles` rl ON rl.`id` = r.`roleId`
WHERE org.`Name` REGEXP '[[:<:]]%__SfrOrgJed__%[[:>:]]' 
AND org.`Form` = 'APIputnal'
AND rl.`name` IN (%__ListRoles__%);
    ]]>.Value


        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            Dim dbRead As MySqlDataReader
            Dim mycmd As New MySqlCommand

            mycmd.Connection = myconnection

            strSQL = strSQL.Replace("%__SfrOrgJed__%", pSfrOrgjed)
            strSQL = strSQL.Replace("%__ListRoles__%", pRoles)

            mycmd.CommandText = strSQL

            dbRead = mycmd.ExecuteReader()

            If dbRead.HasRows Then
                While dbRead.Read()
                    clientNotifyEmails.Add(dbRead.GetString("email"))
                End While
            End If

            dbRead.Close()
        End Using


        Return clientNotifyEmails
    End Function

    Private Function getNotifyEmailsPGSql(ByVal pSfrPG As String, ByVal pRoles As String) As List(Of String)

        Dim ConnectionString As String = WebConfigurationManager.ConnectionStrings("MySQLConnection").ConnectionString
        Dim strSQL As String

        '
        ' Provjerava ima li red u tabeli
        strSQL = <![CDATA[
SELECT org.`Type` AS email, u.`id`, r.roleId, rl.`name`
FROM `sys_usr_role_json` org
INNER JOIN `my_aspnet_users` u ON org.`Type` = CONVERT(u.name USING cp1250) AND u.`applicationId` = 1
INNER JOIN  `my_aspnet_usersinroles` r ON  u.id = r.userId 
INNER JOIN `my_aspnet_roles` rl ON rl.`id` = r.`roleId`
WHERE org.`NamePG` REGEXP '[[:<:]]%__SfrOrgJed__%[[:>:]]' 
AND org.`Form` = 'APIputnal'
AND rl.`name` IN (%__ListRoles__%);
    ]]>.Value


        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            Dim dbRead As MySqlDataReader
            Dim mycmd As New MySqlCommand

            mycmd.Connection = myconnection

            strSQL = strSQL.Replace("%__SfrOrgJed__%", pSfrPG)
            strSQL = strSQL.Replace("%__ListRoles__%", pRoles)

            mycmd.CommandText = strSQL

            dbRead = mycmd.ExecuteReader()

            If dbRead.HasRows Then
                While dbRead.Read()
                    clientNotifyEmails.Add(dbRead.GetString("email"))
                End While
            End If

            dbRead.Close()
        End Using


        Return clientNotifyEmails
    End Function

    Public Sub getForbiddenEmails()

        Dim ConnectionString As String = WebConfigurationManager.ConnectionStrings("MySQLConnection").ConnectionString
        Dim strSQL As String

        '
        ' Provjerava ima li red u tabeli
        strSQL = <![CDATA[
SELECT nf.`EmployeeID`, 
m.`Email` AS default_email
FROM `notify_exclude` nf
INNER JOIN `my_aspnet_users_to_employees` u2e ON nf.`EmployeeID` = u2e.`employees_id`
INNER JOIN `my_aspnet_membership` m ON u2e.`users_id` = m.`userId`
WHERE nf.`template_name` REGEXP '[[:<:]]%__regexpValue__%[[:>:]]';
    ]]>.Value


        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            Dim dbRead As MySqlDataReader
            Dim mycmd As New MySqlCommand

            mycmd.Connection = myconnection

            strSQL = strSQL.Replace("%__regexpValue__%", clientTemplateName)
            mycmd.CommandText = strSQL


            dbRead = mycmd.ExecuteReader()
            If dbRead.HasRows Then
                While dbRead.Read()
                    clientNotifyForbidden.Add(dbRead.GetString("default_email"))
                End While
            End If

            dbRead.Close()
        End Using

    End Sub

    Public Sub remForbiddenEmails()

        ' Get forbidden emails from db 
        '
        getForbiddenEmails()

        For Each el In clientNotifyForbidden

            If clientNotifyEmails.Contains(el) Then
                clientNotifyEmails.Remove(el)
            End If

        Next

    End Sub

    Public Sub getSendEmails()

        Dim _csvNotifyEmails As String = String.Join(",", clientNotifyEmails)
        _csvNotifyEmails = ngApiUtilities.addQouteToCsvWords(_csvNotifyEmails)

        'If _csvNotifyEmails.Length > 0 Then
        getSendEmailsSql(_csvNotifyEmails)
        'End If

    End Sub

    Private Function getSendEmailsSql(ByVal pEmails As String) As Dictionary(Of String, ngEmail)

        Dim ConnectionString As String = WebConfigurationManager.ConnectionStrings("MySQLConnection").ConnectionString
        Dim strSQL As String

        '
        ' Provjerava ima li red u tabeli
        strSQL = <![CDATA[
SELECT IF( ep.`emp_work_default_email` = 1, ep.`emp_work_email`, ep.`emp_oth_email`) AS default_email, m.`Email`,
CONCAT(e.`FirstName`, ' ',e.`LastName`) AS emailName
FROM `evd_employees_pim` ep
INNER JOIN `my_aspnet_users_to_employees` u2e ON ep.`EmployeeID` = u2e.`employees_id`
INNER JOIN `my_aspnet_membership` m ON u2e.`users_id` = m.`userId`
INNER JOIN `evd_employees` e ON ep.`EmployeeID` = e.`EmployeeID`
WHERE m.`Email` IN( %__ListEmails__% );
    ]]>.Value

        clientNotifySendEmails.Clear()

        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            Dim dbRead As MySqlDataReader
            Dim mycmd As New MySqlCommand

            mycmd.Connection = myconnection


            strSQL = strSQL.Replace("%__ListEmails__%", pEmails)

            mycmd.CommandText = strSQL

            dbRead = mycmd.ExecuteReader()

            If dbRead.HasRows Then
                While dbRead.Read()
                    clientNotifySendEmails.Add(dbRead.GetString("email"), New ngEmail With {.membershipEmail = dbRead.GetString("email"), .email = dbRead.GetString("default_email"), .name = dbRead.GetString("emailName")})
                End While
            End If

            dbRead.Close()
        End Using


        Return clientNotifySendEmails
    End Function

    Public Sub getNotifyRolesAndTemplate(ByVal pPnStatus As String, ByVal pnWorkflow As String)
        Dim ConnectionString As String = WebConfigurationManager.ConnectionStrings("MySQLConnection").ConnectionString
        Dim strSQL As String

        '
        ' Provjerava ima li red u tabeli
        strSQL = <![CDATA[
SELECT wf.`notify`, wf.`notify_template`
FROM `putninalog_workflow` wf
WHERE wf.`status_to` = @status_to
AND wf.workflow = @workflow;
    ]]>.Value


        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            Dim dbRead As MySqlDataReader
            Dim mycmd As New MySqlCommand

            mycmd.Connection = myconnection

            mycmd.CommandText = strSQL


            mycmd.Parameters.Clear()
            mycmd.Parameters.AddWithValue("@status_to", pPnStatus)
            mycmd.Parameters.AddWithValue("@workflow", pnWorkflow)
            mycmd.Prepare()

            dbRead = mycmd.ExecuteReader()
            If dbRead.HasRows Then
                While dbRead.Read()
                    clientNotifyRoles = dbRead.GetString("notify")
                    clientTemplateName = dbRead.GetString("notify_template")
                End While
            End If

            dbRead.Close()
        End Using

        If clientNotifyRoles IsNot Nothing AndAlso clientNotifyRoles.Contains("uposlenik") Then
            clientNotifyRoles = ngApiUtilities.remItemFromCsvWords(clientNotifyRoles, "uposlenik")
            clientNotifyEmails.Add(clientEmail)
        End If

        If clientNotifyRoles IsNot Nothing Then
            clientNotifyRoles = ngApiUtilities.addQouteToCsvWords(clientNotifyRoles)
        End If


    End Sub

    Public Sub getTemplate()
        Dim ConnectionString As String = WebConfigurationManager.ConnectionStrings("MySQLConnection").ConnectionString
        Dim strSQL As String

        '
        ' Provjerava ima li red u tabeli
        strSQL = <![CDATA[
SELECT nt.`subject`, nt.`body`
FROM `notify_templates` nt
WHERE nt.`template_name` = @template_name;
    ]]>.Value


        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            Dim dbRead As MySqlDataReader
            Dim mycmd As New MySqlCommand

            mycmd.Connection = myconnection

            mycmd.CommandText = strSQL


            mycmd.Parameters.Clear()
            mycmd.Parameters.AddWithValue("@template_name", clientTemplateName)
            mycmd.Prepare()

            dbRead = mycmd.ExecuteReader()
            If dbRead.HasRows Then
                While dbRead.Read()
                    clientTemplateSubject = dbRead.GetString("subject")
                    clientTemplateBody = dbRead.GetString("body")
                End While
            End If

            dbRead.Close()
        End Using

    End Sub

    Public Function prepForEmailByEmp(ByVal pPutNalId As Integer, ByVal pnStatus As String, ByVal pnVrsta As Integer, ByVal pEmpId As Integer) As Boolean

        Dim _ApiPutniNalog As New NgApiPnPutniNalog()
        Dim _PutNal As ngPnPutniNalog = _ApiPutniNalog.getPnPutniNalogById(pPutNalId, pEmpId)

        Dim pEmpData As NgUser = userSession.getEmployeeData(_PutNal.employee_id)

        If clientEmail Is Nothing Then
            Return False
        End If
        clientEmail = pEmpData.Email
        clientEmpId = pEmpData.EmployeeID

        Dim _workflows As String = String.Format("default-{0}", pnVrsta.ToString)
        getNotifyRolesAndTemplate(pnStatus, _workflows)

        If clientNotifyRoles Is Nothing OrElse clientNotifyRoles.Length = 0 Then
            Return False
        End If
        'If clientNotifyRoles Is Nothing Or clientNotifyRoles.Length = 0 Then
        '    Return False
        'End If

        getNotifyEmails(pEmpData.OrgJedSifra)
        getNotifyEmailsPG(pEmpData.EmployeeID)
        clientNotifyEmails = clientNotifyEmails.Distinct().ToList

        getTemplate()

        'clientNotifyForbidden.Add("aida.pekmez@bhrt.ba")
        'clientNotifyForbidden.Add("ruzica.zoric@bhrt.ba")
        'clientNotifyForbidden.Add("husnija.sehic@bhrt.ba")
        'clientNotifyForbidden.Add("mirela.hiseni@bhrt.ba")


        remForbiddenEmails()
        getSendEmails()
        getTemplate()

        Return True

    End Function

    Public Sub prepForEmailBySigner(ByVal pPutNalId As Integer, ByVal pDoc As Integer)

        Dim _ApiPutniNalog As New NgApiPnPutniNalog()
        Dim pEmpData As ngEmail = _ApiPutniNalog.getLastSigned(pPutNalId, pDoc)       ' puni path (sa  HttpContext) i naziv fajla

        clientEmail = pEmpData.email
        clientEmpId = pEmpData.employeeID
        clientEmailAttPath = pEmpData.attachPath

        Dim _templates As New Dictionary(Of Integer, String)
        _templates.Add(1, "email-pn-potpisan")
        _templates.Add(2, "email-pn-potpisan")

        clientTemplateName = _templates.Item(pDoc)
        clientNotifyEmails.Add(clientEmail)

        getTemplate()

        'clientNotifyForbidden.Add("aida.pekmez@bhrt.ba")
        remForbiddenEmails()

        getSendEmails()

    End Sub


    Public Shared Function getTemplates() As List(Of NgEmailTemplate)
        Dim ConnectionString As String = WebConfigurationManager.ConnectionStrings("MySQLConnection").ConnectionString
        Dim strSQL As String

        '
        ' Provjerava ima li red u tabeli
        strSQL = <![CDATA[
SELECT 
nt.`template_name` AS id,
nt.`template_name`
FROM `notify_templates` nt;
    ]]>.Value

        Dim myda As MySqlClient.MySqlDataAdapter
        Dim DtList As New DataTable

        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            Dim mycmd As New MySqlCommand

            mycmd.Connection = myconnection

            mycmd.CommandText = strSQL


            DtList.Clear()
            myda = New MySqlDataAdapter(mycmd)
            myda.Fill(DtList)
        End Using

        Dim _ngApiUtilities As New ngApiUtilities
        Return JsonConvert.DeserializeObject(Of List(Of NgEmailTemplate))(GetJson(DtList))
    End Function

    Private Shared Function GetJson(ByVal dt As DataTable) As String
        Dim serializer As New System.Web.Script.Serialization.JavaScriptSerializer()
        Dim rows As New List(Of Dictionary(Of String, Object))()
        Dim row As Dictionary(Of String, Object) = Nothing
        For Each dr As DataRow In dt.Rows
            row = New Dictionary(Of String, Object)()
            For Each dc As DataColumn In dt.Columns
                'If dc.ColumnName.Trim() = "TAGNAME" Then
                row.Add(dc.ColumnName.Trim(), dr(dc))
                'End If
            Next
            rows.Add(row)
        Next
        Return serializer.Serialize(rows)
    End Function
End Class
