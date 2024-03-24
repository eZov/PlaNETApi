Imports Microsoft.VisualBasic

Imports System.Web.Configuration
Imports MySql.Data
Imports MySql.Data.MySqlClient
Imports Newtonsoft.Json
Imports System.Data
Imports System.IO
Imports System.Xml.Linq
Imports System.Net
Imports System.Net.Mail


Public Class NgApiEmailNET

    Property userSession As NgApiSession

    Property smtpUsername As String
    Property smtpPassword As String
    Property smtpHost As String
    Property smtpPort As Integer
    Property smtpSSL As Boolean

    Property emailFrom As MailAddress
    Property emailSubject As String = ""
    Property emailBody As String = ""

    Property recEmailtTo As New List(Of NgEmaillAddress)    ' lista emailova za slanje - To
    Property recEmailtCc As New List(Of NgEmaillAddress)    ' lista emailova za slanje - Cc
    Property emailBodies As New List(Of String)             ' lista emailova za slanje - HTML body

    Sub New(ByRef pUserSession As NgApiSession)

        Dim securityProtocol = System.Net.ServicePointManager.SecurityProtocol

        If (securityProtocol <> 0) Then
            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12
        End If

        getSmtpConfig(1)
        userSession = pUserSession

    End Sub

    Sub New(ByRef pUserSession As NgApiSession, ByVal pAddress As String, ByVal pDisplayName As String)
        'smtpUsername = "AKIAY57RQ6W44IG32J7P"
        'smtpPassword = "BB75ehVjJp991ffuoHbtq4eUzn9qoHen6S0UIIjX0c1t"
        'smtpHost = "email-smtp.eu-west-1.amazonaws.com"
        'smtpPort = 587
        'smtpSSL = True


        setSmtpFrom(pAddress, pDisplayName)
        getSmtpConfig(1)
        userSession = pUserSession

    End Sub

    Sub New(ByVal _smtpUsername As String,
            ByVal _smtpPassword As String,
            ByVal _smtpHost As String,
            ByVal _smtpPort As Integer,
            ByVal _smtpSSL As Boolean)

        smtpUsername = _smtpUsername
        smtpPassword = _smtpPassword
        smtpHost = _smtpHost
        smtpPort = _smtpPort
        smtpSSL = _smtpSSL

    End Sub







    Public Sub getSmtpConfig(ByVal pId As Integer)


        Dim rawJson As String = NgApiSys.getSysJSON("api.emailSmtp")

        ' Convert to VB Class typed object
        Dim _SysJSONTbl As NgEmailsSmtpConfigs = JsonHelper.ToClass(Of NgEmailsSmtpConfigs)(rawJson)

        Dim dt As New DataTable
        Dim ds As New DataSet
        ds = JsonConvert.DeserializeObject(Of DataSet)(rawJson)
        dt = ds.Tables(0)
        Dim result() As DataRow = dt.Select("id = " + pId.ToString())

        smtpHost = result(0).Item("host")
        smtpUsername = result(0).Item("username")
        smtpPassword = result(0).Item("password")
        smtpPort = result(0).Item("port")
        smtpSSL = Convert.ToBoolean(result(0).Item("SSL"))


    End Sub


    ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
    '   TEMPLATES
    ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''



    Public Function getSmtpTemplateFromFile(ByVal pFileName As String) As String

        Dim _currentPath = ngApiPath.getCurrentPath()

        Dim _template As String = File.ReadAllText(Path.Combine(_currentPath, pFileName))

        Return _template

    End Function

    Public Function getSmtpTemplateById(ByVal pId As Integer) As String

        Dim dt As DataTable = getSmtpTemplate()
        Dim result() As DataRow = dt.Select("id = " + pId.ToString)

        Dim _template As String = result(0).Item("template")

        Return _template

    End Function

    Public Function getSmtpTemplateByName(ByVal pType As String, pName As String) As String

        Dim dt As DataTable = getSmtpTemplate()
        Dim result() As DataRow = dt.Select("type = '" + pType + "' AND name='" + pName + "'")

        Dim _template As String = result(0).Item("template")

        Return _template

    End Function

    Public Function getSmtpTemplate() As DataTable


        Dim rawJson As String = NgApiSys.getSysJSON("api.emailTemplates")


        ' Convert to VB Class typed object
        Dim _SysJSONTbl As NgEmailsSmtpConfigs = JsonHelper.ToClass(Of NgEmailsSmtpConfigs)(rawJson)

        Dim dt As New DataTable
        Dim ds As New DataSet
        ds = JsonConvert.DeserializeObject(Of DataSet)(rawJson)
        dt = ds.Tables(0)

        Return dt

    End Function


    '   SEND email 


    Public Function SendMail(pEmailTo As String, pEmailName As String,
                             pEmailSubject As String,
                             pEmailBody As String,
                             Optional AttachmentFileName As String = "") As Boolean


        Try
            Dim Message = New MailMessage()
            Message.IsBodyHtml = True

            'Dim _from As New MailAddress(mailBoxEmail, mailBoxName)
            Dim _to As New MailAddress(pEmailTo, pEmailName)

            Message.From = emailFrom
            Message.To.Add(_to)

            Message.Subject = pEmailSubject

            If AttachmentFileName.Length > 5 Then
                Dim _currentPath = ngApiPath.getCurrentPath()
            End If

            Message.Body = pEmailBody

            Using client = New SmtpClient(smtpHost, smtpPort)
                client.Credentials = New NetworkCredential(smtpUsername, smtpPassword)
                client.EnableSsl = smtpSSL

                client.Send(Message)

                Dim _log As String = "Email:" + _to.DisplayName + "(" + _to.Address + ")"
                Dim _comment As String = pEmailSubject
                userSession.insApiLog(_log, _comment)
            End Using


        Catch ex As Exception

            MsgBox(smtpHost & vbCrLf & ex.Message)
            userSession.insApiLog("Email:" + pEmailName + "(" + pEmailTo + ")", "Error: " + ex.Message)
            Return False

        End Try

        Return True

    End Function

    Public Function SendMail(pEmailSubject As String, Optional AttachmentFileName As String = "") As Boolean



        Try
            Dim Message = New MailMessage()
            Message.IsBodyHtml = True

            ' Dim _from As New MailAddress(mailBoxEmail, mailBoxName)

            Message.From = emailFrom
            Message.Subject = pEmailSubject

            If AttachmentFileName.Length > 5 Then
                Dim _currentPath = ngApiPath.getCurrentPath()
            End If


            Using client = New SmtpClient(smtpHost, smtpPort)
                client.Credentials = New NetworkCredential(smtpUsername, smtpPassword)
                client.EnableSsl = smtpSSL

                For i As Integer = 0 To recEmailtTo.Count - 1

                    Message.To.Clear()
                    Dim _to As New MailAddress(recEmailtTo.Item(i).name, recEmailtTo.Item(i).email)
                    Message.To.Add(_to)

                    Message.Body = emailBodies.Item(i)
                    client.Send(Message)

                Next

            End Using


        Catch ex As Exception

            MsgBox(smtpHost & vbCrLf & ex.Message)
            Return False

        End Try

        Return True

    End Function

    Public Function SendMail() As Boolean

        Dim _toEmailEx As String = "..."

        Try
            Dim Message = New MailMessage()
            Message.IsBodyHtml = True

            ' Dim _from As New MailAddress(mailBoxEmail, mailBoxName)

            Message.From = emailFrom
            Message.Subject = emailSubject
            Message.Body = emailBody

            'If AttachmentFileName.Length > 5 Then
            '    Dim _currentPath = HttpContext.Current.Server.MapPath("~/Data/")
            'End If

            Using client = New SmtpClient(smtpHost, smtpPort)
                client.Credentials = New NetworkCredential(smtpUsername, smtpPassword)
                client.EnableSsl = smtpSSL


                For Each el In recEmailtTo
                    Dim _to As New MailAddress(el.email, el.name)
                    Message.To.Add(_to)
                Next

                For Each el In recEmailtCc
                    Dim _cc As New MailAddress(el.email, el.name)
                    Message.CC.Add(_cc)
                Next

                _toEmailEx = "Email:" + Message.To.Item(0).DisplayName + "(" + Message.To.Item(0).Address + ")"

                client.Send(Message)

                Dim _log As String = "Email:" + Message.To.Item(0).DisplayName + "(" + Message.To.Item(0).Address + ")"
                Dim _comment As String = emailSubject
                userSession.insApiLog(_log, _comment)

            End Using


        Catch ex As Exception
            MsgBox(smtpHost & vbCrLf & ex.Message)
            userSession.insApiLog(_toEmailEx, "Error: " + ex.Message)

            Return False
        End Try

        Return True

    End Function

    Public Function SendMail(ByRef pEmailNotify As ngApiEmailNotify, ByVal pPutNalId As Integer, ByVal pEmpId As Integer, Optional ByVal pAttachment As String = "") As Boolean

        If pEmailNotify.clientNotifySendEmails.Count = 0 Then
            Return False
        End If

        Dim _ApiPutniNalog As New NgApiPnPutniNalog()
        Dim _PutNal As ngPnPutniNalog = _ApiPutniNalog.getPnPutniNalogById(pPutNalId, pEmpId)


        Dim _log As String = "Email: send..."

        Try
            Dim Message = New MailMessage()
            Message.IsBodyHtml = True


            Message.From = emailFrom
            Message.Subject = pEmailNotify.clientTemplateSubject.Replace("%__PnId__%", _PutNal.id)


            Using client = New SmtpClient(smtpHost, smtpPort)
                client.Credentials = New NetworkCredential(smtpUsername, smtpPassword)
                client.EnableSsl = smtpSSL


                For Each el In pEmailNotify.clientNotifySendEmails

                    Dim _body = pEmailNotify.clientTemplateBody

                    _body = _body.Replace("%__UserFullName__%", el.Value.name)
                    _body = _body.Replace("%__PnId__%", _PutNal.id)
                    _body = _body.Replace("%__PnName__%", _PutNal.ime_prezime)
                    _body = _body.Replace("%__PnMjesto__%", _PutNal.mjesto_putovanja)
                    _body = _body.Replace("%__PnTrajanje__%", _PutNal.trajanje_putovanja)
                    _body = _body.Replace("%__PnDatum__%", _PutNal.dat_poc_putovanja)
                    _body = _body.Replace("%__PnAkontacija__%", String.Format("{0:#.0,####}", _PutNal.iznos_akontacije))
                    _body = _body.Replace("%__PnAkontacijaIspl__%", String.Format("{0:#.0,####}", _PutNal.ispl_akontacije))
                    _body = _body.Replace("%__PnObracun__%", String.Format("{0:#.0,####}", _PutNal.UkIznosObracuna))
                    _body = _body.Replace("%__PnObracunRazl__%", String.Format("{0:#.0,####}", _PutNal.UkIznosRazlika))

                    Message.To.Clear()
                    Message.To.Add(New MailAddress(el.Value.email, el.Value.name))

                    Message.Body = _body

                    If pAttachment <> "" Then
                        Dim _att = New Attachment(pAttachment)
                        Message.Attachments.Add(_att)
                    End If


                    client.Send(Message)

                    _log = "Email:" + Message.To.Item(0).DisplayName + "(" + Message.To.Item(0).Address + ")"
                    Dim _comment As String = Message.Subject
                    userSession.insApiLog(_log, _comment)
                Next


            End Using


        Catch ex As Exception
            MsgBox(smtpHost & vbCrLf & ex.Message)
            userSession.insApiLog(_log, "Error: " + ex.Message)

            Return False
        End Try

        Return True

    End Function



    Public Sub setSmtpFrom(ByVal pAddress As String, ByVal pDisplayName As String)

        emailFrom = New MailAddress(pAddress, pDisplayName)

    End Sub

    Public Sub set1MailTo(ByVal pEmployeeId As Integer, Optional ByVal pSubject As String = "")

        Dim _ApiEmployee As New NgApiEmployee()

        Dim _employee As NgEmployee = _ApiEmployee.listItem(pEmployeeId)
        Dim _emailTo As String = IIf(_employee.emp_work_default_email = 1, _employee.emp_work_email, _employee.emp_oth_email)


        recEmailtTo.Add(New NgEmaillAddress(_emailTo, _employee.FirstName + " " + _employee.LastName))

        If pSubject.Length > 0 Then
            emailSubject = pSubject
        End If


    End Sub

    Public Sub set1MailCc(ByVal pEmployeeIds As List(Of Integer))

        Dim _ApiEmployee As New NgApiEmployee()

        Dim _employees As List(Of NgEmployee) = _ApiEmployee.listItems(pEmployeeIds)
        Dim _emailTo As String

        For Each el In _employees
            _emailTo = IIf(el.emp_work_default_email = 1, el.emp_work_email, el.emp_oth_email)
            recEmailtCc.Add(New NgEmaillAddress(_emailTo, el.FirstName + " " + el.LastName))
        Next



    End Sub

    ''' <summary>
    ''' Email za _parEmployeeId, From: info@mperun.net
    ''' </summary>
    ''' <param name="_parEmployeeId"></param>
    ''' <param name="_keyValues"></param>
    ''' <param name="_templateName"></param>
    Public Sub sendByTemplate(ByVal _parEmployeeId As Integer, ByRef _keyValues As Hashtable, ByVal _templateName As String,
                              Optional ByVal _ccEmpIds As List(Of Integer) = Nothing)



        Dim _body As String
        Dim _adminUser As String = "R.Zorić ( email: ruzica.zoric@bhrt.ba )"

        Select Case _templateName
            Case "email-creatuser"
                _body = Me.getSmtpTemplateFromFile("email-creatuser.html")
                Me.set1MailTo(_parEmployeeId)
                Me.emailSubject = "Kreiran pristup portalu"

                _body = _body.Replace("%__UserFullName__%", recEmailtTo.Item(0).name)
                _body = _body.Replace("%__UserName__%", _keyValues("%__UserName__%"))
                _body = _body.Replace("%__UserPass__%", _keyValues("%__UserPass__%"))
                _body = _body.Replace("%__AdminUser__%", _adminUser)

                'Me.SendOneMail(_body)
                emailBody = _body
                Me.SendMail()

            Case "email-changepass"
                _body = Me.getSmtpTemplateFromFile("email-changepass.html")
                Me.set1MailTo(_parEmployeeId)
                Me.emailSubject = "Promjena lozinke"

                _body = _body.Replace("%__UserFullName__%", recEmailtTo.Item(0).name)
                _body = _body.Replace("%__UserName__%", _keyValues("%__UserName__%"))
                _body = _body.Replace("%__UserPass__%", _keyValues("%__UserPass__%"))
                _body = _body.Replace("%__AdminUser__%", _adminUser)

                'Me.SendOneMail(_body)
                emailBody = _body
                Me.SendMail()

            Case "email-resetpass"
                _body = Me.getSmtpTemplateFromFile("email-resetpass.html")
                Me.set1MailTo(_parEmployeeId)
                Me.emailSubject = "Administrator je resetovao lozinku"

                _body = _body.Replace("%__UserFullName__%", recEmailtTo.Item(0).name)
                _body = _body.Replace("%__UserName__%", _keyValues("%__UserName__%"))
                _body = _body.Replace("%__UserPass__%", _keyValues("%__UserPass__%"))
                _body = _body.Replace("%__AdminUser__%", _adminUser)

                'Me.SendOneMail(_body)
                emailBody = _body
                Me.SendMail()

            Case "email-dnevniksend"
                _body = Me.getSmtpTemplateFromFile("email-dnevniksend.html")

                Me.set1MailTo(_parEmployeeId)
                Me.emailSubject = "Poslan dnevnik rada"

                If _ccEmpIds IsNot Nothing AndAlso _ccEmpIds.Count > 0 Then
                    set1MailCc(_ccEmpIds)
                End If

                _body = _body.Replace("%__UserFullName__%", recEmailtTo.Item(0).name)
                _body = _body.Replace("%__Dnevnik-MMYY__%", _keyValues("%__Dnevnik-MMYY__%"))
                _body = _body.Replace("%__Dnevnik-Table__%", _keyValues("%__Dnevnik-Table__%"))
                _body = _body.Replace("%__AdminUser__%", _adminUser)

                emailBody = _body
                Me.SendMail()

            Case "email-dnevnikunsend"
                _body = Me.getSmtpTemplateFromFile("email-dnevnikunsend.html")

                Me.set1MailTo(_parEmployeeId)
                Me.emailSubject = "Vraćen dnevnik rada"

                If _ccEmpIds IsNot Nothing Then
                    set1MailCc(_ccEmpIds)
                End If

                _body = _body.Replace("%__UserFullName__%", recEmailtTo.Item(0).name)
                _body = _body.Replace("%__Dnevnik-MMYY__%", _keyValues("%__Dnevnik-MMYY__%"))
                _body = _body.Replace("%__AdminUser__%", _adminUser)

                emailBody = _body
                Me.SendMail()

            Case "email-dnevnikkontstart"
                _body = Me.getSmtpTemplateFromFile("email-dnevnikkontstart.html")

                Me.set1MailTo(_parEmployeeId)
                Me.emailSubject = "Kontrola dnevnika rada u toku"

                If _ccEmpIds IsNot Nothing Then
                    set1MailCc(_ccEmpIds)
                End If

                _body = _body.Replace("%__UserFullName__%", recEmailtTo.Item(0).name)
                _body = _body.Replace("%__Dnevnik-MMYY__%", _keyValues("%__Dnevnik-MMYY__%"))
                _body = _body.Replace("%__AdminUser__%", _adminUser)

                emailBody = _body
                Me.SendMail()

            Case "email-dnevnikkontstop"
                _body = Me.getSmtpTemplateFromFile("email-dnevnikkontstop.html")

                Me.set1MailTo(_parEmployeeId)
                Me.emailSubject = "Kontrola dnevnika rada zaustavljena"

                If _ccEmpIds IsNot Nothing Then
                    set1MailCc(_ccEmpIds)
                End If

                _body = _body.Replace("%__UserFullName__%", recEmailtTo.Item(0).name)
                _body = _body.Replace("%__Dnevnik-MMYY__%", _keyValues("%__Dnevnik-MMYY__%"))
                _body = _body.Replace("%__AdminUser__%", _adminUser)

                emailBody = _body
                Me.SendMail()
            Case Else

        End Select


    End Sub



    Public Function sendByTemplate(
                             ByVal _emailTo As String,
                             ByVal _emailName As String,
                             ByVal _emailSubject As String,
                             ByRef _keyValues As Dictionary(Of String, String),
                             ByVal _body As String
                             ) As Boolean

        Dim __body As String = _body
        For Each e In _keyValues
            __body = __body.Replace(e.Key, e.Value)
        Next

        emailSubject = _emailSubject
        recEmailtTo.Add(New NgEmaillAddress(_emailTo, _emailName))

        emailBody = __body

        Return SendMail()

    End Function


    '
    ' Formatiraj za email
    '
    Public Function ConvertDataTableToHTML(ByVal pDt As DataTable) As String

        Dim dt As DataTable = pDt.Copy()
        dt.Columns.Remove("id")
        dt.Columns.Remove("employeeID")
        dt.Columns.Remove("sifra_placanja")
        dt.Columns.Remove("week_day")
        dt.Columns.Remove("keyday")
        dt.Columns.Remove("locked")
        dt.Columns.Remove("evd_podnio")
        dt.Columns.Remove("evd_odobrio")
        dt.Columns.Remove("evd_kontrolisao")
        dt.Columns.Remove("locked_ext")
        dt.Columns.Remove("vrijeme_ukupno")

        dt.Columns.Add("datum2", GetType(String))
        dt.Columns("datum2").SetOrdinal(0)

        dt.Columns("vrijeme_od").ColumnName = "od"
        dt.Columns("vrijeme_do").ColumnName = "do"
        'dt.Columns("vrijeme_ukupno").ColumnName = "ukupno"

        For Each row As DataRow In dt.Rows
            Dim _datum As String = row.Field(Of String)("datum").Substring(0, 10)
            row("datum2") = _datum.Substring(8, 2) + "." + _datum.Substring(5, 2) + "." + _datum.Substring(2, 2)

            Dim _vrijeme As String = row.Field(Of String)("od")
            row("od") = _vrijeme.Replace("00:00", "")

            _vrijeme = row.Field(Of String)("do")
            row("do") = _vrijeme.Replace("00:00", "")

            '_vrijeme = row.Field(Of String)("ukupno")
            'row("ukupno") = _vrijeme.Replace("00:00", "")
        Next

        dt.Columns.Remove("datum")
        dt.Columns("datum2").ColumnName = "datum"

        ConvertDataTableToHTML =
            <table border="1" cellpadding="2" cellspacing="1" style="border-collapse: separate; mso-table-lspace: 0pt; mso-table-rspace: 0pt; width: 100%;">
                <thead>
                    <tr>
                        <%=
                            From col As DataColumn In dt.Columns.Cast(Of DataColumn)()
                            Select <th><%= col.ColumnName %></th>
                        %>
                    </tr>
                </thead>
                <tbody>
                    <%=
                        From row As DataRow In dt.Rows.Cast(Of DataRow)()
                        Select
                        <tr>
                            <%=
                                From col As DataColumn In dt.Columns.Cast(Of DataColumn)()
                                Select <td style="margin:3px;"><p><%= row(col) %></p></td>
                            %>
                        </tr>
                    %>
                </tbody>
            </table>.ToString()
    End Function

End Class